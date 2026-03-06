// page should not include layout; Router wraps pages with MainLayout
import Hero from '../components/Hero'
import ProductCard from '../components/ProductCard'
import useFetchProducts from '../hooks/useFetchProducts'
import SkeletonProductCard from '../components/UI/SkeletonProductCard'

export default function Home() {
  const { data: products, loading } = useFetchProducts()

  return (
    <>
      <Hero />

      <section className="max-w-6xl mx-auto px-6 py-12">
        <h2 className="text-2xl font-semibold text-gray-900 mb-6">Featured Products</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6">
          {loading
            ? Array.from({ length: 6 }).map((_, i) => <SkeletonProductCard key={i} />)
            : products.map((p) => <ProductCard key={p.id} product={p} />)
          }
        </div>
      </section>
    </>
  )
}
