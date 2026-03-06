import Navbar from '../components/Navbar'
import type { ReactNode } from 'react'
import type { NavVariant } from '../components/Navbar'
import Footer from '../components/Footer'
import { Outlet } from 'react-router-dom'

export default function MainLayout({ children, navVariant }: { children?: ReactNode, navVariant?: NavVariant }) {
  // Support both direct children (used in some routes) and nested routes via <Outlet />
  return (
    <div className="min-h-screen bg-dibi-50 text-neutral-900 antialiased">
      <Navbar variant={navVariant ?? 'auto'} />
      <main className="pt-6">
        <div className="max-w-6xl mx-auto px-6">{children ?? <Outlet />}</div>
      </main>
      <Footer />
    </div>
  )
}
