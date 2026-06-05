import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Pagination } from './Pagination'

describe('Pagination', () => {
  it('renders nothing when totalPages is 1', () => {
    const { container } = render(
      <Pagination page={1} totalPages={1} onPageChange={() => {}} />,
    )
    expect(container.firstChild).toBeNull()
  })

  it('renders nothing when totalPages is 0', () => {
    const { container } = render(
      <Pagination page={1} totalPages={0} onPageChange={() => {}} />,
    )
    expect(container.firstChild).toBeNull()
  })

  it('renders page buttons when totalPages > 1', () => {
    render(<Pagination page={1} totalPages={3} onPageChange={() => {}} />)
    expect(screen.getByText('1')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('3')).toBeInTheDocument()
  })

  it('disables prev button on first page', () => {
    render(<Pagination page={1} totalPages={3} onPageChange={() => {}} />)
    expect(screen.getByText('←')).toBeDisabled()
    expect(screen.getByText('→')).not.toBeDisabled()
  })

  it('disables next button on last page', () => {
    render(<Pagination page={3} totalPages={3} onPageChange={() => {}} />)
    expect(screen.getByText('→')).toBeDisabled()
    expect(screen.getByText('←')).not.toBeDisabled()
  })

  it('calls onPageChange with correct page when button is clicked', async () => {
    const onPageChange = vi.fn()
    render(<Pagination page={1} totalPages={3} onPageChange={onPageChange} />)

    await userEvent.click(screen.getByText('2'))
    expect(onPageChange).toHaveBeenCalledWith(2)
  })

  it('calls onPageChange with page - 1 when prev is clicked', async () => {
    const onPageChange = vi.fn()
    render(<Pagination page={2} totalPages={3} onPageChange={onPageChange} />)

    await userEvent.click(screen.getByText('←'))
    expect(onPageChange).toHaveBeenCalledWith(1)
  })

  it('calls onPageChange with page + 1 when next is clicked', async () => {
    const onPageChange = vi.fn()
    render(<Pagination page={2} totalPages={3} onPageChange={onPageChange} />)

    await userEvent.click(screen.getByText('→'))
    expect(onPageChange).toHaveBeenCalledWith(3)
  })

  it('shows ellipsis for large page counts', () => {
    render(<Pagination page={5} totalPages={10} onPageChange={() => {}} />)
    const ellipses = screen.getAllByText('…')
    expect(ellipses.length).toBeGreaterThan(0)
  })
})
