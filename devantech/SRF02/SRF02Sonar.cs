//------------------------------------------------------------------------------
// SRF02Sonar.cs
//
// Implements functionality of the Devantech SRF02 Ultrasonic Range Finder
// !!This driver supports I2C mode communication!!
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
// http://creativecommons.org/licenses/by-nc-sa/2.5/
//
//------------------------------------------------------------------------------

using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Devantech.Hardware;

namespace Devantech.Hardware.SRF02
{
    /// <summary>
    /// Class implements functionality of the Devantech SRF02 Ultrasonic Range Finder
    /// </summary>
    /// <example>
    /// <code>
    /// using(SRF02Sonar sonar = new SRF02Sonar(0x70))
    /// {
    ///     // Range distance in centimeters
    ///     int distance = GetRange(SonarRangeUnits.Centimeters);
    /// }
    /// </code>
    /// </example>
    public class SRF02Sonar : ISonar, IChangableAddress
    {
        #region Constants

        const byte DEFAULT_ADDRESS = 0x70;

        const byte RANGE_IN_INCHES          = 0x50;
        const byte RANGE_IN_CENTIMETERS     = 0x51;        
        const byte RANGE_IN_MICROSECONDS    = 0x52;

        const byte FAKE_RANGE_IN_CENTIMETERS     = 0x56;
        const byte FAKE_RANGE_IN_INCHES          = 0x57;
        const byte FAKE_RANGE_IN_MICROSECONDS    = 0x58;

        const byte BURST     = 0x5C;
        const byte AUTOTUNE  = 0x60;

        const byte REG_COMMAND_REVISION = 0x00;

        #endregion

        #region Constructors

        /// <summary>
        /// SRF02 Range Finder on default address 0xE0 (0x70 7bit)
        /// </summary>
        public SRF02Sonar() : this(SRF02Sonar.DEFAULT_ADDRESS)
        {            
        }

        /// <summary>
        /// SRF02 Range Finder
        /// </summary>
        /// <param name="deviceAddress">7bit address of the sonar</param>
        public SRF02Sonar(byte deviceAddress)
        {
            this._slave = new I2CSlave(deviceAddress);
        }

        #endregion

        #region ISonar Members

        /// <summary>
        /// Transmits ultrasonic burst
        /// </summary>
        public void Burst()
        {            
            _slave.WriteRegister(0, SRF02Sonar.BURST);
        }

        /// <summary>
        /// Returns range in specified units
        /// </summary>
        /// <param name="rangeUnits">Ranging units</param>
        /// <returns>Distance range</returns>
        public int GetRange(SonarRangeUnits rangeUnits)
        {
            byte command;

            // Set command by range units
            switch (rangeUnits)
            {
                case SonarRangeUnits.Centimeters:
                    command = SRF02Sonar.RANGE_IN_CENTIMETERS;
                    break;
                case SonarRangeUnits.Inches:
                    command = SRF02Sonar.RANGE_IN_INCHES;
                    break;
                default:
                    command = SRF02Sonar.RANGE_IN_MICROSECONDS;
                    break;
            }

            // Write command
            _slave.WriteRegister(0, command);

            // Wait for a while
            Thread.Sleep(40);

            // Read range
            _slave.ReadRegister(2, _dataBuffer);            

            this._autotuneMinimum = (int)Endianity.GetValue(_dataBuffer, 2, 2, ByteOrder.BigEndian);
            return (int)Endianity.GetValue(_dataBuffer, 0, 2, ByteOrder.BigEndian);
        }

        #endregion

        #region IChangableAddress Members
        
        /// <summary>
        /// Changes address of the I2C device
        /// </summary>
        /// <param name="newAddress">New 7bit address</param>
        public void ChangeI2CAddress(byte newAddress)
        {
            _slave.ChangeI2CAddress(SRF02Sonar.REG_COMMAND_REVISION, newAddress);

            _slave.Dispose();

            // Wait a few miliseconds to make things "sattle", otherwise it will fail
            Thread.Sleep(50);
            
            // Create _slave with new address
            _slave = new I2CSlave(newAddress);
        }

        #endregion

        #region SRF02 Members

        /// <summary>
        /// Force autotune
        /// </summary>
        public void Autotune()
        {
            _slave.WriteRegister(SRF02Sonar.REG_COMMAND_REVISION, SRF02Sonar.AUTOTUNE);
        }

        /// <summary>
        /// Minimal meassurable range.
        /// Value is updated after each ranging
        /// </summary>
        public int AutotuneMinimum
        {
            get { return this._autotuneMinimum; }
        }

        /// <summary>
        /// Returns firmware revision of the device
        /// </summary>
        public byte Revision
        {
            get
            {
                _slave.ReadRegister(SRF02Sonar.REG_COMMAND_REVISION, _dataBuffer);
                return _dataBuffer[0];
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose object
        /// </summary>        
        public void Dispose()
        {
            _slave.Dispose();
        }

        #endregion

        private int _autotuneMinimum = 15;        
        private I2CSlave _slave;
        private byte[] _dataBuffer = new byte[4] { 0x00, 0x00, 0x00, 0x00 };
    }
}
