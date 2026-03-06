import React from 'react'

export default function LandingPage() {
	return (
		<main style={{ backgroundColor: '#F1F8E9' }} className="min-h-screen">

			{/* ── Hero Section ── */}
			<section style={{ backgroundColor: '#DCEDC8' }}>
				<div className="max-w-6xl mx-auto px-6 py-20 grid grid-cols-1 md:grid-cols-2 gap-10 items-center">
					<div>
						<h1
							className="text-4xl md:text-5xl font-extrabold leading-tight"
							style={{ color: '#1A3A0F' }}
						>
							Pure, organic essentials — delivered with care
						</h1>
						<p className="mt-6 text-gray-600">
							Shop sustainably sourced groceries, body care, and home essentials
							crafted from nature. Small-batch quality, big-on-purpose.
						</p>
						<div className="mt-8 flex gap-4">
							<a
								href="/shop"
								className="inline-block text-white px-6 py-3 rounded-xl font-medium transition-all duration-200"
								style={{ backgroundColor: '#7CB342' }}
								onMouseOver={e => (e.currentTarget.style.backgroundColor = '#558B2F')}
								onMouseOut={e => (e.currentTarget.style.backgroundColor = '#7CB342')}
							>
								Shop Now
							</a>
							<a
								href="/about"
								className="inline-block px-6 py-3 rounded-xl font-medium border transition-all duration-200"
								style={{ borderColor: '#9CCC65', color: '#33691E' }}
							>
								Learn More
							</a>
						</div>
					</div>

					<div className="flex items-center justify-center">
						<div className="w-full max-w-md bg-white rounded-2xl p-6" style={{ boxShadow: '0 2px 12px rgba(0,0,0,0.08)' }}>
							<img
								alt="organic"
								src="/assets/hero-product.jpg"
								className="w-full h-64 object-cover rounded-xl"
							/>
						</div>
					</div>
				</div>
			</section>

			{/* ── Why Dibi ── */}
			<section style={{ backgroundColor: '#F1F8E9' }}>
				<div className="max-w-6xl mx-auto px-6 py-16">
					<h2
						className="text-2xl font-semibold mb-6"
						style={{ color: '#1A3A0F' }}
					>
						Why choose Dibi?
					</h2>
					<div className="grid grid-cols-1 md:grid-cols-3 gap-6">
						{[
							{
								title: 'Sustainably Sourced',
								body: 'We partner with small farms and artisans who follow regenerative practices.',
							},
							{
								title: 'Thoughtful Packaging',
								body: 'Minimal, recyclable packaging to reduce waste.',
							},
							{
								title: 'Fair Trade',
								body: 'Fair prices for producers and transparent sourcing.',
							},
						].map((item) => (
							<div
								key={item.title}
								className="p-6 bg-white rounded-xl"
								style={{ boxShadow: '0 2px 12px rgba(0,0,0,0.06)' }}
							>
								<span
									className="inline-block w-2 h-2 rounded-full mb-3"
									style={{ backgroundColor: '#F9A825' }}
								/>
								<h3 className="font-semibold" style={{ color: '#1A3A0F' }}>
									{item.title}
								</h3>
								<p className="mt-2 text-sm text-gray-600">{item.body}</p>
							</div>
						))}
					</div>
				</div>
			</section>

		</main>
	)
}