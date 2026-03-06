# Managing Prompts

## Prompt File Format

Each prompt is a **Markdown file** (`.md`) stored in the `PromptStorageMcp/Prompts/` folder.

Every file has two parts:
1. **YAML front-matter** — metadata between `---` delimiters
2. **Content** — the actual prompt template in Markdown

### Template

```markdown
---
name: my-prompt-name
description: A clear description so Copilot knows when to use this prompt
tags: keyword1, keyword2, keyword3
---

# Prompt Title

## Rules
- Rule 1
- Rule 2

## Examples
...your examples and patterns here...
```

### Front-matter fields

| Field | Required | Description |
|-------|----------|-------------|
| `name` | Yes | Unique identifier for the prompt. Use kebab-case (e.g., `unit-tests`) |
| `description` | Yes | **Critical** — Copilot uses this to decide when to use the prompt. Be specific and descriptive |
| `tags` | Yes | Comma-separated keywords for search. Include synonyms and related terms |

## Creating a New Prompt

### 1. Create the file

Add a new `.md` file in `PromptStorageMcp/Prompts/`:

```
PromptStorageMcp/Prompts/my-new-prompt.md
```

### 2. Add the front-matter

```markdown
---
name: error-handling
description: Rules for implementing consistent error handling with custom exceptions, logging, and proper HTTP responses
tags: errors, exceptions, error-handling, logging, try-catch
---
```

### 3. Write the prompt content

Write the rules, patterns, and examples you want Copilot to follow. Use Markdown formatting for readability. Be as specific as possible — the more detail you provide, the better Copilot will follow your patterns.

### 4. Rebuild

```bash
dotnet publish PromptStorageMcp -c Release
```

### 5. Restart Copilot Chat

Close and reopen the Copilot Chat window to reload the MCP server.

## Editing an Existing Prompt

1. Open the `.md` file in `PromptStorageMcp/Prompts/`
2. Make your changes
3. Rebuild: `dotnet publish PromptStorageMcp -c Release`
4. Restart Copilot Chat

## Deleting a Prompt

1. Delete the `.md` file from `PromptStorageMcp/Prompts/`
2. Rebuild: `dotnet publish PromptStorageMcp -c Release`
3. Restart Copilot Chat

## Tips for Writing Effective Prompts

### 1. Be specific in descriptions

❌ Bad: `"Template for tests"`
✅ Good: `"Template for generating unit tests following AAA pattern with xUnit and FluentAssertions"`

The description is what Copilot reads to decide whether to use the prompt.

### 2. Use comprehensive tags

Include synonyms and related terms:

```yaml
tags: testing, xunit, unit-tests, tdd, fluent-assertions, test, tests
```

### 3. Include code examples in the content

Copilot follows patterns better when it sees concrete examples:

```markdown
## Example

` ` `csharp
[Fact]
public void CalculateTotal_ShouldReturnZero_WhenCartIsEmpty()
{
    // Arrange
    var cart = new ShoppingCart();

    // Act
    var total = cart.CalculateTotal();

    // Assert
    total.Should().Be(0m);
}
` ` `
```

### 4. Use clear rules with bullet points

Copilot follows structured instructions better than prose:

```markdown
## Rules
- Use xUnit as the test framework
- Use FluentAssertions for all assertions
- Name tests: MethodName_ShouldBehavior_WhenCondition
- Include at least 3 test methods per public method
```

### 5. Specify what NOT to do

```markdown
## Anti-patterns (do NOT do this)
- Never use Thread.Sleep in tests
- Never test private methods directly
- Never hardcode connection strings
```

## Existing Prompts Reference

| File | Name | Purpose |
|------|------|---------|
| `unit-tests.md` | unit-tests | xUnit + FluentAssertions + AAA pattern |
| `api-endpoints.md` | api-endpoints | ASP.NET Core Minimal APIs |
| `code-review.md` | code-review | Code review checklist |
| `documentation.md` | documentation | XML docs and README rules |
| `refactoring.md` | refactoring | SOLID principles and clean code |

## Prompt Ideas

Here are some prompts you might want to create:

- **error-handling** — Custom exceptions, global error handling, logging patterns
- **database** — EF Core patterns, migrations, repository pattern
- **security** — Authentication, authorization, input validation
- **ci-cd** — GitHub Actions workflow templates
- **architecture** — Clean architecture, CQRS, vertical slices
- **logging** — Structured logging with Serilog/ILogger
- **docker** — Dockerfile and docker-compose patterns
