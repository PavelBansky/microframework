//------------------------------------------------------------------------------
// I2CSlave.cs
//
// Implements I2C functionality for the Devantech hardware
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
// http://creativecommons.org/licenses/by-nc-sa/2.5/
//
//------------------------------------------------------------------------------

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Devantech.Hardware
{
    /// <summary>
    /// Implements I2C functionality for the Devantech hardware
    /// </summary>
    public class I2CSlave : IDisposable
    {
        /// <summary>
        /// KHz speed of the I2C bus
        /// </summary>
        public const int I2C_SPEED = 100;

        /// <summary>
        /// Address change command 1 for Devantech devices
        /// </summary>
        public const byte ADDRESS_CHANGE_1 = 0xA0;
        /// <summary>
        ///  Address change command 2 for Devantech devices
        /// </summary>
        public const byte ADDRESS_CHANGE_2 = 0xAA;
        /// <summary>
        ///  Address change command 3 for Devantech devices
        /// </summary>
        public const byte ADDRESS_CHANGE_3 = 0xA5;

        /// <summary>
        /// I2C operations timeout
        /// </summary>
        const int I2C_TIMEOUT = 5000;

        #region Constructors

        /// <summary>
        /// Creates instance of the I2C slave
        /// </summary>
        /// <param name="deviceAddress">7bit device address</param>
        /// <param name="busSpeed">Speed of the I2C bus</param>
        public I2CSlave(byte deviceAddress, int busSpeed)
        {
            this._slaveDevice = new I2CDevice(new I2CDevice.Configuration(deviceAddress, busSpeed));
        }

        /// <summary>
        /// Creates instance of the I2C slave
        /// </summary>
        /// <param name="deviceAddress">7bit device address</param>
        public I2CSlave(byte deviceAddress) 
            : this(deviceAddress, I2C_SPEED)
        {

        }

        #endregion

        #region Read Operations

        /// <summary>
        /// Generic read operation from I2C slave
        /// </summary>
        /// <param name="readBuffer">Buffer for output</param>
        public void Read(byte[] readBuffer)
        {
            I2CDevice.I2CTransaction[] xact = new I2CDevice.I2CTransaction[] {
                _slaveDevice.CreateReadTransaction(readBuffer)
            };

            lock (_slaveDevice)
            {
                int bytesCount = _slaveDevice.Execute(xact, I2CSlave.I2C_TIMEOUT);

                if (bytesCount < readBuffer.Length)
                    throw new System.IO.IOException(Resources.StringResources.ErrorI2CCommunication.ToString());
            }
        }

        /// <summary>
        /// Reads register from I2C slave
        /// </summary>
        /// <param name="register">Register address</param>
        /// <param name="readBuffer">Buffer for output</param>
        public void ReadRegister(byte register, byte[] readBuffer)
        {
            _registerBuffer[0] = register;
            Write(_registerBuffer);
            Read(readBuffer);
        }

        #endregion

        #region Write Operations
        
        /// <summary>
        /// Generic write operation from I2C slave
        /// </summary>
        /// <param name="writeBuffer">Buffer for input</param>
        public void Write(byte[] writeBuffer)
        {
            I2CDevice.I2CTransaction[] xact = new I2CDevice.I2CTransaction[] {
                _slaveDevice.CreateWriteTransaction(writeBuffer)
            };

            lock (_slaveDevice)
            {
                int bytesCount = _slaveDevice.Execute(xact, I2CSlave.I2C_TIMEOUT);

                if (bytesCount < writeBuffer.Length)
                    throw new System.IO.IOException(Resources.StringResources.ErrorI2CCommunication.ToString());
            }
        }

        /// <summary>
        /// Writes data into register
        /// </summary>
        /// <param name="register">Register address</param>
        /// <param name="value">Data to write</param>
        public void WriteRegister(byte register, byte value)
        {
            _writeBuffer[0] = register;
            _writeBuffer[1] = value;
            Write(_writeBuffer);
        }

        /// <summary>
        /// Writes data into register
        /// </summary>
        /// <param name="register">Register address</param>
        /// <param name="writeBuffer">Buffer for input</param>
        public void WriteRegister(byte register, byte[] writeBuffer)
        {
            byte[] data = new byte[writeBuffer.Length + 1];
            Array.Copy(writeBuffer, 0, data, 1, writeBuffer.Length);

            // Set first byte as the register address
            data[0] = register;
            Write(data);
        }

        #endregion

        /// <summary>
        /// Chnange I2C address of the slave.
        /// Note: Suitable for most Devantech devices
        /// </summary>
        /// <param name="commandRegister">Command register of the device</param>
        /// <param name="newAddress">New 7bit address</param>        
        public void ChangeI2CAddress(byte commandRegister, byte newAddress)
        {
            byte[] changeCommand = new byte[2] { commandRegister, ADDRESS_CHANGE_1 };
            Write(changeCommand);
            changeCommand[1] = ADDRESS_CHANGE_2;
            Write(changeCommand);
            changeCommand[1] = ADDRESS_CHANGE_3;
            Write(changeCommand);

            // Devantech hardware needs new address in 8-bit. So we need to shift.
            changeCommand[1] = (byte)(newAddress << 1);
            Write(changeCommand);            
        }

        #region IDisposable Members

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            _slaveDevice.Dispose();   
        }

        #endregion

        private I2CDevice _slaveDevice;
        private byte[] _registerBuffer = new byte[1] { 0x00 };
        private byte[] _writeBuffer = new byte[2] { 0x00, 0x00 };
    }
}
