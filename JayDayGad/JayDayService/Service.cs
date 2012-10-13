using System;
using Microsoft.SPOT;
using Gadgeteer.Interfaces;
using GTM = Gadgeteer.Modules;
using GT = Gadgeteer;

namespace JayDayService
{
    public class Service
    {
        // do not init static variables as .netmf will fail to dynamically load this assembly
        private static AnalogInput[] sensors;
        private static Sim900Modem modem;
        private static Gadgeteer.Modules.GHIElectronics.Extender extender;
        private static Gadgeteer.Modules.GHIElectronics.Display_HD44780 display_HD44780;
        private static GT.Timer sensorTimer;
             
        private static double sensorVoltage;
        private static double mvpermm;

        private static string version;

        static void sensorTimer_Tick(GT.Timer timer)
        {
            Debug.Print("in service");
            String disp = "";
            for (var x = 0; x < sensors.Length; x++)
            {
                if (sensors[x] != null)
                {
                    var voltage = sensors[x].ReadVoltage();
                    var dist = mvpermm * voltage * 10000.0D;
                    //DisplaySecondLine("S1: " + 
                    disp += voltage.ToString("F2");
                    if (x < sensors.Length - 1)
                    {
                        disp += ",";
                    }
                    Debug.Print("sensor [" + x.ToString() + "] dist: " + dist.ToString("F2") + "CM volt:" + voltage.ToString("F2"));
                }
            }

            DisplaySecondLine(disp);
            
        }

        public static void Init(GTM.GHIElectronics.Display_HD44780 display)
        {
            version = "1.0";
            sensorVoltage = 4.80D;
            mvpermm = sensorVoltage / 512D;
            if (display_HD44780 == null)
            {
                display_HD44780 = display;
            }

            if (extender == null)
            {
                extender = new GTM.GHIElectronics.Extender(3);
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

            if (sensorTimer == null)
            {
                sensorTimer = new GT.Timer(500);
                sensorTimer.Tick += new GT.Timer.TickEventHandler(sensorTimer_Tick);
            }

            if (modem == null)
            {
                modem = new Sim900Modem(2);
            }
            
            Display("ozlo, llc " + version, "");

        }

        public static void DisplaySecondLine(String text2)
        {
            display_HD44780.SetCursor(1, 0);
            display_HD44780.PrintString(text2);
        }

        public static void Display(String text1, String text2)
        {
            display_HD44780.Clear();
            display_HD44780.CursorHome();
            display_HD44780.TurnBacklightOn();
            display_HD44780.PrintString(text1);
            DisplaySecondLine(text2);
        }

        public static void Run()
        {
            
            sensorTimer.Start();
        }

        public static void Stop()
        {
            sensorTimer.Stop();
        }
    }
}
