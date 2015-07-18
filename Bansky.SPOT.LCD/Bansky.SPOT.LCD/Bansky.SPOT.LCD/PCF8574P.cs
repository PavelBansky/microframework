using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Bansky.SPOT.LCD
{
    public class PCF8574P : ITransferProvider
    {
        /// <summary>
        /// Creates new instance of 8bit port expander for I2C
        /// </summary>
        /// <param name="i2cbus">Instance of I2CDevice</param>
        /// <param name="address">Address of the expander</param>
        /// <param name="bigEndian">Most significant bit goes first</param>
        public PCF8574P(I2CDevice i2cbus, ushort address, bool bigEndian)
        {
            this._i2cbus = i2cbus;
            this._config = new I2CDevice.Configuration(address, 100);
            this._bigEndian = bigEndian;
        }

        /// <summary>
        /// Send out one byte to shift register
        /// </summary>
        /// <param name="data">Byte to send</param>
        public void SendByte(byte data)
        {
            if (!_bigEndian)
                data = ReverseBits(data);

            lock (_i2cbus)
            {
                _i2cbus.Config = _config;
                I2CDevice.I2CTransaction[] xact = new I2CDevice.I2CTransaction[]
                {
                    _i2cbus.CreateWriteTransaction(new byte[] { data })
                };

                _i2cbus.Execute(xact, 3000);
            }
        }

        /// <summary>
        /// Reverse bits in byte. MSB becomes LSB
        /// </summary>
        /// <param name="data">Byte to be reversed</param>
        /// <returns>Reversed byte</returns>
        private byte ReverseBits(byte data)
        {
            int output, mask;
            output = 0;
            for (int i = 0; i < 8; i++)
            {
                output <<= 1;
                mask = (int)System.Math.Pow(2, i);
                output |= (data & mask) >> i;
            }

            return (byte)output;
        }

        private I2CDevice _i2cbus;
        private I2CDevice.Configuration _config;
        private bool _bigEndian;
    }
}
