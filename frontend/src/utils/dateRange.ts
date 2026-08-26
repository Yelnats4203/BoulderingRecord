function toDateOnlyString(date: Date): string {
  const year: number = date.getFullYear()
  const month: string = String(date.getMonth() + 1).padStart(2, '0')
  const day: string = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export function getDefaultOneMonthRange(): { climbAtFrom: string; climbAtTo: string } {
  const today: Date = new Date()
  const oneMonthAgo: Date = new Date(today.getFullYear(), today.getMonth() - 1, today.getDate())
  return {
    climbAtFrom: toDateOnlyString(oneMonthAgo),
    climbAtTo: toDateOnlyString(today),
  }
}
