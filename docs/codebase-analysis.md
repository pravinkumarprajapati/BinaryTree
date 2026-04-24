# Codebase Analysis (April 24, 2026)

## Executive Summary

This repository is a .NET 8 console application that implements a multi-agent HR assistant using Microsoft Semantic Kernel and Azure OpenAI. The architecture is intentionally simple and pragmatic: a router agent classifies each user turn into **EMPLOYEE**, **HR**, or **REFUSE**, then delegates to a specialist agent with a constrained prompt and plugin surface.

The code demonstrates a strong baseline for a tutorial/demo project:

- clear separation of concerns across orchestration, prompts, plugins, security, and data;
- tool-boundary input validation;
- pre-model prompt-injection screening;
- audit logging for tool invocations.

The largest gaps are around production hardening and consistency:

- docs are partly out of sync with the current implementation;
- no test project is present;
- startup fails hard without Azure OpenAI settings;
- repository seed contains direct personal/contact fields (fine for mock data, but worth noting in real environments).

## Repository Structure and Responsibilities

### Entry point and host wiring

`Program.cs` composes configuration, dependency injection, and the chat loop. It:

1. loads configuration from `appsettings.json`, user-secrets, and environment variables;
2. registers `InMemoryEmployeeRepository`, plugins, and `AuditFilter`;
3. resolves `AgentOrchestrator` via `AgentFactory.Build`;
4. runs a synchronous console REPL where each turn is sanitized, screened, routed, and answered.

This makes startup and runtime behavior easy to follow for beginners.

### Agent construction

`src/Agent/AgentFactory.cs` builds three `ChatCompletionAgent` instances:

- `RouterAgent` (classification only, low token budget);
- `EmployeeAgent` (job title/department specialist);
- `HRAgent` (salary/leave/promotion/contact/address/bank-last4 specialist).

Each specialist gets its own kernel with only one plugin attached, reducing accidental tool use outside intended scope.

### Turn orchestration

`src/Agent/Orchestrator/AgentOrchestrator.cs`:

- appends user input to shared history;
- asks router for a classification label;
- returns immediate refusal for out-of-scope requests;
- otherwise invokes the selected specialist against a copied chat history;
- appends assistant reply back to shared history.

This is a clean pattern for “router + specialists” while preserving conversation context.

### Data and tool surface

- `src/Data/InMemoryEmployeeRepository.cs` provides five seed employees (`E001`-`E005`).
- `src/Plugins/EmployeePlugin.cs` exposes: `GetJobTitle`, `GetDepartment`.
- `src/Plugins/HRPlugin.cs` exposes: `GetSalary`, `GetLeaveBalance`, `GetLastPromotion`, `GetContactInfo`, `GetAddress`, `GetBankAccountLast4`.

All plugin methods normalize and validate employee IDs before lookup.

### Security controls

- `src/Security/InputSanitizer.cs`: max-length check, control-character stripping, employee-id regex normalization.
- `src/Security/PromptInjectionGuard.cs`: denylist-driven pre-filter for suspicious prompt patterns.
- `src/Security/AuditFilter.cs`: function invocation logging for success/failure and duration.

For a compact sample application, this is a good layered defense approach.

## Strengths

1. **Strong modularity**: prompts, plugins, repository, and orchestration are clearly separated.
2. **Least-privilege plugin attachment**: each specialist only sees relevant tools.
3. **Reasonable deterministic settings**: low temperature for specialists and strict router output normalization.
4. **Early rejection path**: prompt injection checks run before adding content to model-bound turn history.
5. **Operational visibility**: audit logging includes function name, arguments, execution time, and errors.

## Risks and Gaps

1. **Documentation drift**
   - Current README/docs still describe older skill names (e.g., `GetPersonalInfo`, `RunPlannedQuery`) that are not present in the current code.
   - This can confuse contributors and users.

2. **No automated test suite**
   - There are no unit/integration tests to verify routing labels, refusal contract, sanitization behavior, or plugin outputs.

3. **Hard startup dependency on Azure OpenAI settings**
   - App throws at startup when endpoint/key/deployment are absent.
   - Fine for tutorial clarity, but less friendly for local CI and static checks.

4. **Prompt-injection guard is denylist-based**
   - Useful baseline, but bypasses are possible with paraphrasing/obfuscation.
   - Should be considered heuristic, not complete protection.

5. **PII-like sample fields in seed data**
   - Contact/address fields are synthetic but look realistic.
   - Ensure mock-only usage and clear non-production labeling.

## Recommendations (Prioritized)

1. **Align docs with code (high priority)**
   - Update `README.md`, `docs/agent.md`, and `docs/skills.md` to match the current router+specialist architecture and actual plugin functions.

2. **Add tests (high priority)**
   - Unit tests for:
     - `InputSanitizer.Clean` and `NormalizeEmployeeId`;
     - `PromptInjectionGuard.Check` positive/negative cases;
     - router label normalization fallback behavior;
     - plugin methods for valid/invalid IDs.

3. **Add a local “dry-run” mode (medium priority)**
   - Optional startup mode that skips Azure agent initialization and enables running non-LLM checks in environments without `dotnet user-secrets`.

4. **Improve refusal and error consistency (medium priority)**
   - Standardize user-facing error responses for missing employee IDs vs. unknown IDs.

5. **Consider structured logs (medium priority)**
   - Keep current messages but emit structured keys suitable for centralized log search and metrics.

## Suggested Technical Backlog

- [ ] `tests/EmployeeAgent.Tests` with xUnit + fluent assertions.
- [ ] Route-classification regression tests (EMPLOYEE / HR / REFUSE).
- [ ] Plugin contract tests for all tool outputs and key exceptions.
- [ ] Docs refresh to current tool names and architecture.
- [ ] Optional resilience improvements around transient LLM failures.

## Environment Notes for This Analysis

- Static analysis performed by source inspection.
- Build execution could not be completed in this environment because the `dotnet` CLI is not installed.
