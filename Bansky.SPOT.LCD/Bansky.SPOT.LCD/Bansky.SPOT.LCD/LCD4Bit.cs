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
using System.Threading;

namespace Bansky.SPOT.LCD
{
    /// <summary>
    /// API for alfanumeric LCD display, controlled by HD44780 parallel interface in 4-bit mode.
    /// </summary>
    public class LCD4Bit
    {
        /// <summary>
        /// Creates new intstance of HD44780 LCD display in 4-bit mode.
        /// </summary>
        /// <param name="provider">Data transfer provider.</param>
        public LCD4Bit(ITransferProvider provider)
        {
            this.Provider = provider;

            this.Encoding = System.Text.Encoding.UTF8;
            this._backLight = false;

            Init(true);
        }

        /// <summary>
        /// Initializes display for communication.
        /// Sets display to 1 line mode with 5x8 font.
        /// </summary>
        /// <param name="leftToRight">true if entry mode is from left to right.</param>
        public void Init(bool leftToRight)
        {
            Thread.Sleep(100);      // LCD controller needs some warm-up time

            #region 4-bit initialization sequence
            SendLcdCommand(0x03);
            Thread.Sleep(64);
            SendLcdCommand(0x03);
            Thread.Sleep(50);
            SendLcdCommand(0x03);
            Thread.Sleep(50);
            SendLcdCommand(0x02);
            Thread.Sleep(50);
            SendLcdCommand(0x2C);   // Function set: 4-bit, 1 display line, 5x7 font
            Thread.Sleep(20);
            #endregion

            // Set entry mode
            SendLcdCommand((byte)(0x04 | ((leftToRight) ? 0x02 : 0x00)));
        }

        /// <summary>
        /// Clears display and sets cursor position to zero.
        /// </summary>
        public void Clear()
        {
            SendLcdCommand(0x01);
        }

        /// <summary>
        /// Sets the display properties
        /// </summary>
        /// <param name="visible">Is display on.</param>
        /// <param name="backLight">Is backlight on.</param>
        /// <param name="showCursor">Is cursor visible.</param>
        /// <param name="blinkCursor">Is cursor blinking.</param>
        public void Display(bool visible, bool backLight, bool showCursor, bool blinkCursor)
        {
            this._backLight = backLight;

            int command = 0x08;
            command |= (visible) ? 0x04 : 0x00;
            command |= (showCursor) ? 0x02 : 0x00;
            command |= (blinkCursor) ? 0x01 : 0x00;
            SendLcdCommand((byte)command);
        }

        /// <summary>
        /// Sets cursor position to zero and shifts display to original position.
        /// </summary>
        public void Home()
        {
            SendLcdCommand(0x02);
        }

        /// <summary>
        /// Sets cursor position to given address.
        /// </summary>
        /// <param name="address">Display data RAM address.</param>
        public void SetPosition(int address)
        {
            SendLcdCommand((byte)(address | 0x80));
        }

        /// <summary>
        /// Moves cursor left or right.
        /// </summary>
        /// <param name="right">true to move cursor right.</param>
        public void MoveCursor(bool right)
        {
            SendLcdCommand((byte)(0x10 | ((right) ? 0x04 : 0x00)));
        }

        /// <summary>
        /// Shifts whole display area to left or right.
        /// </summary>
        /// <param name="right">true to shift right.</param>
        public void ShiftDisplay(bool right)
        {
            SendLcdCommand((byte)(0x18 | ((right) ? 0x04 : 0x00)));
        }

        /// <summary>
        /// Writes a specified number of bytes to the display using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array that contains data to write to display.</param>
        /// <param name="offset">The zero-based byte offset in the buffer parameter at which to begin copying bytes to display.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            int len = offset + count;
            for (int i = offset; i < len; i++)
            {
                WriteByte(buffer[i]);
            }
        }

        /// <summary>
        /// Writes the specified string to display. 
        /// </summary>
        /// <param name="text">String for output</param>
        public void Write(string text)
        {
            byte[] buffer = this.Encoding.GetBytes(text);
            Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Sends one data byte to the display.
        /// </summary>
        /// <param name="data">The data byte to send.</param>
        public void WriteByte(byte data)
        {
            int output = data;
            output >>= 4;                           // set the first four data-bits
            output |= (_backLight) ? 0x80 : 0;      // set MSB to 1 when back light is On
            output |= 0x40;                         // set R/S HIGH
            output &= 0xDF;                         // set RW LOW
            output &= 0xEF;                         // set Enable LOW
            Provider.SendByte((byte)output);
            output |= 0x10;                         // set Enable HIGH
            Provider.SendByte((byte)output);
            output &= 0xEF;                         // set Enable LOW
            Provider.SendByte((byte)output);

            Thread.Sleep(1);

            data &= 0x0F;                           // set HByte to zero 
            data |= (byte)(output & 0xF0);          // keep the control bits
            Provider.SendByte(data);
            data |= 0x10;                           // set Enable HIGH
            Provider.SendByte(data);
            data &= 0xEF;                           // set Enable LOW
            Provider.SendByte(data);
        }

        /// <summary>
        /// Sends HD44780 lcd interface command.
        /// </summary>
        /// <param name="data">The byte command to send.</param>
        public void SendLcdCommand(byte data)
        {
            int output = data;
            output >>= 4;                           // set the first four data-bits
            output |= (_backLight) ? 0x80 : 0;      // set MSB to 1 when back light is On
            output &= 0xEF;                         // set Enable LOW
            Provider.SendByte((byte)output);
            output |= 0x10;                         // set Enable HIGH
            Provider.SendByte((byte)output);
            output &= 0xEF;                         // set Enable LOW
            Provider.SendByte((byte)output);

            Thread.Sleep(1);

            data &= 0x0F;                           // set four highest bits to zero 
            data |= (byte)(output & 0xF0);          // keep the back light settings
            data &= 0xEF;                           // set Enable LOW
            Provider.SendByte(data);
            data |= 0x10;                           // set Enable HIGH
            Provider.SendByte(data);
            data &= 0xEF;                           // set Enable LOW
            Provider.SendByte(data);
        }

        /// <summary>
        /// Writes data to specified address in character generator RAM.
        /// This method is used to store custom graphic pattern into HD44780.
        /// </summary>
        /// <param name="address">Start offset in CGRAM.</param>
        /// <param name="buffer">Data to write.</param>
        public void WriteToCGRAM(byte address, byte[] buffer)
        {
            SendLcdCommand((byte)(0x40 | address));
            int len = buffer.Length;
            for (int i = 0; i < len; i++)
            {
                WriteByte(buffer[i]);
            }
        }

        /// <summary>
        /// Gets or sets hardware provider for data transfers to display.
        /// </summary>
        public ITransferProvider Provider;

        /// <summary>
        /// Gets or sets the byte encoding for conversion of text.
        /// </summary>
        public System.Text.Encoding Encoding;

        private bool _backLight;
    }
}
