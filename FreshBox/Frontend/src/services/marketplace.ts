import api from './api'

export type ProductListItem = {
  id: string
  name: string
  slug: string
  price: number
  discountPrice?: number | null
  stockQuantity: number
  sellerId: string
  categoryId: string
  averageRating: number
}

export type ProductDetails = {
  id: string
  name: string
  slug: string
  description?: string
  price: number
  discountPrice?: number | null
  stockQuantity: number
  sellerId: string
  categoryId: string
  averageRating: number
  totalReviews: number
}

export type CartItemView = {
  productId: string
  productName: string
  quantity: number
  unitPrice: number
  totalPrice: number
}

export type CartView = {
  userId: string
  items: CartItemView[]
  totalAmount: number
}

export type AddressView = {
  id: string
  userId: string
  name: string
  phone: string
  addressLine: string
  city: string
  postalCode?: string
  isDefault: boolean
}

export type ReviewView = {
  id: string
  productId: string
  userId: string
  userName: string
  rating: number
  comment?: string
  createdAt: string
}

type ProductListResponse = {
  total: number
  page: number
  pageSize: number
  items: ProductListItem[]
}

type ProductReviewsResponse = {
  averageRating: number
  totalReviews: number
  reviews: ReviewView[]
}

type JwtPayload = {
  sub?: string
  nameid?: string
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string
}

function parseJwtPayload(token: string): JwtPayload | null {
  try {
    const parts = token.split('.')
    if (parts.length < 2) return null
    const payload = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const normalized = payload + '='.repeat((4 - (payload.length % 4)) % 4)
    const decoded = atob(normalized)
    return JSON.parse(decoded) as JwtPayload
  } catch {
    return null
  }
}

export function getCurrentUserId(): string {
  const token = localStorage.getItem('token')
  if (token) {
    const payload = parseJwtPayload(token)
    const tokenUserId = payload?.nameid || payload?.sub || payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier']
    if (tokenUserId) {
      localStorage.setItem('demoUserId', tokenUserId)
      return tokenUserId
    }
  }

  const key = 'demoUserId'
  const existing = localStorage.getItem(key)
  if (existing) return existing
  const created = crypto.randomUUID()
  localStorage.setItem(key, created)
  return created
}

export async function getProducts(params?: {
  categoryId?: string
  sellerId?: string
  minPrice?: number
  maxPrice?: number
  minRating?: number
  q?: string
}) {
  const response = await api.get<ProductListResponse>('/products', { params })
  return response.data
}

export async function getProductDetails(productId: string) {
  const response = await api.get<ProductDetails>(`/products/${productId}`)
  return response.data
}

export async function getProductReviews(productId: string) {
  const response = await api.get<ProductReviewsResponse>(`/products/${productId}/reviews`)
  return response.data
}

export async function createReview(payload: { productId: string; userId: string; rating: number; comment?: string }) {
  const response = await api.post('/reviews', payload)
  return response.data
}

export async function addToCart(payload: { userId: string; productId: string; quantity: number }) {
  const response = await api.post('/cart/add', payload)
  return response.data
}

export async function getCart(userId: string) {
  const response = await api.get<CartView>('/cart', { params: { userId } })
  return response.data
}

export async function updateCart(payload: { userId: string; productId: string; quantity: number }) {
  const response = await api.put('/cart/update', payload)
  return response.data
}

export async function removeCart(payload: { userId: string; productId: string }) {
  const response = await api.delete('/cart/remove', { data: payload })
  return response.data
}

export async function getAddresses(userId: string) {
  const response = await api.get<AddressView[]>('/addresses', { params: { userId } })
  return response.data
}

export async function createAddress(payload: {
  userId: string
  name: string
  phone: string
  addressLine: string
  city: string
  postalCode?: string
  isDefault: boolean
}) {
  const response = await api.post('/addresses', payload)
  return response.data
}

export async function updateAddress(
  id: string,
  payload: {
    userId: string
    name: string
    phone: string
    addressLine: string
    city: string
    postalCode?: string
    isDefault: boolean
  },
) {
  const response = await api.put(`/addresses/${id}`, payload)
  return response.data
}

export async function deleteAddress(id: string, userId: string) {
  const response = await api.delete(`/addresses/${id}`, { params: { userId } })
  return response.data
}

export async function checkoutFromCart(payload: {
  userId: string
  addressId: string
  paymentMethod: 'BKash' | 'Nagad' | 'SSLCommerz' | 'Rocket'
  discountCode?: string
}) {
  const response = await api.post('/orders/checkout-from-cart', payload)
  return response.data
}

export async function submitPayment(payload: {
  orderId: string
  userId: string
  amount: number
  paymentMethod: 'BKash' | 'Nagad' | 'SSLCommerz' | 'Rocket'
  transactionId: string
}) {
  const response = await api.post('/payments/submit', payload)
  return response.data
}
