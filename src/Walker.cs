namespace DotProj;

internal class Walker
{
    public static void DeleteDirectory(string directory)
    {
        if (!Directory.Exists(directory)) return;
        
        foreach (var dir in WalkDirectories(directory))
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }
            Directory.Delete(dir);
        }
    }

    public static IEnumerable<string> FindFiles(string directory, Func<string, bool> f)
    {
        foreach (var file in WalkFiles(directory))
        {
            if (f(file)) yield return file;
        }
    }

    public static IEnumerable<string> FindFilesExt(string directory, string[] exts)
    {
        return FindFiles(directory, f =>
        {
            var ext = Path.GetExtension(f);
            if (ext == null) return false;
            return exts.Contains(ext);
        });
    }

    public static IEnumerable<string> WalkFiles(string directory)
    {
        var directories = WalkDirectories(directory);
        foreach (var dir in directories)
        {
            foreach (var file in Directory.GetFiles(dir))
            {
                yield return file;
            }
        };
    }

    public static List<string> WalkDirectories(string directory)
    {
        var list = new List<string>();
        _walkDirectories(list, directory);
        return list;
    }

    private static void _walkDirectories(List<string> list, string directory)
    {
        var dirs = Directory.GetDirectories(directory);
        if (dirs == null) return;

        foreach (var dir in dirs)
        {
            list.Add(dir);
            _walkDirectories(list, dir);
        }

        list.Add(directory);
    }
}