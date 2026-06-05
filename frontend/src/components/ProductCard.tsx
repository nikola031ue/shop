import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { addToCart } from '../api/cart'
import type { Product } from '../types'

interface Props {
  product: Product
}

export function ProductCard({ product }: Props) {
  const queryClient = useQueryClient()
  const [added, setAdded] = useState(false)

  const mutation = useMutation({
    mutationFn: () => addToCart(product.id, 1),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] })
      setAdded(true)
      setTimeout(() => setAdded(false), 1500)
    },
  })

  const outOfStock = product.stock === 0

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden flex flex-col hover:shadow-md transition-shadow">
      <div className="bg-gray-100 h-48 flex items-center justify-center">
        {product.imageUrl ? (
          <img
            src={product.imageUrl}
            alt={product.name}
            className="h-full w-full object-cover"
          />
        ) : (
          <span className="text-gray-300 text-5xl select-none">🛍️</span>
        )}
      </div>

      <div className="p-4 flex flex-col flex-1 gap-2">
        <h3 className="font-semibold text-gray-900 text-sm leading-tight line-clamp-2">
          {product.name}
        </h3>
        <p className="text-gray-500 text-xs line-clamp-2 flex-1">{product.description}</p>

        <div className="flex items-center justify-between mt-2">
          <span className="text-lg font-bold text-gray-900">
            ${product.price.toFixed(2)}
          </span>
          {outOfStock ? (
            <span className="text-xs text-red-500 font-medium">Nema na stanju</span>
          ) : (
            <span className="text-xs text-gray-400">{product.stock} kom</span>
          )}
        </div>

        <button
          onClick={() => mutation.mutate()}
          disabled={outOfStock || mutation.isPending || added}
          className={`mt-1 w-full py-2 px-4 rounded-lg text-sm font-medium transition-colors
            ${added
              ? 'bg-green-500 text-white'
              : outOfStock
              ? 'bg-gray-100 text-gray-400 cursor-not-allowed'
              : 'bg-indigo-600 text-white hover:bg-indigo-700 active:bg-indigo-800 disabled:opacity-70'
            }`}
        >
          {added ? '✓ Dodato' : mutation.isPending ? 'Dodavanje...' : 'Dodaj u korpu'}
        </button>
      </div>
    </div>
  )
}
