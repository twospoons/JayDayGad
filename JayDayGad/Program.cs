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

namespace JayDayGad
{
    public partial class Program
    {
        public static byte[] vram = new byte[128 * 64 * 2];
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
            //Util.BitmapConvertBPP(Resources.GetBitmap(Resources.BitmapResources.test).GetBitmap(), vram, Util.BPP_Type.BPP16_BGR_BE);
            vram[1] = 244;
            vram[2] = 10;
            //oledDisplay.SimpleGraphics.BackgroundColor = GT.Color.White;
            var timer = new GT.Timer(1000);
            timer.Tick += new GT.Timer.TickEventHandler(timer_Tick);
            timer.Start();
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void timer_Tick(GT.Timer timer)
        {
            oledDisplay.FlushRawBitmap(0, 0, 128, 64, vram);
            //oledDisplay.SimpleGraphics.DisplayTextInRectangle("Now running!", 2, 2, 120, 20, GT.Color.Black, Resources.GetFont(Resources.FontResources.NinaB));

        }
    }
}
