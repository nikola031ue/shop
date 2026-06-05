import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../api/products'
import { ProductCard } from '../components/ProductCard'
import { SearchBar } from '../components/SearchBar'
import { Pagination } from '../components/Pagination'
import { useDebounce } from '../hooks/useDebounce'

const PAGE_SIZE = 12

export function ProductsPage() {
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)

  const debouncedSearch = useDebounce(search, 400)

  const { data, isLoading, isError } = useQuery({
    queryKey: ['products', debouncedSearch, page],
    queryFn: () => getProducts({ search: debouncedSearch || undefined, page, pageSize: PAGE_SIZE }),
    placeholderData: (prev) => prev,
  })

  function handleSearchChange(value: string) {
    setSearch(value)
    setPage(1)
  }

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 0

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between gap-4">
          <h1 className="text-xl font-bold text-gray-900 whitespace-nowrap">🛒 Shop</h1>
          <SearchBar value={search} onChange={handleSearchChange} />
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
