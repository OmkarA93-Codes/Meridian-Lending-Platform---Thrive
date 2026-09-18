using LendingPlatform.Api.Models;

namespace LendingPlatform.Api.Services;

/// <summary>
/// Applies the secured-lending underwriting rules to a single application.
///
/// The rules, as given in the brief:
///   - Any loan under £100,000 or over £1.5 million is declined outright.
///   - Loans of £1 million or more need LTV of 60% or less AND a credit score of 950+.
///   - Loans under £1 million are judged on a sliding scale: the lower the LTV,
///     the lower the credit score bar, until LTV reaches 90% where it's an
///     automatic decline regardless of credit score.
///
/// LTV (loan-to-value) is the loan amount as a percentage of the asset securing it.
/// </summary>
public class LoanEvaluationService : ILoanEvaluationService
{
    private const decimal MinLoanAmount = 100_000m;
    private const decimal MaxLoanAmount = 1_500_000m;
    private const decimal LargeLoanThreshold = 1_000_000m;

    public LoanEvaluationResult Evaluate(decimal loanAmount, decimal assetValue, int creditScore)
    {
        if (assetValue <= 0)
        {
            // Can't express a loan as a percentage of an asset worth nothing (or less),
            // so there's no sensible LTV to underwrite against.
            return new LoanEvaluationResult(LoanDecision.Declined, "Asset value must be greater than zero.", 0m);
        }

        var loanToValue = Math.Round(loanAmount / assetValue * 100m, 2);

        if (loanAmount < MinLoanAmount)
        {
            return Decline($"Loan amount is below the £{MinLoanAmount:N0} minimum.", loanToValue);
        }

        if (loanAmount > MaxLoanAmount)
        {
            return Decline($"Loan amount is above the £{MaxLoanAmount:N0} maximum.", loanToValue);
        }

        if (loanAmount >= LargeLoanThreshold)
        {
            return EvaluateLargeLoan(loanToValue, creditScore);
        }

        return EvaluateStandardLoan(loanToValue, creditScore);
    }

    private static LoanEvaluationResult EvaluateLargeLoan(decimal loanToValue, int creditScore)
    {
        const int requiredScore = 950;
        const decimal maxLtv = 60m;

        if (loanToValue <= maxLtv && creditScore >= requiredScore)
        {
            return Approve($"Loan of £1m+ with LTV {loanToValue}% and credit score {creditScore} clears the {maxLtv}% LTV / {requiredScore}+ score bar.", loanToValue);
        }

        return Decline($"Loans of £1m+ require LTV of {maxLtv}% or less and a credit score of {requiredScore}+ (got LTV {loanToValue}%, score {creditScore}).", loanToValue);
    }

    private static LoanEvaluationResult EvaluateStandardLoan(decimal loanToValue, int creditScore)
    {
        // Sliding scale: as LTV climbs, the required credit score climbs with it,
        // until 90% LTV where the risk is too high to accept at any score.
        (decimal ltvCeiling, int requiredScore)[] bands =
        {
            (60m, 750),
            (80m, 800),
            (90m, 900)
        };

        foreach (var (ltvCeiling, requiredScore) in bands)
        {
            if (loanToValue < ltvCeiling)
            {
                return creditScore >= requiredScore
                    ? Approve($"LTV {loanToValue}% (under {ltvCeiling}%) with credit score {creditScore} meets the {requiredScore}+ requirement.", loanToValue)
                    : Decline($"LTV {loanToValue}% (under {ltvCeiling}%) requires a credit score of {requiredScore}+ (got {creditScore}).", loanToValue);
            }
        }

        return Decline($"LTV of {loanToValue}% is 90% or higher, which is outside our risk appetite regardless of credit score.", loanToValue);
    }

    private static LoanEvaluationResult Approve(string reason, decimal loanToValue) =>
        new(LoanDecision.Approved, reason, loanToValue);

    private static LoanEvaluationResult Decline(string reason, decimal loanToValue) =>
        new(LoanDecision.Declined, reason, loanToValue);
}
