public enum IncludeType
{
    Quotes,
    AngleBrackets
}

public static class FileUtils
{
    public static void Init(string include)
    {
        includeDirectories = include.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
    private static string[] includeDirectories;

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
