//------------------------------------------------------------------------------
// SRF05Sonar.cs
//
// Implements the funcationality of SRF05 Ultrasonic Range Finder.
//
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
// http://creativecommons.org/licenses/by-nc-sa/2.5/
//
//------------------------------------------------------------------------------

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Devantech;

namespace Devantech.Hardware.SRF05
{
    /// <summary>
    /// Class implements functionality of the Devantech SRF05 Ultrasonic Range Finder
    /// </summary>
    /// <example>
    /// <code>
    /// using(SRF05Sonar sonar = new SRF05Sonar(Cpu.GPIO1, Cpu.GPIO2))
    /// {
    ///     // range distance in centimeters
    ///     int distance = GetRange(SonarRangeUnits.Centimeters);
    /// }
    /// </code>
    /// </example>    
    public class SRF05Sonar : ISonar
    {
        #region Constructors

        /// <summary>
        /// SRF05 Ultrasonic Range Finder in Mode 1
        /// Note: Different pins for trigger and echo signal
        /// </summary>
        /// <param name="triggerPin">Pin for trigger signal</param>
        /// <param name="echoPin">Pin for echo signal</param>     
        public SRF05Sonar(Cpu.Pin triggerPin, Cpu.Pin echoPin)
        {
            this.triggerPin = triggerPin;
            this.echoPin = echoPin;            
        }

        /// <summary>
        /// SRF05 Ultrasonic Range Finder in Mode 2
        /// Note: Common pin for trigger and echo signal
        /// </summary>
        /// <param name="triggerPin">Pin for trigger and echo signal</param>
        public SRF05Sonar(Cpu.Pin triggerPin)
        {
            this.triggerPin = triggerPin;
            this.echoPin = triggerPin;
        }

        #endregion

        #region ISonar Members

        /// <summary>
        /// Transmits ultrasonic burst
        /// </summary>
        public void Burst()
        {
            OutputPort triggerPort = new OutputPort(triggerPin, true);
            triggerPort.Write(false);
            triggerPort.Dispose();
        }

        /// <summary>
        /// Returns range in specified units
        /// </summary>
        /// <param name="rangeUnits">Ranging units</param>
        /// <returns>Distance range</returns>
        public int GetRange(SonarRangeUnits rangeUnits)
        {
            int range = GetRangeInMicroseconds();

            switch (rangeUnits)
            {
                case SonarRangeUnits.Centimeters:
                    return (int)(range * 0.0173);
                case SonarRangeUnits.Inches:
                    return (int)(range * 0.00681);
                default:
                    return range;                    
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {
            echoPort.Dispose();
        }

        #endregion

        #region SRF05 Members
    
        /// <summary>
        /// Get range and returns value in microseconds
        /// </summary>
        /// <returns></returns>
        private int GetRangeInMicroseconds()
        {
            echoLength = 0;

            // Transmit burst
            Burst();

            // Initialize EchoPort and wait for pulse end
            using (echoPort = new InterruptPort(echoPin, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeBoth))
            {
                echoPort.OnInterrupt += new GPIOInterruptEventHandler(echoPort_OnInterrupt);
                while (echoLength <= 0) { }
            }

            // Return range in microseconds
            return (int)(echoLength / 10);
        }

        private void echoPort_OnInterrupt(Cpu.Pin port, bool state, TimeSpan time)
        {
            if (state)            
                echoStart = time;            
            else
            {
                TimeSpan rangeTime = time - echoStart;
                echoLength = rangeTime.Ticks;                
            }
        }
        #endregion

        private Cpu.Pin triggerPin;
        private Cpu.Pin echoPin;
        private InterruptPort echoPort;
        private TimeSpan echoStart;
        private long echoLength;
    }
}
