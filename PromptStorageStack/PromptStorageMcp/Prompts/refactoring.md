---
name: refactoring
description: Rules and patterns for refactoring code to improve quality, readability, and maintainability using SOLID principles
tags: refactoring, clean-code, solid, patterns, maintainability
---

# Refactoring Rules

> **Note:** Always follow the `code-patterns` prompt rules when refactoring (naming, comments, usings, indentation).

## SOLID Principles

## Principles to Apply
- **Single Responsibility** — each class/method does ONE thing
- **Open/Closed** — open for extension, closed for modification
- **Dependency Inversion** — depend on abstractions, not concretions
- **DRY** — eliminate duplicated logic
- **KISS** — keep it simple

## Method Refactoring
- Max 20 lines per method
- Max 3 parameters — use an options/request object for more
- Extract private methods for logical blocks
- Replace nested conditionals with guard clauses or pattern matching

## Class Refactoring
- Max 200 lines per class
- Extract interfaces for dependencies
- Use composition over inheritance
- Move related methods into dedicated service classes

## Common Refactoring Patterns
1. **Extract Method** — long method → smaller named methods
2. **Extract Class** — class doing too much → split responsibilities
3. **Replace Conditional with Polymorphism** — switch/if chains → strategy pattern
4. **Introduce Parameter Object** — many params → single DTO
5. **Replace Magic Numbers** — hardcoded values → named constants

## Output Format
When refactoring, always:
1. Explain WHAT you're changing and WHY
2. Show the before and after
3. Ensure all existing tests still pass
4. Do NOT change behavior — only structure
