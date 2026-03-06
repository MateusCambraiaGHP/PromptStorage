using PromptStorageMcp.Models;

namespace PromptStorageMcp.Services;

public sealed class PromptService
{
    private readonly string _promptsFolder;

    public PromptService()
    {
        _promptsFolder = Path.Combine(AppContext.BaseDirectory, "Prompts");
    }

    public PromptService(string promptsFolder)
    {
        _promptsFolder = promptsFolder;
    }

    public List<PromptInfo> GetAllPrompts()
    {
        if (!Directory.Exists(_promptsFolder))
            return [];

        var prompts = new List<PromptInfo>();

        foreach (var file in Directory.GetFiles(_promptsFolder, "*.md"))
        {
            var prompt = ParsePromptFile(file);
            if (prompt is not null)
                prompts.Add(prompt);
        }

        return prompts;
    }

    public PromptInfo? GetPromptByName(string name)
    {
        var all = GetAllPrompts();
        return all.FirstOrDefault(p =>
            p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public PromptInfo? GetPromptByTag(string tag)
    {
        var all = GetAllPrompts();
        return all.FirstOrDefault(p =>
            p.Tags.Any(t => t.Contains(tag, StringComparison.OrdinalIgnoreCase)));
    }

    public List<PromptInfo> SearchPrompts(string query)
    {
        var all = GetAllPrompts();
        return all.Where(p =>
            p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            p.Tags.Any(t => t.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    private static PromptInfo? ParsePromptFile(string filePath)
    {
        var text = File.ReadAllText(filePath);
        var fileName = Path.GetFileNameWithoutExtension(filePath);

        var name = fileName;
        var description = "";
        var tags = Array.Empty<string>();
        var content = text;

        if (text.StartsWith("---"))
        {
            var endIndex = text.IndexOf("---", 3);
            if (endIndex > 0)
            {
                var frontMatter = text[3..endIndex].Trim();
                content = text[(endIndex + 3)..].Trim();

                foreach (var line in frontMatter.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex < 0) continue;

                    var key = line[..colonIndex].Trim().ToLowerInvariant();
                    var value = line[(colonIndex + 1)..].Trim();

                    switch (key)
                    {
                        case "name":
                            name = value;
                            break;
                        case "description":
                            description = value;
                            break;
                        case "tags":
                            tags = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            break;
                    }
                }
            }
        }

        return new PromptInfo
        {
            Name = name,
            Description = description,
            Tags = tags,
            Content = content,
            FileName = fileName
        };
    }
}
