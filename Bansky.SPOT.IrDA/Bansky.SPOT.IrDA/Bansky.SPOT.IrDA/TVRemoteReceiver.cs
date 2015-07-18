//------------------------------------------------------------------------------
// Bansky.SPOT.IrDA
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

namespace Bansky.SPOT.IrDA
{
    /// <summary>
    /// Abstract class representing IR remote receiver
    /// </summary>
    abstract public class TVRemoteReceiver : IDisposable
    {
        /// <summary>
        /// Get or set pin used for IR demodulator
        /// </summary>
        public Cpu.Pin ReceiverPin;

        public abstract void Dispose();

        /// <summary>
        /// Data received delegate
        /// </summary>
        /// <param name="sender">Instance where event occured</param>
        /// <param name="command">Command received</param>
        /// <param name="address">Address received</param>
        public delegate void TVRemoteDataHandler(TVRemoteReceiver sender, int command, int address);
        
        /// <summary>
        /// Event occures when data is received
        /// </summary>
        public event TVRemoteDataHandler DataReceived;

        protected virtual void OnDataReceived(int command, int address)
        {
            TVRemoteDataHandler handler = DataReceived;
            if (handler != null)
                handler(this, command, address);
        }  
    }
}
