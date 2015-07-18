//------------------------------------------------------------------------------
// ByteOrder.cs
//
// Enum for byte orders
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
// http://creativecommons.org/licenses/by-nc-sa/2.5/
//
//------------------------------------------------------------------------------

using System;
using Microsoft.SPOT;

namespace Devantech.Hardware
{
    /// <summary>
    /// Byte orders
    /// </summary>
    public enum ByteOrder 
    {
        /// <summary>
        /// Little endian
        /// </summary>
        LittleEndian, 
        /// <summary>
        /// Big endian
        /// </summary>
        BigEndian 
    }
}
