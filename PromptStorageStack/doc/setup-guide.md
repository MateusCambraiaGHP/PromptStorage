# Setup Guide

## Prerequisites

- **Visual Studio 2022 version 17.14 or later**
- **GitHub Copilot** extension installed and active
- **.NET 10 SDK** installed

## Step 1: Build the MCP Server

Open a terminal in the solution root and run:

```bash
dotnet publish PromptStorageMcp -c Release
```

This creates the executable at:

```
PromptStorageMcp\bin\Release\net10.0\PromptStorageMcp.dll
```

## Step 2: Register the MCP Server in Visual Studio

### Option A: Solution-level config (recommended)

Create a file at `.vs/mcp.json` in the solution root:

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

### Option B: Global config (works across all solutions)

Create the file at:

```
%USERPROFILE%\.config\GitHub Copilot\mcp.json
```

Same JSON content as above. This makes your prompt library available in **every** solution you open.

### Option C: Via Visual Studio UI

1. Open **Tools → Options**
2. Search for **"MCP"** in the search bar
3. Navigate to **GitHub Copilot → MCP Servers**
4. Click **Edit in JSON**
5. Paste the JSON config above

### Config locations (priority order)

| Location | Scope |
|----------|-------|
| `.vs/mcp.json` | Current solution only |
| `.vscode/mcp.json` | Current solution (also read by VS) |
| `%USERPROFILE%\.config\GitHub Copilot\mcp.json` | All solutions (global) |

## Step 3: Reload the Solution

Close and reopen the solution so Visual Studio picks up the new MCP configuration.

## Step 4: Switch to Agent Mode

In the Copilot Chat panel:

1. Look at the **top of the chat panel**
2. You'll see a mode selector (Ask / Edit / **Agent**)
3. Click **Agent**

> MCP tools are **only** available in Agent Mode.

## Step 5: Configure Copilot Instructions File

This is the key step that makes Copilot **automatically call your MCP tools** before writing any code.

Create a file at `.github/copilot-instructions.md` in your solution root. Copilot reads this file automatically on every request:

```markdown
# Copilot Custom Instructions

Before generating any code, tests, documentation, API endpoints, or performing refactoring,
ALWAYS check the user's personal prompt storage MCP server for a matching template:

1. Call `list_prompts` to see all available prompt templates
2. Call `get_prompt_for_task` with the task description to find the best matching template
3. Follow the rules and patterns defined in the returned prompt template exactly

If no matching prompt is found, proceed with default behavior.

## MANDATORY — Always Read First
ALWAYS call `get_prompt("code-patterns")` before writing ANY code.
The `code-patterns` prompt contains global rules (naming, comments, usings, indentation, planning)
that apply to EVERY task, regardless of what the user asked for.
```

### What this does

The instructions file tells Copilot to follow this flow on **every** request:

```
1. ALWAYS call get_prompt("code-patterns")
   → Loads global rules: no comments, no unused usings,
     descriptive names, indentation, PLAN BEFORE CODING

2. Call list_prompts
   → Discovers all your available templates

3. Call get_prompt_for_task("your request")
   → Finds the best matching template (unit-tests, api-endpoints, etc.)

4. Follow the returned template rules exactly
   → Generates code using YOUR patterns
```

### Why `code-patterns` is loaded first

The `code-patterns` prompt contains your **mandatory rules** that apply to ALL code:

| Rule | What it enforces |
|------|-----------------|
| 🔴 Plan before coding | Never write code without discussing the approach first |
| No comments | Never add comments unless extremely necessary |
| No unused usings | Always clean up unused imports |
| Descriptive names | `customer =>` not `c =>`, `order =>` not `x =>` |
| Indentation | 3+ parameters → each on its own line |

These rules are applied **on top of** any task-specific prompt (unit-tests, api-endpoints, etc.).

### Where to place the file

| Location | Scope |
|----------|-------|
| `.github/copilot-instructions.md` | Per repository (recommended) |

> This file is automatically read by Copilot in Visual Studio. No extra configuration needed.

## Step 6: Verify Tools Are Registered

In Agent Mode, click the **tools icon** (🔨) at the bottom of the chat input.
You should see your 4 tools listed:

- `list_prompts` (prompt-storage)
- `get_prompt` (prompt-storage)
- `search_prompts` (prompt-storage)
- `get_prompt_for_task` (prompt-storage)

## Step 7: Test It

Type in Copilot Chat:

```
List my prompts
```

Copilot should call your `list_prompts` tool and display all your templates.

Then try:

```
Create unit tests for my OrderService class
```

Copilot should:
1. Call `get_prompt_for_task("unit tests")` or `get_prompt("unit-tests")`
2. Read your unit test template
3. Generate tests following your AAA pattern, naming conventions, and library choices

## Updating After Changes

Every time you add, edit, or delete a prompt `.md` file:

```bash
dotnet publish PromptStorageMcp -c Release
```

Then restart the Copilot Chat window (close and reopen it) to reload the MCP server.
