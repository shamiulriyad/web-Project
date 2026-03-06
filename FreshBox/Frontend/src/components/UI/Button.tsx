import React from 'react'

type ButtonProps = React.ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: 'primary' | 'secondary' | 'ghost'
  loading?: boolean
}

export default function Button({ variant = 'primary', loading, children, ...rest }: ButtonProps) {
  const base = 'rounded-xl px-4 py-2 font-semibold transition-shadow duration-200 shadow-sm'
  const variants: Record<string, string> = {
    primary: `bg-dibi-500 text-white hover:bg-dibi-700`,
    secondary: `bg-dibi-400 text-white hover:bg-dibi-500`,
    ghost: `bg-transparent border border-dibi-500 text-dibi-500 hover:bg-dibi-50`,
  }

  return (
    <button className={`${base} ${variants[variant]} ${loading ? 'opacity-60 cursor-wait' : ''}`} {...rest}>
      {loading ? 'Loading...' : children}
    </button>
  )
}
