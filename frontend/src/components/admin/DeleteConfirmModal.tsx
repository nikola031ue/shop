interface Props {
  productName: string
  onConfirm: () => void
  onClose: () => void
  isLoading: boolean
}

export function DeleteConfirmModal({ productName, onConfirm, onClose, isLoading }: Props) {
  return (
    <div
      className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
      onClick={onClose}
    >
      <div
        className="bg-white rounded-2xl shadow-xl w-full max-w-sm p-6 space-y-4"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="text-center space-y-2">
          <div className="text-3xl">🗑️</div>
          <h2 className="text-base font-semibold text-gray-900">Obriši artikal</h2>
          <p className="text-sm text-gray-500">
            Da li si siguran da želiš obrisati{' '}
            <span className="font-medium text-gray-700">"{productName}"</span>?
            Ova akcija se ne može poništiti.
          </p>
        </div>

        <div className="flex gap-3">
          <button
            onClick={onClose}
            disabled={isLoading}
            className="flex-1 py-2.5 border border-gray-200 text-gray-600 text-sm font-medium rounded-xl hover:bg-gray-50 disabled:opacity-40 transition-colors"
          >
            Odustani
          </button>
          <button
            onClick={onConfirm}
            disabled={isLoading}
            className="flex-1 py-2.5 bg-red-500 text-white text-sm font-medium rounded-xl hover:bg-red-600 disabled:opacity-60 transition-colors"
          >
            {isLoading ? 'Brisanje...' : 'Obriši'}
          </button>
        </div>
      </div>
    </div>
  )
}
