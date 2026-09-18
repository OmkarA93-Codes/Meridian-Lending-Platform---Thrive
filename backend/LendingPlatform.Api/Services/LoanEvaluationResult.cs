using LendingPlatform.Api.Models;

namespace LendingPlatform.Api.Services;

/// <summary>
/// Outcome of running the underwriting rules against a single set of inputs.
/// </summary>
public record LoanEvaluationResult(LoanDecision Decision, string Reason, decimal LoanToValue);

public interface ILoanEvaluationService
{
    LoanEvaluationResult Evaluate(decimal loanAmount, decimal assetValue, int creditScore);
}
