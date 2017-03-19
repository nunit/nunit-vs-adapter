//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

//////////////////////////////////////////////////////////////////////
// SET PACKAGE VERSION
//////////////////////////////////////////////////////////////////////

var version = "2.1.1-b2";
var modifier = "";

var dbgSuffix = configuration == "Debug" ? "-dbg" : "";
var packageVersion = version + modifier + dbgSuffix;

if (BuildSystem.IsRunningOnAppVeyor)
{
	var tag = AppVeyor.Environment.Repository.Tag;

	if (tag.IsTag)
	{
		packageVersion = tag.Name;
	}
	else
	{
		var buildNumber = AppVeyor.Environment.Build.Number.ToString("00000");
		var branch = AppVeyor.Environment.Repository.Branch;
		var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

		if (branch == "master" && !isPullRequest)
		{
			packageVersion = version + "-dev-" + buildNumber + dbgSuffix;
		}
		else
		{
			var suffix = "-ci-" + buildNumber + dbgSuffix;

			if (isPullRequest)
				suffix += "-pr-" + AppVeyor.Environment.PullRequest.Number;
			else
				suffix += "-" + branch;

			// Nuget limits "special version part" to 20 chars. Add one for the hyphen.
			if (suffix.Length > 21)
				suffix = suffix.Substring(0, 21);

			suffix = suffix.Replace(".", "");

			packageVersion = version + suffix;
		}
	}

	AppVeyor.UpdateBuildVersion(packageVersion);
}

var packageName = "NUnitTestAdapter-" + packageVersion;

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var PACKAGE_DIR = PROJECT_DIR + "package/";
var PACKAGE_IMAGE_DIR = PACKAGE_DIR + packageName + "/";
var TOOLS_DIR = PROJECT_DIR + "tools/";
var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";
var DEMO_BIN_DIR = PROJECT_DIR + "src/NUnitTestDemo/NUnitTestDemo/bin/" + configuration + "/";
var VSIXDIR = PROJECT_DIR+"src/NUnitTestAdapterInstall/bin/"+configuration+"/";
var TEST_BIN_DIR = PROJECT_DIR+"src/NUnitTestAdapterTests/bin/"+configuration+"/";

// Solutions
var ADAPTER_SOLUTION = PROJECT_DIR + "NUnitTestAdapter.sln";
var DEMO_SOLUTION = PROJECT_DIR + "src/NUnitTestDemo/NUnitTestDemo.sln";

// Test Runners
var NUNIT_CONSOLE = TOOLS_DIR + "NUnit.Runners/tools/nunit-console.exe";

// Test Assemblies
var ADAPTER_TESTS = TEST_BIN_DIR + "NUnit.VisualStudio.TestAdapter.Tests.dll";
var DEMO_TESTS = DEMO_BIN_DIR + "NUnitTestDemo.dll";

// Custom settings for VSTest
var VSTestCustomSettings = new VSTestSettings()
{
	ArgumentCustomization = args => args.Append("/TestAdapterPath:" + BIN_DIR)
};

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(BIN_DIR);
	CleanDirectory(DEMO_BIN_DIR);
});


//////////////////////////////////////////////////////////////////////
// INITIALIZE FOR BUILD
//////////////////////////////////////////////////////////////////////

