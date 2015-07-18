using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Hardware;
using System.Threading;
using Microsoft.SPOT.Presentation.Media;

namespace CameraApp
{
    public class Program : Microsoft.SPOT.Application
    {

        private C328R camera;
        private Window mainWindow;
        private Image imageView;

        public static void Main()
        {
            Program myApplication = new Program();

            Window mainWindow = myApplication.CreateWindow();

            // Create the object that configures the GPIO pins to buttons.
            GPIOButtonInputProvider inputProvider = new GPIOButtonInputProvider(null);

            // Start the application
            myApplication.Run(mainWindow);
        }

        public Window CreateWindow()
        {
            // Create a window object and set its size to the
            // size of the display.
            mainWindow = new Window();
            mainWindow.Height = SystemMetrics.ScreenHeight;
            mainWindow.Width = SystemMetrics.ScreenWidth;
            mainWindow.Background = new SolidColorBrush(Color.Black);
            
            imageView = new Image();            
            imageView.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            imageView.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center;            
            mainWindow.Child = imageView;
                        
            mainWindow.AddHandler(Buttons.ButtonDownEvent, new ButtonEventHandler(OnButtonDown), false);
            
            mainWindow.Visibility = Visibility.Visible;
            Buttons.Focus(mainWindow);
            
            // Create camera
            camera = new C328R(new SerialPort.Configuration(SerialPort.Serial.COM2, SerialPort.BaudRate.Baud115200, false));
            InitCamera();

            return mainWindow;
        }

        private void OnButtonDown(object sender, ButtonEventArgs e)
        {
            // Picture data buffer
            byte[] pictureData;

            // Get instant Jpeg picture - give some process delay
            camera.GetJpegPicture(C328R.PictureType.Jpeg, out pictureData, 800);

            // If some data exists - show'em
            if (pictureData.Length > 0)
            {
                mainWindow.Background = new SolidColorBrush(Colors.Black);
                imageView.Bitmap = new Bitmap(pictureData, Bitmap.BitmapImageType.Jpeg);
                imageView.Invalidate();
            }
            else
            {
                // if no image was taken - turn background red and reset camera
                mainWindow.Background = new SolidColorBrush(Colors.Red);
                camera.Reset(true);
                InitCamera();              
            }            
        }

        private void InitCamera()
        {
            // Synchronize with camera
            camera.Sync();

            // Set baud rate - optional
            camera.SetBaudRate(C328R.BaudRate.Baud115200);
            // Set light frequency - optional
            camera.LigtFrequency(C328R.FrequencyType.F50Hz);

            // Initiate camera and picture details
            camera.Initial(C328R.ColorType.Color16, C328R.PreviewResolution.R160x120, C328R.JpegResolution.R320x240);          
        }
    }
}
