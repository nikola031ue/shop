import { useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createProduct, updateProduct, type ProductPayload } from '../../api/adminProducts'
import type { Product } from '../../types'

interface Props {
  product: Product | null
  onClose: () => void
}

export function ProductFormModal({ product, onClose }: Props) {
  const isEdit = product !== null
  const queryClient = useQueryClient()

  const [form, setForm] = useState<ProductPayload>({
    name: product?.name ?? '',
    description: product?.description ?? '',
    price: product?.price ?? 0,
    stock: product?.stock ?? 0,
    imageUrl: product?.imageUrl ?? '',
  })
  const [error, setError] = useState('')

  const mutation = useMutation({
    mutationFn: (payload: ProductPayload) =>
      isEdit ? updateProduct(product.id, payload) : createProduct(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
      queryClient.invalidateQueries({ queryKey: ['products'] })
      onClose()
    },
    onError: (err: unknown) => {
      const status = (err as { response?: { status?: number } }).response?.status
      if (status === 401) setError('Sesija je istekla — prijavi se ponovo.')
      else if (status === 400) setError('Neispravni podaci — provjeri polja i pokušaj ponovo.')
      else setError('Greška pri čuvanju. Pokušaj ponovo.')
    },
  })

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    const payload: ProductPayload = {
      ...form,
      imageUrl: form.imageUrl?.trim() || null,
    }
    mutation.mutate(payload)
  }

  function set<K extends keyof ProductPayload>(key: K, value: ProductPayload[K]) {
    setForm((f) => ({ ...f, [key]: value }))
  }

  return (
    <div
      className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-2xl shadow-xl w-full max-w-md max-h-[90vh] overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900">
            {isEdit ? 'Izmijeni artikal' : 'Dodaj artikal'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
            aria-label="Zatvori"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        <form onSubmit={handleSubmit} className="px-6 py-5 space-y-4">
          <Field label="Naziv *">
            <input
              type="text"
              value={form.name}
              onChange={(e) => set('name', e.target.value)}
              required
              maxLength={200}
              className={inputClass}
              placeholder="Nike Air Max 270"
            />
          </Field>

          <Field label="Opis *">
            <textarea
              value={form.description}
              onChange={(e) => set('description', e.target.value)}
              required
              maxLength={2000}
              rows={3}
              className={inputClass + ' resize-none'}
              placeholder="Kratki opis artikla..."
            />
          </Field>

          <div className="grid grid-cols-2 gap-4">
            <Field label="Cijena ($) *">
              <input
                type="number"
                value={form.price}
                onChange={(e) => set('price', parseFloat(e.target.value) || 0)}
                required
                min={0}
                step={0.01}
                className={inputClass}
                placeholder="0.00"
              />
            </Field>

            <Field label="Zaliha *">
              <input
                type="number"
                value={form.stock}
                onChange={(e) => set('stock', parseInt(e.target.value) || 0)}
                required
                min={0}
                className={inputClass}
                placeholder="0"
              />
            </Field>
          </div>

          <Field label="URL slike (opcionalno)">
            <input
              type="url"
              value={form.imageUrl ?? ''}
              onChange={(e) => set('imageUrl', e.target.value)}
              className={inputClass}
              placeholder="https://..."
            />
          </Field>

          {error && (
            <p className="text-sm text-red-500 bg-red-50 rounded-lg px-3 py-2">{error}</p>
          )}

          <div className="flex gap-3 pt-1">
            <button
              type="button"
              onClick={onClose}
              className="flex-1 py-2.5 border border-gray-200 text-gray-600 text-sm font-medium rounded-xl hover:bg-gray-50 transition-colors"
            >
              Odustani
            </button>
            <button
              type="submit"
              disabled={mutation.isPending}
              className="flex-1 py-2.5 bg-indigo-600 text-white text-sm font-medium rounded-xl hover:bg-indigo-700 disabled:opacity-60 transition-colors"
            >
              {mutation.isPending ? 'Čuvanje...' : isEdit ? 'Sačuvaj izmene' : 'Dodaj artikal'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

const inputClass =
  'w-full px-3 py-2.5 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent'

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="space-y-1">
      <label className="block text-sm font-medium text-gray-700">{label}</label>
      {children}
    </div>
  )
}
