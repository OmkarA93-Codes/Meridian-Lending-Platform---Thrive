# Submission report

This document walks through the solution against each evaluation criterion in the brief. For the underlying detail it points to the README, the code itself, and `AI_LOG.md`, rather than repeating them at length.

## Correctness of business logic

The rules are implemented in one place —
`backend/LendingPlatform.Api/Services/LoanEvaluationService.cs` — so there's a single source of truth for the decision logic rather than it being duplicated across the API and the frontend:

- Loans under £100,000 or over £1.5 million are declined outright, before any  LTV or credit-score check runs.
- Loans of £1 million or more require LTV ≤ 60% **and** credit score ≥ 950.
- Loans under £1 million step through the sliding LTV bands (< 60% / < 80% /< 90%) with their corresponding score requirements (750 / 800 / 900), and anything at 90% LTV or above is declined regardless of score.
Every boundary implied by the brief is covered by a test in
`LoanEvaluationServiceTests.cs` rather than just the happy path — the exact £100k/£1.5m/£1m limits, and each LTV band edge on both sides 
(e.g. a loan atexactly 60% LTV is tested against both what the 60%-band and the 80%-band would require, to prove it lands in the right one). `InMemoryLoanRepositoryTests.cs`separately checks the portfolio aggregation (approved-only total value, all-applications mean LTV).

## Clarity and maintainability of code

- Method and variable names describe what they mean in the domain (e.g.
 `EvaluateLargeLoan`, `loanToValue`, `requiredScore`) rather the generic names.
- The sliding-scale bands are expressed as a small data table
 (`(ltvCeiling, requiredScore)` tuples) that's iterated over, instead of a chain of near-identical if/else blocks — so a change to a threshold is a one-line edit, and the shape of the rule is visible at a glance.
- Every decision returns a plain-English reason string, which does double duty: it's what the frontend shows the applicant, and it made the rules much easier for me to sanity-check by eye while testing.
- On the frontend, formatting helpers (currency, timestamps) and the API
 client are kept out of components so components stay focused on markup and state.

## Separation of concerns and modularity

**Backend**, layered so each piece has one job:
- `Models/` — plain data shapes (request DTO, stored entity, summary DTO).
- `Services/ILoanEvaluationService` — pure business logic, no knowledge of HTTP, storage, or JSON at all. This is what's unit tested directly.
- `Services/ILoanRepository` — storage and aggregation, hidden behind an
 interface. `InMemoryLoanRepository` is one implementation of it swapping in a real database later means writing a new class, not touching the rules or the endpoints.
- `Program.cs` — just wiring (DI, CORS, routes) and translating between HTTP and the two services above.

**Frontend**, split by responsibility rather than by page:
- `api.js` — the only file that knows about `fetch` or the API's URL.
- `components/` — one component per concern (the form, the decision panel, a stat card, the applications table), each taking plain props rather than reaching into global state.
- `App.jsx` — owns state and orchestrates the above; it's the only place
  that decides *when* to call the API.

## Effective use of AI, quality of prompting, iteration, and critical review

Covered in full in [`AI_LOG.md`](./AI_LOG.md).
 In short: I used Claude to generate the initial implementation from the brief, then treated that as a draft to review rather than a finished answer — pushing back on the rule boundary test data until it landed on exact, unambiguous percentages; catching and fixing an enum-serialization bug that would have silently broken the frontend (`Approved`/`Declined` coming back as `0`/`1`); asking for the "total value written" vs. "mean LTV" assumptions to be made explicit rather than picked silently; and redirecting the first-draft visual design away from a generic SaaS-dashboard look towards something grounded in what the product actually is.

## Ability to explain reasoning, assumptions, and trade-offs

**Assumptions** (also in the README, since a reviewer of the code should be able to find them without this report):
- "Total value of loans written to date" = sum of **approved** loan amounts only.
- "Mean average LTV across all applications" = mean across **every**
  application, approved and declined.
- Band boundaries are read literally: "LTV < 60%" excludes exactly 60%, so a  loan at exactly 60% LTV is judged against the next band up.

**Trade-offs**, made deliberately for a time-boxed exercise rather than
overlooked:
- **In-memory storage over a real database.** Keeps the setup to "clone and run," but means data doesn't survive a restart and won't scale past a single instance.
- **Minimal API over controller classes.** Three small, closely-related
  endpoints didn't seem to justify the extra ceremony of controller classes; I'd reconsider that once the API had more surface area than this.
- **No authentication.** Out of scope for demonstrating the underwriting
  logic itself, but called out explicitly in the README as something a real version would need on day one.
- **Validation kept minimal** (range checks only) rather than building out field-level error handling, to keep the focus on the business logic the test is actually evaluating.

The full list of production considerations I'd otherwise raise is in the README under "Production considerations."



