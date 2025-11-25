using Diag = System.Diagnostics;

namespace DotProj;

public class Process
{
    public static void Run(string cmd, string[] args, string dir="./")
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
        }
    }
}