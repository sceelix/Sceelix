using System.Reflection;
using System.Runtime.InteropServices;
using Sceelix.Annotations;
using Sceelix.Core.Annotations;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Sceelix.Meshes")]
[assembly: AssemblyDescription("Sceelix Engine library that defines and manipulates geometric meshes.")]
[assembly: AssemblyConfiguration("Sceelix")]
[assembly: AssemblyCompany("Sceelix")]
[assembly: AssemblyProduct("Sceelix.Meshes")]
[assembly: AssemblyCopyright("Copyright © Sceelix Project 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyWebsite("https://sceelix.com")]
[assembly: AssemblyTags("MeshEntity", "Actors")]


#if WINDOWS32
[assembly: AssemblyNativeReferences("Assimp32.dll")]
#elif WINDOWS64

[assembly: AssemblyNativeReferences("Assimp64.dll")]
#elif MACOS
[assembly: AssemblyNativeReferences("../Resources/libassimp.dylib")]
#elif LINUX
[assembly: AssemblyNativeReferences("libassimp.so")]
#endif

[assembly: EngineLibrary]


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: Guid("76cf0233-8942-4a49-af32-f7e0552e6eb7")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]