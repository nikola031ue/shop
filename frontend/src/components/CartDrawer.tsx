import { useEffect } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getCart, clearCart } from '../api/cart'
import { useCart } from '../context/CartContext'
import { CartDrawerItem } from './CartDrawerItem'

export function CartDrawer() {
  const { isOpen, closeCart } = useCart()
  const queryClient = useQueryClient()

  const { data: cart, isLoading } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
    enabled: isOpen,
  })

  const clearMutation = useMutation({
    mutationFn: clearCart,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['cart'] }),
  })

  useEffect(() => {
    if (!isOpen) return
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && closeCart()
    document.addEventListener('keydown', onKey)
    return () => document.removeEventListener('keydown', onKey)
  }, [isOpen, closeCart])

  const itemCount = cart?.items.reduce((sum, i) => sum + i.quantity, 0) ?? 0

  return (
    <>
      {/* Overlay */}
      <div
        className={`fixed inset-0 bg-black/40 z-40 transition-opacity duration-300 ${
          isOpen ? 'opacity-100' : 'opacity-0 pointer-events-none'
        }`}
        onClick={closeCart}
        aria-hidden="true"
      />

      {/* Drawer panel */}
      <aside
        role="dialog"
        aria-label="Korpa"
        aria-modal="true"
        className={`fixed right-0 top-0 h-full w-full max-w-sm bg-white shadow-2xl z-50 flex flex-col
          transform transition-transform duration-300 ease-in-out
          ${isOpen ? 'translate-x-0' : 'translate-x-full'}`}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-5 py-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900">
            Korpa
            {itemCount > 0 && (
              <span className="ml-2 text-sm font-normal text-gray-500">({itemCount} artikala)</span>
            )}
          </h2>
          <button
            onClick={closeCart}
            className="text-gray-400 hover:text-gray-600 transition-colors"
            aria-label="Zatvori korpu"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Items */}
        <div className="flex-1 overflow-y-auto px-5">
          {isLoading && (
            <div className="flex flex-col gap-4 py-4">
              {[1, 2, 3].map((i) => (
                <div key={i} className="h-16 bg-gray-100 rounded-lg animate-pulse" />
              ))}
            </div>
          )}

          {!isLoading && cart?.items.length === 0 && (
            <div className="flex flex-col items-center justify-center h-full text-center py-16">
              <span className="text-5xl mb-4">🛒</span>
              <p className="text-gray-500 font-medium">Korpa je prazna</p>
              <p className="text-gray-400 text-sm mt-1">Dodaj neke proizvode!</p>
            </div>
          )}

          {cart && cart.items.length > 0 && (
            <ul className="divide-y divide-gray-50">
              {cart.items.map((item) => (
                <CartDrawerItem key={item.cartItemId} item={item} />
              ))}
            </ul>
          )}
        </div>

        {/* Footer */}
        {cart && cart.items.length > 0 && (
          <div className="border-t border-gray-100 px-5 py-4 space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-500">Ukupno</span>
              <span className="text-lg font-bold text-gray-900">${cart.total.toFixed(2)}</span>
            </div>

            <button
              className="w-full py-3 px-4 bg-indigo-600 text-white font-medium rounded-xl hover:bg-indigo-700 active:bg-indigo-800 transition-colors"
              onClick={() => {/* checkout - tiket #24 */}}
            >
              Nastavi sa plaćanjem →
            </button>

            <button
              onClick={() => clearMutation.mutate()}
              disabled={clearMutation.isPending}
              className="w-full py-2 text-sm text-gray-400 hover:text-red-500 transition-colors disabled:opacity-40"
            >
              Isprazni korpu
            </button>
          </div>
        )}
      </aside>
    </>
  )
}
