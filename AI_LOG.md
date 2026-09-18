# AI log

Tool used: Claude (Anthropic), chat interface with code execution.

## How I used it

I gave Claude the full test brief in one prompt — the overview, the four
inputs/outputs, the exact business rules table, and the submission
requirements — and asked for a complete solution stratergies: a C# API, a React frontend that didn't look like a bare-bones form, and this log.
 I asked specifically for the code to read as something a person would write, and for the frontend to be visually considered rather than a default-looking page.
Rather than accepting the first pass, I went through it critically before treating it as done:

- **Rule boundaries.** The brief's bands ("LTV < 60%", "LTV < 80%", etc.) are easy to get subtly wrong at the edges (is exactly 60% in the first band or the second?). I had Claude write the rules so the boundary behaviour is explicit in code, then checked it by asking for a test matrix that pins down every boundary — 100k/1.5m loan limits, the 1m large-loan threshold,and each LTV/score band edge — rather than only the happy-path cases.
 A couple of the initial InlineData test values didn't land on the exact
 percentage I wanted (rounding from odd loan/asset combinations), so I askedfor the numbers to be adjusted to clean, exact percentages so the boundary being tested is unambiguous.
- **"Total value of loans written" vs. "mean LTV across all applications".**
  These two stats read as applying to different populations (written =
  approved only; mean LTV = every application), which isn't stated outright in the brief. I asked Claude to make that assumption explicit in the code comments and in the README rather than silently picking one, so it's something a reviewer can agree or disagree with.
- **JSON enum serialization.** The first version of the API would have
  returned the loan decision as a raw 0/1 rather than "Approved"/"Declined", which the frontend depends on. I caught this while reviewing the response shape and asked for a string enum converter to be added.
- **Verifying the frontend without a live install.** This environment
  couldn't reach npm's registry to install React/Vite, so I couldn't just `npm run dev` and eyeball it. I asked Claude to syntax-check every component and do a full bundle resolution pass with esbuild against the locally cached React packages to catch import/typo errors before calling it finished, rather than leaving that untested.
- **Visual design.** The first instinct for a "compelling" frontend leans towards generic SaaS-dashboard styling (rounded cards, one accent colour,  gradient hero). I pushed back on that and asked for a design grounded in  what the product actually is — an underwriting desk — which led to the ledger/ink-stamp direction (a literal "APPROVED"/"DECLINED" stamp as the one bold moment, a serif used for figures like a paper ledger, muted paper/ink/brass palette) rather than a generic dashboard template.

## What I'd still want to sanity-check myself

- Running `dotnet test` for real, since the sandbox this was built in doesn't have the .NET SDK installed — the test logic was reasoned through by hand and cross-checked against the rules table, but I haven't seen it executed.
- Running the frontend against the live API to confirm the CORS
 configuration and the `VITE_API_BASE_URL` default line up on my machine.
