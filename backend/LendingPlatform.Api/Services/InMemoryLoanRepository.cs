using LendingPlatform.Api.Models;

namespace LendingPlatform.Api.Services;

/// <summary>
/// Keeps applications in memory for the lifetime of the process. That's fine for
/// this exercise - a real version of this service would swap this out for a
/// proper datastore (see README "Production considerations").
/// </summary>
public class InMemoryLoanRepository : ILoanRepository
{
    private readonly List<LoanApplication> _applications = new();
    private readonly object _lock = new();

    public LoanApplication Add(LoanApplication application)
    {
        lock (_lock)
        {
            _applications.Add(application);
        }

        return application;
    }

    public IReadOnlyList<LoanApplication> GetAll()
    {
        lock (_lock)
        {
            // Newest first - that's what you want to see on a dashboard.
            return _applications.OrderByDescending(a => a.SubmittedAtUtc).ToList();
        }
    }

    public PortfolioSummary GetSummary()
    {
        lock (_lock)
        {
            var approved = _applications.Where(a => a.Decision == LoanDecision.Approved).ToList();
            var declined = _applications.Where(a => a.Decision == LoanDecision.Declined).ToList();

            return new PortfolioSummary
            {
                TotalApplicants = _applications.Count,
                ApprovedCount = approved.Count,
                DeclinedCount = declined.Count,
                TotalValueWritten = approved.Sum(a => a.LoanAmount),
                MeanLoanToValue = _applications.Count == 0
                    ? 0m
                    : Math.Round(_applications.Average(a => a.LoanToValue), 2)
            };
        }
    }
}
