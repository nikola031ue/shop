import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { CartProvider } from './context/CartContext'
import { CartDrawer } from './components/CartDrawer'
import { ProductsPage } from './pages/ProductsPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000, retry: 1 },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <CartProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<ProductsPage />} />
          </Routes>
          <CartDrawer />
        </BrowserRouter>
      </CartProvider>
    </QueryClientProvider>
  )
}
