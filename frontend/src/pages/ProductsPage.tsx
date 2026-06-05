import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../api/products'
import { getCart } from '../api/cart'
import { ProductCard } from '../components/ProductCard'
import { SearchBar } from '../components/SearchBar'
import { Pagination } from '../components/Pagination'
import { useDebounce } from '../hooks/useDebounce'
import { useCart } from '../context/CartContext'

const PAGE_SIZE = 12

export function ProductsPage() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const { openCart } = useCart()

  const debouncedSearch = useDebounce(search, 400)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['products', debouncedSearch, page],
    queryFn: () => getProducts({ search: debouncedSearch || undefined, page, pageSize: PAGE_SIZE }),
    placeholderData: (prev) => prev,
  })

  const { data: cart } = useQuery({
    queryKey: ['cart'],
    queryFn: getCart,
    staleTime: 0,
  })

  const cartCount = cart?.items.reduce((sum, i) => sum + i.quantity, 0) ?? 0

  function handleSearchChange(value: string) {
    setSearch(value)
    setPage(1)
  }

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 0

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center gap-4">
          <h1 className="text-xl font-bold text-gray-900 whitespace-nowrap">🛒 Shop</h1>
          <div className="flex-1">
            <SearchBar value={search} onChange={handleSearchChange} />
          </div>
          <button
            onClick={openCart}
            className="relative p-2 text-gray-600 hover:text-indigo-600 transition-colors"
            aria-label={`Otvori korpu${cartCount > 0 ? `, ${cartCount} artikala` : ''}`}
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 11-4 0 2 2 0 014 0z"
              />
            </svg>
            {cartCount > 0 && (
              <span className="absolute -top-1 -right-1 w-5 h-5 bg-indigo-600 text-white text-xs font-bold rounded-full flex items-center justify-center leading-none">
                {cartCount > 99 ? '99+' : cartCount}
              </span>
            )}
          </button>
        </div>
      </header>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {isLoading && !data && (
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
            {Array.from({ length: PAGE_SIZE }).map((_, i) => (
              <div key={i} className="bg-white rounded-xl h-72 animate-pulse border border-gray-100" />
            ))}
          </div>
        )}

        {isError && (
          <div className="text-center py-20">
            <p className="text-red-500 font-medium">Greška pri učitavanju proizvoda.</p>
            <p className="text-gray-400 text-sm mt-1">Provjeri da li je backend pokrenut.</p>
          </div>
        )}

        {data && (
          <>
            <div className="flex items-center justify-between mb-6">
              <p className="text-sm text-gray-500">
                {data.totalCount === 0
                  ? 'Nema rezultata'
                  : `${data.totalCount} ${data.totalCount === 1 ? 'proizvod' : 'proizvoda'}`}
                {debouncedSearch && (
                  <span className="ml-1">
                    za <span className="font-medium text-gray-700">"{debouncedSearch}"</span>
                  </span>
                )}
              </p>
            </div>

            {data.items.length === 0 ? (
              <div className="text-center py-20">
                <span className="text-5xl">🔍</span>
                <p className="text-gray-500 mt-4">Nema proizvoda koji odgovaraju pretrazi.</p>
                <button
                  onClick={() => handleSearchChange('')}
                  className="mt-3 text-sm text-indigo-600 hover:underline"
                >
                  Prikaži sve proizvode
                </button>
              </div>
            ) : (
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
                {data.items.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>
            )}

            {totalPages > 1 && (
              <div className="mt-10 flex justify-center">
                <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
              </div>
            )}
          </>
        )}
      </main>
    </div>
  )
}
