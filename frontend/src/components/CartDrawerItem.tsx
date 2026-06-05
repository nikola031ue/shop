import { useMutation, useQueryClient } from '@tanstack/react-query'
import { removeCartItem, updateCartItem } from '../api/cart'
import type { CartItem } from '../types'

interface Props {
  item: CartItem
}

export function CartDrawerItem({ item }: Props) {
  const queryClient = useQueryClient()

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ['cart'] })

  const updateMutation = useMutation({
    mutationFn: (quantity: number) => updateCartItem(item.cartItemId, quantity),
    onSuccess: invalidate,
  })

  const removeMutation = useMutation({
    mutationFn: () => removeCartItem(item.cartItemId),
    onSuccess: invalidate,
  })

  const isPending = updateMutation.isPending || removeMutation.isPending

  return (
    <li className="flex gap-3 py-4">
      <div className="w-14 h-14 rounded-lg bg-gray-100 flex items-center justify-center shrink-0 text-2xl select-none">
        🛍️
      </div>

      <div className="flex-1 min-w-0">
        <p className="text-sm font-medium text-gray-900 truncate">{item.productName}</p>
        <p className="text-sm text-gray-500 mt-0.5">${item.unitPrice.toFixed(2)} / kom</p>

        <div className="flex items-center gap-2 mt-2">
          <button
            onClick={() => updateMutation.mutate(item.quantity - 1)}
            disabled={isPending || item.quantity <= 1}
            className="w-7 h-7 rounded-md border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-center text-base leading-none"
            aria-label="Smanji količinu"
          >
            −
          </button>
          <span className="w-6 text-center text-sm font-medium text-gray-900">
            {item.quantity}
          </span>
          <button
            onClick={() => updateMutation.mutate(item.quantity + 1)}
            disabled={isPending}
            className="w-7 h-7 rounded-md border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-center text-base leading-none"
            aria-label="Povećaj količinu"
          >
            +
          </button>
        </div>
      </div>

      <div className="flex flex-col items-end justify-between shrink-0">
        <button
          onClick={() => removeMutation.mutate()}
          disabled={isPending}
          className="text-gray-300 hover:text-red-400 disabled:opacity-40 transition-colors"
          aria-label={`Ukloni ${item.productName}`}
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        <p className="text-sm font-semibold text-gray-900">
          ${(item.unitPrice * item.quantity).toFixed(2)}
        </p>
      </div>
    </li>
  )
}
