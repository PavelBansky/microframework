Infrared Receiver Driver for Sony TV Remotes
============================================

_(April 19th 2009)_

.NET Micro Framework drivers for infrared receivers similar to TSOP4838.

##SonyReceiver 
Simple driver that creates event whenever TV Remote command is received and decoded.

	public static void Main()
	{
	    TVRemoteReceiver sony = new SonyReceiver(Meridian.Pins.GPIO1);
	    sony.DataReceived += new TVRemoteReceiver.TVRemoteDataHandler(sony_OnDataReceived);
	
	    while (true)
	    {
	        Thread.Sleep(20);
	    }
	}
	
	static void sony_OnDataReceived(TVRemote sender, int command, int address)
	{
	    Debug.Print("----");
	    Debug.Print(address.ToString());
	    Debug.Print(command.ToString());
	}

##TVRemoteInputProvider
WPF input provider that creates WPF button event whenever TV Remote command is received and decoded.

	SonyReceiver sonyRemote = new SonyReceiver(Meridian.Pins.GPIO1);
	
	TVRemoteButtonPad[] buttons = new TVRemoteButtonPad[]
	{
	    new TVRemoteButtonPad(116, Button.VK_UP),
	    new TVRemoteButtonPad(117, Button.VK_DOWN),
	    new TVRemoteButtonPad(051, Button.VK_RIGHT),
	    new TVRemoteButtonPad(052, Button.VK_LEFT),
	    new TVRemoteButtonPad(101, Button.VK_RETURN),
	};
	
	// Create the object that configures the TV Remote commands to buttons.
	TVRemoteInputProvider sonyProvider = new TVRemoteInputProvider(null, 
	                                              sonyRemote, 
	                                              (int)SonyReceiver.Devices.TV, 
	                                              buttons);
	                                              
	sonyProvider.ButtonAction = RawButtonActions.ButtonDown;
	
	// Create the object that configures the GPIO pins to buttons.
	GPIOButtonInputProvider inputProvider = new GPIOButtonInputProvider(null);


More information and circuits can be found here:

[http://bansky.net/blog/2009/04/microframework-device-controlled-via-tv-remote/](http://bansky.net/blog/2009/04/microframework-device-controlled-via-tv-remote/)