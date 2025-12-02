namespace DotProj;

internal class Project
{
    private static string[] PROJECT_COMMANDS = ["pack", "release"];

    private const string OUTPUT_NAME = "APP";
    private static string[] IGNORE_DIRS = [OUTPUT_NAME, "bin", "obj", "src"];
    private static string[] IGNORE_FILES = ["dotnet-tools.json"];
    private static string[] IGNORE_EXTENSIONS = [".sln", ".csproj"];


    public static void RunCommand(Action<string> log, string projectDir, string[] args)
    {
        if (IsProjectDirectory(projectDir))
        {
            _runCommand(log, projectDir, args);
        }
        else if (IsSolutionDirectory(projectDir))
        {
            foreach (var subDir in GetSlnProjectDirectories(projectDir))
            {
                _runCommand(log, subDir, args);
            }
        }
        else
        {
            log("[!] Directory isn't solution or project");
        }
    }

    private static void _runCommand(Action<string> log, string projectDir, string[] args)
    {
        // Read command name
        var commandName = args[0];

        // Check commands
        if (commandName == "pack")
        {
            if (args.Length < 2)
            {
                log("[!] Need also name of the local source");
                return;
            }
            var source = args[1];
            if (!PushNuget(projectDir, source))
            {
                log("[!] Wasn't able to publish package.");
                log("  - Make sure project is building ok");
                return;
            }
        }
        else if (commandName == "release")
        {
            if (!Release(log, projectDir))
            {
                log("[!] Can't release the project");
                return;
            }
        }
    }

    public static bool Release(Action<string> log, string projectDir)
    {
        // Build the project
        if (!publishProject(projectDir)) return false;

        // Get publish directory
        var publishDir = GetPublishDirectory(projectDir);
        if (publishDir == default)
        {
            log("[!] Can't find publish directory");
            return false;
        }

        // Make sure OUTPUT_NAME directory is created and empty
        var appDir = Path.Combine(projectDir, OUTPUT_NAME);
        if (Directory.Exists(appDir)) FileUtil.DeleteDirectory(appDir);
        Directory.CreateDirectory(appDir);

        // Copy Release Output to OUTPUT_NAME directory
        foreach (var file in Directory.GetFiles(publishDir))
        {
            var fileName = Path.GetFileName(file);
            File.Copy(file, Path.Combine(appDir, fileName));
        }

        // Copying the rest root files into the APP directory
        packRootDir(projectDir, appDir);

        return true;
    }

    private static void packRootDir(string projectDir, string targetDir)
    {
        // Copy directories
        foreach (var dir in Directory.GetDirectories(projectDir))
        {
            var name = Path.GetFileName(dir);
            if (IGNORE_DIRS.Contains(name)) continue;
            if (name.StartsWith(".")) continue;

            FileUtil.CopyDirectory(dir, Path.Combine(targetDir, name));
        }

        // Copy files
        foreach (var file in Directory.GetFiles(projectDir))
        {
            var name = Path.GetFileName(file);
            var ext = Path.GetExtension(file);
            if (IGNORE_FILES.Contains(name)) continue;
            if (IGNORE_EXTENSIONS.Contains(ext)) continue;
            if (name.StartsWith(".")) continue;

            File.Copy(file, Path.Combine(targetDir, name));
        }
    }

    public static bool PushNuget(string projectDir, string source)
    {
        // Build the project
        if (!packProject(projectDir)) return false;

        // Now check that .nupkg is found
        var binDir = Path.Combine(projectDir, "bin");
        var nupkgFile = FileUtil.FindFilesExt(binDir, [".nupkg"]).FirstOrDefault();
        if (nupkgFile == null) return false;

        // Output that file is found
        Console.WriteLine("Found nuget package: " + nupkgFile);

        // Publishing with dotnet
        return Process.Run("dotnet", ["nuget", "push", nupkgFile, "--source", source], dir: projectDir);
    }

    public static bool IsProjectDirectory(string dir)
    {
        var files = Directory.GetFiles(dir);
        if (files == null) return false;

        return files.Any(f => Path.GetExtension(f) == ".csproj");
    }

    public static bool IsSolutionDirectory(string dir)
    {
        var files = Directory.GetFiles(dir);
        if (files == null) return false;
        return files.Any(f => Path.GetExtension(f) == ".sln");
    }

    private static bool packProject(string directory)
    {
        return Process.Run("dotnet", ["pack", "--configuration", "release"], dir: directory);
    }

    private static bool buildProject(string directory)
    {
        return Process.Run("dotnet", ["build", "--configuration", "release"], dir: directory);
    }

    private static bool publishProject(string directory)
    {
        // --sc
        return Process.Run("dotnet", ["publish"], dir: directory);
    }

    public static IEnumerable<string> GetSlnProjectDirectories(string directory)
    {
        foreach (var dir in Directory.GetDirectories(directory))
        {
            // Get base name of the directory
            var dirName = Path.GetFileName(dir);
            if (dirName == null) continue;

            // Ignore tests
            if (dirName.ToLower().StartsWith("test")) continue;

            // Returning full directory path
            if (IsProjectDirectory(dir)) yield return dir;
        }
    }

    public static void CleanBuild(string directory)
    {
        var binDir = Path.Combine(directory, "bin");
        var objDir = Path.Combine(directory, "obj");

        FileUtil.DeleteDirectory(binDir);
        FileUtil.DeleteDirectory(objDir);
    }

    public static bool IsProjectCommand(string command)
    {
        return PROJECT_COMMANDS.Contains(command);
    }

    public static string? GetPublishDirectory(string projectDir)
    {
        var releaseDir = Path.Combine(projectDir, "bin", "Release");
        var netDir = Directory.GetDirectories(releaseDir).FirstOrDefault();
        if (netDir == default) return null;

        var publishDir = Path.Combine(netDir, "publish");
        if (!Directory.Exists(publishDir)) return null;

        return publishDir;
    }
}