import { act, renderHook } from '@testing-library/react'
import { useDebounce } from './useDebounce'

describe('useDebounce', () => {
  beforeEach(() => vi.useFakeTimers())
  afterEach(() => vi.useRealTimers())

  it('returns initial value immediately', () => {
    const { result } = renderHook(() => useDebounce('initial', 400))
    expect(result.current).toBe('initial')
  })

  it('does not update before delay elapses', () => {
    const { result, rerender } = renderHook(({ value }) => useDebounce(value, 400), {
      initialProps: { value: 'first' },
    })

    rerender({ value: 'second' })
    act(() => vi.advanceTimersByTime(200))

    expect(result.current).toBe('first')
  })

  it('updates after delay elapses', () => {
    const { result, rerender } = renderHook(({ value }) => useDebounce(value, 400), {
      initialProps: { value: 'first' },
    })

    rerender({ value: 'second' })
    act(() => vi.advanceTimersByTime(400))

    expect(result.current).toBe('second')
  })

  it('cancels intermediate updates and uses only the last value', () => {
    const { result, rerender } = renderHook(({ value }) => useDebounce(value, 400), {
      initialProps: { value: 'first' },
    })

    rerender({ value: 'second' })
    act(() => vi.advanceTimersByTime(200))

    rerender({ value: 'third' })
    act(() => vi.advanceTimersByTime(400))

    expect(result.current).toBe('third')
  })
})
