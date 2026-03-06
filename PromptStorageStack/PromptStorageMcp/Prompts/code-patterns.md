---
name: code-patterns
description: MANDATORY code patterns and style rules that MUST be followed in ALL code generation. Covers planning, comments, usings, naming, indentation, and formatting. ALWAYS read this prompt before writing ANY code.
tags: patterns, code-style, naming, formatting, comments, usings, indentation, rules, always, mandatory, global
---

# ⚠️ MANDATORY CODE PATTERNS — ALWAYS FOLLOW

These rules apply to ALL code generation, refactoring, and modifications. No exceptions.

---

## 🔴 RULE #1 — PLAN BEFORE CODING
**NEVER write code without planning and discussing first.**

Before making ANY code change:
1. Analyze the problem
2. Present the options and trade-offs to the user
3. Discuss and agree on the best pattern together
4. Only THEN start writing code

This is the **most important rule**. Do NOT skip it.

---

## Comments
- **NEVER add comments** unless it is extremely necessary to explain complex logic
- If you see an existing unnecessary comment, **remove it**
- Code should be self-explanatory through good naming and structure
- The only acceptable comments: `//Arrange`, `//Act`, `//Assert` in tests

---

## Usings
- **NEVER leave unused usings** — remove them immediately
- If you see unused usings in existing code being edited, clean them up

---

## Variable and Parameter Naming
- **NEVER use short or abbreviated names** — not in LINQ, lambdas, loops, or anywhere
- Always use descriptive names that explain what the variable represents

❌ Bad:
```csharp
var result = customers.Where(c => c.IsActive).Select(c => c.Name);
items.ForEach(x => x.Process());
orders.Any(o => o.IsPaid);
var e = new Exception();
```

✅ Good:
```csharp
var result = customers.Where(customer => customer.IsActive).Select(customer => customer.Name);
items.ForEach(item => item.Process());
orders.Any(order => order.IsPaid);
var exception = new Exception();
```

---

## Indentation — More Than 3 Parameters
When a method has **more than 3 parameters**, break each parameter into its own line.
This applies to **declarations**, **calls**, and **constructor injection**:

Method declaration:
```csharp
public CreateReservationHandler(
    IReservationRepository repository,
    IValidator<CreateReservationRequest> validator,
    IMapper mapper,
    ILogger<CreateReservationHandler> logger)
{
}
```

Method/constructor call:
```csharp
var handler = new CreateReservationHandler(
    repository,
    validator,
    mapper,
    logger);
```

LINQ chains (when long or multiple operations):
```csharp
var activeCustomers = customers
    .Where(customer => customer.IsActive)
    .OrderBy(customer => customer.Name)
    .Select(customer => customer.Email)
    .ToList();
```

---

## General
- Prefer `var` when the type is obvious from the right side
- Use `string.Empty` instead of `""`
- Use `is null` / `is not null` instead of `== null` / `!= null`
- Use pattern matching where it improves readability
- Use early returns (guard clauses) to reduce nesting
