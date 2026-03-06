import React from 'react'
import Router from './routes/Router'
import ErrorBoundary from './components/ErrorBoundary'


export default function App() {
  // eslint-disable-next-line no-console
  console.log('App: render')
  return (
    <div className="min-h-screen bg-dibi-50">
      <ErrorBoundary>
        <Router />
       </ErrorBoundary>
    </div>
  )
}
