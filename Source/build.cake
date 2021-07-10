///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

#tool "nuget:?package=NUnit.ConsoleRunner&version=3.12"
#addin nuget:?package=Cake.Yarn&version=0.4.8
#addin "Cake.FileHelpers&version=4.0.1"


var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var platform = Argument("platform", "Windows64");
var applicationVersion = "1.0.0.0";



///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("ContentBuild")
.Does(() => {
	using(var process = StartAndReturnProcess("../Content/Sceelix.Content.Windows.bat", new ProcessSettings{ WorkingDirectory = "../Content/" }))
	{
		process.WaitForExit();
		// This should output 0 as valid arguments supplied
		Information("Exit code: {0}", process.GetExitCode());
	}
});

Task("Clean")
.Does(() => {
	
	var dir = "../Build";
	if (DirectoryExists(dir)){
		DeleteDirectory(dir, new DeleteDirectorySettings {
		Recursive = true,
		Force = true
		});
	};
});

Task("RestoreNuget")
.Does(() => {
	NuGetRestore("./Sceelix.sln");
});


Task("ZipSamples")
.Does(() => {	
	Zip("../Extras", "../Extras/Samples.zip", GetFiles("../Extras/Samples/**/*.*"));
});

Task("ZipAPI")
.Does(() => {	
	var files = GetFiles("../Extras/API/**/*.{sln|cs|zip|csproj}").Concat(GetFiles("../Extras/API/**/ReadMeFirst.txt"));
	Zip("../Extras", "../Extras/API.zip", files);
});

Task("ZipUnityPlugin")
.Does(() => {
	CopyDirectory("./Sceelix.External.Unity3D/Assets/Sceelix", "../Extras/Unity Plugin/Unity Plugin/Sceelix");
	CopyFile("./Sceelix.External.Unity3D/Assets/ReadMe.txt", "../Extras/Unity Plugin/Unity Plugin/ReadMe.txt");
	Zip("../Extras/Unity Plugin", "../Extras/Unity Plugin.zip");
	DeleteDirectory("../Extras/Unity Plugin", new DeleteDirectorySettings { Recursive = true,Force = true});
});

Task("Setup")
.IsDependentOn("ZipSamples")
.IsDependentOn("ZipAPI")
.IsDependentOn("ZipUnityPlugin");


Task("Build")
.Does(() => {
	MSBuild("./Sceelix.sln", configurator =>
		configurator.SetConfiguration(configuration)
		.SetVerbosity(Verbosity.Minimal)
		.WithProperty("Platform", platform)
		);
});

Task("FullBuild")
.IsDependentOn("Clean")
.IsDependentOn("RestoreNuget")
.IsDependentOn("Setup")
.IsDependentOn("Build");


Task("Test")
.Does(() => {
	NUnit3($"./*.Tests/bin/{configuration}/**/*Tests.dll");
});


Task("FullBuildAndTest")
.IsDependentOn("FullBuild")
.IsDependentOn("Test");

Task("WebsiteDev")
.Does(() => {
	Yarn.FromPath("../Website").RunScript("start");
});

Task("SetVersion")
   .Does(() => {
	   ReplaceRegexInFiles("./**/AssemblyInfo.cs",
						  "(?<=^\\[assembly: AssemblyVersion\\(\\\")(.+?)(?=\\\")",
						  applicationVersion, 
						   System.Text.RegularExpressions.RegexOptions.Multiline);

	   ReplaceRegexInFiles("./**/AssemblyInfo.cs",
						  "(?<=^\\[assembly: AssemblyFileVersion\\(\\\")(.+?)(?=\\\")",
						  applicationVersion, 
						   System.Text.RegularExpressions.RegexOptions.Multiline);
   });


RunTarget(target);