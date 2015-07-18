SMTP library for .NET Micro Framework
=====================================

_(August 2nd 2008)_

Library to send e-mails from .NET Micro Framework applications. More information can be found in the [original blog article.](http://bansky.net/blog/2008/08/sending-e-mails-from-net-micro-framework/)


##SmtpClient

	using (SmtpClient smtp = new SmtpClient("smtp.hostname.net", 25))
	{
	    // Send message
	    smtp.Send("john@doe.com",
	              "foo@bar.net",
	              "Good news",
	              "How are you Foo?");
	}


##Authentication

	using (SmtpClient smtp = new SmtpClient("smtp.hostname.net", 25))
	{
	    // Create message
	    MailMessage message = new MailMessage("john@doe.com",
	                                          "foo@bar.net",
	                                          "Good news",
	                                          "How are you Foo?");
	
	    // Authenicate to server
	    smtp.Authenticate = true;
	    smtp.Username = "userlogin";
	    smtp.Password = "userpassword";
	
	    // Send message
	    smtp.Send(message);
	}

##Attachments

	MailMessage message = new MailMessage();
	// Set sender name and address
	message.From = new MailAddress("foobar@contoso.com", "Foo Bar");
	
	// Set recipients
	message.To.Add(new MailAddress("john.doe@customer.com", "John Doe"));
	message.Cc.Add(new MailAddress("manager@contoso.com"));
	
	message.Subject = "Hello World";
	message.Body = "from now on you can send e-mails from <b>.NET Micro Framework</b>.";
	// Format body as HTML
	message.IsBodyHtml = true;
	
	// Create new attachment and define it's name
	Attachment attachment = new Attachment("Snwoflake.gif");        
	attachment.ContentType = "image/gif";
	attachment.TransferEncoding = TransferEncoding.Base64;
	// Attachment content
	attachment.Content = Base64.Encode( Resources.GetBytes(
	                                    Resources.BinaryResources.Snowflake_gif),
	                                    true);
	
	// Add attachment to message
	message.Attachments.Add(attachment);
	
	// Create new SMTP instance
	SmtpClient smtp = new SmtpClient("smtp.contoso.com", 25);
	try
	{
	    // Authenicate to server
	    smtp.Authenticate = true;
	    smtp.Username = "userlogin";
	    smtp.Password = "userpassword";
	
	    // Send message
	    smtp.Send(message);
	}
	catch (SmtpException e)
	{
	    // Exception handling here 
	    Debug.Print(e.Message);
	    Debug.Print("Error Code: " + e.ErrorCode.ToString());
	}
	finally
	{
	    smtp.Dispose();
	}

##Additional headers

	MailMessage message = new MailMessage("john@doe.com",
	                                      "foo@bar.net",
	                                      "Good news",
	                                      "How are you Foo?");
	
	message.Headers = "X-Priority: 1\r\n";
	message.Headers += "X-MSMail-Priority: High\r\n";
	message.Headers += "X-Mailer: Micro Framework mail sender\r\n";