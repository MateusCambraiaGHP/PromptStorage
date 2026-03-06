# How It Works

## Overview

The Prompt Storage MCP Server is a **.NET 10 console application** that acts as a personal prompt library for GitHub Copilot. It uses the **Model Context Protocol (MCP)** to expose your prompt templates as tools that Copilot can call automatically.

## Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    Visual Studio                         │
│                                                          │
│  ┌─────────────┐         ┌────────────────────────┐      │
│  │ Copilot Chat │ ◄─────► │  Prompt Storage MCP   │      │
│  │ (Agent Mode) │  stdio  │  Server (.NET 10)     │      │
│  └─────────────┘         │                        │      │
│        │                  │  ┌──────────────────┐ │      │
│        │                  │  │ Tools            │ │      │
│        ▼                  │  │  - list_prompts  │ │      │
│  Generates code           │  │  - get_prompt    │ │      │
│  using YOUR patterns      │  │  - search_prompts│ │      │
│                           │  │  - get_prompt_   │ │      │
│                           │  │    for_task      │ │      │
│                           │  └──────────────────┘ │      │
│                           │           │           │      │
│                           │           ▼           │      │
│                           │  ┌──────────────────┐ │      │
│                           │  │ Prompts/ folder  │ │      │
│                           │  │  *.md files      │ │      │
│                           │  └──────────────────┘ │      │
│                           └────────────────────────┘      │
└──────────────────────────────────────────────────────────┘
```

## Communication Flow

### Step-by-step: What happens when you ask Copilot to generate code

```
1. You type in Copilot Chat:
   "Create unit tests for my OrderService class"

2. Copilot reads .github/copilot-instructions.md
   → Instructions say: "ALWAYS check the prompt storage MCP server first"

3. Copilot calls your MCP tool: list_prompts
   → Your server scans the Prompts/ folder
   → Returns all available templates with names, descriptions, tags

4. Copilot decides which prompt matches your request
   → Calls: get_prompt("unit-tests")

5. Your server reads unit-tests.md and returns the full template
   → AAA pattern, xUnit, FluentAssertions rules, naming conventions, etc.

6. Copilot generates code following YOUR exact patterns
   → Tests use your naming convention
   → Tests use your preferred libraries
   → Tests follow your structure rules
```

## Transport: stdio

The MCP server communicates with Visual Studio via **standard input/output (stdio)**:

- Visual Studio **launches** the server as a child process
- Copilot sends **JSON-RPC messages** via stdin
- The server responds via stdout
- When Copilot Chat is closed, the process is terminated

This means:
- ✅ No network ports needed
- ✅ No HTTP server to configure
- ✅ Fully local — nothing leaves your machine
- ✅ VS manages the process lifecycle automatically

## Project Structure

```
PromptStorageMcp/
├── Program.cs                  # Entry point — registers MCP server with stdio transport
├── Models/
│   └── PromptInfo.cs           # Data model for prompt metadata
├── Services/
│   └── PromptService.cs        # Reads .md files, parses YAML front-matter
├── Tools/
│   └── PromptTools.cs          # MCP tools that Copilot calls
└── Prompts/                    # Your prompt templates (markdown files)
    ├── unit-tests.md
    ├── api-endpoints.md
    ├── code-review.md
    ├── documentation.md
    └── refactoring.md
```

## Key Components

### Program.cs

Registers the MCP server with the stdio transport and discovers tools from the assembly:

```csharp
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
```

### PromptTools.cs

Contains 4 tools decorated with `[McpServerTool]` and `[Description]`. The descriptions are critical — they tell Copilot **when** and **why** to call each tool.

### PromptService.cs

Reads `.md` files from the `Prompts/` folder and parses the YAML front-matter (between `---` delimiters) to extract metadata like `name`, `description`, and `tags`.

### copilot-instructions.md

A special file at `.github/copilot-instructions.md` that Copilot reads automatically. It instructs Copilot to always check your MCP server before generating code.
