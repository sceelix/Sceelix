using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sceelix.Core.Annotations;
using Sceelix.MyNewEngineLibrary;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Sceelix.MyNewEngineLibrary")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Sceelix.MyNewEngineLibrary")]
[assembly: AssemblyCopyright("Copyright © MyAuthorName 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f3503990-a824-47a7-94bd-d60ae2d18c9c")]

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


// We need to add this tag have Sceelix recognize it as a library that
// holds new Entities, Procedures and Parameters.
[assembly: EngineLibrary]