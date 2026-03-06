# Prompt Storage MCP Server

A personal prompt template library that integrates with GitHub Copilot in Visual Studio via the Model Context Protocol (MCP).

## How It Works

```
You ask Copilot ──► Copilot reads your MCP tools ──► Calls list_prompts / get_prompt
       │                                                        │
       │                    ◄───── Returns your template ◄──────┘
       │
       ▼
Copilot generates code following YOUR patterns
```

## Setup in Visual Studio

### 1. Build the project

```bash
dotnet build PromptStorageMcp
dotnet publish PromptStorageMcp -c Release
```

### 2. Register as MCP Server in Visual Studio

Create a file called **`mcp.json`** inside the **`.vs`** folder at the root of your solution directory. If the `.vs` folder doesn't exist, create it.

**File to create:** `<your-solution-root>\.vs\mcp.json`

```json
{
  "servers": {
    "prompt-storage": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "C:\\FULL\\PATH\\TO\\PromptStorageMcp\\bin\\Release\\net10.0\\PromptStorageMcp.dll"
      ]
    }
  }
}
```

> ⚠️ Replace the path with your actual full path to the published DLL.

#### Alternative config locations

| File location | Scope |
|---------------|-------|
| **`.vs\mcp.json`** | Current solution only **(recommended)** |
| `.vscode\mcp.json` | Current solution (also read by Visual Studio) |
| `%USERPROFILE%\.config\GitHub Copilot\mcp.json` | All solutions globally |

After creating the file, **close and reopen the solution** so Visual Studio picks up the new MCP server.

### 3. Enable Agent Mode and Activate the MCP Server

1. In Copilot Chat, switch to **Agent Mode** (toggle at the top of the chat panel).
2. Click the **🔧 tools icon** on the right side of the send message bar (next to the send button).
3. In the tools list, find **prompt-storage** and **check the box** to enable it.

> This allows Copilot to automatically discover and call your MCP tools.

### 4. Test it

In Copilot Chat (Agent Mode), type:

> "use your mcp tools" and autorize it.

The agent should call the `list_prompts` tool and return a list of all your available prompt templates with their names, descriptions, and tags.

## Available Tools

| Tool | Description |
|------|-------------|
| `list_prompts` | Lists all prompt templates with names, descriptions, and tags |
| `get_prompt` | Retrieves the full content of a specific prompt by name |
| `search_prompts` | Searches prompts by keyword across names, descriptions, and tags |
| `get_prompt_for_task` | Finds the best matching prompt for a given coding task description |

## Adding New Prompts

Just create a new `.md` file in the `Prompts/` folder with this format:

```markdown
---
name: my-new-prompt
description: What this prompt does (keep it clear for Copilot to understand)
tags: keyword1, keyword2, keyword3
---

# Your Prompt Title

## Rules
- Rule 1
- Rule 2

## Examples
...your examples here...
```

Then rebuild the project. That's it — Copilot will discover it automatically!

## Existing Prompt Templates

| Prompt | Description |
|--------|-------------|
| `unit-tests` | xUnit + FluentAssertions with AAA pattern |
| `api-endpoints` | ASP.NET Core Minimal APIs with validation |
| `code-review` | Comprehensive code review checklist |
| `documentation` | XML docs and README generation rules |
| `refactoring` | Clean code and SOLID refactoring patterns |

## Project Structure

```
PromptStorageMcp/
├── Program.cs                  # MCP server entry point (stdio transport)
├── Models/
│   └── PromptInfo.cs           # Prompt metadata model
├── Services/
│   └── PromptService.cs        # Reads and parses .md files
├── Tools/
│   └── PromptTools.cs          # MCP tools exposed to Copilot
└── Prompts/                    # Your prompt templates (add .md files here)
    ├── unit-tests.md
    ├── api-endpoints.md
    ├── code-review.md
    ├── documentation.md
    └── refactoring.md
```
