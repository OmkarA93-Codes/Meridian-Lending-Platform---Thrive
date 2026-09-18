import { useEffect, useState } from 'react'
import Header from './components/Header.jsx'
import ApplicationForm from './components/ApplicationForm.jsx'
import DecisionPanel from './components/DecisionPanel.jsx'
import StatCard from './components/StatCard.jsx'
import ApplicationsTable from './components/ApplicationsTable.jsx'
import { submitApplication, fetchApplications, fetchSummary } from './api.js'

function formatGBP(value) {
  return new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency: 'GBP',
    maximumFractionDigits: 0
  }).format(value || 0)
}

export default function App() {
  const [activeTab, setActiveTab] = useState('apply')
  const [applications, setApplications] = useState([])
  const [summary, setSummary] = useState(null)
  const [latestDecision, setLatestDecision] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState(null)
  const [loadError, setLoadError] = useState(null)

  async function refreshPortfolio() {
    try {
      const [applicationsData, summaryData] = await Promise.all([fetchApplications(), fetchSummary()])
      setApplications(applicationsData)
      setSummary(summaryData)
      setLoadError(null)
    } catch (err) {
      setLoadError(
        'Could not reach the API. Make sure the backend is running (see README) and refresh the page.'
      )
    }
  }

  useEffect(() => {
    refreshPortfolio()
  }, [])

  async function handleSubmit(payload) {
    setIsSubmitting(true)
    setError(null)
    try {
      const created = await submitApplication(payload)
      setLatestDecision(created)
      await refreshPortfolio()
    } catch (err) {
      setError(err.message || 'Something went wrong submitting that application.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="shell">
      <Header activeTab={activeTab} onTabChange={setActiveTab} />

      {loadError && <div className="form-error" style={{ marginBottom: 24 }}>{loadError}</div>}

      {activeTab === 'apply' ? (
        <div className="intake-grid">
          <ApplicationForm onSubmit={handleSubmit} isSubmitting={isSubmitting} error={error} />
          <DecisionPanel latestDecision={latestDecision} />
        </div>
      ) : (
        <div>
          <div className="stat-row">
            <StatCard label="Total applicants" value={summary ? summary.totalApplicants : '—'} />
            <StatCard label="Approved" value={summary ? summary.approvedCount : '—'} tone="approve" />
            <StatCard label="Declined" value={summary ? summary.declinedCount : '—'} tone="decline" />
            <StatCard
              label="Value written to date"
              value={summary ? formatGBP(summary.totalValueWritten) : '—'}
            />
            <StatCard
              label="Mean LTV, all applications"
              value={summary ? `${summary.meanLoanToValue.toFixed(2)}%` : '—'}
            />
          </div>
          <ApplicationsTable applications={applications} />
        </div>
      )}
    </div>
  )
}
