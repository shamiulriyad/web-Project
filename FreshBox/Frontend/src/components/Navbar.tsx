import { useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { ShoppingCart, Heart, Menu, X } from 'lucide-react'
import useCategories from '../hooks/useCategories'
import { useAuth } from '../context/useAuth'

// Define Category type
interface Category {
  id: string | number;
  slug: string;
  name: string;
}

export type NavVariant = 'guest' | 'user' | 'auto'

export default function Navbar({ variant = 'auto' }: { variant?: NavVariant }) {
  const { data: categories, loading } = useCategories()
  const { isLoggedIn, logout } = useAuth()
  const location = useLocation()
  const [open, setOpen] = useState(false)

  // Force guest on explicit public routes like landing, login, register
  const publicPaths = ['/', '/login', '/register']
  const effective: 'guest' | 'user' = variant === 'auto'
    ? (publicPaths.includes(location.pathname) ? 'guest' : (isLoggedIn ? 'user' : 'guest'))
    : variant

  return (
    <header className="sticky top-0 z-40 text-dibi-900 border-b bg-dibi-50">
      <div className="max-w-6xl mx-auto px-6 py-3 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-3">
          <div className="w-10 h-10 bg-dibi-500 rounded-full shadow-soft flex items-center justify-center text-white font-bold">D</div>
          <span className="text-lg font-semibold text-dibi-900">Dibi</span>
        </Link>

        <div className="hidden md:flex items-center gap-4">
          <div className="relative">
            <select className="px-3 py-2 border rounded-lg bg-dibi-50">
              <option>All categories</option>
              {!loading && Array.isArray(categories) && categories.map((c: Category) => (
                <option key={c.id} value={c.slug}>{c.name}</option>
              ))}
            </select>
          </div>

          <div className="relative">
            <input placeholder="Search products..." className="px-4 py-2 rounded-lg border w-72" />
          </div>
        </div>

        <nav className="flex items-center gap-4">
          <Link to="/shop" className="text-sm text-gray-700 hidden md:inline">Shop</Link>

          {effective === 'guest' ? (
            <>
              <Link to="/login" className="px-4 py-2 rounded-xl bg-white border border-dibi-200 text-dibi-900 hidden md:inline">Login</Link>
              <Link to="/register" className="px-4 py-2 rounded-xl bg-dibi-500 text-white hidden md:inline">Register</Link>
            </>
          ) : (
            <>
              <Link to="/wishlist" className="text-gray-700"><Heart size={18} /></Link>
              <Link to="/cart" className="relative text-gray-700"><ShoppingCart size={18} /><span className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full px-1">3</span></Link>
              <Link to="/profile" className="hidden md:inline text-sm text-dibi-900">Profile</Link>
              <Link to="/orders" className="hidden md:inline text-sm text-dibi-900">Orders</Link>
              <button onClick={() => logout()} className="px-3 py-1 rounded-md text-sm text-dibi-700 border border-dibi-200 hidden md:inline">Sign out</button>
            </>
          )}

          <button className="md:hidden p-2" onClick={() => setOpen(!open)}>
            {open ? <X size={20} /> : <Menu size={20} />}
          </button>
        </nav>
      </div>

      {/* Mobile menu */}
      {open && (
        <div className="md:hidden bg-dibi-50 border-t border-dibi-200">
          <div className="px-4 py-3 flex flex-col gap-2">
            {effective === 'guest' ? (
              <>
                <Link to="/login" className="px-4 py-2 rounded-md bg-white border border-dibi-200 text-dibi-900">Login</Link>
                <Link to="/register" className="px-4 py-2 rounded-md bg-dibi-500 text-white">Register</Link>
              </>
            ) : (
              <>
                <Link to="/profile" className="px-4 py-2 rounded-md bg-white border border-dibi-200 text-dibi-900">Profile</Link>
                <Link to="/orders" className="px-4 py-2 rounded-md bg-white border border-dibi-200 text-dibi-900">Orders</Link>
                <Link to="/cart" className="px-4 py-2 rounded-md bg-white border border-dibi-200 flex items-center gap-2 text-dibi-900">Cart</Link>
                <button onClick={() => logout()} className="px-4 py-2 rounded-md bg-transparent border border-dibi-200 text-dibi-900">Sign out</button>
              </>
            )}
          </div>
        </div>
      )}
    </header>
  )
}
