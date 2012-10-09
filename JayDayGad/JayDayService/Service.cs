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
        private static Gadgeteer.Modules.Seeed.CellularRadio cellularRadio;
        private static Gadgeteer.Modules.GHIElectronics.Extender extender;
        private static Gadgeteer.Modules.GHIElectronics.Display_HD44780 display_HD44780;
        private static GT.Timer sensorTimer;
        private static double sensorVoltage;
        private static double mvpermm;

        static void sensorTimer_Tick(GT.Timer timer)
        {
            Debug.Print("in service");
            for (var x = 0; x < sensors.Length; x++)
            {
                if (sensors[x] != null)
                {
                    var voltage = sensors[x].ReadVoltage();
                    var dist = mvpermm * voltage * 10000.0D;

                    Debug.Print("sensor [" + x.ToString() + "] dist: " + dist.ToString("F2") + "CM volt:" + voltage.ToString("F2"));
                }
            }
            
        }

        public static void Init()
        {
            sensorVoltage = 4.80D;
            mvpermm = sensorVoltage / 512D;
            if (display_HD44780 == null)
                display_HD44780 = new GTM.GHIElectronics.Display_HD44780(1);
            if (cellularRadio == null)
                cellularRadio = new GTM.Seeed.CellularRadio(2);
            if (extender == null)
                extender = new GTM.GHIElectronics.Extender(3);
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
