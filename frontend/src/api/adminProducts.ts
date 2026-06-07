import client from './client'
import type { Product } from '../types'

export interface ProductPayload {
  name: string
  description: string
  price: number
  stock: number
  imageUrl: string | null
}

export async function createProduct(payload: ProductPayload): Promise<Product> {
  const { data } = await client.post<Product>('/products', payload)
  return data
}

export async function updateProduct(id: string, payload: ProductPayload): Promise<void> {
  await client.put(`/products/${id}`, { ...payload, id })
}

export async function deleteProduct(id: string): Promise<void> {
  await client.delete(`/products/${id}`)
}
