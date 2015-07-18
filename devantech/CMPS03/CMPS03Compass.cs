//------------------------------------------------------------------------------
// CMPS03Compass.cs
//
// Implements functionality of the Devantech CMPS03 Compass module
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
using System.Threading;

namespace Devantech.Hardware.CMPS03
{
    /// <summary>
    /// Implements functionality of the Devantech CMPS03 Compass module
    /// </summary>
    /// <example>
    /// <code>
    /// using (CMPS03Compass compass = new CMPS03Compass())
    /// {
    ///     // Gets actual azimuth
    ///     float azimuth = compass.GetAzimuth();            
    /// }
    /// </code>
    /// </example>
    public class CMPS03Compass : IDisposable, IChangableAddress
    {
        #region Constants

        const byte DEFAULT_ADDRESS = 0x60;

        const byte REG_REVISION = 0;
        const byte REG_BEARING = 1;
        const byte REG_AZIMUTH_HIGH = 2;
        const byte REG_UNLOCK_1 = 12;
        const byte REG_COMMAND = 15;

        const byte FACTORY_RESET_1 = 0x55;
        const byte FACTORY_RESET_2 = 0x5A;
        const byte FACTORY_RESET_3 = 0xA5;
        const byte FACTORY_RESET_4 = 0xF2;

        #endregion

        #region Constructors

        /// <summary>
        /// CMPS03 Compass Module on default address 0xC0 (0x60 7bit)
        /// </summary>
        public CMPS03Compass() : this(CMPS03Compass.DEFAULT_ADDRESS)
        {
            
        }

        /// <summary>
        /// CMPS03 Compass Module
        /// </summary>
        /// <param name="deviceAddress">7bit address of the sonar</param>
        public CMPS03Compass(byte deviceAddress)
        {
            this._slave = new I2CSlave(deviceAddress);
        }

        #endregion

        #region CMPS03Compass

        /// <summary>
        /// Returns actual azimuth
        /// </summary>
        /// <returns>Azimuth</returns>
        public float GetAzimuth()
        {
            _slave.ReadRegister(CMPS03Compass.REG_AZIMUTH_HIGH, _dataBuffer);

            return Endianity.GetValue(_dataBuffer, ByteOrder.BigEndian) / 10f;
        }

        /// <summary>
        /// Returns actual azimuth as the bearing of the byte
        /// </summary>
        /// <returns>Azimuth byte bearing</returns>
        public byte GetBearing()
        {
            _slave.ReadRegister(CMPS03Compass.REG_BEARING, _dataBuffer);

            return _dataBuffer[0];
        }

        /// <summary>
        /// Returns firmware revision of the device
        /// </summary>
        public byte Revision
        {
            get
            {
                _slave.ReadRegister(CMPS03Compass.REG_REVISION, _dataBuffer);

                return _dataBuffer[0];
            }
        }

        /// <summary>
        /// Restores factory calibration.
        /// Works only for revision 14 and above
        /// </summary>
        public void FactoryCalibration()
        {
            byte[] data = new byte[4] {
                CMPS03Compass.FACTORY_RESET_1,
                CMPS03Compass.FACTORY_RESET_2,
                CMPS03Compass.FACTORY_RESET_3,
                CMPS03Compass.FACTORY_RESET_4,
            };

            _slave.WriteRegister(CMPS03Compass.REG_UNLOCK_1, data);
        }

        #endregion

        #region IChangableAddress Members

        /// <summary>
        /// Changes address of the I2C device.
        /// Works only for revision 14 and above
        /// </summary>
        /// <param name="newAddress">New 7bit address</param>
        public void ChangeI2CAddress(byte newAddress)
        {
            byte[] data = new byte[4] {
                I2CSlave.ADDRESS_CHANGE_1,
                I2CSlave.ADDRESS_CHANGE_2,
                I2CSlave.ADDRESS_CHANGE_3,
                (byte)(newAddress << 1),
            };

            _slave.WriteRegister(CMPS03Compass.REG_UNLOCK_1, data);
            _slave.Dispose();

            // Wait a few miliseconds to make things "sattle", otherwise it will fail
            Thread.Sleep(50);

            // Create _slave with new address
            _slave = new I2CSlave(newAddress);

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
