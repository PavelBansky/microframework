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
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using MicroTetris.GameLogic;

namespace MicroTetris.Presentation
{
    /// <summary>
    /// Windows with the high score table
    /// </summary>
    public class HighScoreWindow : Window
    {
        /// <summary>
        /// Lenght of the name to be edited
        /// </summary>
        const int NAME_LENGTH = 3;

        /// <summary>
        /// Allowed chars in names
        /// </summary>
        private char[] allowedChars = new char[26] { 'A', 'B', 'C', 'D', 
                                                     'E', 'F', 'G', 'H',
                                                     'I', 'J', 'K', 'L', 
                                                     'M', 'N', 'O', 'P', 
                                                     'Q', 'R', 'S', 'T', 
                                                     'U', 'V', 'W', 'X', 
                                                     'Y', 'Z'};

        private int selectedItem, selectedLetter;
        private int[] letterIndexes;
        private bool editMode;

        TetrisApp parentApp;
        ListBox scoreListBox;
        TextFlow hintTextFlow;

        /// <summary>
        /// Crates new HighScoreWindow
        /// </summary>
        /// <param name="parentApp">Parent application</param>
        public HighScoreWindow(TetrisApp parentApp)
        {
            this.parentApp = parentApp;

            editMode = false;
            letterIndexes = new int[NAME_LENGTH];

            InitializeComponents();
        }

        /// <summary>
        /// Start edit mode for given high score item
        /// </summary>
        /// <param name="index">Index in high to edit</param>
        public void EditItem(int index)
        {
            if (index < scoreListBox.Items.Count)
            {
                editMode = true;
                selectedItem = index;
                selectedLetter = 0;
                ScoreItem scoreItem = (ScoreItem)scoreListBox.Items[selectedItem];
                scoreItem.Highlite = true;
                UpdateName();
                UpdateHint();
            }
        }

        /// <summary>
        /// Creates all WPF controls of the window
        /// </summary>
        private void InitializeComponents()
        {
            this.Width = SystemMetrics.ScreenWidth;
            this.Height = SystemMetrics.ScreenHeight;
            this.Background = new SolidColorBrush(Colors.Black);

            #region Caption
            Text caption = new Text(Resources.GetString(Resources.StringResources.HighScore));
            caption.Font = Resources.GetFont(Resources.FontResources.Consolas23);
            caption.ForeColor = Colors.Red;
            caption.SetMargin(0, 10, 0, 15);            
            caption.TextAlignment = TextAlignment.Center;            
            #endregion

            #region Score ListBox
            scoreListBox = new ListBox();
            scoreListBox.Background = this.Background;
            scoreListBox.HorizontalAlignment = HorizontalAlignment.Center;

            foreach (ScoreRecord scoreRecord in parentApp.HighScore.Table)
            {
                ScoreItem scoreItem = new ScoreItem(scoreRecord.Name, scoreRecord.Score);                
                scoreItem.Background = scoreListBox.Background;                
                scoreListBox.Items.Add(scoreItem);
            }
            #endregion

            #region HintLabel
            hintTextFlow = new TextFlow();
            hintTextFlow.SetMargin(0, 15, 0, 0);
            hintTextFlow.TextAlignment = TextAlignment.Center;        
            UpdateHint();
            #endregion

            StackPanel mainStack = new StackPanel(Orientation.Vertical);            
            mainStack.HorizontalAlignment = HorizontalAlignment.Center;
            mainStack.Children.Add(caption);
            mainStack.Children.Add(scoreListBox);
            mainStack.Children.Add(hintTextFlow);

            this.Child = mainStack;

            this.Visibility = Visibility.Visible;
            Buttons.Focus(this);            
        }

        /// <summary>
        /// Button down handler
        /// </summary>        
        protected override void OnButtonDown(ButtonEventArgs e)
        {
            // Close windows if not editing
            if (!editMode)
            {
                this.Close();
                parentApp.SetFocus();
            }
            else
            {
                switch (e.Button)
                {
                    case Button.Up:
                        letterIndexes[selectedLetter]++;
                        break;
                    case Button.Down:
                        letterIndexes[selectedLetter]--;
                        break;
                    case Button.Left:
                        selectedLetter--;
                        break;
                    case Button.Right:
                        selectedLetter++;
                        break;
                    case Button.Select:
                        SaveItem();
                        break;

                }
                UpdateName();
            }
        }

        /// <summary>
        /// Saves edited item into HighScore Table.
        /// Ends the editing mode.
        /// </summary>
        private void SaveItem()
        {
            editMode = false;
            ScoreItem scoreItem = (ScoreItem)scoreListBox.Items[selectedItem];
            scoreItem.Highlite = false;
            parentApp.HighScore.Table[selectedItem].Name = LettersToString();            
            parentApp.PersistHighScore();
            UpdateHint();
        }

        /// <summary>
        /// Updates edited name
        /// </summary>
        private void UpdateName()
        {
            // Test selected letter value
            if (selectedLetter >= NAME_LENGTH)
                selectedLetter = NAME_LENGTH - 1;
            else if (selectedLetter < 0)
                selectedLetter = 0;

            // Test char index value
            if (letterIndexes[selectedLetter] >= allowedChars.Length)
                letterIndexes[selectedLetter] = 0;
            else if (letterIndexes[selectedLetter] < 0)
                letterIndexes[selectedLetter] = allowedChars.Length - 1;

            // Update scoreItem
            ScoreItem scoreItem = (ScoreItem)scoreListBox.Items[selectedItem];
            scoreItem.SelectedLetter = selectedLetter;
            scoreItem.Name = LettersToString();
        }

        /// <summary>
        /// Updates hints according to editMode value
        /// </summary>
        private void UpdateHint()
        {
            Font hintFont  = Resources.GetFont(Resources.FontResources.NinaB);
            Color hintColor = ColorUtility.ColorFromRGB(206, 206, 206);

            hintTextFlow.TextRuns.Clear();

            // Print editing hints
            if (editMode)
            {
                hintTextFlow.TextRuns.Add(
                            Resources.GetString(Resources.StringResources.UseArrows),
                            hintFont,
                            hintColor);
                hintTextFlow.TextRuns.Add(TextRun.EndOfLine);
                hintTextFlow.TextRuns.Add(
                            Resources.GetString(Resources.StringResources.PressSelect),
                            hintFont,
                            hintColor);
            }
            else
            {
                hintTextFlow.TextRuns.Add(
                            Resources.GetString(Resources.StringResources.PressAnyKey),
                            hintFont,
                            hintColor);
            }

            hintTextFlow.Invalidate();
        }

        /// <summary>
        /// Converts letter array into string
        /// </summary>
        /// <returns>String</returns>
        private string LettersToString()
        {
            string output = string.Empty;
            foreach (int letter in letterIndexes)
                output += allowedChars[letter].ToString();

            return output;
        }
    }
}
