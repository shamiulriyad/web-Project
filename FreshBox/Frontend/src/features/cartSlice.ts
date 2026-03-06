import { createSlice } from '@reduxjs/toolkit'
import type { PayloadAction } from '@reduxjs/toolkit'

type CartItem = {
  id: string
  name: string
  price: number
  qty: number
}

const initialState: { items: CartItem[] } = { items: [] }

const cartSlice = createSlice({
  name: 'cart',
  initialState,
  reducers: {
    addItem(state, action: PayloadAction<CartItem>) {
      const existing = state.items.find(i => i.id === action.payload.id)
      if (existing) existing.qty += action.payload.qty
      else state.items.push(action.payload)
    },
    removeItem(state, action: PayloadAction<string>) {
      state.items = state.items.filter(i => i.id !== action.payload)
    },
    clear(state) {
      state.items = []
    }
  }
})

export const { addItem, removeItem, clear } = cartSlice.actions
export default cartSlice.reducer
