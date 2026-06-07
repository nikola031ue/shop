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

      const freshProvider = new BrowserProvider(window.ethereum)
      const accounts = await freshProvider.send('eth_requestAccounts', [])
      const addr = accounts[0] as string
      setAccount(addr)

      const rawBalance = await freshProvider.getBalance(addr)
      setEthBalance(parseFloat(formatEther(rawBalance)).toFixed(4))

      return addr
    } catch (err: unknown) {
      const code = (err as { code?: unknown }).code
      if (code === 4001 || code === 'ACTION_REJECTED') {
        setError('Konekcija odbijena - odobri pristup u MetaMask-u.')
      } else {
        setError('Greška pri konekciji. Pokušaj ponovo.')
      }
      return null
    } finally {
      setIsConnecting(false)
    }
  }, [])

  // onSubmitted se poziva čim korisnik potvrdi u MetaMask-u (prije potvrde bloka)
  const sendEth = useCallback(async (
    toAddress: string,
    usdAmount: number,
    onSubmitted?: (txHash: string) => void,
  ): Promise<TransferResult> => {
    if (!window.ethereum) throw new Error('MetaMask nije instaliran.')

    const provider = new BrowserProvider(window.ethereum)
    const signer = await provider.getSigner()
    const walletAddress = await signer.getAddress()

    const ethAmount = (usdAmount * USD_TO_ETH).toFixed(8)

    const tx = await signer.sendTransaction({
      to: toAddress,
      value: parseEther(ethAmount),
    })

    // Odmah obavijesti komponentu da je tx poslan (MetaMask potvrdio)
    onSubmitted?.(tx.hash)

    // Čekaj 1 blok potvrde na Sepolia (~12 sekundi)
    await tx.wait(1)

    return { txHash: tx.hash, walletAddress }
  }, [])

  return { isInstalled, account, ethBalance, isConnecting, error, connect, sendEth, USD_TO_ETH }
}
