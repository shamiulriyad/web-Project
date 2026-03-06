module.exports = {
  content: [
    './index.html',
    './src/**/*.{ts,tsx,js,jsx}',
  ],
  theme: {
    extend: {
      colors: {
        dibi: {
          50: '#F1F8E9',
          100: '#EEFBEF',
          200: '#D4F5D1',
          400: '#7FD66A',
          500: '#3BB02B',
          700: '#2A7B1B',
          900: '#18440B'
        }
      },
      boxShadow: {
        soft: '0 6px 20px rgba(16, 24, 40, 0.08)'
      },
      borderRadius: {
        xl: '14px'
      }
    }
  },
  plugins: [],
}
