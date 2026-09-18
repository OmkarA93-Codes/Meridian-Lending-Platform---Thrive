function formatGBP(value) {
  return new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency: 'GBP',
    maximumFractionDigits: 0
  }).format(value)
}

export default function DecisionPanel({ latestDecision }) {
  return (
    <div className="card decision-panel">
      <div className="card-header">
        <h2>Decision</h2>
        <span className="subtitle">Outcome of the most recent application.</span>
      </div>

      {!latestDecision ? (
        <div className="decision-empty">
          <div className="glyph">M</div>
          <p>Submit an application on the left and the underwriting decision will appear here.</p>
        </div>
      ) : (
        <div className="decision-result" key={latestDecision.id}>
          <div className={`stamp ${latestDecision.decision === 'Approved' ? 'approved' : 'declined'}`}>
            {latestDecision.decision === 'Approved' ? 'Approved' : 'Declined'}
          </div>
          <p className="decision-reason">{latestDecision.reason}</p>
          <div className="decision-metrics">
            <div className="decision-metric">
              <div className="value">{latestDecision.loanToValue.toFixed(2)}%</div>
              <div className="label">Loan to value</div>
            </div>
            <div className="decision-metric">
              <div className="value">{latestDecision.creditScore}</div>
              <div className="label">Credit score</div>
            </div>
            <div className="decision-metric">
              <div className="value">{formatGBP(latestDecision.loanAmount)}</div>
              <div className="label">Loan amount</div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
