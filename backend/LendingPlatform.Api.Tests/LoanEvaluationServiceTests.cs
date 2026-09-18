using LendingPlatform.Api.Models;
using LendingPlatform.Api.Services;
using Xunit;

namespace LendingPlatform.Api.Tests;

public class LoanEvaluationServiceTests
{
    private readonly LoanEvaluationService _sut = new();

    // --- Overall loan amount limits -----------------------------------------

    [Fact]
    public void Declines_when_loan_amount_is_below_the_minimum()
    {
        var result = _sut.Evaluate(loanAmount: 99_999m, assetValue: 200_000m, creditScore: 999);

        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Declines_when_loan_amount_is_above_the_maximum()
    {
        var result = _sut.Evaluate(loanAmount: 1_500_001m, assetValue: 3_000_000m, creditScore: 999);

        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void At_the_minimum_loan_amount_normal_rules_apply()
    {
        // £100,000 against a £1,000,000 asset is a 10% LTV, well inside the 750-score band.
        var result = _sut.Evaluate(loanAmount: 100_000m, assetValue: 1_000_000m, creditScore: 750);

        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void At_the_maximum_loan_amount_normal_rules_apply()
    {
        // £1,500,000 falls into the £1m+ bracket: needs LTV <= 60% and score >= 950.
        var result = _sut.Evaluate(loanAmount: 1_500_000m, assetValue: 2_500_000m, creditScore: 960);

        Assert.Equal(LoanDecision.Approved, result.Decision);
        Assert.Equal(60m, result.LoanToValue);
    }

    // --- Loans of £1 million or more ----------------------------------------

    [Fact]
    public void Large_loan_approves_at_exactly_60_percent_LTV_and_950_score()
    {
        var result = _sut.Evaluate(loanAmount: 1_200_000m, assetValue: 2_000_000m, creditScore: 950);

        Assert.Equal(60m, result.LoanToValue);
        Assert.Equal(LoanDecision.Approved, result.Decision);
    }

    [Fact]
    public void Large_loan_declines_when_LTV_is_over_60_percent_even_with_a_perfect_score()
    {
        var result = _sut.Evaluate(loanAmount: 1_300_000m, assetValue: 2_000_000m, creditScore: 999); // LTV 65%

        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    [Fact]
    public void Large_loan_declines_when_score_is_just_under_950_even_with_low_LTV()
    {
        var result = _sut.Evaluate(loanAmount: 1_000_000m, assetValue: 5_000_000m, creditScore: 949); // LTV 20%

        Assert.Equal(LoanDecision.Declined, result.Decision);
    }

    // --- Loans under £1 million: sliding LTV / credit score bands ----------

    [Theory]
    [InlineData(500_000, 1_000_000, 750, LoanDecision.Approved)]  // 50% LTV, score meets 750 bar
    [InlineData(500_000, 1_000_000, 749, LoanDecision.Declined)]  // 50% LTV, one point under
    [InlineData(600_000, 1_000_000, 799, LoanDecision.Declined)]  // exactly 60% LTV moves into the 80%-band, needs 800
    [InlineData(600_000, 1_000_000, 800, LoanDecision.Approved)]  // exactly 60% LTV, meets the 800 bar
    [InlineData(700_000, 1_000_000, 800, LoanDecision.Approved)]  // 70% LTV, score meets 800 bar
    [InlineData(700_000, 1_000_000, 799, LoanDecision.Declined)]  // 70% LTV, one point under
    [InlineData(800_000, 1_000_000, 899, LoanDecision.Declined)]  // exactly 80% LTV moves into the 90%-band, needs 900
    [InlineData(800_000, 1_000_000, 900, LoanDecision.Approved)]  // exactly 80% LTV, meets the 900 bar
    [InlineData(850_000, 1_000_000, 900, LoanDecision.Approved)]  // 85% LTV, score meets 900 bar
    [InlineData(850_000, 1_000_000, 899, LoanDecision.Declined)]  // 85% LTV, one point under
    [InlineData(900_000, 1_000_000, 999, LoanDecision.Declined)]  // exactly 90% LTV - automatic decline
    [InlineData(950_000, 1_000_000, 999, LoanDecision.Declined)]  // 95% LTV - automatic decline
    public void Standard_loan_sliding_scale_is_applied_correctly(
        decimal loanAmount, decimal assetValue, int creditScore, LoanDecision expected)
    {
        var result = _sut.Evaluate(loanAmount, assetValue, creditScore);

        Assert.Equal(expected, result.Decision);
    }

    // --- Input edge cases -----------------------------------------------------

    [Fact]
    public void Declines_when_asset_value_is_zero_because_LTV_is_undefined()
    {
        var result = _sut.Evaluate(loanAmount: 200_000m, assetValue: 0m, creditScore: 999);

        Assert.Equal(LoanDecision.Declined, result.Decision);
        Assert.Equal(0m, result.LoanToValue);
    }

    [Fact]
    public void Loan_to_value_is_rounded_to_two_decimal_places()
    {
        var result = _sut.Evaluate(loanAmount: 333_333m, assetValue: 1_000_000m, creditScore: 750);

        Assert.Equal(33.33m, result.LoanToValue);
    }
}
