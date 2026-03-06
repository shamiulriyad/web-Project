import { useEffect, useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import {
  addToCart,
  createReview,
  getCurrentUserId,
  getProductDetails,
  getProductReviews,
  type ProductDetails as ProductDetailsModel,
  type ReviewView,
} from '../services/marketplace'

export default function ProductDetails() {
  const { id } = useParams()
  const userId = useMemo(() => getCurrentUserId(), [])

  const [product, setProduct] = useState<ProductDetailsModel | null>(null)
  const [reviews, setReviews] = useState<ReviewView[]>([])
  const [averageRating, setAverageRating] = useState(0)
  const [totalReviews, setTotalReviews] = useState(0)
  const [quantity, setQuantity] = useState(1)
  const [rating, setRating] = useState(5)
  const [comment, setComment] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = async () => {
    if (!id) return
    try {
      setLoading(true)
      setError(null)
      const [productData, reviewData] = await Promise.all([getProductDetails(id), getProductReviews(id)])
      setProduct(productData)
      setReviews(reviewData.reviews)
      setAverageRating(Number(reviewData.averageRating ?? 0))
      setTotalReviews(Number(reviewData.totalReviews ?? 0))
    } catch {
      setError('Failed to load product details.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [id])

  const onAddToCart = async () => {
    if (!product) return
    try {
      await addToCart({ userId, productId: product.id, quantity })
      alert('Added to cart')
    } catch {
      setError('Failed to add to cart. Please login as a customer.')
    }
  }

  const onSubmitReview = async () => {
    if (!id) return
    try {
      await createReview({ productId: id, userId, rating, comment: comment.trim() || undefined })
      setComment('')
      await load()
    } catch {
      setError('Failed to submit review.')
    }
  }

  if (loading) return <div className="max-w-5xl mx-auto p-6">Loading product...</div>

  if (!product) return <div className="max-w-5xl mx-auto p-6">Product not found.</div>

  const displayPrice = Number(product.discountPrice ?? product.price)
  const hasDiscount = product.discountPrice !== null && product.discountPrice !== undefined && Number(product.discountPrice) < Number(product.price)

  return (
    <section className="max-w-5xl mx-auto px-6 py-10">
      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}

      <div className="grid md:grid-cols-2 gap-8">
        <div className="bg-white rounded-xl shadow-soft p-6">
          <img src="/assets/placeholder.png" alt={product.name} className="w-full h-80 object-cover rounded-lg" />
        </div>

        <div className="space-y-4">
          <h1 className="text-2xl font-semibold text-dibi-900">{product.name}</h1>
          <p className="text-neutral-700">{product.description || 'No description available.'}</p>
          <p className="text-sm text-neutral-600">Rating: {averageRating.toFixed(2)} ({totalReviews} reviews)</p>

          <div className="text-xl">
            <span className="font-semibold text-accent">${displayPrice.toFixed(2)}</span>
            {hasDiscount && <span className="line-through text-dibi-200 ml-2 text-base">${Number(product.price).toFixed(2)}</span>}
          </div>

          <p className="text-sm text-neutral-600">In stock: {product.stockQuantity}</p>

          <div className="flex items-center gap-2">
            <button onClick={() => setQuantity((q) => Math.max(1, q - 1))} className="w-8 h-8 border rounded">-</button>
            <span className="w-8 text-center">{quantity}</span>
            <button onClick={() => setQuantity((q) => q + 1)} className="w-8 h-8 border rounded">+</button>
          </div>

          <button onClick={onAddToCart} className="px-5 py-2 rounded bg-dibi-500 text-white">Add to Cart</button>
        </div>
      </div>

      <div className="mt-10 grid md:grid-cols-2 gap-8">
        <div className="bg-white rounded-xl shadow-soft p-5">
          <h2 className="text-lg font-semibold mb-4">Write a Review</h2>
          <div className="space-y-3">
            <select value={rating} onChange={(e) => setRating(Number(e.target.value))} className="w-full border rounded px-3 py-2">
              <option value={5}>5 - Excellent</option>
              <option value={4}>4 - Good</option>
              <option value={3}>3 - Average</option>
              <option value={2}>2 - Poor</option>
              <option value={1}>1 - Bad</option>
            </select>
            <textarea
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              placeholder="Share your feedback"
              className="w-full border rounded px-3 py-2 min-h-28"
            />
            <button onClick={onSubmitReview} className="px-4 py-2 rounded bg-dibi-500 text-white">Submit Review</button>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-soft p-5">
          <h2 className="text-lg font-semibold mb-4">Customer Reviews</h2>
          <div className="space-y-3 max-h-96 overflow-auto">
            {reviews.length === 0 && <p className="text-sm text-neutral-600">No reviews yet.</p>}
            {reviews.map((review) => (
              <div key={review.id} className="border rounded p-3">
                <div className="flex justify-between">
                  <p className="font-medium">{review.userName || 'User'}</p>
                  <p className="text-sm">⭐ {review.rating}</p>
                </div>
                <p className="text-sm text-neutral-700 mt-1">{review.comment || 'No comment'}</p>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
