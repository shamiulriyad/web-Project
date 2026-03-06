import React from 'react'
import { Outlet } from 'react-router-dom'

export default function AdminLayout() {
  return (
    <div className="min-h-screen bg-dibi-50 text-neutral-700">
      <div className="bg-white border-b border-dibi-200 p-4 text-dibi-900">Admin Header</div>
      <main className="p-6">
        <Outlet />
      </main>
    </div>
  )
}
