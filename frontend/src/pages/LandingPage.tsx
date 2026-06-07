import { useNavigate } from 'react-router-dom'

export function LandingPage() {
  const navigate = useNavigate()

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-sm space-y-6 text-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">🛒 Shop</h1>
          <p className="text-gray-500 mt-2 text-sm">Odaberi kako želiš da nastaviš</p>
        </div>

        <div className="space-y-3">
          <button
            onClick={() => navigate('/shop')}
            className="w-full py-4 bg-indigo-600 text-white font-medium rounded-2xl hover:bg-indigo-700 active:bg-indigo-800 transition-colors text-base"
          >
            🛍️ Pregled proizvoda
          </button>

          <button
            onClick={() => navigate('/admin')}
            className="w-full py-4 bg-white text-gray-700 font-medium rounded-2xl border border-gray-200 hover:bg-gray-50 active:bg-gray-100 transition-colors text-base"
          >
            ⚙️ Admin panel
          </button>
        </div>
      </div>
    </div>
  )
}
