/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}", // IMPORTANT: Tells Tailwind where your code is
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#8A2BE2', // Electric Purple
          light: '#9D4EDD',
          dark: '#7B27CC',
        },
        dark: {
          950: '#0F1115', // Main background
          900: '#1E2025', // Cards
          800: '#2D2F36', // Borders
        }
      }
    },
  },
  plugins: [],
}