using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace CameraApp
{
    /// <summary>
    /// C328R still image Jpeg camera driver for .NET Micro Framework.
    /// Camera Manufacturer: COMedia Ltd. http://www.comedia.com.hk/
    /// 
    /// Driver Author: Pavel Bansky http://bansky.net/blog
    /// 
    /// This code was written by Pavel Bansky. It is released under the terms of 
    /// the Creative Commons "Attribution NonCommercial ShareAlike 2.5" license.
    /// http://creativecommons.org/licenses/by-nc-sa/2.5/
    /// </summary>
    public class C328R
    {
        public enum ColorType { GreyScale2 = 1, GreyScale4 = 2, GreyScale8 = 3, Color12 = 5, Color16 = 6, Jpeg = 7 };
        public enum PreviewResolution { R80x60 = 1, R160x120 = 3 };
        public enum JpegResolution { R80x64 = 1, R160x128 = 3, R320x240 = 5, R640x480 = 7 };
        public enum PictureType { Snapshot = 1, Preview = 2, Jpeg = 5 };
        public enum SnapshoteType { Compressed = 0, Uncompressed = 1 };
        public enum FrequencyType { F50Hz = 0, F60Hz = 1 };
        public enum BaudRate { Baud7200, Baud9600, Baud14400, Baud19200, Baud28800, Baud38400, Baud57600, Baud115200 };

        #region Command constants
        const byte CMD_PREFIX = 0xAA;
        const byte CMD_INITIAL = 0x01;
        const byte CMD_GETPICTURE = 0x04;
        const byte CMD_SNAPSHOT = 0x05;
        const byte CMD_PACKAGESIZE = 0x06;

        const byte CMD_BAUDRATE = 0x07;
        const byte CMD_RESET = 0x08;
        const byte CMD_POWEROFF = 0x09;

        const byte CMD_DATA = 0x0A;
        const byte CMD_SYNC = 0x0D;
        const byte CMD_ACK = 0x0E;

        const byte CMD_NAK = 0x0F;
        const byte CMD_LIGHTFREQ = 0x13;
        #endregion

        /// <summary>
        /// Size of the data package with image
        /// </summary>
        const int PACKAGE_SIZE = 512;

        private SerialPort serialPort;
        private byte[] command = new byte[6];        

        /// <summary>
        /// Create new C328R camera instance
        /// </summary>
        /// <param name="serialConfig">Configuration for serial port</param>
        public C328R(SerialPort.Configuration serialConfig)
        {
            serialPort = new SerialPort(serialConfig);            
        }

        /// <summary>
        /// Tries to sync with camera
        /// </summary>
        /// <returns>True if succeeded</returns>
        public bool Sync()
        {
            // Create 'Sync' command
            command[0] = CMD_PREFIX;
            command[1] = CMD_SYNC;
            command[2] = 0;
            command[3] = 0;
            command[4] = 0;
            command[5] = 0;

            byte[] recvCommand = new byte[6];
            int i = 0;
            bool stat = false;

            while (true)
            {
                i++;
                if (i > 60)
                {
                    stat = false;
                    break;
                }

                stat = SendCommand(command);               

                // Wait for ACK                
                stat = WaitForResponse(ref recvCommand, 100);                
                if (!stat || recvCommand[1] != CMD_ACK || recvCommand[2] != CMD_SYNC)
                {
                    continue;
                }

                // Wait for SYNC
                stat = WaitForResponse(ref recvCommand, 100);
                if (!stat || recvCommand[1] != CMD_SYNC)
                {
                    continue;
                }

                stat = SendACK();
                
                break;
            }            
            return stat;
        }

        /// <summary>
        /// Inititate camera with specific settings
        /// </summary>
        /// <param name="colorType">Color depth</param>
        /// <param name="previewResolution">Preview resolution</param>
        /// <param name="jpegResolution">Jpeg image resolution</param>
        /// <returns>True if succeeded</returns>
        public bool Initial(ColorType colorType, PreviewResolution previewResolution, JpegResolution jpegResolution)
        {
            // Create the command
            command[0] = CMD_PREFIX;
            command[1] = CMD_INITIAL;
            command[2] = 0;
            command[3] = (byte)colorType;
            command[4] = (byte)previewResolution;
            command[5] = (byte)jpegResolution;

            // send 'Initial' command
            SendCommand(command);
            
            // Wait for ACK
            if (! ReceiveACK(CMD_INITIAL, 100)) 
                return false;

            // Create package size command
            int packsize = PACKAGE_SIZE;
            command[0] = CMD_PREFIX;
            command[1] = CMD_PACKAGESIZE;
            command[2] = 0x08;
            command[3] = (byte)packsize; // PACKAGE_SIZE Low byte
            command[4] = (byte)(packsize >> 8); // PACKAGE_SIZE High byte
            command[5] = 0x00;

            // Send 'Set Package Size' command
            SendCommand(command);

            // Wait for ACK
            if (!ReceiveACK(CMD_PACKAGESIZE, 100))
                return false;

            return true;
        }

        /// <summary>
        /// Make snapshot and keep it the buffer
        /// </summary>
        /// <param name="snapshotType">Snapshot type</param>
        /// <param name="skipFrameCounter">Number of dropped frame before compression. 0 means current frame.</param>
        /// <returns>True if succeeded</returns>
        public bool Snapshot(SnapshoteType snapshotType, int skipFrameCounter)
        {
            // Create snapshot command
            command[0] = CMD_PREFIX;
            command[1] = CMD_SNAPSHOT;
            command[2] = (byte)snapshotType;
            command[3] = (byte)(skipFrameCounter); 
            command[4] = (byte)(skipFrameCounter >> 8); 
            command[5] = 0x00;

            // send 'Snapshot' command
            SendCommand(command);

            // Receive ACK
            if (!ReceiveACK(CMD_SNAPSHOT, 100))
                return false;

            return true;
        }

        /// <summary>
        /// Get Jpeg picture from camera
        /// </summary>
        /// <param name="pictureType">Picture type (Snapshot/Jpeg)</param>
        /// <param name="dataBuffer">Buffer for received data</param>
        /// <param name="processDelay">Time to process the image in camera. Larger Jpegs takes about 1 sec.</param>
        /// <returns>True if succeeded</returns>
        public bool GetJpegPicture(PictureType pictureType, out byte[] dataBuffer, int processDelay)
        {
            dataBuffer = new byte[0];
            int pictureDataSize;

            // Send 'Get Picture' command
            if (!GetPictureCommand(pictureType, processDelay, out pictureDataSize))
                return false;

            // init data buffer
            dataBuffer = new byte[pictureDataSize];
            int bufferPosition = 0;
            int packageCounter = 0;
            int errorCounter = 0;
            byte[] response = new byte[PACKAGE_SIZE];

            // Loop to read all data
            while (bufferPosition < dataBuffer.Length && errorCounter <= 15)
            {                
                SendACK(packageCounter);
                Thread.Sleep(40);

                // Wait for data package
                Array.Clear(response, 0, PACKAGE_SIZE);
                bool stat = WaitForResponse(ref response, 1000);

                // If data package received process it else increase error counter
                if (stat)
                {
                    // Get data size in packet
                    int packetSize = response[3] << 8;
                    packetSize |= response[2];

                    // Copy data from packet into data buffer
                    Array.Copy(response, 4, dataBuffer, bufferPosition, packetSize);

                    // Move buffer position and get ready for next package
                    bufferPosition += packetSize;
                    packageCounter++;
                }
                else
                    errorCounter++;
            }

            // Send final package ACK
            SendACK(packageCounter);  

            return true;
        }

        /// <summary>
        /// Get raw picture from camera
        /// </summary>
        /// <param name="pictureType">Picture type (Snapshot/Preview)</param>
        /// <param name="dataBuffer">Buffer for received data</param>
        /// <param name="processDelay">Time to process the image in camera. Larger Jpegs takes about 1 sec.</param>
        /// <returns>True if succeeded</returns>
        public bool GetRawPicture(PictureType pictureType, out byte[] dataBuffer, int processDelay)
        {
            dataBuffer = new byte[0];
            int dataSize;

            // Send 'Get Picture' command
            if (!GetPictureCommand(pictureType, processDelay, out dataSize))
                return false;

            // init data buffer
            dataBuffer = new byte[dataSize];

            // Read whole image at once
            if (!WaitForResponse(ref dataBuffer, 2000))
                return false;

            // Send final package ACK
            SendACK(0x00);

            return true;
        }

        /// <summary>
        /// Set the light frequency of camera
        /// </summary>
        /// <param name="lightFrequency">Light frequency (50Hz / 60 Hz)</param>
        /// <returns>True if succeeded</returns>
        public bool LigtFrequency(FrequencyType lightFrequency)
        {
            // Create 'Light Frequency' command
            command[0] = CMD_PREFIX;
            command[1] = CMD_LIGHTFREQ;
            command[2] = (byte)lightFrequency;
            command[3] = 0x00;
            command[4] = 0x00;
            command[5] = 0x00;

            // Send 'Light Frequency' command
            SendCommand(command);

            // Receive ACK
            if (!ReceiveACK(CMD_LIGHTFREQ, 100))
                return false;

            return true;
        }

        /// <summary>
        /// Set communicatin speed that will be used by camera until physically power off.
        /// </summary>
        /// <param name="baudRate">Baudrate</param>
        /// <returns>True if succeeded</returns>
        public bool SetBaudRate(BaudRate baudRate)
        {
            byte divider1;            

            switch (baudRate)
            {
                case BaudRate.Baud7200:
                    divider1 = 0xFF;                    
                    break;
                case BaudRate.Baud9600:
                    divider1 = 0xBF;
                    break;
                case BaudRate.Baud14400:
                    divider1 = 0x7F;
                    break;
                case BaudRate.Baud19200:
                    divider1 = 0x5F;
                    break;
                case BaudRate.Baud28800:
                    divider1 = 0x3F;
                    break;
                case BaudRate.Baud38400:
                    divider1 = 0x2F;
                    break;
                case BaudRate.Baud57600:
                    divider1 = 0x1F;
                    break;
                case BaudRate.Baud115200:
                    divider1 = 0x0F;
                    break;
                default:
                    divider1 = 0xBF;
                    break;
            }

            // Create 'Set Baudrate' command
            command[0] = CMD_PREFIX;
            command[1] = CMD_BAUDRATE;
            command[2] = divider1; // Divider 1
            command[3] = 0x01; // Divider 2
            command[4] = 0x00;
            command[5] = 0x00;

            // Send 'Set Baudrate' command
            SendCommand(command);

            // Receive ACK
            if (!ReceiveACK(CMD_BAUDRATE, 100))
                return false;

            return true;
        }

        /// <summary>
        /// Reset the camera
        /// </summary>
        /// <param name="completeReset">True for complete reset, False for state machine reset</param>
        /// <returns>True if succeeded<</returns>
        public bool Reset(bool completeReset)
        {
            // Create 'Reset' command
            command[0] = CMD_PREFIX;
            command[1] = CMD_RESET;
            command[2] = (byte)((completeReset == true) ? 0x00 : 0x01);
            command[3] = 0x00;
            command[4] = 0x00;
            command[5] = 0xFF;

            // Send 'Reset' command
            SendCommand(command);

            // Receive ACK
            if (!ReceiveACK(CMD_RESET, 100))
                return false;

            return true;
        }

        /// <summary>
        /// Power off the camera
        /// </summary>
        /// <returns>True if succeeded</returns>
        public bool PowerOff()
        {
            // Create 'Power Off' command
            command[0] = CMD_PREFIX;
            command[1] = CMD_POWEROFF;
            command[2] = 0x00;
            command[3] = 0x00;
            command[4] = 0x00;
            command[5] = 0x00;

            // Send 'Power Off' command
            SendCommand(command);

            // Receive ACK
            if (!ReceiveACK(CMD_POWEROFF, 100))
                return false;

            return true;
        }

        /// <summary>
        /// Waits for response from camera
        /// </summary>
        /// <param name="readBuffer">Buffer for response</param>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <returns>False if timeout occured</returns>
        private bool WaitForResponse(ref byte[] readBuffer, int timeout)
        {            
            int recv = serialPort.Read(readBuffer, 0, readBuffer.Length, timeout);            
            return recv != 0;
        }

        /// <summary>
        /// Waits for response and parse it for ACK
        /// </summary>
        /// <param name="expectedACKCommand">Command to be ACKnowlegde</param>
        /// <param name="timeout">Timeout in miliseconds</param>
        /// <exception>Throws exception with error code when NACK is received.
        /// See C328R user manula for more information about error codes.
        /// Error codes:
        /// Picture Type Error      01h   Parameter Error                   0Bh 
        /// Picture Up Scale        02h   Send Register Timeout             0Ch 
        /// Picture Scale Error     03h   Command ID Error                  0Dh 
        /// Unexpected Reply        04h   Picture Not Ready                 0Fh 
        /// Send Picture Timeout    05h   Transfer Package Number Error     10h 
        /// Unexpected Command      06h   Set Transfer Package Size Wrong   11h 
        /// SRAM JPEG Type Error    07h   Command Header Error              F0h
        /// SRAM JPEG Size Error    08h   Command Length Error              F1h
        /// Picture Format Error    09h   Send Picture Error                F5h
        /// Picture Size Error      0Ah   Send Command Error                FFh 
        /// </exception>
        /// <returns>True if ACK for expected command was received</returns>
        private bool ReceiveACK(byte expectedACKCommand, int timeout)
        {
            byte[] responseBuffer = new byte[6];
            bool stat = WaitForResponse(ref responseBuffer, timeout);

            // If NAK is received instead of ACK - raise exception
            if (stat && responseBuffer[1] == CMD_NAK)
            {
                throw new Exception("C328R Error " + responseBuffer[4]);
            }

            // If no ACK or ACK for different command received - return false
            if (!stat || responseBuffer[1] != CMD_ACK || responseBuffer[2] != expectedACKCommand)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Send ACK command
        /// </summary>
        /// <param name="packageId">Package Id for ACK</param>
        /// <returns></returns>
        private bool SendACK(int packageId)
        {
            byte[] ackCommand = new byte[6] { CMD_PREFIX, CMD_ACK, 0, 0, (byte)packageId, (byte)(packageId >> 8)};
            return SendCommand(ackCommand);
        }

        /// <summary>
        /// Send ACK command
        /// </summary>
        /// <returns>True if succeeded</returns>
        private bool SendACK()
        {
            return SendACK(0x00);
        } 

        /// <summary>
        /// Send generic command
        /// </summary>
        /// <param name="commandArray">Byte array with command and arguments</param>
        /// <returns>True if succeeded</returns>
        private bool SendCommand(byte[] commandArray)
        {
            int len = commandArray.Length;            
            int send = serialPort.Write(commandArray, 0, len);            
            Thread.Sleep(10);

            return send == len;
        }

        /// <summary>
        /// Helper method: Send GetPicture command and receive expected data size of the picture
        /// </summary>
        /// <param name="pictureType">Picture type (Snapshot/Preview/Jpeg)</param>
        /// <param name="processDelay">Time to process the image in camera</param>
        /// <param name="pictureDataSize">Picture data size</param>
        /// <returns>True if succeeded</returns>
        private bool GetPictureCommand(PictureType pictureType, int processDelay, out int pictureDataSize)
        {
            pictureDataSize = 0;

            // Create 'Get Picture' command
            command[0] = CMD_PREFIX;
            command[1] = CMD_GETPICTURE;
            command[2] = (byte)pictureType;
            command[3] = 0x00;
            command[4] = 0x00;
            command[5] = 0x00;

            // Send 'Get Picture' command
            SendCommand(command);

            // Give camera time to proceed the image
            Thread.Sleep(processDelay);

            // Receive ACK
            if (!ReceiveACK(CMD_GETPICTURE, 1000))
                return false;

            // Receive DATA command, with inormations about image
            byte[] response = new byte[6];
            bool stat = WaitForResponse(ref response, 500);
            if (!stat || response[1] != CMD_DATA)
                return false;

            // Get dataSize from three bytes
            pictureDataSize = response[5] << 8;
            pictureDataSize |= response[4] << 8;
            pictureDataSize |= response[3];

            return true;
        }    
    }
}
