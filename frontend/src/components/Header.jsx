export default function Header({ activeTab, onTabChange }) {
  return (
    <header className="topbar">
      <div className="wordmark">
        <span className="mark">Meridian Lending</span>
        <span className="tagline">Underwriting desk</span>
      </div>
      <nav className="tabs">
        <button
          className={activeTab === 'apply' ? 'active' : ''}
          onClick={() => onTabChange('apply')}
        >
          New application
        </button>
        <button
          className={activeTab === 'portfolio' ? 'active' : ''}
          onClick={() => onTabChange('portfolio')}
        >
          Portfolio
        </button>
      </nav>
    </header>
  )
}
