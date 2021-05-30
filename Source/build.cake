///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

#tool "nuget:?package=NUnit.ConsoleRunner&version=3.12"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var platform = Argument("platform", "Windows64");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Default")
.Does(() => {
	Information("Hello Cake!");
});

Task("Content Build")
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
	Information("Cleanup");
});

Task("Build")
.IsDependentOn("Clean")
.Does(() => {
	MSBuild("./Sceelix.sln", configurator =>
		configurator.SetConfiguration(configuration)
		.SetVerbosity(Verbosity.Minimal)
		.WithProperty("Platform", platform)
		);

});

Task("Test")
.IsDependentOn("Build")
.Does(() => {
	//NUnit3($"./Sceelix.Points.Tests/bin/Release/net461/Sceelix.Points.Tests.dll");
	NUnit3($"./*.Tests/bin/{configuration}/**/*Tests.dll");
});

RunTarget(target);