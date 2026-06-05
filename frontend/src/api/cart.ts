import client from './client'
import type { Cart } from '../types'

export async function getCart(): Promise<Cart> {
  const { data } = await client.get<Cart>('/cart')
  return data
}

export async function addToCart(productId: string, quantity: number): Promise<void> {
  await client.post('/cart/items', { productId, quantity })
}
