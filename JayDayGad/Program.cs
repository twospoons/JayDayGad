using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;

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
        GTM.GHIElectronics.Display_HD44780 display_HD44780;
        MethodInfo serviceRun;
        MethodInfo serviceStop;
        MethodInfo serviceInit;
        GT.Timer timer;
        bool modemOn = false;
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            display_HD44780 = new GTM.GHIElectronics.Display_HD44780(1);
            CheckForServiceUpdate();

            Display("Loading assembly", "");

            JayDayService.Service.Init(display_HD44780);
            JayDayService.Service.Run();
            //var bytes = Resources.GetBytes(Resources.BinaryResources.JayDayService);
            //var asm = Assembly.Load(bytes);
            //var t = asm.GetType("JayDayService.Service");
            //if (t != null)
            //{
            //    Display("Assembly Loaded", "Starting Service");
            //    serviceRun = t.GetMethod("Run");
            //    serviceInit = t.GetMethod("Init");
            //    serviceStop = t.GetMethod("Stop");
            //    // display is shared.
            //    serviceInit.Invoke(null, new object[] { display_HD44780 });
            //    serviceRun.Invoke(null, null);
            //    // PowerState.Reboot(true) .. to free up memory..
            //}

            timer = new GT.Timer(10000);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
        }

        void timer_Tick(GT.Timer timer)
        {
            //if (!modemOn)
            //{
            //    JayDayService.Service.ModemOn();
            //    modemOn = true;
            //}
            //else
            //{
            //    JayDayService.Service.ModemOff();
            //    modemOn = true;                
            //    timer.Stop();
            //}

            //JayDayService.Service.UpdateSignalStrength();
        }

        void CheckForServiceUpdate()
        {            
            Display("Update check", "");
            Thread.Sleep(1000);
            Display("Version is", "latest");
            Thread.Sleep(1000);
        }

        void Display(String text1, String text2)
        {
            display_HD44780.Clear();
            display_HD44780.ShutBacklightOff();
            display_HD44780.CursorHome();
            display_HD44780.TurnBacklightOn();
            display_HD44780.PrintString(text1);
            display_HD44780.SetCursor(1, 0);
            display_HD44780.PrintString(text2);
        }
    }
}
