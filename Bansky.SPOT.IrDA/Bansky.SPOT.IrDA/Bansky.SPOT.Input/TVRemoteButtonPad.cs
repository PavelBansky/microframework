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
using Microsoft.SPOT.Hardware;

namespace Bansky.SPOT.Input
{
    /// <summary>
    /// Class represents TV Remote command to WPF button binding
    /// </summary>
    public class TVRemoteButtonPad
    {
        public TVRemoteButtonPad(int comand, Button button)
        {
            this.Command = comand;
            this.Button = button;
        }

        /// <summary>
        /// TV Remote controller command
        /// </summary>
        public int Command;
        /// <summary>
        /// WPF Button
        /// </summary>
        public Button Button;
    }
}