Task("NuGetRestore")
    .Does(() =>
{
    NuGetRestore(ADAPTER_SOLUTION);
	NuGetRestore(DEMO_SOLUTION);
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("NuGetRestore")
    .Does(() =>
    {
		BuildSolution(ADAPTER_SOLUTION, configuration);
		BuildSolution(DEMO_SOLUTION, configuration);
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("TestAdapterUsingConsole")
	.IsDependentOn("Build")
	.Does(() =>
	{
        int rc = StartProcess(
			NUNIT_CONSOLE,
			new ProcessSettings()
			{
				Arguments = ADAPTER_TESTS
			});

		if (rc != 0)
		{
			var message = rc > 0
				? string.Format("Test failure: {0} tests failed", rc)
				: string.Format("Test exited with rc = {0}", rc);

			throw new CakeException(message);
		}
	});

Task("TestAdapterUsingVSTest")
	.IsDependentOn("Build")
	.Does(() =>
	{
		VSTest(ADAPTER_TESTS, VSTestCustomSettings);
	});

Task("TestDemo")
	.IsDependentOn("Build")
	.Does(() =>
	{
		try
		{
			VSTest(DEMO_TESTS, VSTestCustomSettings);
		}
		catch(Exception ex)
		{
			Information("\nNOTE: Demo tests failed as expected.");
			Information("This is normally not an error.\n");
		}
	});

//////////////////////////////////////////////////////////////////////
// PACKAGE
//////////////////////////////////////////////////////////////////////

Task("CreatePackageDir")
	.Does(() =>
	{
		CreateDirectory(PACKAGE_DIR);
	});

Task("CreateWorkingImage")
	.IsDependentOn("Build")
	.IsDependentOn("CreatePackageDir")
	.Does(() =>
	{
		CreateDirectory(PACKAGE_IMAGE_DIR);
		CleanDirectory(PACKAGE_IMAGE_DIR);

		CopyFileToDirectory("LICENSE.txt", PACKAGE_IMAGE_DIR);

		var binFiles = new FilePath[]
		{
			BIN_DIR + "NUnit.VisualStudio.TestAdapter.dll",
			BIN_DIR + "Mono.Cecil.dll",
			BIN_DIR + "Mono.Cecil.Pdb.dll",
			BIN_DIR + "Mono.Cecil.Mdb.dll",
			BIN_DIR + "Mono.Cecil.Rocks.dll",
			BIN_DIR + "nunit.core.dll",
			BIN_DIR + "nunit.core.interfaces.dll",
			BIN_DIR + "nunit.util.dll"
		};

		var binDir = PACKAGE_IMAGE_DIR + "bin/";
		CreateDirectory(binDir);
		CopyFiles(binFiles, binDir);
	});

Task("PackageZip")
	.IsDependentOn("CreateWorkingImage")
	.Does(() =>
	{
		Zip(PACKAGE_IMAGE_DIR, File(PACKAGE_DIR + packageName + ".zip"));
	});

Task("PackageNuGet")
	.IsDependentOn("CreateWorkingImage")
	.Does(() => 
	{
        NuGetPack("nuget/NUnitVisualStudioTestAdapter.nuspec", new NuGetPackSettings()
        {
            Version = packageVersion,
            BasePath = PACKAGE_IMAGE_DIR,
            OutputDirectory = PACKAGE_DIR
        });
	});

Task("PackageVsix")
	.IsDependentOn("Build")
	.IsDependentOn("CreatePackageDir")
	.Does(() =>
	{
        System.Console.WriteLine("Packaging the vsix");
		CopyFile(
			VSIXDIR + "NUnitTestAdapter.vsix", 
			PACKAGE_DIR + packageName + ".vsix");
	});

//////////////////////////////////////////////////////////////////////
// HELPER METHODS
//////////////////////////////////////////////////////////////////////

void BuildSolution(string solutionPath, string configuration)
{
	MSBuild(solutionPath, new MSBuildSettings()
		.SetConfiguration(configuration)
        .SetMSBuildPlatform(MSBuildPlatform.x86)
		.SetVerbosity(Verbosity.Minimal)
		.SetNodeReuse(false)
	);
}

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Rebuild")
    .IsDependentOn("Clean")
	.IsDependentOn("Build");

Task("Test")
	.IsDependentOn("TestAdapterUsingConsole")
	.IsDependentOn("TestAdapterUsingVSTest");

Task("Package")
	.IsDependentOn("Build")
	.IsDependentOn("PackageZip")
	.IsDependentOn("PackageNuGet")
	.IsDependentOn("PackageVsix");

Task("Appveyor")
	.IsDependentOn("Build")
	.IsDependentOn("Test")
	.IsDependentOn("Package");

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
