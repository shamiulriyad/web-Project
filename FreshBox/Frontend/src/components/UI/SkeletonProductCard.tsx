import React from 'react'

export default function SkeletonProductCard() {
  return (
    <div className="bg-white p-4 rounded-2xl shadow-soft animate-pulse">
      <div className="h-52 bg-gray-100 rounded-lg mb-4" />
      <div className="h-4 bg-gray-100 rounded w-3/4 mb-2" />
      <div className="h-4 bg-gray-100 rounded w-1/2" />
      <div className="mt-4 h-10 bg-gray-100 rounded" />
    </div>
  )
}
