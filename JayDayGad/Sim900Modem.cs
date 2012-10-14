using System;
using Microsoft.SPOT;
using GTM = Gadgeteer.Modules.GHIElectronics;
using GTI = Gadgeteer.Interfaces;
using GT = Gadgeteer;
using System.Threading;
using System.Text;

namespace JayDayService
{
    public class ModemResponse
    {
        public bool GotExpectedResponse { get; set; }
        public bool Error { get; set; }
        public bool TimeOut { get; set; }
        public string ResponseText { get; set; }
    }

    public class Sim900Modem
    {
        GTM.Extender modem;
        GTI.DigitalOutput resetPin;
        GTI.DigitalOutput powerPin;
        GT.Socket socket; 
        System.IO.Ports.SerialPort serial;
        byte[] bufferd;
        Gadgeteer.Modules.GHIElectronics.Display_HD44780 display;
        private static bool signalOk;

        public Sim900Modem(int port, GTM.Display_HD44780 display)
        {
            this.display = display;
            socket = GT.Socket.GetSocket(port, true, null, null);
            powerPin = new GTI.DigitalOutput(socket, GT.Socket.Pin.Three, false, null);
            serial = new System.IO.Ports.SerialPort("COM2", 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            serial.ReadTimeout = 1000;
            serial.Open();
            bufferd = new byte[32];
        }

        public void SendHttpData(string domainName, string url)
        {
            if (!signalOk)
            {
                Utilities.WriteDebug("Too weak signal", display);
                return;
            }

            SendData("ATE0\r\n", "OK\r\n"); // OK - local echo off
            Thread.Sleep(100);
            SendData("AT+CMGF=1\r\n", "OK\r\n"); // OK - text mode
            Thread.Sleep(100);
            SendData("AT+CGATT=1\r\n", "OK\r\n"); // OK
            Thread.Sleep(100);
            // att gophone
            //SendData("AT+CGDCONT=1, \"IP\", \"wap.cingular\"\r\n", "OK\r\n"); // OK
            // crossbridge apn
            SendData("AT+CGDCONT=1, \"IP\", \"a105.2way.net\"\r\n", "OK\r\n"); // OK
            Thread.Sleep(100);
            // att gophone
            //SendData("AT+CSTT=\"wap.cingular\", \"WAP@CINGULARGPRS.COM\", \"CINGULAR1\"\r\n", "OK\r\n"); // OK
            SendData("AT+CSTT=\"a105.2way.net\", \"WAP@CINGULARGPRS.COM\", \"CINGULAR1\"\r\n", "OK\r\n");
            Thread.Sleep(100);
            SendData("AT+CIICR\r\n", "OK\r\n"); // OK
            Thread.Sleep(100);
            SendData("AT+CIFSR\r\n", "\r\n"); // IP ADDRESS
            Thread.Sleep(100);
            SendData("AT+CIPSTATUS\r\n", "OK\r\n\r\nSTATE: IP STATUS\r\n"); // OK\r\nSTATE: IP STATUS\r\n
            Thread.Sleep(100);
            SendData("AT+CIPSTART=\"TCP\",\"" + domainName + "\",\"80\"\r\n", "OK\r\n\r\nCONNECT OK\r\n");
            // OK\r\nCONNECT OK\r\n
            Thread.Sleep(100);
            SendData("AT+CIPSEND\r\n", ">");
            Thread.Sleep(100);
            SendData(
                "GET " + url + " HTTP/1.1\r\nHost: " + domainName + "\r\nConnection: keep-alive\r\n\r\n" + (char)26,
                "\r\n||\r\n");
            Thread.Sleep(100);
            SendData("AT+CIPSHUT\r\n", "\r\nSHUT OK\r\n");
            Thread.Sleep(100);
        }

        public void Reset()
        {
            resetPin.Write(true);
            Thread.Sleep(1000);
            resetPin.Write(false);
        }

        private ModemResponse SendData(string data, string waitFor, bool debug = true)
        {
            serial.Flush();
            if (debug)
                Utilities.WriteDebug(Utilities.Replace(data, "\r\n", ""), display);

            if (data.Length > 0)
            {
                var buffer = Encoding.UTF8.GetBytes(data);
                serial.Write(buffer, 0, buffer.Length);
            }
            var now = DateTime.Now.Ticks;
            var outPut = "";
            var waitForFound = false;
            var error = false;
            var timeOut = false;
            var moreData = false;
            int bufferLen;

            Thread.Sleep(100);
            while ((!timeOut && !waitForFound && !error) || moreData)
            {
                if (!(DateTime.Now.Ticks < now + (10000 * 10000)))
                {
                    timeOut = true;
                }

                if (serial.BytesToRead > 0)
                {
                    bufferLen = serial.BytesToRead < 32 ? serial.BytesToRead : 32;
                    serial.Read(bufferd, 0, bufferLen);
                    for (var i = 0; i < bufferLen; i++)
                    {
                        outPut += (char)bufferd[i];
                    }
                    Debug.Print(outPut);
                    if (outPut.IndexOf("ERROR\r\n") != -1)
                    {
                        error = true;
                    }
                    else if (outPut.IndexOf(waitFor) != -1)
                    {
                        waitForFound = true;
                    }
                }

                moreData = serial.BytesToRead > 0;
                if (!moreData)
                    Thread.Sleep(100);

                Thread.Sleep(10);
            }

            if (debug)
                Utilities.WriteDebug(Utilities.Replace(outPut, "\r\n", ""), display);

            return new ModemResponse
            {
                Error = error,
                TimeOut = timeOut,
                GotExpectedResponse = waitForFound,
                ResponseText = outPut
            };

        }

        public ModemResponse GetImei()
        {
            var imei = SendData("AT+GSN\r\n", "OK\r\n");
            imei.ResponseText = Utilities.Replace(imei.ResponseText, "\r\n", "");
            imei.ResponseText = Utilities.Replace(imei.ResponseText, "OK", "");
            imei.ResponseText = Utilities.Replace(imei.ResponseText, "AT+GSN", "");
            return imei;
        }

        public void UpdateSignalStrength()
        {
            var resp = GetSignalStrength();
            Utilities.WriteDebug(resp, display);
        }


        public string GetSignalStrength()
        {
            signalOk = false;
            SendData("ATE0\r\n", "OK\r\n"); // OK - local echo off
            var ret = SendData("AT+CSQ\r\n", "OK\r\n", false);
            string signalQuality = "No signal";
            if (ret.GotExpectedResponse)
            {
                var resp = ret.ResponseText.Substring(ret.ResponseText.IndexOf("+CSQ: ") + 6);
                resp = resp.Substring(0, resp.IndexOf("\r\n"));
                resp = resp.Substring(0, resp.IndexOf(","));
                if (resp != "99")
                {
                    signalOk = true;
                    var db = -113 + (Convert.ToInt16(resp) * 2);
                    if (db <= -95)
                    {
                        signalQuality = "+   : " + db + "dBm";//" (" + resp + ")";
                    }
                    else if (db <= -85)
                    {
                        signalQuality = "++  : " + db + "dBm";// (" + resp + ")";
                    }
                    else if (db <= -75)
                    {
                        signalQuality = "+++ : " + db + "dBm";// (" + resp + ")";
                    }
                    else
                    {
                        signalQuality = "++++: " + db + "dBm";// (" + resp + ")";
                    }
                }

                return signalQuality;
            }

            return "";
        }

        //public void PowerDown2()
        //{
        //    int checkResponse = 0;
        //    int tryTurnOffCounter = 0;
        //    bool modemOff = false;
        //    ModemResponse response;
        //    Utilities.WriteDebug("Power down modem", display); 
        //    while (tryTurnOffCounter < 5 && !modemOff)
        //    {
        //        powerPin.Write(true);
        //        Thread.Sleep(2000);
        //        powerPin.Write(false);
        //        checkResponse = 0;
        //        Thread.Sleep(500);
        //        while (checkResponse < 5 && !modemOff)
        //        {
        //            response = SendData("", "NORMAL POWER DOWN\r\n");
        //            if(!response.GotExpectedResponse)
        //            {
        //                checkResponse++;
        //                Thread.Sleep(0);
        //            }
        //            else
        //            {
        //                modemOff = true;
        //            }
        //            checkResponse++;
        //        }

        //        if (!modemOff)
        //        {
        //            Utilities.WriteDebug("Modem not off", display);
        //        }
        //        else
        //        {
        //            Utilities.WriteDebug("Modem off", display);
        //        }
        //        tryTurnOffCounter++;
        //    }
        //}

        public void PowerDown()
        {
            
            serial.DiscardOutBuffer();
            serial.DiscardInBuffer();

            var iterations = 0;
            while (iterations < 5)
            {
                Utilities.WriteDebug("Power down modem", display);
                powerPin.Write(true);
                Thread.Sleep(1500);
                powerPin.Write(false);
                Thread.Sleep(500);
                Utilities.WriteDebug("Waiting for gsm", display);
                var response = SendData("AT", "NORMAL POWER DOWN\r\n", false);                
                if (response.GotExpectedResponse)
                {
                    Utilities.WriteDebug("Modem off", display);
                    break;
                }
                Utilities.WriteDebug("Modem not off", display);
                iterations++;
            }
        }

        public void PowerUp()
        {
            //powerPin.Write(true);
            //Thread.Sleep(2000);
            //powerPin.Write(false);
            //return;

            serial.DiscardOutBuffer();
            serial.DiscardInBuffer();
            var iterations = 0;
            while (iterations < 5)
            {
                Utilities.WriteDebug("Resetting modem", display);
                powerPin.Write(true);
                Thread.Sleep(2000);
                powerPin.Write(false);
                Thread.Sleep(500);
                Utilities.WriteDebug("Waiting for gsm", display);
                // call ready only works when there is a sim in it, just testing for now
                var response = SendData("AT", "IIII", false);
                //var response = SendData("AT+GSN\r\n", "OK\r\n");
                if (response.GotExpectedResponse)
                {
                    Utilities.WriteDebug("Modem ready", display);
                    Thread.Sleep(1000);
                    break;
                }
                Utilities.WriteDebug("Modem not ready", display);
                iterations++;
                Thread.Sleep(5000);
            }
        }

    }
}
