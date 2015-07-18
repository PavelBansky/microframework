//------------------------------------------------------------------------------
// Bansky.SPOT.LCD
//
// Bansky.SPOT.LCD provides API and helper function for LCD displays,
// to be used in .NET Micro Framework application. Goal of this library is
// to provide display output capabilites for headless Micro Framework CPUs.
//
// http://bansky.net/blog
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution 3.0 Unported" license.
// http://creativecommons.org/licenses/by/3.0/
//
//------------------------------------------------------------------------------
using System;

namespace Bansky.SPOT.LCD
{
    /// <summary>
    /// Interface to transfer providers used for sending data to LCD display.
    /// </summary>
    public interface ITransferProvider
    {
        /// <summary>
        /// Sends out one byte
        /// </summary>
        /// <param name="data">Byte to be send.</param>
        void SendByte(byte data);
    }
}
