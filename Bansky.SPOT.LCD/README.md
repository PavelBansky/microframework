HD44780 LCD Driver support for Micro Framework
==============================================

_(October 1st 2009)_

.NET Micro Framework support for HD44780 LCD Driver. HD44780 requires 11 parallel bits to operate (or 7 based on configuration), this would waste 11 GPIOs of the CPU.  This library support connectivity between CPU and HD44780 via shift register (3 GPIOs only) or via I2C port extender. 

##HC4094 Shift Register
Shift register takes serial bits and then sends them all at once to parallel output.   

	// Create instance of shift register
	HC4094 shifter = new HC4094(  Pins.GPIO_PORT_Y_7,     // Data pin
	                              Pins.GPIO_PORT_Y_6,     // Clock pin
	                              Pins.GPIO_PORT_Y_5,     // Strobe pin
	                              false);                 // Little Endian
	
	// Create new LCD instance and use shift register as a transport layer
	LCD4Bit lcd = new LCD4Bit(shifter);
	
	// Turn display on, turn back light on, hide small cursor, show big blinking cursor
	lcd.Display(true, true, false, true);
	
	lcd.Clear();                    // Clear screen
	lcd.Write("Hello world!");      // Write message
	lcd.SetPosition(40);            // Move to second line
	lcd.Write("Micro Framework");   // Write second line of message

Two wiring diagrams based on endianity. Left is little endian, right is big endian

![Wiring](http://bansky.net/blog_stuff/images/alphanumericLCD_3Wire.png)

More information and circuits can be found here:
[http://bansky.net/blog/2008/10/interfacing-lcd-with-3-wires-from-net-micro-framework/](http://bansky.net/blog/2008/10/interfacing-lcd-with-3-wires-from-net-micro-framework/)

##I2C Gpio Expander
PCF8574P is an gpio extender that receives bits over I2C bus and then sends them all at once to parallel output.

	I2CDevice I2Cbus = new I2CDevice(new I2CDevice.Configuration(0, 100));
	
	// Create instance of PCF8574P
	PCF8574P expander = new PCF8574P(   I2Cbus,       // I2C bus instance
	                                    0x27,         // Address on I2C bus
	                                    true);        // Use big endian
	
	// Create new LCD instance and use shift register as a transport layer
	LCD4Bit lcd = new LCD4Bit(expander);
	
	// Turn display on, turn back light on, hide small cursor, show big blinking cursor
	lcd.Display(true, true, false, true);
	
	lcd.Clear();                    // Clear screen
	lcd.Write("Hello world!");      // Write message
	lcd.SetPosition(40);            // Move to second line
	lcd.Write("Micro Framework");   // Write second line of message

![Wiring](http://bansky.net/blog_stuff/images/alphanumericLCD_I2C.png)

More information and circuits can be found here:
[http://bansky.net/blog/2008/10/interfacing-lcd-using-i2c-port-extender/](http://bansky.net/blog/2008/10/interfacing-lcd-using-i2c-port-extender/)