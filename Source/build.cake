///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

#tool "nuget:?package=NUnit.ConsoleRunner&version=3.12"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var platform = Argument("platform", "Windows64");

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

Task("Setup")
.IsDependentOn("ZipSamples")
.IsDependentOn("ZipAPI");


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
.IsDependentOn("ContentBuild")
.IsDependentOn("Setup");
.IsDependentOn("Build");


Task("Test")
.Does(() => {
	NUnit3($"./*.Tests/bin/{configuration}/**/*Tests.dll");
});


Task("FullBuildAndTest")
.IsDependentOn("FullBuild")
.IsDependentOn("Test");

RunTarget(target);