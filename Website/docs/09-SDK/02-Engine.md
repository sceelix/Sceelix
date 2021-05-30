# Extending the Engine

Sceelix was developed using C# 

Automatic loading and through reflection

attribute types
parameter types


The Sceelix Designer already packages a set of samples which you can use to understand how to put these libraries to use. On the top menu, click Help -> API Samples. If this is the first time, the files will be quickly extracted and the folder with the samples shown to you.

![](images/ApiSamples.png)

The “Sceelix.MyExternalApplication” folder contains a Visual Studio/MonoDevelop solution and a sample project. Before you can compile it, you may need to fix the references to the Sceelix Libraries. For instance, you should replace the references to Sceelix.Core and Sceelix.Geometry.3D with the ones contained in the “Bin” folder of your Sceelix installation folder (or .app, if you’re running the Mac version).

More instructions can be found on the sample project file ReadMe.txt and throughout the sample comments.