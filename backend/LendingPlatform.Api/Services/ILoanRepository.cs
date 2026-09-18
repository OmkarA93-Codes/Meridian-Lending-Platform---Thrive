using LendingPlatform.Api.Models;

namespace LendingPlatform.Api.Services;

public interface ILoanRepository
{
    LoanApplication Add(LoanApplication application);

    IReadOnlyList<LoanApplication> GetAll();

    PortfolioSummary GetSummary();
}
