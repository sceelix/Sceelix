======================================================
Content building using the MonoGame Content Pipeline 
======================================================

Thos folder contains several shaders and otherwise resources required by the Sceelix Designer.
Running "Sceelix.Content.Windows.bat" will proceed to execute the MonoGame Content Pipeline Tool (MGCB) and zip the built resources into a single file.

Unfortunately, MGCB can on can only be executed on Windows, so updates to the sources must be rebuilt on a Windows machine.

Since we need to be able to compile and run the solution on Linux, MacOS and on CI/CD pipelines, an already built version (Content.zip) is provided here.