using LendingPlatform.Api.Models;
using LendingPlatform.Api.Services;
using Xunit;

namespace LendingPlatform.Api.Tests;

public class InMemoryLoanRepositoryTests
{
    [Fact]
    public void Summary_of_an_empty_portfolio_is_all_zeroes()
    {
        var repository = new InMemoryLoanRepository();

        var summary = repository.GetSummary();

        Assert.Equal(0, summary.TotalApplicants);
        Assert.Equal(0, summary.ApprovedCount);
        Assert.Equal(0, summary.DeclinedCount);
        Assert.Equal(0m, summary.TotalValueWritten);
        Assert.Equal(0m, summary.MeanLoanToValue);
    }

    [Fact]
    public void Summary_only_counts_approved_loans_towards_total_value_written()
    {
        var repository = new InMemoryLoanRepository();

        repository.Add(Application(loanAmount: 200_000m, ltv: 40m, decision: LoanDecision.Approved));
        repository.Add(Application(loanAmount: 300_000m, ltv: 95m, decision: LoanDecision.Declined));
        repository.Add(Application(loanAmount: 150_000m, ltv: 50m, decision: LoanDecision.Approved));

        var summary = repository.GetSummary();

        Assert.Equal(3, summary.TotalApplicants);
        Assert.Equal(2, summary.ApprovedCount);
        Assert.Equal(1, summary.DeclinedCount);
        Assert.Equal(350_000m, summary.TotalValueWritten); // declined loan is excluded
    }

    [Fact]
    public void Mean_LTV_is_averaged_across_every_application_regardless_of_decision()
    {
        var repository = new InMemoryLoanRepository();

        repository.Add(Application(loanAmount: 200_000m, ltv: 40m, decision: LoanDecision.Approved));
        repository.Add(Application(loanAmount: 300_000m, ltv: 90m, decision: LoanDecision.Declined));

        var summary = repository.GetSummary();

        Assert.Equal(65m, summary.MeanLoanToValue); // (40 + 90) / 2
    }

    private static LoanApplication Application(decimal loanAmount, decimal ltv, LoanDecision decision) => new()
    {
        LoanAmount = loanAmount,
        AssetValue = loanAmount / (ltv / 100m),
        CreditScore = 800,
        LoanToValue = ltv,
        Decision = decision,
        Reason = "test fixture"
    };
}
