/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        dibi: {
          50:  '#F1F8E9', // page background — soft organic green
          100: '#DCEDC8',
          200: '#C5E1A5',
          300: '#AED581',
          400: '#9CCC65', // light green (secondary button)
          500: '#7CB342', // primary button / CTA
          600: '#558B2F', // hover state
          700: '#33691E',
          800: '#1B5E20',
          900: '#1A3A0F', // headings
        },
        accent: '#F9A825', // warm yellow accent
      },
      boxShadow: {
        soft: '0 2px 12px rgba(0, 0, 0, 0.06)',
      },
      borderRadius: {
        xl: '0.75rem',
        '2xl': '1rem',
      },
    },
  },
  plugins: [],
}