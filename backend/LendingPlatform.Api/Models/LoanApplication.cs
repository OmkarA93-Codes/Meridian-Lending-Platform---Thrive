namespace LendingPlatform.Api.Models;

/// <summary>
/// A loan application as it lives in the system once it has been evaluated.
/// This is the record we keep for reporting - it captures both the applicant's
/// inputs and the outcome of applying the business rules to them.
/// </summary>
public class LoanApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ApplicantName { get; set; } = "Unnamed applicant";

    public decimal LoanAmount { get; set; }

    public decimal AssetValue { get; set; }

    public int CreditScore { get; set; }

    
    public decimal LoanToValue { get; set; }

    public LoanDecision Decision { get; set; }

    /// <summary>
    /// Plain-English explanation of why the decision came out the way it did,
    /// mainly so the frontend has something honest to show the applicant.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; } = DateTime.UtcNow;
}
