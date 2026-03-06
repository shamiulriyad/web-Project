import React from 'react'
import { ShoppingCart } from 'lucide-react'
import { Link } from 'react-router-dom'

export default function Header() {
  return (
    <header className="bg-dibi-50 border-b border-dibi-200">
      <div className="max-w-6xl mx-auto px-6 py-4 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-3">
          <div className="w-10 h-10 bg-dibi-500 rounded-full shadow-soft flex items-center justify-center text-white font-bold">D</div>
          <span className="text-lg font-semibold">Dibi</span>
        </Link>

        <nav className="flex items-center gap-4">
          <Link to="/shop" className="text-sm text-gray-700">Shop</Link>
          <Link to="/about" className="text-sm text-gray-700">About</Link>
          <Link to="/login" className="text-sm text-gray-700">Login</Link>
          <Link to="/register" className="text-sm text-gray-700">Register</Link>
          <button className="p-2 rounded-md bg-transparent hover:bg-dibi-100">
            <ShoppingCart size={18} />
          </button>
        </nav>
      </div>
    </header>
  )
}
