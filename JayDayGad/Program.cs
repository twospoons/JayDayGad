using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.Seeed;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Interfaces;

namespace JayDayGad
{
    public partial class Program
    {

        AnalogInput[] sensors = new AnalogInput[3];

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/
            
            sensors[0] = extender.SetupAnalogInput(GT.Socket.Pin.Three);
            sensors[1] = extender.SetupAnalogInput(GT.Socket.Pin.Four);
            sensors[2] = extender.SetupAnalogInput(GT.Socket.Pin.Five);
            
            var timer = new GT.Timer(500);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }
        private static double sensorVoltage = 4.80D;
        private static double mvpermm = sensorVoltage / 512D;
        void timer_Tick(GT.Timer timer)
        {
            //var x = 1;
            //Debug.Print("---------------------------------------------------------------");
            for (var x = 0; x < sensors.Length; x++)
            {
                var voltage = sensors[x].ReadVoltage();
                var dist = mvpermm * voltage * 10000.0D;
                
                Debug.Print("sensor [" + x.ToString() + "] dist: " + dist.ToString("F2") + "CM volt:" + voltage.ToString("F2"));
            }
            //Debug.Print("---------------------------------------------------------------");
        }
    }
}
