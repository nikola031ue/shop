import client from './client'

export interface CreateOrderResponse {
  orderId: string
}

export async function createOrder(
  transactionHash: string,
  walletAddress: string,
): Promise<CreateOrderResponse> {
  const { data } = await client.post<CreateOrderResponse>('/orders', {
    transactionHash,
    walletAddress,
  })
  return data
}
