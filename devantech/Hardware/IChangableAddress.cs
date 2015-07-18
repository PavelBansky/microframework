//------------------------------------------------------------------------------
// IChangableAddress.cs
//
// Interface for hardware that allows changing of the address
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
    /// Interface for hardware that allows changing of the address
    /// </summary>
    public interface IChangableAddress
    {
        /// <summary>
        /// Changes address of the device
        /// </summary>
        /// <param name="newAddress">New address</param>
        void ChangeI2CAddress(byte newAddress);
    }
}
