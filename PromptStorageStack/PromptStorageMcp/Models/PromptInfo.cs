using System.ComponentModel;

namespace PromptStorageMcp.Models;

public sealed class PromptInfo
{
    [Description("Unique identifier for the prompt, used to retrieve it (e.g., 'unit-tests', 'api-endpoints')")]
    public required string Name { get; init; }

    [Description("Human-readable explanation of what the prompt does and when Copilot should use it")]
    public required string Description { get; init; }

    [Description("Keywords for searching and matching prompts to coding tasks (e.g., 'testing', 'xunit', 'api')")]
    public required string[] Tags { get; init; }

    [Description("The full prompt template content with rules, patterns, and examples that Copilot must follow")]
    public required string Content { get; init; }

    [Description("The markdown file name without extension that stores this prompt on disk")]
    public required string FileName { get; init; }
}
