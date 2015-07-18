using System;

using Microsoft.SPOT;
using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using System.IO.Ports;
using System.IO;
using Microsoft.SPOT.IO;

namespace CameraApp30
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
            mainWindow.Background = new SolidColorBrush(Color.White);

            imageView = new Image();
            imageView.HorizontalAlignment = Microsoft.SPOT.Presentation.HorizontalAlignment.Center;
            imageView.VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center;
            mainWindow.Child = imageView;

            mainWindow.AddHandler(Buttons.ButtonDownEvent, new ButtonEventHandler(OnButtonDown), false);

            mainWindow.Visibility = Visibility.Visible;
            Buttons.Focus(mainWindow);

            // Create camera
            camera = new C328R(new SerialPort("COM2", 115200));
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

                // Save operation takes some time
                SaveFileOnDisk(pictureData);
            }
            else
            {
                // if no image was taken - turn background red and reset camera
                mainWindow.Background = new SolidColorBrush(Colors.Red);
                imageView.Bitmap = new Bitmap(10, 10);

                mainWindow.Invalidate();
                camera.Reset(true);                
            }

            InitCamera();
        }

        private void InitCamera()
        {
            // Synchronize with camera
            Debug.Print(camera.Sync().ToString());

            // Set baud rate - optional
            camera.SetBaudRate(C328R.BaudRate.Baud115200);
            // Set light frequency - optional
            camera.LigtFrequency(C328R.FrequencyType.F50Hz);

            // Initiate camera and picture details
            Debug.Print(camera.Initial(C328R.ColorType.Jpeg, C328R.PreviewResolution.R160x120, C328R.JpegResolution.R640x480).ToString());            
        }

        private void SaveFileOnDisk(byte[] pictureData)
        {
            // This directory is valid for Tahoe-II SD card slot only!!
            Directory.SetCurrentDirectory(@"\SD1");

            // Some unique picture name based on time
            string pictureName = string.Concat(
                                    "IMG_",
                                    DateTime.Now.Hour.ToString(),
                                    DateTime.Now.Minute.ToString(),
                                    DateTime.Now.Second.ToString(),
                                    ".jpeg");

            // Save the data into file
            using (FileStream fs = new FileStream(pictureName, 
                                                  FileMode.Create, 
                                                  FileAccess.Write, 
                                                  FileShare.ReadWrite))
            {
                fs.Write(pictureData, 0, pictureData.Length);
                fs.Close();
            }
        }
    }
}
