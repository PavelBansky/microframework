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
using System.Threading;

namespace Bansky.SPOT.IrDA
{
    /// <summary>
    /// TV Remote receiver class for Sony devices
    /// </summary>
    public class SonyReceiver : TVRemoteReceiver
    {
        /// <summary>
        /// Creates new Sony TV Remote Receiver instance
        /// </summary>
        /// <param name="receiverPin">Infrared demodulator pin</param>
        public SonyReceiver(Cpu.Pin receiverPin)
        {
            this.ReceiverPin = receiverPin;
            _irdaPort = new InputPort(ReceiverPin, false, Port.ResistorMode.PullUp);
            _pulses = new long[DATA_LENGTH];

            // Start reading thread
            _runningThread = true;
            _mainThread = new Thread(new ThreadStart(DoWork));
            _mainThread.Start();
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public override void Dispose()
        {
            _runningThread = false;
            _mainThread.Join(1000);

            _irdaPort.Dispose();
        }

        /// <summary>
        /// Main working thread
        /// </summary>
        private void DoWork()
        {
            int address, command;
            while (_runningThread)
            {
                Receive(out command, out address);                
                OnDataReceived(command, address);

                // Little pause to set things down
                Thread.Sleep(130);
            }
        }

        /// <summary>
        /// Blocking method receiving data from IrDA
        /// </summary>
        /// <param name="command">Command received</param>
        /// <param name="address">Address received</param>
        private void Receive(out int command, out int address)
        {
            address = 0;
            command = 0;

            // wait for start sequence
            while (PulseIn(_irdaPort, false) < START_SEQUENCE) { }

            // receive data
            for (int i = 0; i < DATA_LENGTH; i++)
            {
                _pulses[i] = PulseIn(_irdaPort, false);
            }

            // Decode pulses
            DecodePulses(_pulses, out command, out address);
        }

        /// <summary>
        /// Decodes received pulses using SONY protocl
        /// </summary>
        /// <param name="pulses">Array with pulses</param>
        /// <param name="command">Command decoded</param>
        /// <param name="address">Address decoded</param>
        private void DecodePulses(long[] pulses, out int command, out int address)
        {
            command = 0;
            address = 0;
            byte mask = 0;

            // Decode command
            for (int i = 0; i < 7; i++)
            {
                mask = (pulses[i] > LOGICAL_ONE) ? (byte)1 : (byte)0;
                mask <<= i;

                command |= mask;
            }

            // Decode address
            for (int i = 7; i < DATA_LENGTH; i++)
            {
                mask = (pulses[i] > LOGICAL_ONE) ? (byte)1 : (byte)0;
                mask <<= (i - 7);

                address |= mask;
            }
        }

        /// <summary>
        /// Evenet thrower
        /// </summary>
        protected override void OnDataReceived(int command, int address)
        {
            base.OnDataReceived(command, address);
        }

        /// <summary>
        /// Blocking method for input pulse measuring
        /// </summary>
        /// <param name="port">Input port</param>
        /// <param name="state">State to measure length</param>
        /// <returns>Pulse length in ticks</returns>
        private long PulseIn(InputPort port, bool state)
        {
            DateTime startTime;
            TimeSpan delta;

            while (port.Read() != state) { }
            startTime = DateTime.Now;
            while (port.Read() == state) { }
            delta = DateTime.Now - startTime;

            return delta.Ticks;
        }  

        private InputPort _irdaPort;
        private long[] _pulses;
        private Thread _mainThread;
        private bool _runningThread;        

        const int DATA_LENGTH = 12;
        const int START_SEQUENCE = 20000;
        const int LOGICAL_ONE = 12000;

        /// <summary>
        /// Sony device addresses
        /// </summary>
        public enum Devices
        {
            All = -1,
            TV = 1,
            VCR1 = 2,
            LaserDisc = 6,
            VCR2=7,
            HD=8,
            VCR3=9,
            Camera = 11,            
            Cassete = 14,            
            CD = 17,        
            DAT=18,
        }
    }
}
