function formatGBP(value) {
  return new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency: 'GBP',
    maximumFractionDigits: 0
  }).format(value)
}

function formatTimestamp(isoString) {
  return new Date(isoString).toLocaleString('en-GB', {
    day: '2-digit',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
}

export default function ApplicationsTable({ applications }) {
  if (applications.length === 0) {
    return (
      <div className="ledger">
        <div className="ledger-empty">No applications recorded yet. Submitted applications will be listed here.</div>
      </div>
    )
  }

  return (
    <div className="ledger">
      <table>
        <thead>
          <tr>
            <th>Applicant</th>
            <th className="numeric">Loan amount</th>
            <th className="numeric">Asset value</th>
            <th className="numeric">LTV</th>
            <th className="numeric">Score</th>
            <th>Decision</th>
            <th>Submitted</th>
          </tr>
        </thead>
        <tbody>
          {applications.map((application) => (
            <tr key={application.id}>
              <td className="applicant-cell">{application.applicantName}</td>
              <td className="numeric">{formatGBP(application.loanAmount)}</td>
              <td className="numeric">{formatGBP(application.assetValue)}</td>
              <td className="numeric">{application.loanToValue.toFixed(2)}%</td>
              <td className="numeric">{application.creditScore}</td>
              <td>
                <span className={`tag ${application.decision === 'Approved' ? 'approved' : 'declined'}`}>
                  {application.decision}
                </span>
              </td>
              <td className="timestamp-cell">{formatTimestamp(application.submittedAtUtc)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
