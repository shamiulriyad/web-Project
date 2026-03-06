import React from 'react'
import { motion } from 'framer-motion'

export default function Hero() {
  return (
    <section className="bg-dibi-50 py-12">
      <div className="max-w-6xl mx-auto px-6 flex flex-col md:flex-row items-center gap-8">
        <div className="flex-1">
          <motion.h1
            initial={{ opacity: 0, y: 8 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="text-3xl sm:text-4xl font-extrabold text-gray-900 mb-4"
          >
            Pure, organic products for a healthier life
          </motion.h1>
          <p className="text-gray-700 max-w-xl">Carefully sourced, ethically produced, and delivered fresh. Discover everyday essentials for mindful living.</p>
          <div className="mt-6">
            <a href="#" className="inline-block bg-dibi-500 text-white px-6 py-3 rounded-xl shadow-soft">Shop Now</a>
          </div>
        </div>

        <div className="flex-1">
          <div className="bg-white rounded-xl shadow-soft p-6">
            <img src="/src/assets/hero.jpg" alt="organic" className="w-full h-56 object-cover rounded-md" />
          </div>
        </div>
      </div>
    </section>
  )
}
