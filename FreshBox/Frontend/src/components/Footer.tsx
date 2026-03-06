import React from 'react'

export default function Footer() {
  return (
    <footer className="mt-12 border-t bg-dibi-50">
      <div className="max-w-6xl mx-auto px-6 py-8 text-sm text-neutral-700 flex items-center justify-between">
        <span>© {new Date().getFullYear()} Dibi — Organic Essentials</span>
        <span>Made with care • Minimal, organic design</span>
      </div>
    </footer>
  )
}
