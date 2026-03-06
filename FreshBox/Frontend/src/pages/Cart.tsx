import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { getCart, getCurrentUserId, removeCart, updateCart, type CartView } from '../services/marketplace'

export default function Cart() {
  const [cart, setCart] = useState<CartView | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const userId = useMemo(() => getCurrentUserId(), [])

  const loadCart = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await getCart(userId)
      setCart(data)
    } catch {
      setError('Failed to load cart. Please login and try again.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadCart()
  }, [])

  const changeQuantity = async (productId: string, nextQuantity: number) => {
    try {
      await updateCart({ userId, productId, quantity: nextQuantity })
      await loadCart()
    } catch {
      setError('Failed to update item quantity.')
    }
  }

  const removeItem = async (productId: string) => {
    try {
      await removeCart({ userId, productId })
      await loadCart()
    } catch {
      setError('Failed to remove item.')
    }
  }

  return (
    <section className="max-w-5xl mx-auto px-6 py-10">
      <h1 className="text-2xl font-semibold text-dibi-900 mb-6">Your Cart</h1>

      {loading && <p className="text-sm text-neutral-600">Loading cart...</p>}
      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}

      {!loading && !error && (cart?.items.length ?? 0) === 0 && (
        <div className="bg-white rounded-xl p-6 shadow-soft">
          <p className="text-neutral-700">Your cart is empty.</p>
          <Link to="/shop" className="inline-block mt-4 px-4 py-2 rounded bg-dibi-500 text-white">Continue Shopping</Link>
        </div>
      )}

      {!loading && !error && (cart?.items.length ?? 0) > 0 && (
        <div className="grid md:grid-cols-3 gap-6">
          <div className="md:col-span-2 bg-white rounded-xl shadow-soft divide-y">
            {cart?.items.map((item) => (
              <div key={item.productId} className="p-4 flex items-center justify-between gap-4">
                <div>
                  <h3 className="font-medium text-dibi-900">{item.productName}</h3>
                  <p className="text-sm text-neutral-600">${Number(item.unitPrice).toFixed(2)} each</p>
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => changeQuantity(item.productId, Math.max(0, item.quantity - 1))}
                    className="w-8 h-8 border rounded"
                  >
                    -
                  </button>
                  <span className="w-8 text-center">{item.quantity}</span>
                  <button
                    onClick={() => changeQuantity(item.productId, item.quantity + 1)}
                    className="w-8 h-8 border rounded"
                  >
                    +
                  </button>
                </div>

                <div className="text-right">
                  <p className="font-semibold text-dibi-900">${Number(item.totalPrice).toFixed(2)}</p>
                  <button onClick={() => removeItem(item.productId)} className="text-sm text-red-600">Remove</button>
                </div>
              </div>
            ))}
          </div>

          <div className="bg-white rounded-xl p-5 shadow-soft h-fit">
            <h2 className="text-lg font-semibold mb-4">Summary</h2>
            <div className="flex justify-between text-sm mb-2">
              <span className="text-neutral-600">Items</span>
              <span>{cart?.items.reduce((sum, item) => sum + item.quantity, 0)}</span>
            </div>
            <div className="flex justify-between font-semibold text-dibi-900 pt-2 border-t">
              <span>Total</span>
              <span>${Number(cart?.totalAmount ?? 0).toFixed(2)}</span>
            </div>
            <Link to="/checkout" className="block w-full mt-5 text-center px-4 py-2 rounded bg-dibi-500 text-white">
              Proceed to Checkout
            </Link>
          </div>
        </div>
      )}
    </section>
  )
}
