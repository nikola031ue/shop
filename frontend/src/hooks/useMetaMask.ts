import { useState, useCallback } from 'react'
import { BrowserProvider, formatEther, parseEther } from 'ethers'
import { SEPOLIA_CHAIN_ID } from '../config/contracts'

export interface TransferResult {
  txHash: string
  walletAddress: string
}

// Konverzija: $1 USD = 0.00001 ETH (fiksni kurs za testnet demo)
const USD_TO_ETH = 0.00001

export function useMetaMask() {
  const [account, setAccount] = useState<string | null>(null)
  const [ethBalance, setEthBalance] = useState<string | null>(null)
  const [isConnecting, setIsConnecting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const isInstalled = typeof window !== 'undefined' && Boolean(window.ethereum)

  const connect = useCallback(async (): Promise<string | null> => {
    if (!window.ethereum) {
      setError('MetaMask nije instaliran.')
      return null
    }

    setIsConnecting(true)
    setError(null)

    try {
      const provider = new BrowserProvider(window.ethereum)

      const network = await provider.getNetwork()
      if (network.chainId !== SEPOLIA_CHAIN_ID) {
        try {
          await window.ethereum.request({
            method: 'wallet_switchEthereumChain',
            params: [{ chainId: '0xaa36a7' }],
          })
        } catch {
          setError('Prebaci MetaMask na Sepolia testnet i pokušaj ponovo.')
          return null
        }
      }

      // Ponovo kreiraj provider nakon switch-a mreže
      const freshProvider = new BrowserProvider(window.ethereum)
      const accounts = await freshProvider.send('eth_requestAccounts', [])
      const addr = accounts[0] as string
      setAccount(addr)

      const rawBalance = await freshProvider.getBalance(addr)
      setEthBalance(parseFloat(formatEther(rawBalance)).toFixed(4))

      return addr
    } catch (err: unknown) {
      if ((err as { code?: number }).code === 4001) {
        setError('Odbio si konekciju u MetaMask-u.')
      } else {
        setError(err instanceof Error ? err.message : 'Greška pri konekciji.')
      }
      return null
    } finally {
      setIsConnecting(false)
    }
  }, [])

  const sendEth = useCallback(async (
    toAddress: string,
    usdAmount: number,
  ): Promise<TransferResult> => {
    if (!window.ethereum) throw new Error('MetaMask nije instaliran.')

    const provider = new BrowserProvider(window.ethereum)
    const signer = await provider.getSigner()
    const walletAddress = await signer.getAddress()

    // Konvertujemo USD iznos u ETH (fiksni kurs za testnet)
    const ethAmount = (usdAmount * USD_TO_ETH).toFixed(6)

    const tx = await signer.sendTransaction({
      to: toAddress,
      value: parseEther(ethAmount),
    })

    return { txHash: tx.hash, walletAddress }
  }, [])

  return { isInstalled, account, ethBalance, isConnecting, error, connect, sendEth }
}
