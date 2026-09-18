# Meridian Lending — Secured Lending Platform

A small underwriting tool: submit a loan amount, the value of the asset securing it, and the applicant's credit score, and it applies a fixed set of business rules to approve or decline the loan. Built as a C# minimal API backend and a React (Vite) frontend.

## What's here

```
backend/
  LendingPlatform.Api/        the API (ASP.NET Core minimal API, .NET 8)
  LendingPlatform.Api.Tests/  xUnit tests for the underwriting rules
  LendingPlatform.sln
frontend/                     React + Vite single-page app
AI_LOG.md                     prompts and notes on how AI was used.


## How to Run the Project :-

You'll need the [.NET 8 SDK](https://dotnet.microsoft.com/download) and
[Node.js](https://nodejs.org) (18+) installed.

### 1. Backend

```bash
cd backend/LendingPlatform.Api
dotnet run
```

This starts the API on `http://localhost:5209` (see
`Properties/launchSettings.json`). Swagger UI is available at
`http://localhost:5209/swagger` while running in development.

To run the tests:

```bash
cd backend
dotnet test
```

### 2. Frontend

In a second terminal:

```bash
cd frontend
npm install
npm run dev
```

This starts the app on `http://localhost:5173` and points it at the API on
`http://localhost:5209` by default. If your API runs somewhere else, copy
`.env.example` to `.env` and change `VITE_API_BASE_URL`.

## API

| Method | Route                     | Description                                   |
|--------|---------------------------|------------------------------------------------|
| POST   | `/api/applications`       | Submit a new loan application, returns the decision |
| GET    | `/api/applications`       | List every application received, newest first |
| GET    | `/api/applications/summary` | Portfolio-level stats (see below)           |

**POST `/api/applications` body:**

```json
{
  "applicantName": "J. Okafor",
  "loanAmount": 450000,
  "assetValue": 600000,
  "creditScore": 820
}
```

## Business rules

- Any loan under £100,000 or over £1.5 million is declined outright.
- **Loans of £1 million or more**: approved only if LTV is 60% or less *and*
  credit score is 950 or above.
- **Loans under £1 million**, judged on a sliding scale:
  - LTV < 60% → needs credit score ≥ 750
  - LTV < 80% → needs credit score ≥ 800
  - LTV < 90% → needs credit score ≥ 900
  - LTV ≥ 90% → declined regardless of credit score

LTV (loan-to-value) is the loan amount as a percentage of the asset value it's secured against, rounded to two decimal places. The rules are implemented in
`backend/LendingPlatform.Api/Services/LoanEvaluationService.cs`, with every boundary case covered by tests in `LoanEvaluationServiceTests.cs`.

## Assumptions

The brief left a couple of things open to interpretation, so to be explicit:

- **"Total value of loans written to date"** — I've taken "written" to mean loans that were actually approved, not the sum of every amount applied for. Declined applications don't count towards it.
- **"Mean average LTV across all applications"** — this one does read as *all* applications, so the mean includes both approved and declined ones.
- Where an LTV band boundary is ambiguous (e.g. exactly 60%),
 I treated the bands as the brief states them literally: "LTV < 60%" does not include exactly 60%, so a loan at exactly 60% LTV falls into the next band up. This is called out in the test names so it's easy to challenge if the intent was different.
- Credit score is validated as an integer between 1 and 999 inclusive, per the stated input range; the API returns a 400 for anything outside that.
- Applicant name is optional input for the decision itself (it plays no part in the business rules) but is asked for so the portfolio table has something more meaningful to show than a UUID.

## Production considerations

This was built to demonstrate approach rather than to be production-ready. Things I would recommend to change for a real version:

- **Persistence.** Applications are held in memory and lost on restart. A real version needs a proper datastore (Postgres/SQL Server) behind`ILoanRepository`, which is already the seam I'd swap an implementation into.
- **Authentication & authorization.** There's currently no concept of who is allowed to submit or view applications.
- **Auditability.** Underwriting decisions like these are usually regulated; a production system would need an immutable audit trail of the rules version applied to each decision, not just the plain-English reason string returned today.
- **Configurable rules.** The thresholds are constants in code. In practice lending criteria change, so these would likely move to configuration or a rules engine that can be updated without a redeploy.
- **Concurrency & scale.** The in-memory repository uses a simple lock, which is fine for a single instance handling a handful of requests, but wouldn't hold up multi-instance or under real load.
- **Input validation.** Validation today is minimal (range checks). A
production API would validate more defensively (e.g. sensible upper bounds on asset value, trimming/sanitising applicant name) and return
 field-level validation errors rather than a single message.

## AI usage

See [`AI_LOG.md`](./AI_LOG.md) for the prompts used and notes on what was corrected along the way.
