//------------------------------------------------------------------------------
// Endianity.cs
//
// Implements basic endianity operations
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
// http://creativecommons.org/licenses/by-nc-sa/2.5/
//
//------------------------------------------------------------------------------

using System;

namespace Devantech.Hardware
{
    /// <summary>
    /// Implements basic endianity operations
    /// </summary>
    public static class Endianity
    {
        /// <summary>
        /// Gets value from the byte array
        /// </summary>
        /// <param name="byteArray">Byte array</param>
        /// <param name="byteOrder">Byte order</param>
        /// <returns>Long value</returns>
        public static long GetValue(byte[] byteArray, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.BigEndian)
                return FromBigEndian(byteArray, 0, byteArray.Length);
            else
                throw new NotImplementedException(Resources.StringResources.NotImplementedLittleEndian.ToString());
        }

        /// <summary>
        /// Get value from the byte array
        /// </summary>
        /// <param name="byteArray">Byte array</param>
        /// <param name="startIndex">Start index in array</param>
        /// <param name="length">Number of bytes to parse</param>
        /// <param name="byteOrder">Byte order</param>
        /// <returns>Long value</returns>
        public static long GetValue(byte[] byteArray, int startIndex, int length, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.BigEndian)
                return FromBigEndian(byteArray, startIndex, length);
            else
                throw new NotImplementedException(Resources.StringResources.NotImplementedLittleEndian.ToString());
        }

        /// <summary>
        /// Splits number into the byte array
        /// </summary>
        /// <param name="number">Value to split</param>
        /// <param name="outputArray">Array where the bytes be stored</param>
        /// <param name="byteOrder">Byte order of the array</param>
        public static void GetBytes(long number, byte[] outputArray, ByteOrder byteOrder)
        {
            if (byteOrder == ByteOrder.BigEndian)
                ToBigEndian(number, outputArray);
            else
                throw new NotImplementedException(Resources.StringResources.NotImplementedLittleEndian.ToString());
        }

        /// <summary>
        /// Splits number into the byte array in Big Endian
        /// </summary>
        /// <param name="number">Number to split</param>
        /// <param name="outputArray">Array where the bytes be stored</param>
        public static void ToBigEndian(long number, byte[] outputArray)
        {
            int length = outputArray.Length;

            outputArray[length-1] = (byte)number;
            for (int i = length-2; i >= 0; i--)
                outputArray[i] = (byte)(number >> (8 * (i+1)));            
        }

        /// <summary>
        /// Gets value from array byte organized as Big Endian
        /// </summary>
        /// <param name="byteArray">Byte array</param>
        /// <param name="startIndex">Start index</param>
        /// <param name="length">Number of bytes to parse</param>
        /// <returns>Long value</returns>
        private static long FromBigEndian(byte[] byteArray, int startIndex, int length)
        {
            long retValue = 0;
            int stopIndex = startIndex+length-1;
            for (int i = startIndex; i < (stopIndex); i++)
            {
                retValue |= byteArray[i];
                retValue = retValue << 8;
            }
            return retValue | byteArray[stopIndex];
        }
    }
}
