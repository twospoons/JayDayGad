using System;
using Microsoft.SPOT;
using Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules.GHIElectronics;
using GT = Gadgeteer;
using System.Collections;
using System.Threading;

namespace JayDayService
{
    public class Service
    {
        // do not init static variables as .netmf will fail to dynamically load this assembly
        private static AnalogInput[] sensors;
        private static Sim900Modem modem;
        private static GTM.Extender extender;
        private static GTM.Display_HD44780 display;
        private static GT.Timer sensorTimer;
        private static GT.Timer firstTimeTimer;
             
        private static double sensorVoltage;
        private static double mvpermm;

        private static string version;
        private static int maxSensorIterations;
        private static int currentSensorIteration;

        private static ArrayList sensorMedians;

        private static void SendSensorData(float[] values)
        {
            modem.PowerUp();
            string data = "[";
            for(var x = 0; x < values.Length; x++)
            {
                data += values[x].ToString("F2") + ",";
            }

            data = data.Substring(0, data.Length - 1) + "]";
            modem.UpdateSignalStrength();
            Thread.Sleep(2000);
            var imei = modem.GetImei();
            Utilities.WriteDebug(imei.ResponseText, display);
            var url = "/DataLogger/Data?data=" + data + "&from=" + imei.ResponseText;
            modem.SendHttpData("tracker.ozlollc.com", url);

            modem.PowerDown();
        }

        private static void SendData()
        {
            var finalMedians = new float[sensors.Length];
            // sort the readings..                    
            for (var x = 0; x < sensors.Length; x++)
            {
                var storedMedians = new float[maxSensorIterations];
                for (var i = 0; i < maxSensorIterations; i++)
                {
                    storedMedians[i] = ((float[])sensorMedians[i])[x];
                }

                Utilities.QuickSort(0, maxSensorIterations - 1, ref storedMedians);
                finalMedians[x] = storedMedians[storedMedians.Length / 2];
            }


            // send final medians..
            var disp = "";
            for (var x = 0; x < sensors.Length; x++)
            {
                disp += finalMedians[x].ToString("F1") + " ";
            }

            SendSensorData(finalMedians);

            Utilities.WriteDebug(disp, display);
        }

        static void sensorTimer_Tick(GT.Timer timer)
        {
            bool dataSent = false;
            try
            {
                sensorTimer.Stop();
                var medians = ReadSensorData();
                sensorMedians.Add(medians);
                
                currentSensorIteration++;

                if (currentSensorIteration == maxSensorIterations)
                {                    
                    dataSent = true; // even though something might fail, we will clear out our arrays                    
                    Utilities.WriteDebug("done reading", display);
                    Thread.Sleep(1000);
                    currentSensorIteration = 0;
                    SendData();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                // if we just sent data, clear the array list with medians
                if (dataSent)
                {
                    sensorMedians.Clear();
                }
                sensorTimer.Start();
            }
        }

        public static void Init(GTM.Display_HD44780 display)
        {
            version = "v 1.0";
            sensorVoltage = 4.80D;
            mvpermm = sensorVoltage / 512D;
            Service.display = display;

            if (extender == null)
            {
                extender = new GTM.Extender(3);
            }

            if (sensors == null)
            {
                sensors = new AnalogInput[3];
            }

            if (sensors[0] == null)
                sensors[0] = extender.SetupAnalogInput(GT.Socket.Pin.Three);
            if (sensors[1] == null)
                sensors[1] = extender.SetupAnalogInput(GT.Socket.Pin.Four);
            if (sensors[2] == null)
                sensors[2] = extender.SetupAnalogInput(GT.Socket.Pin.Five);

            sensorMedians = new ArrayList();
            maxSensorIterations = 4; // must be div by 2

            if (sensorTimer == null)
            {
                sensorTimer = new GT.Timer(500);
                sensorTimer.Tick += new GT.Timer.TickEventHandler(sensorTimer_Tick);
            }

            if (firstTimeTimer == null)
            {
                firstTimeTimer = new GT.Timer(500);
                firstTimeTimer.Tick += new GT.Timer.TickEventHandler(firstTimeTimer_Tick);
            }

            if (modem == null)
            {
                modem = new Sim900Modem(2, display);
            }

            Utilities.DisplayOnLine1(String.Concat("ozlo, llc ", version), display);
        }

        static void firstTimeTimer_Tick(GT.Timer timer)
        {
            modem.PowerDown();
            firstTimeTimer.Stop();
            sensorTimer.Start();
        }

        public static void Run()
        {
            firstTimeTimer.Start();
        }

        public static void Stop()
        {
            sensorTimer.Stop();
        }

        private static float[] ReadSensorData()
        {
            Utilities.WriteDebug("sensors on", display);                        
            var maxReadings = 20;

            var medians = new float[sensors.Length];
            var list = new float[maxReadings];

            for (var x = 0; x < sensors.Length; x++)
            {                
                sensors[x].Active = true;
                Thread.Sleep(50);
                for (var i = 0; i < maxReadings; i++)
                {
                    list[i] = (float) sensors[x].ReadVoltage();
                    Thread.Sleep(50);
                }
                
                Utilities.QuickSort(0, maxReadings - 1, ref list);
                medians[x] = list[maxReadings / 2];
            }

            string disp = "";
            for (var x = 0; x < sensors.Length; x++)
            {
                disp += medians[x].ToString("F1") + " ";
            }

            Utilities.WriteDebug(disp, display);
            return medians;

        }


    }
}
