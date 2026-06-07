export interface Product {
  id: string
  name: string
  description: string
  price: number
  stock: number
  imageUrl: string | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface CartItem {
  cartItemId: string
  productId: string
  productName: string
  unitPrice: number
  quantity: number
}

export interface Cart {
  sessionId: string
  items: CartItem[]
  total: number
}

export type OrderStatus = 'Pending' | 'Confirmed' | 'Shipped' | 'Delivered' | 'Cancelled'

export interface OrderItem {
  id: string
  productId: string
  productName: string
  unitPrice: number
  quantity: number
}

export interface Order {
  id: string
  sessionId: string
  status: OrderStatus
  totalPrice: number
  transactionHash: string
  walletAddress: string
  createdAt: string
  items: OrderItem[]
}
