import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { getProducts } from '../api/products'
import { deleteProduct } from '../api/adminProducts'
import { useAuth } from '../context/AuthContext'
import { useDebounce } from '../hooks/useDebounce'
import { SearchBar } from '../components/SearchBar'
import { ProductFormModal } from '../components/admin/ProductFormModal'
import { DeleteConfirmModal } from '../components/admin/DeleteConfirmModal'
import type { Product } from '../types'

const PAGE_SIZE = 20

export function AdminProductsPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const debouncedSearch = useDebounce(search, 400)

  const [formOpen, setFormOpen] = useState(false)
  const [editProduct, setEditProduct] = useState<Product | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['admin-products', debouncedSearch, page],
    queryFn: () => getProducts({ search: debouncedSearch || undefined, page, pageSize: PAGE_SIZE }),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: string) => deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-products'] })
      queryClient.invalidateQueries({ queryKey: ['products'] })
      setDeleteTarget(null)
    },
  })

  function handleLogout() {
    logout()
    navigate('/admin/login', { replace: true })
  }

  function handleEdit(product: Product) {
    setEditProduct(product)
    setFormOpen(true)
  }

  function handleAdd() {
    setEditProduct(null)
    setFormOpen(true)
  }

  function handleFormClose() {
    setFormOpen(false)
    setEditProduct(null)
  }

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 0

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <span className="font-bold text-gray-900">🛒 Admin</span>
            <span className="text-gray-300">|</span>
            <span className="text-sm text-gray-500">Artikli</span>
          </div>
          <div className="flex items-center gap-3">
            <SearchBar value={search} onChange={(v) => { setSearch(v); setPage(1) }} placeholder="Pretraži artikle..." />
            <button
              onClick={handleAdd}
              className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-lg hover:bg-indigo-700 transition-colors whitespace-nowrap"
            >
              <span className="text-lg leading-none">+</span> Dodaj artikal
            </button>
            <button
              onClick={handleLogout}
              className="text-sm text-gray-500 hover:text-red-500 transition-colors whitespace-nowrap"
            >
              Odjavi se
            </button>
          </div>
        </div>
      </header>

      {/* Content */}
      <main className="max-w-6xl mx-auto px-4 sm:px-6 py-6">
        <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
          {/* Table header */}
          <div className="flex items-center justify-between px-5 py-3 border-b border-gray-50">
            <p className="text-sm text-gray-500">
              {data ? `${data.totalCount} artikala` : '—'}
            </p>
          </div>

          {isLoading ? (
            <div className="divide-y divide-gray-50">
              {Array.from({ length: 8 }).map((_, i) => (
                <div key={i} className="h-14 px-5 animate-pulse flex items-center gap-4">
                  <div className="h-3 bg-gray-100 rounded w-48" />
                  <div className="h-3 bg-gray-100 rounded w-24 ml-auto" />
                </div>
              ))}
            </div>
          ) : data?.items.length === 0 ? (
            <div className="text-center py-16 text-gray-400">
              <p className="text-3xl mb-2">📦</p>
              <p>Nema artikala</p>
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-xs text-gray-500 uppercase tracking-wide">
                <tr>
                  <th className="text-left px-5 py-3 font-medium">Naziv</th>
                  <th className="text-left px-5 py-3 font-medium hidden md:table-cell">Opis</th>
                  <th className="text-right px-5 py-3 font-medium">Cijena</th>
                  <th className="text-right px-5 py-3 font-medium">Zaliha</th>
                  <th className="px-5 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {data?.items.map((product) => (
                  <tr key={product.id} className="hover:bg-gray-50/50 transition-colors">
                    <td className="px-5 py-3.5 font-medium text-gray-900 max-w-[200px] truncate">
                      {product.name}
                    </td>
                    <td className="px-5 py-3.5 text-gray-500 max-w-[300px] truncate hidden md:table-cell">
                      {product.description}
                    </td>
                    <td className="px-5 py-3.5 text-right font-medium text-gray-900">
                      ${product.price.toFixed(2)}
                    </td>
                    <td className="px-5 py-3.5 text-right">
                      <span className={`font-medium ${product.stock === 0 ? 'text-red-500' : 'text-gray-700'}`}>
                        {product.stock}
                      </span>
                    </td>
                    <td className="px-5 py-3.5">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleEdit(product)}
                          className="text-xs px-3 py-1.5 rounded-lg border border-gray-200 text-gray-600 hover:border-indigo-300 hover:text-indigo-600 transition-colors"
                        >
                          Izmijeni
                        </button>
                        <button
                          onClick={() => setDeleteTarget(product)}
                          className="text-xs px-3 py-1.5 rounded-lg border border-gray-200 text-gray-600 hover:border-red-300 hover:text-red-500 transition-colors"
                        >
                          Obriši
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between px-5 py-3 border-t border-gray-50 text-sm">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="px-3 py-1.5 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40"
              >
                ← Prethodna
              </button>
              <span className="text-gray-500">Stranica {page} od {totalPages}</span>
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="px-3 py-1.5 rounded-lg border border-gray-200 text-gray-600 hover:bg-gray-50 disabled:opacity-40"
              >
                Sljedeća →
              </button>
            </div>
          )}
        </div>
      </main>

      {formOpen && (
        <ProductFormModal
          product={editProduct}
          onClose={handleFormClose}
        />
      )}

      {deleteTarget && (
        <DeleteConfirmModal
          productName={deleteTarget.name}
          onConfirm={() => deleteMutation.mutate(deleteTarget.id)}
          onClose={() => setDeleteTarget(null)}
          isLoading={deleteMutation.isPending}
        />
      )}
    </div>
  )
}
