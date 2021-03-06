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
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Input;
using MicroTetris.GameLogic;
using MicroTetris3;

namespace MicroTetris.Presentation
{
    /// <summary>
    /// Game main menu window
    /// </summary>
    public class MainMenuWindow : Window
    {
        GameWindow gameWindow;
        ListBox menuListBox;
        TetrisApp parentApp;

        /// <summary>
        /// Creates new MainMenuWindow
        /// </summary>
        /// <param name="parentApp">Parent application</param>
        public MainMenuWindow(TetrisApp parentApp)
        {
            this.parentApp = parentApp;

            InitializeComponents();
        }

        /// <summary>
        /// Creates all WPF controls of the window
        /// </summary>
        private void InitializeComponents()
        {
            this.Width = SystemMetrics.ScreenWidth;
            this.Height = SystemMetrics.ScreenHeight;
            this.Background = new SolidColorBrush(Colors.Black);

            Image logoImage = new Image(Resources.GetBitmap(Resources.BitmapResources.Logo));

            #region ListBox event handler
            Color selectedItemColor = Colors.White;
            Color unselectedItemColor = ColorUtility.ColorFromRGB(206, 206, 206);
            Brush selectedBackground = new SolidColorBrush(ColorUtility.ColorFromRGB(0, 148, 255));

            menuListBox = new ListBox();
            menuListBox.Background = this.Background;
            menuListBox.HorizontalAlignment = HorizontalAlignment.Center;

            // Event handler for menu items
            menuListBox.SelectionChanged += delegate(object sender, SelectionChangedEventArgs e)
            {
                int previousSelectedIndex = e.PreviousSelectedIndex;
                if (previousSelectedIndex != -1)
                {
                    // Change previously-selected listbox item color to unselected color
                    ((Text)menuListBox.Items[previousSelectedIndex].Child).ForeColor = unselectedItemColor;
                    menuListBox.Items[previousSelectedIndex].Background = menuListBox.Background;
                }

                // Change newly-selected listbox item color to selected color and background
                ((Text)menuListBox.Items[e.SelectedIndex].Child).ForeColor = selectedItemColor;
                menuListBox.Items[e.SelectedIndex].Background = selectedBackground;
            };
            #endregion

            #region Menu Items
            // Menu items from resources
            string[] menuItems = new string[4] { Resources.GetString(Resources.StringResources.RookieLevel),
                                                 Resources.GetString(Resources.StringResources.AdvancedLevel),
                                                 Resources.GetString(Resources.StringResources.ExtremeLevel),
                                                 Resources.GetString(Resources.StringResources.ViewHighScore)};
            // Add items into listbox
            foreach(string item in menuItems)
            {
                Text itemText = new Text(Resources.GetFont(Resources.FontResources.NinaB), item);
                itemText.Width = this.Width - 40;
                itemText.ForeColor = unselectedItemColor;
                itemText.TextAlignment = TextAlignment.Center;
                itemText.SetMargin(5);
                
                ListBoxItem listBoxItem = new ListBoxItem();                
                listBoxItem.Background = menuListBox.Background;                
                listBoxItem.Child = itemText;

                menuListBox.Items.Add(listBoxItem);
            }
            
            menuListBox.SelectedIndex = 0;
            #endregion

            // Add all controls to stack panel
            StackPanel mainStackPanel = new StackPanel(Orientation.Vertical);
            mainStackPanel.Children.Add(logoImage);
            mainStackPanel.Children.Add(menuListBox);

            this.Child = mainStackPanel;

            this.Visibility = Visibility.Visible;
            Buttons.Focus(menuListBox);            
        }

        /// <summary>
        /// Button event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnButtonDown(ButtonEventArgs e)
        {
            if (e.Button == Button.Select)
            {
                switch (menuListBox.SelectedIndex)
                {
                    case 0:
                        StartGame(1);
                        break;
                    case 1:
                        StartGame(5);
                        break;
                    case 2:
                        StartGame(10);
                        break;
                    case 3:
                        ViewHighScore(-1);
                        break;
                }
            }
        }

        /// <summary>
        /// Focus event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnGotFocus(FocusChangedEventArgs e)
        {
            // Whenever this window gets focus, it gives it to listbox
            Buttons.Focus(menuListBox);
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Start new game at specified level
        /// </summary>
        /// <param name="startLevel">Starting level</param>
        private void StartGame(int startLevel)
        {
            gameWindow = new GameWindow(parentApp);
            gameWindow.OnClose += new GameWindow.CloseDelegate(GameWindow_OnGameOver);            
            gameWindow.StartGame(startLevel);
        }

        /// <summary>
        /// Shows HighScore table
        /// </summary>
        /// <param name="position"></param>
        private void ViewHighScore(int position)
        {
            HighScoreWindow scoreWindow = new HighScoreWindow(parentApp);            
        }

        /// <summary>
        /// Event handler for GameWindws close event
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="gameStatistics">Game statistics</param>
        private void GameWindow_OnGameOver(object sender, GameStatistics gameStatistics)
        {
            ScoreRecord scoreRecord = new ScoreRecord();            
            scoreRecord.Score = gameStatistics.Score;

            // Add score into HighScore table
            int scoreIndex = parentApp.HighScore.AddRecord(scoreRecord);

            // Show high score window
            HighScoreWindow scoreWindow = new HighScoreWindow(parentApp);

            // if high score has been reached then edit name
            if (scoreIndex > -1)
                scoreWindow.EditItem(scoreIndex);
        }
    }
}
