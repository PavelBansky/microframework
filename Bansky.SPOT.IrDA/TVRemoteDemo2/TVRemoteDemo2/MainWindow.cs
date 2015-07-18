using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Hardware;

namespace TVRemoteDemo2
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponents();
            channel = 1;
        }        

        private void InitializeComponents()
        {                   
            this.Height = SystemMetrics.ScreenHeight;
            this.Width = SystemMetrics.ScreenWidth;

            Text textChannel = new Text();
            textChannel.Font = Resources.GetFont(Resources.FontResources.Consolas24);
            textChannel.TextContent = Resources.GetString(Resources.StringResources.Channel);
            textChannel.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            textChannel.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Top;

            Text textVolume = new Text();
            textVolume.Font = Resources.GetFont(Resources.FontResources.Consolas24);
            textVolume.TextContent = Resources.GetString(Resources.StringResources.Volume);
            textVolume.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            textVolume.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Bottom;
            textVolume.SetMargin(0, 50, 0, 0);

            channelLabel = new Text();            
            channelLabel.Font = Resources.GetFont(Resources.FontResources.Consolas48);
            channelLabel.TextContent = channel.ToString();
            channelLabel.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            channelLabel.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Top;

            volumeBar = new VolumeBar();
            volumeBar.HorizontalAlignment = HorizontalAlignment.Center;
            volumeBar.VerticalAlignment = VerticalAlignment.Bottom;
            volumeBar.Value = 10;

            StackPanel panel = new StackPanel(Orientation.Vertical);
            panel.HorizontalAlignment = HorizontalAlignment.Center;
            panel.VerticalAlignment = VerticalAlignment.Stretch;
            panel.Children.Add(textChannel);
            panel.Children.Add(channelLabel);
            panel.Children.Add(textVolume);
            panel.Children.Add(volumeBar);

            // Add the text control to the window.
            this.Child = panel;

            // Set the window visibility to visible.
            this.Visibility = Visibility.Visible;

            // Attach the button focus to the window.
            Buttons.Focus(this);         
        }

        protected override void OnButtonUp(ButtonEventArgs e)
        {            
            switch (e.Button)
            {
                case Button.VK_0: 
                    channel = 0;
                    break;
                case Button.VK_1:
                    channel = 1;
                    break;
                case Button.VK_2:
                    channel = 2;
                    break;
                case Button.VK_3:
                    channel = 3;
                    break;
                case Button.VK_4:
                    channel = 4;
                    break;
                case Button.VK_5:
                    channel = 5;
                    break;
                case Button.VK_6:
                    channel = 6;
                    break;
                case Button.VK_7:
                    channel = 7;
                    break;
                case Button.VK_8:
                    channel = 8;
                    break;
                case Button.VK_9:
                    channel = 9;
                    break;
                case Button.VK_NEXT:
                    channel += 1;
                    break;
                case Button.VK_PRIOR:
                    channel -= 1;
                    break;
                case Button.VK_VOLUME_UP:
                    volumeBar.Value += 1;
                    break;
                case Button.VK_VOLUME_DOWN:
                    volumeBar.Value -= 1;
                    break;
            }

            channelLabel.TextContent = channel.ToString();
        }

        private int channel;
        private VolumeBar volumeBar;
        private Text channelLabel;
    }
}
