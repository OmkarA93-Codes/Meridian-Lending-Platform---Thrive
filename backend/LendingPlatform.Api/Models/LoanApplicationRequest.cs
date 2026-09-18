namespace LendingPlatform.Api.Models;

/// <summary>
/// Payload submitted by the client when a new loan application is raised.
/// Kept deliberately separate from <see cref="LoanApplication"/> so the shape
/// of what a caller can send never drifts silently from what we store.
/// </summary>
public class LoanApplicationRequest
{
    public string? ApplicantName { get; set; }

    public decimal LoanAmount { get; set; }

    public decimal AssetValue { get; set; }

    public int CreditScore { get; set; }
}
