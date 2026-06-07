import { BrowserRouter, Route, Routes, Navigate } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { CartProvider } from './context/CartContext'
import { AuthProvider } from './context/AuthContext'
import { CartDrawer } from './components/CartDrawer'
import { ProtectedRoute } from './components/ProtectedRoute'
import { LandingPage } from './pages/LandingPage'
import { ProductsPage } from './pages/ProductsPage'
import { LoginPage } from './pages/LoginPage'
import { AdminProductsPage } from './pages/AdminProductsPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { staleTime: 30_000, retry: 1 },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <CartProvider>
          <BrowserRouter>
            <Routes>
              {/* Landing */}
              <Route path="/" element={<LandingPage />} />

              {/* Korisnički dio */}
              <Route path="/shop" element={<ProductsPage />} />

              {/* Admin */}
              <Route path="/admin/login" element={<LoginPage />} />
              <Route
                path="/admin/products"
                element={
                  <ProtectedRoute>
                    <AdminProductsPage />
                  </ProtectedRoute>
                }
              />
              <Route path="/admin" element={<Navigate to="/admin/products" replace />} />
            </Routes>

            {/* CartDrawer je globalan — izvan Routes */}
            <CartDrawer />
          </BrowserRouter>
        </CartProvider>
      </AuthProvider>
    </QueryClientProvider>
  )
}
