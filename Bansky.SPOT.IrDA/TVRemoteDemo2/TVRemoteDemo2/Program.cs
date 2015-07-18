using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using DeviceSolutions.SPOT.Hardware;
using Bansky.SPOT.IrDA;
using Bansky.SPOT.Input;

namespace TVRemoteDemo2
{
    public class Program : Application
    {
        public Program()
        {
            SonyReceiver sonyRemote = new SonyReceiver(Meridian.Pins.GPIO1);

            TVRemoteButtonPad[] buttons = new TVRemoteButtonPad[]
            {
                new TVRemoteButtonPad(0, Button.VK_1),
                new TVRemoteButtonPad(1, Button.VK_2),
                new TVRemoteButtonPad(2, Button.VK_3),
                new TVRemoteButtonPad(3, Button.VK_4),
                new TVRemoteButtonPad(4, Button.VK_5),
                new TVRemoteButtonPad(5, Button.VK_6),
                new TVRemoteButtonPad(6, Button.VK_7),
                new TVRemoteButtonPad(7, Button.VK_8),
                new TVRemoteButtonPad(8, Button.VK_9),
                new TVRemoteButtonPad(9, Button.VK_0),
                new TVRemoteButtonPad(16, Button.VK_NEXT),
                new TVRemoteButtonPad(17, Button.VK_PRIOR),
                new TVRemoteButtonPad(18, Button.VK_VOLUME_UP),
                new TVRemoteButtonPad(19, Button.VK_VOLUME_DOWN),            
            };

            TVRemoteInputProvider inputProvider = new TVRemoteInputProvider(null, sonyRemote, (int)SonyReceiver.Devices.TV, buttons);
        }

        /// <summary>
        /// OnStartUp event handler
        /// </summary>        
        protected override void OnStartup(EventArgs e)
        {
            this.MainWindow = new MainWindow();
            base.OnStartup(e);
        }

        public static void Main()
        {
            new Program().Run();
        } 
    }
}
