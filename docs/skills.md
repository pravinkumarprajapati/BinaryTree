# Skills (Tools) Catalogue

These are the `[KernelFunction]`s the LLM can call. Names, descriptions and
parameter docs are exactly what the model sees, so the model can pick the
right tool for a user question.

> ID format throughout: `^E\d{3,6}$` (e.g. `E001`). Anything else is rejected
> by `InputSanitizer.NormalizeEmployeeId` before any lookup happens.

---

## EmployeePlugin

### 1. `EmployeePlugin.GetSalary`

| Field         | Value                                                            |
|---------------|------------------------------------------------------------------|
| Description   | Get an employee's monthly salary by employee ID.                 |
| Parameters    | `employeeId : string` — must match `^E\d{3,6}$`                  |
| Returns       | `"Salary for {FullName}: {amount as currency}"`                  |
| Example       | "What is E001's salary?"                                         |
| Security      | ID validated; KeyNotFoundException if employee missing.          |

### 2. `EmployeePlugin.GetLeaveBalance`

| Field         | Value                                                            |
|---------------|------------------------------------------------------------------|
| Description   | Get an employee's remaining leave balance (in days) by ID.       |
| Parameters    | `employeeId : string` — must match `^E\d{3,6}$`                  |
| Returns       | `"{FullName} has {N} leave day(s) remaining."`                   |
| Example       | "How many leaves does E002 have left?"                           |
| Security      | ID validated; KeyNotFoundException if missing.                   |

### 3. `EmployeePlugin.GetDepartment`

| Field         | Value                                                            |
|---------------|------------------------------------------------------------------|
| Description   | Get the department an employee belongs to by ID.                 |
| Parameters    | `employeeId : string` — must match `^E\d{3,6}$`                  |
| Returns       | `"{FullName} works in {Department}."`                            |
| Example       | "Which department is E003 in?"                                   |
| Security      | ID validated; KeyNotFoundException if missing.                   |

### 4. `EmployeePlugin.GetLastPromotion`

| Field         | Value                                                            |
|---------------|------------------------------------------------------------------|
| Description   | Get the date of an employee's last promotion by ID.              |
| Parameters    | `employeeId : string` — must match `^E\d{3,6}$`                  |
| Returns       | `"{FullName} was last promoted on {yyyy-MM-dd}."` or `"never promoted."` |
| Example       | "When was E004 last promoted?"                                   |
| Security      | ID validated; KeyNotFoundException if missing.                   |

### 5. `EmployeePlugin.GetPersonalInfo`

| Field         | Value                                                            |
|---------------|------------------------------------------------------------------|
| Description   | Get an employee's basic personal info (name, email, phone) by ID.|
| Parameters    | `employeeId : string` — must match `^E\d{3,6}$`                  |
| Returns       | `"{FullName} | {Email} | {Phone}"`                               |
| Example       | "Show me E005's contact info."                                   |
| Security      | ID validated; only non-sensitive fields exposed; no SSN/DOB.     |

---

## LogAnalyticsPlugin

### 6. `LogAnalyticsPlugin.RunPlannedQuery`

| Field         | Value                                                            |
|---------------|------------------------------------------------------------------|
| Description   | Run the planned Azure Log Analytics query and report whether records were found. |
| Parameters    | *(none)* — body is hardcoded; the LLM cannot inject KQL.         |
| Returns       | `"Record found"` on HTTP 2xx, `"No records returned."` otherwise.|
| Example       | "Plan a task." / "Run the planned query."                        |
| Security      | AAD token via `DefaultAzureCredential`; 10 s timeout; 3 retries; fixed payload. |

#### What it does under the hood

```
POST https://api.loganalytics.azure.com/v1/workspaces/DEMO_WORKSPACE/query
Authorization: Bearer <AAD token via DefaultAzureCredential>
Content-Type: application/json

{ "query": "AzureActivity | summarize count() by Category" }
```

A 2xx response means the query ran — we report **"Record found"** as
specified.

---

## How to add a new skill

1. Create a method on a plugin class:

   ```csharp
   [KernelFunction, Description("Short, action-oriented description for the LLM.")]
   public string DoSomething([Description("What this parameter is.")] string arg)
   {
       var safe = InputSanitizer.NormalizeXxx(arg);
       // ...
       return "...";
   }
   ```

2. Register the plugin in `AgentFactory.Build`:

   ```csharp
   kernel.Plugins.AddFromType<MyNewPlugin>("MyNewPlugin");
   ```

3. Add a row above in this file so future-you remembers what it does.

4. Add an entry to the system prompt **only** if its scope falls outside
   the existing employee-data scope; otherwise no prompt change is needed.
