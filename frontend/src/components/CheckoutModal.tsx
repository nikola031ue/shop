import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useMetaMask } from '../hooks/useMetaMask'
import { createOrder } from '../api/orders'
import { SHOP_WALLET_ADDRESS } from '../config/contracts'

interface Props {
  total: number
  onClose: () => void
  onSuccess: () => void
}

type Step = 'idle' | 'sending' | 'success'

// $1 USD = 0.00001 ETH (isti kurs kao u hooku)
const USD_TO_ETH = 0.00001

export function CheckoutModal({ total, onClose, onSuccess }: Props) {
  const { isInstalled, account, ethBalance, isConnecting, error, connect, sendEth } =
    useMetaMask()
  const [step, setStep] = useState<Step>('idle')
  const [txHash, setTxHash] = useState('')
  const [txError, setTxError] = useState('')
  const queryClient = useQueryClient()

  const orderMutation = useMutation({
    mutationFn: ({ hash, wallet }: { hash: string; wallet: string }) =>
      createOrder(hash, wallet),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] })
    },
  })

  async function handlePay() {
    setTxError('')
    setStep('sending')
    try {
      const { txHash: hash, walletAddress } = await sendEth(SHOP_WALLET_ADDRESS, total)
      setTxHash(hash)
      await orderMutation.mutateAsync({ hash, wallet: walletAddress })
      setStep('success')
      setTimeout(onSuccess, 2500)
    } catch (err: unknown) {
      setStep('idle')
      if ((err as { code?: number }).code === 4001) {
        setTxError('Odbio si transakciju u MetaMask-u.')
      } else {
        setTxError(err instanceof Error ? err.message : 'Transakcija nije uspjela.')
      }
    }
  }

  function shortenAddress(addr: string) {
    return `${addr.slice(0, 6)}...${addr.slice(-4)}`
  }

  const ethAmount = (total * USD_TO_ETH).toFixed(6)

  return (
    <div
      className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-2xl shadow-xl w-full max-w-sm"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-semibold text-gray-900">Plaćanje</h2>
          {step !== 'sending' && (
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors"
              aria-label="Zatvori"
            >
              <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          )}
        </div>

        <div className="px-6 py-5 space-y-5">
          {/* Ukupno */}
          <div className="bg-gray-50 rounded-xl px-4 py-3 space-y-1">
            <div className="flex items-center justify-between">
              <span className="text-sm text-gray-500">Ukupno</span>
              <span className="text-xl font-bold text-gray-900">${total.toFixed(2)}</span>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-xs text-gray-400">≈ ETH (Sepolia testnet)</span>
              <span className="text-sm font-medium text-indigo-600">{ethAmount} ETH</span>
            </div>
          </div>

          {/* Success */}
          {step === 'success' && (
            <div className="text-center py-4 space-y-2">
              <div className="text-4xl">✅</div>
              <p className="font-semibold text-gray-900">Plaćanje uspješno!</p>
              <p className="text-xs text-gray-400 break-all">{txHash}</p>
              <p className="text-sm text-gray-500">Porudžbina je kreirana.</p>
            </div>
          )}

          {step !== 'success' && (
            <>
              {isInstalled ? (
                account ? (
                  /* Povezan, spreman za plaćanje */
                  <div className="space-y-3">
                    <div className="flex items-center gap-2 bg-green-50 text-green-700 rounded-lg px-3 py-2 text-sm">
                      <span className="w-2 h-2 bg-green-500 rounded-full shrink-0" />
                      <span className="font-medium">{shortenAddress(account)}</span>
                      <span className="text-green-500 ml-auto">Sepolia</span>
                    </div>

                    {ethBalance !== null && (
                      <p className="text-xs text-gray-500 text-center">
                        Balans: <span className="font-medium text-gray-700">{ethBalance} ETH</span>
                      </p>
                    )}

                    {txError && (
                      <p className="text-xs text-red-500 bg-red-50 rounded-lg px-3 py-2">{txError}</p>
                    )}

                    <button
                      onClick={handlePay}
                      disabled={step === 'sending'}
                      className="w-full py-3 bg-indigo-600 text-white font-medium rounded-xl hover:bg-indigo-700 disabled:opacity-60 transition-colors flex items-center justify-center gap-2"
                    >
                      {step === 'sending' ? (
                        <>
                          <svg className="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                          </svg>
                          Čeka potvrdu u MetaMask-u...
                        </>
                      ) : (
                        `Plati ${ethAmount} ETH`
                      )}
                    </button>
                  </div>
                ) : (
                  /* MetaMask instaliran, nije povezan */
                  <div className="space-y-3">
                    {error && (
                      <p className="text-xs text-red-500 bg-red-50 rounded-lg px-3 py-2">{error}</p>
                    )}
                    <button
                      onClick={connect}
                      disabled={isConnecting}
                      className="w-full py-3 border-2 border-indigo-600 text-indigo-600 font-medium rounded-xl hover:bg-indigo-50 disabled:opacity-60 transition-colors flex items-center justify-center gap-2"
                    >
                      <MetaMaskIcon />
                      {isConnecting ? 'Konekcija...' : 'Poveži MetaMask'}
                    </button>
                  </div>
                )
              ) : (
                /* MetaMask nije instaliran */
                <div className="space-y-3">
                  <div className="bg-amber-50 border border-amber-200 rounded-xl px-4 py-3 text-sm text-amber-800 space-y-1">
                    <p className="font-medium">MetaMask nije instaliran</p>
                    <p className="text-xs">
                      Instaliraj{' '}
                      <a
                        href="https://metamask.io/download/"
                        target="_blank"
                        rel="noreferrer"
                        className="underline"
                      >
                        MetaMask ekstenziju
                      </a>{' '}
                      ili pošalji ETH ručno:
                    </p>
                  </div>
                  <div className="bg-gray-50 rounded-xl px-4 py-3 space-y-1">
                    <p className="text-xs text-gray-500">Wallet adresa prodavnice (Sepolia)</p>
                    <p className="text-xs font-mono text-gray-800 break-all select-all">
                      {SHOP_WALLET_ADDRESS}
                    </p>
                    <p className="text-xs text-gray-500 mt-1">
                      Iznos: <span className="font-medium">{ethAmount} ETH</span>
                    </p>
                  </div>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  )
}

function MetaMaskIcon() {
  return (
    <svg width="20" height="20" viewBox="0 0 212 189" fill="none" xmlns="http://www.w3.org/2000/svg">
      <polygon points="201,0 118,61 134,25" fill="#E2761B" stroke="#E2761B" strokeLinecap="round" strokeLinejoin="round" />
      <polygon points="10,0 94,61 78,25" fill="#E4761B" stroke="#E4761B" strokeLinecap="round" strokeLinejoin="round" />
      <polygon points="172,136 149,170 196,183 211,137" fill="#E4761B" stroke="#E4761B" strokeLinecap="round" strokeLinejoin="round" />
      <polygon points="0,137 15,183 62,170 40,136" fill="#E4761B" stroke="#E4761B" strokeLinecap="round" strokeLinejoin="round" />
      <polygon points="59,82 45,103 92,105 90,55" fill="#E4761B" stroke="#E4761B" strokeLinecap="round" strokeLinejoin="round" />
      <polygon points="152,82 121,55 119,105 166,103" fill="#E4761B" stroke="#E4761B" strokeLinecap="round" strokeLinejoin="round" />
      <polygon points="62,170 89,157 65,140" fill="#E4761B" stroke="#E4761B" strokeLinecap="round" strokeLinejoin="round" />
      <polygon points="122,157 148,170 146,140" fill="#E4761B" stroke="#E4761B" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  )
}
