import { Link } from 'react-router-dom'
import { motion } from 'framer-motion'
import { Heart } from 'lucide-react'
import { addToCart, getCurrentUserId } from '../services/marketplace'

type Product = {
  id: string | number
  name?: string
  price?: number
  discountPrice?: number | null
  image?: string
}

export default function ProductCard({ product }: { product: Product }) {
  const onAdd = async () => {
    try {
      const userId = getCurrentUserId()
      await addToCart({ userId, productId: String(product.id), quantity: 1 })
    } catch {
      alert('Please login as customer to add items to cart.')
    }
  }

  const salePrice = Number(product.discountPrice ?? product.price ?? 0)
  const hasDiscount = product.discountPrice !== null && product.discountPrice !== undefined && Number(product.discountPrice) < Number(product.price ?? 0)

  return (
    <motion.div whileHover={{ scale: 1.02 }} className="bg-white p-4 rounded-xl shadow-soft">
      <Link to={`/product/${product.id}`}>
        <div className="relative overflow-hidden rounded-xl">
          {hasDiscount && (
            <span className="absolute top-3 left-3 bg-accent text-dibi-900 text-xs px-2 py-1 rounded">Sale</span>
          )}
          <motion.img src={product.image || '/assets/placeholder.png'} alt={product.name} className="w-full h-52 object-cover rounded-lg" whileHover={{ scale: 1.03 }} />
        </div>
      </Link>

      <div className="mt-3 flex items-start justify-between">
        <div>
          <h3 className="font-semibold text-sm text-dibi-900">{product?.name ?? 'Product'}</h3>
          <div className="text-sm text-neutral-700 mt-1">
            {hasDiscount ? (
              <>
                <span className="text-accent font-semibold">${salePrice.toFixed(2)}</span>
                <span className="line-through text-dibi-200 ml-2">${(Number(product?.price ?? 0)).toFixed(2)}</span>
              </>
            ) : (
              <span className="font-semibold text-dibi-900">${salePrice.toFixed(2)}</span>
            )}
          </div>
        </div>

        <div className="flex flex-col items-end gap-2">
          <button onClick={onAdd} className="bg-dibi-500 text-white px-3 py-1 rounded-md text-sm">Add</button>
          <button className="text-gray-500"><Heart size={16} /></button>
        </div>
      </div>
    </motion.div>
  )
}
