//------------------------------------------------------------------------------
// Bansky.SPOT.Input
//
// http://bansky.net/blog
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution 3.0 Unported" license.
// http://creativecommons.org/licenses/by/3.0/
//
//------------------------------------------------------------------------------
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Bansky.SPOT.IrDA;

namespace Bansky.SPOT.Input
{
    /// <summary>
    /// TV Remote Receiver Input provider
    /// </summary>
    public class TVRemoteInputProvider
    {
        /// <summary>
        /// Creates new instance of input provider
        /// </summary>
        /// <param name="source">Presentation source</param>
        /// <param name="controller">TV Remote Receiver controller driver</param>
        /// <param name="address">Address of device. Use -1 for everything.</param>
        /// <param name="buttons">Command to WPF button maping</param>
        public TVRemoteInputProvider(PresentationSource source, TVRemoteReceiver controller, int address, TVRemoteButtonPad[] buttons)
        {
            // Set the input source.
            this.source = source;
            // Register our object as an input source with the input manager and get back an
            // InputProviderSite object which forwards the input report to the input manager,
            // which then places the input in the staging area.
            site = InputManager.CurrentInputManager.RegisterInputProvider(this);
            // Create a delegate that refers to the InputProviderSite object's ReportInput method
            callback = new ReportInputCallback(site.ReportInput);
            Dispatcher = Dispatcher.CurrentDispatcher;

            deviceAddress = address;
            this.buttons = buttons;
            
            ButtonAction = RawButtonActions.ButtonUp;

            controller.DataReceived += new TVRemoteReceiver.TVRemoteDataHandler(controller_DataReceived);
        }

        /// <summary>
        /// Get or set button action for received command
        /// </summary>
        public RawButtonActions ButtonAction;

        /// <summary>
        /// Data received handler.
        /// Fires WPF button pressed evet according to command received
        /// </summary>
        private void controller_DataReceived(TVRemoteReceiver sender, int command, int address)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                if (command == buttons[i].Command && (deviceAddress == -1 || deviceAddress == address))
                {
                    RawButtonInputReport report = new RawButtonInputReport(source, new TimeSpan(DateTime.Now.Ticks), buttons[i].Button, ButtonAction);
                    // Queue the button press to the input provider site.
                    Dispatcher.BeginInvoke(callback, report);
                    break;
                }
            }
        } 
        private delegate bool ReportInputCallback(InputReport inputReport);
        public readonly Dispatcher Dispatcher;
        private ReportInputCallback callback;
        private InputProviderSite site;
        private PresentationSource source;
        private TVRemoteButtonPad[] buttons;
        private int deviceAddress;
    }
}
