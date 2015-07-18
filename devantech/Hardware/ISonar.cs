//------------------------------------------------------------------------------
// ISonar.cs
//
// Interface for Devantech range finders hardware
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
    /// Interface for Devantech range finders hardware
    /// </summary>
    public interface ISonar : IDisposable
    {
        /// <summary>
        /// Transmits ultrasonic burst
        /// </summary>
        void Burst();

        /// <summary>
        /// Returns range in specified units
        /// </summary>
        /// <param name="rangeUnits">Ranging units</param>
        /// <returns>Distance range</returns>
        int GetRange(SonarRangeUnits rangeUnits);
    }
}
