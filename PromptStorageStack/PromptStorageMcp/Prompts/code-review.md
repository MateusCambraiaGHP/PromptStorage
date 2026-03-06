---
name: code-review
description: Checklist and rules for performing a thorough code review
tags: code-review, review, quality, best-practices
---

# Code Review Prompt

When reviewing code, check ALL of the following categories and provide feedback:

## 1. Correctness
- Does the code do what it's supposed to do?
- Are there off-by-one errors, null reference risks, or race conditions?
- Are edge cases handled?

## 2. Security
- Is user input validated and sanitized?
- Are there SQL injection, XSS, or CSRF vulnerabilities?
- Are secrets hardcoded?
- Is authentication/authorization properly applied?

## 3. Performance
- Are there N+1 query problems?
- Are large collections loaded into memory unnecessarily?
- Are there missing `async/await` patterns?
- Could any operation benefit from caching?

## 4. Maintainability
- Are method and variable names descriptive?
- Is the code DRY (Don't Repeat Yourself)?
- Are methods too long (>30 lines)?
- Is the Single Responsibility Principle followed?

## 5. Error Handling
- Are exceptions caught at the right level?
- Are custom exceptions used where appropriate?
- Is there proper logging on failures?

## Output Format
For each issue found, provide:
- **Severity**: 🔴 Critical / 🟡 Warning / 🔵 Suggestion
- **Location**: File and line number
- **Issue**: Description of the problem
- **Fix**: Suggested solution with code example
