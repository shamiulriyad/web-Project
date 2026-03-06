import React from 'react'
import { Outlet } from 'react-router-dom'

export default function AdminLayout() {
  return (
    <div>
      <aside>
        <h2>Admin Panel</h2>
        {/* Sidebar links */}
      </aside>
      <main>
        <Outlet /> {/* ← Nested admin pages render here */}
      </main>
    </div>
  )
}