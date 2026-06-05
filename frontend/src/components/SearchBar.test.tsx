import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { SearchBar } from './SearchBar'

describe('SearchBar', () => {
  it('renders with placeholder', () => {
    render(<SearchBar value="" onChange={() => {}} placeholder="Pretraži..." />)
    expect(screen.getByPlaceholderText('Pretraži...')).toBeInTheDocument()
  })

  it('calls onChange when user types', async () => {
    const onChange = vi.fn()
    render(<SearchBar value="" onChange={onChange} />)

    await userEvent.type(screen.getByRole('textbox'), 'Nike')

    expect(onChange).toHaveBeenCalledTimes(4)
    expect(onChange).toHaveBeenLastCalledWith('e')
  })

  it('shows clear button when value is not empty', () => {
    render(<SearchBar value="Nike" onChange={() => {}} />)
    expect(screen.getByLabelText('Obriši pretragu')).toBeInTheDocument()
  })

  it('hides clear button when value is empty', () => {
    render(<SearchBar value="" onChange={() => {}} />)
    expect(screen.queryByLabelText('Obriši pretragu')).not.toBeInTheDocument()
  })

  it('calls onChange with empty string when clear button is clicked', async () => {
    const onChange = vi.fn()
    render(<SearchBar value="Nike" onChange={onChange} />)

    await userEvent.click(screen.getByLabelText('Obriši pretragu'))

    expect(onChange).toHaveBeenCalledWith('')
  })
})
