import client from './client'
import type { Order, PagedResult } from '../types'

interface GetOrdersParams {
  page?: number
  pageSize?: number
}

export async function getOrders(params: GetOrdersParams): Promise<PagedResult<Order>> {
  const { data } = await client.get<PagedResult<Order>>('/orders', { params })
  return data
}

export async function updateOrderStatus(id: string, status: string): Promise<void> {
  await client.put(`/orders/${id}/status`, { status })
}
