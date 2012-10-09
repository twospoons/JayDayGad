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
using Gadgeteer.Interfaces;
using System.Reflection;
using System.IO;
//using Microsoft.SPOT.Hardware;

namespace JayDayGad
{
    public partial class Program
    {

        //AnalogInput[] sensors = new AnalogInput[3];
        MethodInfo serviceRun;
        MethodInfo serviceStop;
        MethodInfo serviceInit;
        GT.Timer timer;
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            var bytes = Resources.GetBytes(Resources.BinaryResources.JayDayService);
            var asm = Assembly.Load(bytes);
            var t = asm.GetType("JayDayService.Service");
            if (t != null)
            {
                serviceRun = t.GetMethod("Run");
                serviceInit = t.GetMethod("Init");
                serviceStop = t.GetMethod("Stop");
                serviceInit.Invoke(null, null);
                serviceRun.Invoke(null, null);
                // PowerState.Reboot(true) .. to free up memory..
            }
            
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
            
            //sensors[0] = extender.SetupAnalogInput(GT.Socket.Pin.Three);
            //sensors[1] = extender.SetupAnalogInput(GT.Socket.Pin.Four);
            //sensors[2] = extender.SetupAnalogInput(GT.Socket.Pin.Five);
            
            timer = new GT.Timer(500);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void timer_Tick(GT.Timer timer)
        {
            Debug.Print("it");
            //serviceRun.Invoke(null, null);
            timer.Stop();
        }
        //private static double sensorVoltage = 4.80D;
        //private static double mvpermm = sensorVoltage / 512D;
        //void timer_Tick(GT.Timer timer)
        //{
        //    //var x = 1;
        //    //Debug.Print("---------------------------------------------------------------");
        //    for (var x = 0; x < sensors.Length; x++)
        //    {
        //        var voltage = sensors[x].ReadVoltage();
        //        var dist = mvpermm * voltage * 10000.0D;
                
        //        Debug.Print("sensor [" + x.ToString() + "] dist: " + dist.ToString("F2") + "CM volt:" + voltage.ToString("F2"));
        //    }

        //    var t = serviceGetSomething.Invoke(null, null);
        //    Debug.Print(t.ToString());
        //    //Debug.Print("---------------------------------------------------------------");
        //}
    }
}
