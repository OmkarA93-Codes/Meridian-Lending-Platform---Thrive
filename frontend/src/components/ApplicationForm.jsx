import { useMemo, useState } from 'react'

const initialState = {
  applicantName: '',
  loanAmount: '',
  assetValue: '',
  creditScore: ''
}

function formatGBP(value) {
  return new Intl.NumberFormat('en-GB', {
    style: 'currency',
    currency: 'GBP',
    maximumFractionDigits: 0
  }).format(value)
}

export default function ApplicationForm({ onSubmit, isSubmitting, error }) {
  const [values, setValues] = useState(initialState)

  const loanAmountNumber = Number(values.loanAmount)
  const assetValueNumber = Number(values.assetValue)

  const previewLtv = useMemo(() => {
    if (!loanAmountNumber || !assetValueNumber) return null
    return Math.round((loanAmountNumber / assetValueNumber) * 10000) / 100
  }, [loanAmountNumber, assetValueNumber])

  function handleChange(field) {
    return (event) => {
      setValues((prev) => ({ ...prev, [field]: event.target.value }))
    }
  }

  function handleSubmit(event) {
    event.preventDefault()
    onSubmit({
      applicantName: values.applicantName.trim(),
      loanAmount: loanAmountNumber,
      assetValue: assetValueNumber,
      creditScore: Number(values.creditScore)
    })
  }

  const isValid =
    loanAmountNumber > 0 &&
    assetValueNumber > 0 &&
    Number(values.creditScore) >= 1 &&
    Number(values.creditScore) <= 999

  return (
    <div className="card">
      <div className="card-header">
        <h2>New application</h2>
        <span className="subtitle">Enter the loan terms to run them against the underwriting rules.</span>
      </div>
      <form className="form" onSubmit={handleSubmit}>
        <div className="field">
          <label htmlFor="applicantName">Applicant name</label>
          <input
            id="applicantName"
            type="text"
            placeholder="e.g. J. Okafor"
            value={values.applicantName}
            onChange={handleChange('applicantName')}
          />
        </div>

        <div className="field-row">
          <div className="field">
            <label htmlFor="loanAmount">
              Loan amount <span className="hint">GBP</span>
            </label>
            <input
              id="loanAmount"
              type="number"
              min="0"
              step="1000"
              placeholder="450000"
              value={values.loanAmount}
              onChange={handleChange('loanAmount')}
              required
            />
          </div>
          <div className="field">
            <label htmlFor="assetValue">
              Asset value <span className="hint">GBP</span>
            </label>
            <input
              id="assetValue"
              type="number"
              min="0"
              step="1000"
              placeholder="600000"
              value={values.assetValue}
              onChange={handleChange('assetValue')}
              required
            />
          </div>
        </div>

        <div className="field">
          <label htmlFor="creditScore">
            Credit score <span className="hint">1–999</span>
          </label>
          <input
            id="creditScore"
            type="number"
            min="1"
            max="999"
            placeholder="820"
            value={values.creditScore}
            onChange={handleChange('creditScore')}
            required
          />
        </div>

        {previewLtv !== null && (
          <div className="ltv-preview">
            <span>Loan-to-value</span>
            <strong>{previewLtv.toFixed(2)}%</strong>
          </div>
        )}

        {error && <div className="form-error">{error}</div>}

        <div className="submit-row">
          <button className="btn-primary" type="submit" disabled={!isValid || isSubmitting}>
            {isSubmitting ? 'Assessing…' : 'Submit for a decision'}
          </button>
          {loanAmountNumber > 0 && (
            <span className="hint">{formatGBP(loanAmountNumber)} requested</span>
          )}
        </div>
      </form>
    </div>
  )
}
