using System.Text.Json.Serialization;
using LendingPlatform.Api.Models;
using LendingPlatform.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Serialize enums (e.g. LoanDecision) as their string names rather than numbers -
// "Approved"/"Declined" is far more useful to a frontend than 0/1.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

const string FrontendCorsPolicy = "FrontendCorsPolicy";

// The React dev server runs on a different port to the API, so it needs an
// explicit CORS allowance.
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<ILoanEvaluationService, LoanEvaluationService>();
builder.Services.AddSingleton<ILoanRepository, InMemoryLoanRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(FrontendCorsPolicy);

var applications = app.MapGroup("/api/applications");

applications.MapPost("/", (LoanApplicationRequest request, ILoanEvaluationService evaluator, ILoanRepository repository) =>
{
    if (request.CreditScore is < 1 or > 999)
    {
        return Results.BadRequest(new { error = "Credit score must be between 1 and 999." });
    }

    if (request.LoanAmount <= 0)
    {
        return Results.BadRequest(new { error = "Loan amount must be greater than zero." });
    }

    if (request.AssetValue <= 0)
    {
        return Results.BadRequest(new { error = "Asset value must be greater than zero." });
    }

    var result = evaluator.Evaluate(request.LoanAmount, request.AssetValue, request.CreditScore);

    var application = new LoanApplication
    {
        ApplicantName = string.IsNullOrWhiteSpace(request.ApplicantName) ? "Unnamed applicant" : request.ApplicantName.Trim(),
        LoanAmount = request.LoanAmount,
        AssetValue = request.AssetValue,
        CreditScore = request.CreditScore,
        LoanToValue = result.LoanToValue,
        Decision = result.Decision,
        Reason = result.Reason
    };

    repository.Add(application);

    return Results.Created($"/api/applications/{application.Id}", application);
})
.WithName("SubmitApplication");

applications.MapGet("/", (ILoanRepository repository) => Results.Ok(repository.GetAll()))
    .WithName("GetApplications");

applications.MapGet("/summary", (ILoanRepository repository) => Results.Ok(repository.GetSummary()))
    .WithName("GetPortfolioSummary");

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
