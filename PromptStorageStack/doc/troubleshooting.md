# Troubleshooting

## Common Issues

---

### Copilot doesn't call my MCP tools

**Symptoms:** You ask Copilot to generate code, but it doesn't call `list_prompts` or `get_prompt`.

**Causes and fixes:**

| Check | Fix |
|-------|-----|
| Not in Agent Mode | Switch from "Ask" or "Edit" to **Agent** mode at the top of Copilot Chat |
| Tools not enabled | Click the **tools icon** (🔨) in the chat input and ensure your tools have checkmarks |
| MCP config not loaded | Close and reopen the solution after adding `.vs/mcp.json` |
| Server not built | Run `dotnet publish PromptStorageMcp -c Release` |
| Wrong DLL path | Verify the path in `.vs/mcp.json` matches your actual publish output |

---

### "No prompts found in the prompt library"

**Cause:** The server can't find the `Prompts/` folder relative to the DLL.

**Fix:** Make sure the `.csproj` includes:

```xml
<ItemGroup>
  <Content Include="Prompts\*.md">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

Then rebuild: `dotnet publish PromptStorageMcp -c Release`

Verify the `Prompts/` folder exists next to the DLL:

```
bin\Release\net10.0\
├── PromptStorageMcp.dll
└── Prompts\
    ├── unit-tests.md
    ├── api-endpoints.md
    └── ...
```

---

### MCP tools show up but Copilot doesn't use them automatically

**Cause:** Copilot decides when to call tools based on relevance. It may not always call them.

**Fixes:**

1. **Check `.github/copilot-instructions.md`** exists at the solution root with:
   ```markdown
   Before generating any code, always check the user's prompt storage
   MCP server for a matching template.
   ```

2. **Be explicit** in your chat message:
   > "Use my prompt storage to find a pattern for unit tests, then generate them"

3. **Improve tool descriptions** in `PromptTools.cs` — phrases like "ALWAYS call this before generating code" help Copilot prioritize the tool.

---

### Visual Studio doesn't recognize `.vs/mcp.json`

**Cause:** Your Visual Studio version might be older than 17.14.

**Check:** Go to **Help → About Microsoft Visual Studio** and verify the version.

**Fix:** Update to Visual Studio 2022 version **17.14 or later**.

**Alternative:** Try placing the file at `.vscode/mcp.json` instead — Visual Studio also reads this location.

---

### Server crashes or doesn't start

**Debug steps:**

1. Test the server manually in a terminal:
   ```bash
   dotnet PromptStorageMcp\bin\Release\net10.0\PromptStorageMcp.dll
   ```
   The process should start and wait for input (no output is normal — it communicates via stdio).

2. Press `Ctrl+C` to stop it.

3. If it throws an error, fix the issue and rebuild.

---

### Changes to `.md` files aren't reflected

**Cause:** The MCP server reads from the `bin/Release` output folder, not the source folder.

**Fix:** After any change to prompt files:

```bash
dotnet publish PromptStorageMcp -c Release
```

Then restart Copilot Chat (close and reopen the chat panel).

---

### YAML front-matter not parsed correctly

**Cause:** Format issues in the `.md` file header.

**Rules:**
- The file must start with `---` on the **very first line** (no blank lines before it)
- The closing `---` must be on its own line
- Use `key: value` format (space after colon)
- Tags must be comma-separated: `tags: tag1, tag2, tag3`

**Correct format:**
```markdown
---
name: my-prompt
description: My description here
tags: tag1, tag2, tag3
---

Content starts here...
```

**Incorrect format (will fail):**
```markdown

---
name:my-prompt
description:Missing space after colon
tags:tag1,tag2
---
```

---

## Getting Help

If you encounter an issue not listed here:

1. Check the **Output window** in Visual Studio (**View → Output**) and select the relevant log source
2. Test your MCP server manually from the command line
3. Verify your `.vs/mcp.json` has valid JSON (no trailing commas, correct escaping of backslashes)
