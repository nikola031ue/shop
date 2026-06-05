import client from './client'
import type { Cart } from '../types'

export async function getCart(): Promise<Cart> {
  const { data } = await client.get<Cart>('/cart')
  return data
}

export async function addToCart(productId: string, quantity: number): Promise<void> {
  await client.post('/cart/items', { productId, quantity })
}

export async function updateCartItem(cartItemId: string, quantity: number): Promise<void> {
  await client.put(`/cart/items/${cartItemId}`, { quantity })
}

export async function removeCartItem(cartItemId: string): Promise<void> {
  await client.delete(`/cart/items/${cartItemId}`)
}

export async function clearCart(): Promise<void> {
  await client.delete('/cart')
}
