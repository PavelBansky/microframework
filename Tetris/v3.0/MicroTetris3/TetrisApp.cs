//-----------------------------------------------------------------------------
// 
//  Tetris game for .NET Micro Framework
//
//  http://bansky.net/blog
// 
// This code was written by Pavel Bansky. It is released under the terms of 
// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
// http://creativecommons.org/licenses/by-nc-sa/2.5/
//-----------------------------------------------------------------------------

using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using MicroTetris3;
using MicroTetris.GameLogic;
using MicroTetris.Presentation;

namespace MicroTetris
{
    /// <summary>
    /// MicroTetris Application
    /// </summary>
    public class TetrisApp : Application
    {        
        /// <summary>
        /// Game HighScoreTable
        /// </summary>
        public HighScoreTable HighScore;
        private static ExtendedWeakReference highScoreEWD;

        private TetrisApp()
        {
            // Create the object that configures the GPIO pins to buttons.
            GPIOButtonInputProvider inputProvider = new GPIOButtonInputProvider(null);

            // Create ExtendedWeakReference for high score table
            highScoreEWD = ExtendedWeakReference.RecoverOrCreate(
                                                    typeof(TetrisApp), 
                                                    0, 
                                                    ExtendedWeakReference.c_SurvivePowerdown);
            // Set persistance priority
            highScoreEWD.Priority = (int)ExtendedWeakReference.PriorityLevel.Important;

            // Try to recover previously saved HighScore
            HighScore = (HighScoreTable)highScoreEWD.Target;
            
            // If nothing was recovered - create new
            if (HighScore == null)
                HighScore = new HighScoreTable();
        }

        /// <summary>
        /// OnStartUp event handler
        /// </summary>        
        protected override void OnStartup(EventArgs e)
        {
            this.MainWindow = new MainMenuWindow(this);
            base.OnStartup(e);
        }

        /// <summary>
        /// Sets focus to MainWindow
        /// </summary>
        public void SetFocus()
        {            
            Buttons.Focus(this.MainWindow);
        }

        /// <summary>
        /// Persists high score to the FLASH memmory
        /// </summary>
        public void PersistHighScore()
        {
            // Persist HighScore by settinig the Target property
            // of ExtendedWeakReference
            highScoreEWD.Target = HighScore;
        }

        public static void Main()
        {
            new TetrisApp().Run();
        }
    }
}
