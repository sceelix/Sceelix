============================================================================
Sceelix External Application
Author: Sceelix
============================================================================

This project shows how to use the Sceelix Engine Libraries from an external application. With this, you could load 
and execute graphs and use them inside your own projects, such as games, so as to achieve load and runtime procedural 
generation. You could also use this to build a server that runs graphs remotely or simply use locally to call batch 
operations from other systems. 

Before you can use this application, you may need to fix the references to the Sceelix Libraries. For instance, you 
should replace the references to Sceelix.Core and Sceelix.Geometry.3D with the ones contained in the "Bin" folder of 
your Sceelix installation folder (or .app, if you're running the Mac version).

Also, do not forget to replace the first line of the Main function within the "Program.cs" file:
	
	LicenseManager.LoadLicenseFromString("XkTTBqnLk0wH......LdSouYwDTidBGhNw==");

with your own key. This is required to authorize the use of the libraries. 
If you have a valid Sceelix License, open the Sceelix Designer and go to Help->Get Developer Key and paste the key instead 
of this sample key.

That's it! All the remaining instructions are inside the code samples themselves.

Happy coding!