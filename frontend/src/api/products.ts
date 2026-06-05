import client from './client'
import type { PagedResult, Product } from '../types'

interface GetProductsParams {
  search?: string
  page?: number
  pageSize?: number
}

export async function getProducts(params: GetProductsParams): Promise<PagedResult<Product>> {
  const { data } = await client.get<PagedResult<Product>>('/products', { params })
  return data
}
