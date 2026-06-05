import { act, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ProductsPage } from './ProductsPage'
import * as productsApi from '../api/products'

vi.mock('../api/products')

const mockProducts = (overrides: Partial<Parameters<typeof productsApi.getProducts>[0]> = {}) => {
  const items = [
    { id: '1', name: 'Nike Air Max', description: 'Opis', price: 89.99, stock: 10, imageUrl: null },
    { id: '2', name: 'Adidas Majica', description: 'Opis 2', price: 39.99, stock: 5, imageUrl: null },
  ]
  void overrides
  return Promise.resolve({ items, totalCount: 2, page: 1, pageSize: 12 })
}

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={client}>
      <ProductsPage />
    </QueryClientProvider>,
  )
}

describe('ProductsPage', () => {
  beforeEach(() => {
    vi.mocked(productsApi.getProducts).mockImplementation(mockProducts)
  })

  afterEach(() => vi.clearAllMocks())

  it('shows product names after loading', async () => {
    renderPage()

    await waitFor(() => {
      expect(screen.getByText('Nike Air Max')).toBeInTheDocument()
      expect(screen.getByText('Adidas Majica')).toBeInTheDocument()
    })
  })

  it('shows total count', async () => {
    renderPage()

    await waitFor(() => {
      expect(screen.getByText(/2 proizvoda/)).toBeInTheDocument()
    })
  })

  it('resets to page 1 when search changes', async () => {
    renderPage()
    await waitFor(() => screen.getByText('Nike Air Max'))

    const input = screen.getByPlaceholderText(/Pretraži/)
    await userEvent.type(input, 'nike')

    await waitFor(() => {
      const calls = vi.mocked(productsApi.getProducts).mock.calls
      const lastCall = calls[calls.length - 1][0]
      expect(lastCall.page).toBe(1)
    })
  })

  it('passes search term to API after debounce', async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true })
    renderPage()
    await waitFor(() => screen.getByText('Nike Air Max'))

    const input = screen.getByPlaceholderText(/Pretraži/)
    await userEvent.type(input, 'nike')
    await act(async () => { vi.advanceTimersByTime(400) })

    await waitFor(() => {
      const calls = vi.mocked(productsApi.getProducts).mock.calls
      const lastCall = calls[calls.length - 1][0]
      expect(lastCall.search).toBe('nike')
    })

    vi.useRealTimers()
  })

  it('shows empty state when no products found', async () => {
    vi.mocked(productsApi.getProducts).mockResolvedValue({
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 12,
    })

    renderPage()

    await waitFor(() => {
      expect(screen.getByText(/Nema proizvoda koji odgovaraju pretrazi/)).toBeInTheDocument()
    })
  })

  it('shows error message when API fails', async () => {
    vi.mocked(productsApi.getProducts).mockRejectedValue(new Error('Network error'))

    renderPage()

    await waitFor(() => {
      expect(screen.getByText(/Greška pri učitavanju/)).toBeInTheDocument()
    })
  })

  it('does not show pagination for single page of results', async () => {
    renderPage()
    await waitFor(() => screen.getByText('Nike Air Max'))

    expect(screen.queryByText('←')).not.toBeInTheDocument()
  })

  it('shows pagination when results span multiple pages', async () => {
    vi.mocked(productsApi.getProducts).mockResolvedValue({
      items: Array.from({ length: 12 }, (_, i) => ({
        id: `${i}`,
        name: `Product ${i}`,
        description: 'Opis',
        price: 10,
        stock: 5,
        imageUrl: null,
      })),
      totalCount: 25,
      page: 1,
      pageSize: 12,
    })

    renderPage()

    await waitFor(() => {
      const nav = screen.getByRole('navigation', { name: 'Paginacija' })
      expect(within(nav).getByText('2')).toBeInTheDocument()
    })
  })
})
