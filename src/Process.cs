namespace DotProj;

using Diag = System.Diagnostics;

public class Process
{
    public static bool Run(string cmd, string[] args, string dir="./")
    {
        var p = new Diag.ProcessStartInfo
        {
            FileName = cmd,
            WorkingDirectory = dir,
        };
        foreach (var arg in args)
        {
            p.ArgumentList.Add(arg);
        }
        using (var proc = Diag.Process.Start(p))
        {
            proc?.WaitForExit();
            return (proc?.ExitCode ?? -1) == 0;
        }
    }
}