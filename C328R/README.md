C328R Jpeg camera for .NET Micro Framework
==========================================

_(March 2nd 2008)_

C328R is Jpeg camera connected via serial port to micro processor

	// Create camera  
	C328R camera = new C328R(new SerialPort.Configuration(
	                            SerialPort.Serial.COM2, 
	                            SerialPort.BaudRate.Baud115200, 
	                            false));  
	
	// Synchronize with camera 
	camera.Sync();   
	
	// Set baud rate - optional  
	camera.SetBaudRate(C328R.BaudRate.Baud115200); 
	// Set light frequency - optional 
	camera.LigtFrequency(C328R.FrequencyType.F50Hz); 
	
	// Initiate camera and picture details
	camera.Initial(C328R.ColorType.Jpeg, 
	                C328R.PreviewResolution.R160x120, 
	                C328R.JpegResolution.R320x240);

##Taking Pictures

	// Picture data buffer
	byte[] pictureData;
	
	// Make Jpeg snapshot
	camera.Snapshot(C328R.SnapshoteType.Compressed, 0);
	// Make raw snapshot
	//camera.Snapshot(C328R.SnapshoteType.UnCompressed, 0);
	
	// Give C328R some time to process the picture
	Thread.Sleep(1000);
	
	// Get stored Jpeg snapshot from camera
	// Give 0 processDelay - because we already gave 1000 ms.
	camera.GetJpegPicture(C328R.PictureType.Snapshot, out pictureData, 0);

More information and circuits can be found here:
[http://bansky.net/blog/2008/03/jpeg-camera-and-micro-framework/](http://bansky.net/blog/2008/03/jpeg-camera-and-micro-framework/)