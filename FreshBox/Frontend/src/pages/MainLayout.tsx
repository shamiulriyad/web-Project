import React from 'react'
import { Outlet } from 'react-router-dom'

export default function MainLayout() {
  return (
    <div>
      <header>
        <h1>My Shop</h1>
        {/* Add nav links if needed */}
      </header>
      <main>
        <Outlet /> {/* ← Nested routes render here */}
      </main>
      <footer>
        <p>© 2026 My Shop</p>
      </footer>
    </div>
  )
}