namespace LendingPlatform.Api.Models;

/// <summary>
/// Aggregate view across every application received so far. This is recalculated
/// on demand from the stored applications rather than kept as running counters,
/// since the volumes involved in a demo like this are tiny.
/// </summary>
public class PortfolioSummary
{
    public int TotalApplicants { get; set; }

    public int ApprovedCount { get; set; }

    public int DeclinedCount { get; set; }

    /// <summary>
    /// Sum of the loan amounts for applications that were actually approved -
    /// i.e. loans that have genuinely been "written", not just applied for.
    /// </summary>
    public decimal TotalValueWritten { get; set; }

    /// <summary>
    /// Mean of the loan-to-value ratio across ALL applications received,
    /// approved or declined, since the brief asks for the average across
    /// "all applications" rather than only the successful ones.
    /// </summary>
    public decimal MeanLoanToValue { get; set; }
}
