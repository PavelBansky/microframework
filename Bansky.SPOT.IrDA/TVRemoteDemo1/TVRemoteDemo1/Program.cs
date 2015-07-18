using System;
using Microsoft.SPOT;
using System.Threading;
using DeviceSolutions.SPOT.Hardware;
using Bansky.SPOT.IrDA;

namespace TVRemoteDemo1
{
    public class Program
    {
        public static void Main()
        {
            TVRemoteReceiver sony = new SonyReceiver(Meridian.Pins.GPIO1);
            sony.DataReceived += new TVRemoteReceiver.TVRemoteDataHandler(sony_OnDataReceived);

            while (true)
            {
                Thread.Sleep(20);
            }
        }

        static void sony_OnDataReceived(TVRemoteReceiver sender, int command, int address)
        {
            Debug.Print("----");
            Debug.Print(address.ToString());
            Debug.Print(command.ToString());
        }
    }
}
