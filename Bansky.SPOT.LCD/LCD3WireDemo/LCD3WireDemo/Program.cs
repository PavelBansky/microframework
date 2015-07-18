using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.SJJ;
using Bansky.SPOT.LCD;

namespace LCD3WireDemo
{
    public class Program
    {
        public static void Main()
        {
            // Create instance of shift register
            HC4094 shifter = new HC4094(Pins.GPIO_PORT_Y_7, 	// Data pin
                                          Pins.GPIO_PORT_Y_6, 	// Clock pin
                                          Pins.GPIO_PORT_Y_5,	// Strobe pin
                                          false);               // Little Endian

            // Create new LCD instance and use shift register as a transport layer
            LCD4Bit lcd = new LCD4Bit(shifter);

            // Creating custom characters (Smiley face and gimp)
            byte[] buffer = new byte[] {    0x07, 0x08, 0x10, 0x10, 0x13, 0x13, 0x10, 0x10,
                                            0x1F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x04,
                                            0x1C, 0x02, 0x01, 0x01, 0x19, 0x19, 0x01, 0x01,
                                            0x10, 0x10, 0x12, 0x11, 0x10, 0x10, 0x08, 0x07,
                                            0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00, 0x1F,
                                            0x01, 0x01, 0x09, 0x11, 0x01, 0x01, 0x02, 0x1C,

                                            0x15, 0x15, 0x0E, 0x04, 0x04, 0x0A, 0x11, 0x11,
                                            0x04, 0x04, 0x0E, 0x15, 0x04, 0x0A, 0x11, 0x11
                                       };

            // Load custom characters to display CGRAM
            lcd.WriteToCGRAM(0x00, buffer);

            // Turn displat on, turn back light on, hide small cursor, show big blinking cursor
            lcd.Display(true, true, false, true);

            lcd.Clear();
            lcd.Write("Start me up!");
            Thread.Sleep(3000);

            lcd.Clear();
            lcd.Display(true, true, false, false);

            // Print the special characters with the face
            lcd.Write(new byte[] { 0x00, 0x01, 0x02 }, 0, 3);
            lcd.Write(" .NET Micro");

            // Move to second line
            lcd.SetPosition(40);

            // Print the special characters with the face
            lcd.Write(new byte[] { 0x03, 0x04, 0x05 }, 0, 3);
            lcd.Write("  Framework");
            Thread.Sleep(2000);

            // Blink with back light
            for (int i = 0; i < 4; i++)
            {
                lcd.Display(true, ((i % 2) != 0), false, false);
                Thread.Sleep(400);
            }

            lcd.Clear();
            string message = "* Hello World! *";
            // Let gimp write the message
            for (int i = 0; i < message.Length; i++)
            {
                lcd.SetPosition(i + 40);
                lcd.WriteByte((byte)(((i % 2) == 0) ? 0x06 : 0x07));

                lcd.SetPosition(i);
                lcd.Write(message[i].ToString());

                Thread.Sleep(200);

                lcd.SetPosition(i + 40);
                lcd.Write(" ");
            }
            Thread.Sleep(1500);

            lcd.Clear();
            lcd.SetPosition(16);

            lcd.Write("http://bansky.net/blog");
            // Scroll the page url
            while (true)
            {
                lcd.ShiftDisplay(false);
                Thread.Sleep(400);
            }
        }
    }
}
