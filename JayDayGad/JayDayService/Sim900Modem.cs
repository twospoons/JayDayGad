using System;
using Microsoft.SPOT;
using GTM = Gadgeteer.Modules.GHIElectronics;
using GTI = Gadgeteer.Interfaces;
using Gadgeteer;

namespace JayDayService
{
    public class Sim900Modem
    {
        GTM.Extender modem;
        GTI.DigitalOutput resetPin;
        GTI.DigitalOutput powerPin;
        Socket socket; 
        GTI.Serial serial;

        public Sim900Modem(int port)
        {
            modem = new GTM.Extender(port);            
            socket = Socket.GetSocket(port, true, modem, null);
            //serial = new GTI.Serial(socket, 19200, GTI.Serial.SerialParity.None, GTI.Serial.SerialStopBits.One, 8, GTI.Serial.HardwareFlowControl.NotRequired, modem);            
        }

        public void PowerOn()
        {
        }

        public void PowerOff()
        {
        }

        public void Reset()
        {
        }


    }
}
