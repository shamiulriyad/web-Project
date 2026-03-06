import { useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  checkoutFromCart,
  getAddresses,
  getCart,
  getCurrentUserId,
  submitPayment,
  type AddressView,
  type CartView,
} from '../services/marketplace'

type PaymentMethod = 'BKash' | 'Nagad' | 'SSLCommerz' | 'Rocket'

export default function Checkout() {
  const navigate = useNavigate()
  const userId = useMemo(() => getCurrentUserId(), [])
  const [cart, setCart] = useState<CartView | null>(null)
  const [addresses, setAddresses] = useState<AddressView[]>([])
  const [selectedAddressId, setSelectedAddressId] = useState('')
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('BKash')
  const [discountCode, setDiscountCode] = useState('')
  const [placing, setPlacing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const load = async () => {
      try {
        setError(null)
        const [cartData, addressData] = await Promise.all([getCart(userId), getAddresses(userId)])
        setCart(cartData)
        setAddresses(addressData)
        const defaultAddress = addressData.find((a) => a.isDefault)
        setSelectedAddressId(defaultAddress?.id ?? addressData[0]?.id ?? '')
      } catch {
        setError('Failed to load checkout data. Please login and try again.')
      }
    }

    void load()
  }, [])

  const placeOrder = async () => {
    if (!selectedAddressId) {
      setError('Please select an address.')
      return
    }
    if (!cart || cart.items.length === 0) {
      setError('Cart is empty.')
      return
    }

    try {
      setPlacing(true)
      setError(null)

      const order = await checkoutFromCart({
        userId,
        addressId: selectedAddressId,
        paymentMethod,
        discountCode: discountCode.trim() || undefined,
      })

      const orderId = String(order?.id)
      if (!orderId) {
        throw new Error('Order creation failed')
      }

      const payableAmount = Number(order?.finalAmount ?? order?.totalAmount ?? cart.totalAmount)

      await submitPayment({
        orderId,
        userId,
        amount: payableAmount,
        paymentMethod,
        transactionId: `${paymentMethod}-${Date.now()}`,
      })

      navigate('/checkout/success')
    } catch {
      setError('Checkout failed. Please verify your login, address, and cart, then try again.')
      navigate('/checkout/failed')
    } finally {
      setPlacing(false)
    }
  }

  return (
    <section className="max-w-5xl mx-auto px-6 py-10">
      <h1 className="text-2xl font-semibold text-dibi-900 mb-6">Checkout</h1>
      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}

      <div className="grid md:grid-cols-3 gap-6">
        <div className="md:col-span-2 space-y-6">
          <div className="bg-white rounded-xl shadow-soft p-5">
            <h2 className="text-lg font-semibold mb-3">Delivery Address</h2>
            {addresses.length === 0 && (
              <p className="text-sm text-neutral-600">No address found. Please add one from profile address management.</p>
            )}
            <div className="space-y-2">
              {addresses.map((address) => (
                <label key={address.id} className="flex items-start gap-3 border rounded p-3 cursor-pointer">
                  <input
                    type="radio"
                    name="address"
                    checked={selectedAddressId === address.id}
                    onChange={() => setSelectedAddressId(address.id)}
                  />
                  <div>
                    <p className="font-medium">{address.name} {address.isDefault && <span className="text-xs text-dibi-500">(Default)</span>}</p>
                    <p className="text-sm text-neutral-700">{address.phone}</p>
                    <p className="text-sm text-neutral-700">{address.addressLine}, {address.city}</p>
                  </div>
                </label>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-soft p-5">
            <h2 className="text-lg font-semibold mb-3">Payment Method</h2>
            <select
              value={paymentMethod}
              onChange={(e) => setPaymentMethod(e.target.value as PaymentMethod)}
              className="w-full border rounded px-3 py-2"
            >
              <option value="BKash">BKash</option>
              <option value="Nagad">Nagad</option>
              <option value="SSLCommerz">SSLCommerz</option>
              <option value="Rocket">Rocket</option>
            </select>

            <div className="mt-4">
              <label className="block text-sm mb-1">Discount Code (optional)</label>
              <input
                value={discountCode}
                onChange={(e) => setDiscountCode(e.target.value)}
                placeholder="Enter coupon code"
                className="w-full border rounded px-3 py-2"
              />
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-soft p-5 h-fit">
          <h2 className="text-lg font-semibold mb-4">Order Summary</h2>
          <div className="space-y-2 text-sm">
            {cart?.items.map((item) => (
              <div key={item.productId} className="flex justify-between gap-3">
                <span className="text-neutral-700">{item.productName} × {item.quantity}</span>
                <span>${Number(item.totalPrice).toFixed(2)}</span>
              </div>
            ))}
          </div>
          <div className="border-t mt-4 pt-3 flex justify-between font-semibold text-dibi-900">
            <span>Total</span>
            <span>${Number(cart?.totalAmount ?? 0).toFixed(2)}</span>
          </div>

          <button
            onClick={placeOrder}
            disabled={placing || !cart || cart.items.length === 0 || addresses.length === 0}
            className="w-full mt-4 px-4 py-2 rounded bg-dibi-500 text-white disabled:opacity-60"
          >
            {placing ? 'Placing Order...' : 'Place Order'}
          </button>
        </div>
      </div>
    </section>
  )
}
