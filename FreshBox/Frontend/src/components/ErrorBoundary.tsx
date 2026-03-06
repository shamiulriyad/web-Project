import React from 'react'




type State = { hasError: boolean; error?: Error }

type Props = React.PropsWithChildren<object>;

export default class ErrorBoundary extends React.Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error }
  }

  componentDidCatch(error: unknown, info: React.ErrorInfo) {
    // Log error to the console or a monitoring service
    // Keep this to help debugging during development
    // eslint-disable-next-line no-console
    console.error(error, info)
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center p-6">
          <div className="max-w-xl text-dibi-900 p-6 rounded-2xl shadow-soft">
            <h2 className="text-xl font-semibold mb-2">Something went wrong</h2>
            <p className="text-sm text-gray-600 mb-4">An unexpected error occurred while rendering the page.</p>
            <pre className="text-xs bg-gray-100 p-3 rounded text-red-700 overflow-auto">{String(this.state.error)}</pre>
            <div className="mt-4">
              <button
                className="px-4 py-2 bg-dibi-500 text-white rounded"
                onClick={() => this.setState({ hasError: false, error: undefined })}
              >
                Try again
              </button>
            </div>
          </div>
        </div>
      )
    }

    return this.props.children
  }
}
