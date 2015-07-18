//------------------------------------------------------------------------------
// SD20ServoDriver.cs
//
// Implements funcationality of the Devantech SD20 Servo Driver
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
// http://creativecommons.org/licenses/by-nc-sa/2.5/
//
//------------------------------------------------------------------------------

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Devantech.Hardware;

namespace Devantech.Hardware.SD20
{
    /// <summary>
    /// Class implements funcationality of the Devantech SD20 Servo Driver
    /// </summary>
    /// <example>
    /// <code>
    /// using (SD20ServoDriver SD20 = new SD20ServoDriver())
    /// {
    ///     // Set servo to operate from 0 to 90 degrees
    ///     SD20.SetStandardMode();
    /// 
    ///     // Set servo to middle position
    ///     SD20.SetServo(1, 127);
    /// 
    ///     // Set servo to operate from 0 to 180 degrees (HS 311)
    ///     SD20.SetExtendedMode(530, 1920);
    /// 
    ///     // Set servo to the highest position
    ///     SD20.SetServo(1, 250);
    /// }     
    /// </code>
    /// </example>
    public class SD20ServoDriver : IDisposable
    {
        #region Constants
        
        const byte DEFAULT_ADDRESS = 0x61;
        const byte REG_EXTENDED_MODE_CTRL   = 21;
        const byte REG_EXTENDED_MODE_HIGH   = 22;
        const byte REG_EXTENDED_MODE_LOW    = 23;
        
        const byte REG_REVISION = 0;

        #endregion

        #region Constructors

        /// <summary>
        /// SD20 Servo Driver on default address 0xC2 (0x61 7bit)
        /// </summary>
        public SD20ServoDriver() : this(SD20ServoDriver.DEFAULT_ADDRESS)
        {
        }

        /// <summary>
        /// SD20 Servo Driver
        /// </summary>
        /// <param name="deviceAddress">7bit address of the SD20</param>
        public SD20ServoDriver(byte deviceAddress)
        {
            this._slave = new I2CSlave(deviceAddress);
        }

        #endregion

        #region SD20ServoDriver Members

        /// <summary>
        /// Sets servo into position
        /// </summary>
        /// <param name="servoIndex">Servo</param>
        /// <param name="servoPosition">Position</param>
        public void SetServo(byte servoIndex, byte servoPosition)
        {
            // Input validation
            if (servoIndex < 1 && servoIndex > 20)
                throw new ArgumentOutOfRangeException();

            _slave.WriteRegister(servoIndex, servoPosition);            
        }

        /// <summary>
        /// Gets current position of the servo
        /// </summary>
        /// <param name="servoIndex">Servo</param>
        /// <returns>Servo position</returns>
        public byte GetServo(byte servoIndex)
        {
            // Input validation
            if (servoIndex < 1 && servoIndex > 20)
                throw new ArgumentOutOfRangeException();            

            _slave.ReadRegister(servoIndex, _dataBuffer);

            return _dataBuffer[0];
        }

        /// <summary>
        /// Sets extended mode of SD20. Servos can operate in wider range.
        /// <example>
        ///     SetExtendedMode(550, 1800);
        /// Servo impulses will be from 0.55ms to 2.35ms
        /// </example>
        /// </summary>
        /// <param name="offset">Offset in microseconds (short impulse)</param>
        /// <param name="range">Width of the range in microseconds</param>
        public void SetExtendedMode(int offset, int range)
        {            
            SetMode(offset - 20, (byte)(65280 / range));
        }        

        /// <summary>
        /// Set SD20 to standard mode. Servos operates from 0 - 90 degrees
        /// </summary>
        public void SetStandardMode()
        {
            SetMode(980, 0);
        }        

        /// <summary>
        /// Writes SD20 mode into proper registry
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="variation"></param>
        private void SetMode(int offset, byte variation)
        {            
            Endianity.GetBytes(offset, _dataBuffer, ByteOrder.BigEndian);

            _slave.WriteRegister(SD20ServoDriver.REG_EXTENDED_MODE_CTRL, variation);
            _slave.WriteRegister(SD20ServoDriver.REG_EXTENDED_MODE_HIGH, _dataBuffer[0]);
            _slave.WriteRegister(SD20ServoDriver.REG_EXTENDED_MODE_LOW, _dataBuffer[1]);
        }

        /// <summary>
        /// Returns 'true' if SD20 operates in extended mode
        /// </summary>
        public bool ExtendedMode
        {
            get
            {
                _slave.ReadRegister(SD20ServoDriver.REG_EXTENDED_MODE_CTRL, _dataBuffer);
                
                return _dataBuffer[0] != 0;
            }
        }

        /// <summary>
        /// Returns firmware revision of the device
        /// </summary>
        public byte Revision
        {
            get
            {
                _slave.ReadRegister(SD20ServoDriver.REG_REVISION, _dataBuffer);

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

        private I2CSlave _slave;
        private byte[] _dataBuffer = new byte[2] { 0x00, 0x00 };
    }
}
