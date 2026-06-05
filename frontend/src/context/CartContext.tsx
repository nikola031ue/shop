import { createContext, useContext, useState, type ReactNode } from 'react'

interface CartContextValue {
  isOpen: boolean
  openCart: () => void
  closeCart: () => void
}

const CartContext = createContext<CartContextValue | null>(null)

export function CartProvider({ children }: { children: ReactNode }) {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <CartContext.Provider
      value={{ isOpen, openCart: () => setIsOpen(true), closeCart: () => setIsOpen(false) }}
    >
      {children}
    </CartContext.Provider>
  )
}

export function useCart() {
  const ctx = useContext(CartContext)
  if (!ctx) throw new Error('useCart must be used within CartProvider')
  return ctx
}
