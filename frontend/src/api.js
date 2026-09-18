const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5209'

async function handle(response) {
  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    throw new Error(body.error || `Request failed with status ${response.status}`)
  }
  return response.json()
}

export async function submitApplication(payload) {
  const response = await fetch(`${BASE_URL}/api/applications`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  })
  return handle(response)
}

export async function fetchApplications() {
  const response = await fetch(`${BASE_URL}/api/applications`)
  return handle(response)
}

export async function fetchSummary() {
  const response = await fetch(`${BASE_URL}/api/applications/summary`)
  return handle(response)
}
