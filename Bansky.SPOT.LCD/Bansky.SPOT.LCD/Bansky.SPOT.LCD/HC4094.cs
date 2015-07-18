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
using Microsoft.SPOT.Hardware;

namespace Bansky.SPOT.LCD
{
    /// <summary>
    /// Shift register API
    /// </summary>
    public class HC4094 : ITransferProvider, IDisposable
    {
        /// <summary>
        /// Creates new instance of shift register.
        /// </summary>
        /// <param name="dataPin">Pin used for data.</param>
        /// <param name="clockPin">Pin used for clock.</param>
        /// <param name="strobePin">Pin used for strobe/clear/RCLK</param>
        /// <param name="bigEndian">Most significant bit goes first</param>
        public HC4094(Cpu.Pin dataPin, Cpu.Pin clockPin, Cpu.Pin strobePin, bool bigEndian)
        {
            _strobePort = new OutputPort(strobePin, false);
            _dataPort = new OutputPort(dataPin, false);
            _clockPort = new OutputPort(clockPin, false);
            _bigEndian = bigEndian;
        }

        /// <summary>
        /// Dispose the ShiftRegister
        /// </summary>
        public void Dispose()
        {
            _strobePort.Dispose();
            _dataPort.Dispose();
            _clockPort.Dispose();
        }

        /// <summary>
        /// Send out one byte to shift register
        /// </summary>
        /// <param name="data">Byte to send</param>
        public void SendByte(byte data)
        {
            ShiftOut(data);
            _strobePort.Write(true);
            _strobePort.Write(false);
        }

        /// <summary>
        /// Send out one byte in serial transfer.       
        /// </summary>
        /// <param name="data">Byte to send</param>
        private void ShiftOut(byte data)
        {
            int output, mask, i, incr;
            if (_bigEndian)
            {
                i = 7; incr = -1;
            }
            else
            {
                i = 0; incr = +1;
            }

            for(int step = 0; step < 8; step++)
            {
                mask = (int)System.Math.Pow(2, i);
                output = data & mask;
                output >>= i;
                i += incr;

                _dataPort.Write((output != 0));
                _clockPort.Write(true);
                _clockPort.Write(false);                  
            }
        }

        private OutputPort _strobePort;
        private OutputPort _dataPort;
        private OutputPort _clockPort;
        private bool _bigEndian;
    }
}
