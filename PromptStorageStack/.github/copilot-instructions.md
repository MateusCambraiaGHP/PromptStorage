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
