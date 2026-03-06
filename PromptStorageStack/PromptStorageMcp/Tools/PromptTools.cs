using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;
using PromptStorageMcp.Services;

namespace PromptStorageMcp.Tools;

[McpServerToolType]
public sealed class PromptTools
{
    private readonly PromptService _promptService = new();

    [McpServerTool(Name = "list_prompts"),
     Description(
        "Lists ALL prompt templates the user has stored in their personal prompt library. " +
        "ALWAYS call this tool first before generating any code, tests, documentation, " +
        "API endpoints, or refactoring, to check if the user has a preferred pattern or template. " +
        "Returns the name, description, and tags of every available prompt.")]
    public string ListPrompts()
    {
        var prompts = _promptService.GetAllPrompts();

        if (prompts.Count == 0)
            return "No prompts found in the prompt library.";

        var sb = new StringBuilder();
        sb.AppendLine($"Found {prompts.Count} prompt(s) in the library:\n");

        foreach (var prompt in prompts)
        {
            sb.AppendLine($"- **{prompt.Name}**");
            sb.AppendLine($"  Description: {prompt.Description}");
            sb.AppendLine($"  Tags: {string.Join(", ", prompt.Tags)}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    [McpServerTool(Name = "get_prompt"),
     Description(
        "Retrieves the FULL content of a specific prompt template by name. " +
        "Call this after list_prompts to get the complete template/instructions " +
        "that the user wants you to follow when generating code. " +
        "You MUST follow the rules and patterns defined in the returned prompt.")]
    public string GetPrompt(
        [Description("The exact name of the prompt to retrieve (e.g., 'unit-tests', 'api-endpoints')")]
        string name)
    {
        var prompt = _promptService.GetPromptByName(name);

        if (prompt is null)
            return $"Prompt '{name}' not found. Call list_prompts to see available prompts.";

        var sb = new StringBuilder();
        sb.AppendLine($"# Prompt: {prompt.Name}");
        sb.AppendLine($"**Description:** {prompt.Description}");
        sb.AppendLine($"**Tags:** {string.Join(", ", prompt.Tags)}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(prompt.Content);

        return sb.ToString();
    }

    [McpServerTool(Name = "search_prompts"),
     Description(
        "Searches the user's prompt library by keyword. " +
        "Use this when the user asks for something and you want to find " +
        "the most relevant prompt template. Searches across names, descriptions, and tags.")]
    public string SearchPrompts(
        [Description("The keyword to search for (e.g., 'test', 'api', 'refactor', 'documentation')")]
        string query)
    {
        var results = _promptService.SearchPrompts(query);

        if (results.Count == 0)
            return $"No prompts found matching '{query}'.";

        var sb = new StringBuilder();
        sb.AppendLine($"Found {results.Count} prompt(s) matching '{query}':\n");

        foreach (var prompt in results)
        {
            sb.AppendLine($"- **{prompt.Name}**: {prompt.Description}");
            sb.AppendLine($"  Tags: {string.Join(", ", prompt.Tags)}");
        }

        return sb.ToString();
    }

    [McpServerTool(Name = "get_prompt_for_task"),
     Description(
        "Automatically finds and returns the best prompt template for a given coding task. " +
        "Call this with a task description like 'unit tests', 'create API', 'refactor code', " +
        "'write documentation' and it will return the full prompt template to follow. " +
        "ALWAYS call this before starting any code generation task.")]
    public string GetPromptForTask(
        [Description("The coding task description (e.g., 'unit tests', 'api endpoint', 'refactoring', 'documentation')")]
        string task)
    {
        var results = _promptService.SearchPrompts(task);

        if (results.Count == 0)
            return $"No prompt template found for task '{task}'. Proceed with default behavior.";

        var best = results[0];
        var sb = new StringBuilder();
        sb.AppendLine($"# Template for: {task}");
        sb.AppendLine($"**Using prompt:** {best.Name}");
        sb.AppendLine($"**Description:** {best.Description}");
        sb.AppendLine();
        sb.AppendLine("## Instructions to follow:");
        sb.AppendLine();
        sb.AppendLine(best.Content);

        return sb.ToString();
    }
}
