
import { useEffect, useState } from 'react'
import { getProducts, type ProductListItem } from '../services/marketplace'

function useFetchProducts() {
    const [data, setData] = useState<ProductListItem[]>([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        let mounted = true
        getProducts()
            .then((res) => {
                if (mounted) setData(res.items)
            })
            .catch((err: unknown) => {
                if (!mounted) return
                if (err instanceof Error) setError(err.message)
                else setError('Failed to load products')
            })
            .finally(() => {
                if (mounted) setLoading(false)
            })

        return () => {
            mounted = false
        }
    }, [])

    return { data, loading, error }
}

export default useFetchProducts

