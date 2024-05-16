public enum IncludeType
{
    Quotes,
    AngleBrackets
}

public static class FileUtils
{
    private static string[] includeDirectories;
    static FileUtils()
    {
        var output = File.ReadAllText("IncludePaths.txt");
        includeDirectories = output.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public static string ReadFile(string fileName, IncludeType type)
    {
        foreach (var directory in includeDirectories)
        {
            var fullPath = Path.Combine(directory, fileName);
            if (File.Exists(fullPath)) return File.ReadAllText(fullPath);
        }
        throw new FileNotFoundException();
    }
}
