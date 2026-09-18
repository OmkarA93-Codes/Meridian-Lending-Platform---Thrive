export default function StatCard({ label, value, tone }) {
  return (
    <div className={`stat-card ${tone || ''}`}>
      <div className="label">{label}</div>
      <div className="value">{value}</div>
    </div>
  )
}
