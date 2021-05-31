============================================================================
Sceelix Engine Library Sample
Author: Sceelix
============================================================================

This sample demonstrates how to extend the Sceelix Engine with new procedures, functions, entities and other features.

Before you can use this application, you may need to fix the references to the Sceelix Libraries. For instance, you 
should replace the references to Sceelix.Core and Sceelix.Geometry.3D with the ones contained in the "Bin" folder of 
your Sceelix installation folder (or .app, if you're running the Mac version).

In order to test this library, all you need to do is to place the compiled assembly (and the respective XML 
documentation file) inside the Sceelix/Bin folder. To facilitate development, you can build the library directly 
into that folder and instruct the IDE to start the Sceelix.Designer.exe directly. In Visual Studio, you can do that in 
	
	 Project properties -> Debug -> Start External program    and select the Sceelix.Designer.exe executable (inside the 'Bin' folder or equivalent).

Before proceeding with the code samples, you should look into the 'AssemblyInfo.cs' file. There, you'll see what you'll 
need to setup in order for Sceelix to recognize this library as an Engine library.

That's it! All the remaining instructions are inside the code samples themselves.

Happy coding!