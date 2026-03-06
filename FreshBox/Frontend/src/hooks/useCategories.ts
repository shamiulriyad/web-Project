import { useEffect, useState } from 'react'
import api from '../services/api'

export default function useCategories() {
  const [data, setData] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let mounted = true
    api.get('/categories')
      .then(res => { if (mounted) setData(res.data) })
      .catch(err => { if (mounted) setError(err.message) })
      .finally(() => { if (mounted) setLoading(false) })
    return () => { mounted = false }
  }, [])

  return { data, loading, error }
}
