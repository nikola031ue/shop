import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate, Link } from 'react-router-dom'
import { getOrders, updateOrderStatus } from '../api/adminOrders'
import { useAuth } from '../context/AuthContext'
import type { OrderStatus } from '../types'

const PAGE_SIZE = 20

const STATUS_LABELS: Record<OrderStatus, string> = {
  Pending: 'Na čekanju',
  Confirmed: 'Potvrđena',
  Shipped: 'Poslana',
  Delivered: 'Isporučena',
  Cancelled: 'Otkazana',
}

const STATUS_COLORS: Record<OrderStatus, string> = {
  Pending: 'bg-yellow-50 text-yellow-700 border-yellow-200',
  Confirmed: 'bg-blue-50 text-blue-700 border-blue-200',
  Shipped: 'bg-indigo-50 text-indigo-700 border-indigo-200',
  Delivered: 'bg-green-50 text-green-700 border-green-200',
  Cancelled: 'bg-red-50 text-red-500 border-red-200',
}

const ALL_STATUSES: OrderStatus[] = ['Pending', 'Confirmed', 'Shipped', 'Delivered', 'Cancelled']

export function AdminOrdersPage() {
  const { logout } = useAuth()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<OrderStatus | 'All'>('All')

  const { data, isLoading } = useQuery({
    queryKey: ['admin-orders', page],
    queryFn: () => getOrders({ page, pageSize: PAGE_SIZE }),
  })

  const statusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: OrderStatus }) =>
      updateOrderStatus(id, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-orders'] }),
  })

  const filteredItems = statusFilter === 'All'
    ? data?.items ?? []
    : (data?.items ?? []).filter((o) => o.status === statusFilter)

  function handleLogout() {
    logout()
    navigate('/admin/login', { replace: true })
  }

  function formatDate(iso: string) {
    return new Date(iso).toLocaleString('sr-RS', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit',
    })
  }

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 0

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="max-w-6xl mx-auto px-4 sm:px-6 h-14 flex items-center justify-between gap-4">
          <div className="flex items-center gap-4">
            <span className="font-bold text-gray-900">🛒 Admin</span>
            <span className="text-gray-300">|</span>
            <Link to="/admin/products" className="text-sm text-gray-500 hover:text-gray-900 transition-colors">
              Artikli
            </Link>
            <span className="text-sm font-medium text-indigo-600">Porudžbine</span>
          </div>
          <button
            onClick={handleLogout}
            className="text-sm text-gray-500 hover:text-red-500 transition-colors"
          >
            Odjavi se
          </button>
        </div>
      </header>

      <main className="max-w-6xl mx-auto px-4 sm:px-6 py-6 space-y-4">
        {/* Filter po statusu */}
        <div className="flex items-center gap-2 flex-wrap">
          <button
            onClick={() => setStatusFilter('All')}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
              statusFilter === 'All'
                ? 'bg-gray-900 text-white'
                : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'
            }`}
          >
            Sve ({data?.totalCount ?? 0})
          </button>
          {ALL_STATUSES.map((s) => {
            const count = data?.items.filter((o) => o.status === s).length ?? 0
            return (
              <button
                key={s}
                onClick={() => setStatusFilter(s)}
                className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                  statusFilter === s
                    ? 'bg-gray-900 text-white'
                    : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-50'
                }`}
              >
                {STATUS_LABELS[s]} ({count})
              </button>
            )
          })}
        </div>

        {/* Tabela */}
        <div className="bg-white rounded-xl border border-gray-100 overflow-hidden">
          {isLoading ? (
            <div className="divide-y divide-gray-50">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="h-16 px-5 animate-pulse flex items-center gap-4">
                  <div className="h-3 bg-gray-100 rounded w-32" />
                  <div className="h-3 bg-gray-100 rounded w-48 ml-auto" />
                </div>
              ))}
            </div>
          ) : filteredItems.length === 0 ? (
            <div className="text-center py-16 text-gray-400">
              <p className="text-3xl mb-2">📋</p>
              <p>Nema porudžbina</p>
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-xs text-gray-500 uppercase tracking-wide">
                <tr>
                  <th className="text-left px-5 py-3 font-medium">Datum</th>
                  <th className="text-left px-5 py-3 font-medium hidden lg:table-cell">Artikli</th>
                  <th className="text-right px-5 py-3 font-medium">Iznos</th>
                  <th className="text-left px-5 py-3 font-medium hidden md:table-cell">Tx Hash</th>
                  <th className="text-center px-5 py-3 font-medium">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {filteredItems.map((order) => {
                  const isUpdating = statusMutation.isPending &&
                    (statusMutation.variables as { id: string })?.id === order.id

                  return (
                    <tr key={order.id} className="hover:bg-gray-50/50 transition-colors">
                      <td className="px-5 py-4 text-gray-600 whitespace-nowrap">
                        {formatDate(order.createdAt)}
                      </td>

                      <td className="px-5 py-4 hidden lg:table-cell">
                        <div className="space-y-0.5">
                          {order.items.map((item) => (
                            <p key={item.id} className="text-gray-600 truncate max-w-[220px]">
                              {item.quantity}× {item.productName}
                            </p>
                          ))}
                        </div>
                      </td>

                      <td className="px-5 py-4 text-right font-semibold text-gray-900 whitespace-nowrap">
                        ${order.totalPrice.toFixed(2)}
                      </td>

                      <td className="px-5 py-4 hidden md:table-cell">
                        <a
                          href={`https://sepolia.etherscan.io/tx/${order.transactionHash}`}
                          target="_blank"
                          rel="noreferrer"
                          className="font-mono text-xs text-indigo-600 hover:underline"
                          title={order.transactionHash}
                        >
                          {order.transactionHash.slice(0, 10)}...{order.transactionHash.slice(-6)}
                        </a>
                      </td>

                      <td className="px-5 py-4 text-center">
                        <select
                          value={order.status}
                          disabled={isUpdating}
                          onChange={(e) =>
                            statusMutation.mutate({
                              id: order.id,
                              status: e.target.value as OrderStatus,
                            })
                          }
                          className={`text-xs font-medium px-2.5 py-1.5 rounded-lg border cursor-pointer
                            focus:outline-none focus:ring-2 focus:ring-indigo-500 transition-opacity
                            ${isUpdating ? 'opacity-50' : ''}
                            ${STATUS_COLORS[order.status]}`}
                        >
                          {ALL_STATUSES.map((s) => (
                            <option key={s} value={s}>{STATUS_LABELS[s]}</option>
                          ))}
                        </select>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          )}

          {/* Paginacija */}
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
    </div>
  )
}
