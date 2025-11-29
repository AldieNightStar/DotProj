using System.Text;

namespace DotProj;

class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            showUsage();
            return;
        }
        var name = args[0];

        if (Project.IsProjectCommand(name))
        {
            Project.RunCommand(Console.WriteLine, ".", args);
        }
        else
        {
            var type = args.ElementAtOrDefault(1) ?? "console";
            createProject(args[0], type);
        }
    }

    private static void showUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotproj [name] [type]");
        Console.WriteLine("\nExample:");
        Console.WriteLine("  dotproj project1          - Create console project");
        Console.WriteLine("  dotproj project1 console  - Create console project");
        Console.WriteLine("  dotproj project1 classlib - Create classlib project");
        Console.WriteLine("");
        Console.WriteLine("Inside Project:");
        Console.WriteLine("  dotproj pack sourceName   - Push .nupkg package to local source");
        Console.WriteLine("  dotproj release           - Release dotnet program in it's zip folder");
    }

    private static void createProject(string name, string type)
    {
        if (name.ToLower().StartsWith("test"))
        {
            Console.WriteLine("[!] Project should not start with 'test' word");
            return;
        }
        var projectDir = Directory.CreateDirectory(name).FullName;

        // Create new dotnet build
        Process.Run("dotnet", ["new", "sln"], dir: projectDir);

        // Add projects
        Process.Run("dotnet", ["new", type, "-n", name], dir: projectDir);
        Process.Run("dotnet", ["new", "xunit", "-n", "Test"], dir: projectDir);

        // Add into sln
        Process.Run("dotnet", ["sln", "add", name], dir: projectDir);
        Process.Run("dotnet", ["sln", "add", "Test"], dir: projectDir);

        // Make Test aware of project
        Process.Run("dotnet", ["add", "Test", "reference", name], dir: projectDir);

        // Create folders
        var src = Directory.CreateDirectory(Path.Combine(projectDir, name, "src")).FullName;

        // Move all CS files
        foreach (var csFile in findCsFiles(Path.Combine(projectDir, name)))
        {
            var baseName = Path.GetFileName(csFile);
            File.Move(csFile, Path.Combine(src, baseName));
        }

        // Git init there
        Process.Run("git", ["init"], dir: projectDir);

        // Create .gitignore
        File.WriteAllText(Path.Combine(projectDir, ".gitignore"), generateGitIgnore([name, "Test"]));

        // Git commit initial message
        Process.Run("git", ["add", "."], dir: projectDir);
        Process.Run("git", ["commit", "-m", "Initial"], dir: projectDir);
    }

    private static string generateGitIgnore(string[] projects)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("obj/**");
        sb.AppendLine("bin/**");
        foreach (var name in projects)
        {
            sb.AppendLine($"{name}/obj/**");
            sb.AppendLine($"{name}/bin/**");
        }
        return sb.ToString();
    }

    private static IEnumerable<string> findCsFiles(string dir)
    {
        foreach (var file in Directory.GetFiles(dir))
        {
            if (file.EndsWith(".cs"))
            {
                yield return file;
            }
        }
    }
}