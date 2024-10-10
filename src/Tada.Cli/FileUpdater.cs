namespace Tada.Cli;

public static class FileUpdater
{
    public static bool FileExists(string relativePath) 
    {
        return File.Exists(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
    }
    public static void UpdateContent(string filePath, string oldValue, string newValue)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        string content = File.ReadAllText(filePath);


        File.WriteAllText(filePath, content.Replace(oldValue, newValue));
    }

    public static void UpdateContent(string filePath, Dictionary<string, string> replacementValues)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        string content = File.ReadAllText(filePath);

        foreach (var item in replacementValues)
        {
            content = content.Replace(item.Key, item.Value);
        }

        File.WriteAllText(filePath, content);
    }

    public static void AddContentToBeginningOfFile(string filePath, string value)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File not found: " + filePath);
            return;
        }

        string content = File.ReadAllText(filePath);

        if (!content.Contains(value))
        {
            content = value + content;
        }

        File.WriteAllText(filePath, content);
    }
}