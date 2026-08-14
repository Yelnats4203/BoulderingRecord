import type { SessionResponse } from '../types/sessions'

function parseDateOnly(value: string): Date {
  const [year, month, day] = value.split('-').map(Number)
  return new Date(Date.UTC(year, month - 1, day))
}

function formatDateOnly(date: Date): string {
  const year = String(date.getUTCFullYear())
  const month = String(date.getUTCMonth() + 1).padStart(2, '0')
  const day = String(date.getUTCDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function formatShortLabel(date: Date): string {
  const month = String(date.getUTCMonth() + 1).padStart(2, '0')
  const day = String(date.getUTCDate()).padStart(2, '0')
  return `${month}/${day}`
}

export function getDefaultDateRange(today: Date = new Date()): { dateFrom: string; dateTo: string } {
  const dateTo = new Date(Date.UTC(today.getFullYear(), today.getMonth(), today.getDate()))
  const dateFrom = new Date(dateTo.getTime())
  dateFrom.setUTCMonth(dateFrom.getUTCMonth() - 2)

  return { dateFrom: formatDateOnly(dateFrom), dateTo: formatDateOnly(dateTo) }
}

export interface DailyStat {
  label: string
  completed: number
  uncompleted: number
}

export function groupByDay(sessions: SessionResponse[]): DailyStat[] {
  const totalsByDate = new Map<string, { completed: number; uncompleted: number }>()

  for (const session of sessions) {
    const totals = totalsByDate.get(session.date) ?? { completed: 0, uncompleted: 0 }
    for (const gradeCount of session.gradeCounts) {
      totals.completed += gradeCount.completedCount
      totals.uncompleted += gradeCount.uncompletedCount
    }
    totalsByDate.set(session.date, totals)
  }

  return [...totalsByDate.entries()]
    .filter(([, totals]) => totals.completed > 0 || totals.uncompleted > 0)
    .sort(([dateA], [dateB]) => dateA.localeCompare(dateB))
    .map(([date, totals]) => ({
      label: formatShortLabel(parseDateOnly(date)),
      completed: totals.completed,
      uncompleted: totals.uncompleted,
    }))
}

export interface GradeStat {
  label: string
  completed: number
  uncompleted: number
}

export function groupByGrade(sessions: SessionResponse[]): GradeStat[] {
  const totalsByGrade = new Map<number, { completed: number; uncompleted: number }>()

  for (const session of sessions) {
    for (const gradeCount of session.gradeCounts) {
      const totals = totalsByGrade.get(gradeCount.grade) ?? { completed: 0, uncompleted: 0 }
      totals.completed += gradeCount.completedCount
      totals.uncompleted += gradeCount.uncompletedCount
      totalsByGrade.set(gradeCount.grade, totals)
    }
  }

  return [...totalsByGrade.entries()]
    .filter(([, totals]) => totals.completed > 0 || totals.uncompleted > 0)
    .sort(([gradeA], [gradeB]) => gradeA - gradeB)
    .map(([grade, totals]) => ({ label: `V${grade}`, completed: totals.completed, uncompleted: totals.uncompleted }))
}

export function calculateWeeklyFrequency(sessions: SessionResponse[], dateFrom: string, dateTo: string): number {
  const start = parseDateOnly(dateFrom)
  const end = parseDateOnly(dateTo)
  const totalDays = Math.floor((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1
  const totalWeeks = totalDays / 7

  return totalWeeks > 0 ? sessions.length / totalWeeks : 0
}

export interface GymStat {
  gymName: string
  count: number
}

export function countByGym(sessions: SessionResponse[]): GymStat[] {
  const countsByGym = new Map<string, number>()

  for (const session of sessions) {
    if (!session.gymName) {
      continue
    }
    countsByGym.set(session.gymName, (countsByGym.get(session.gymName) ?? 0) + 1)
  }

  return [...countsByGym.entries()]
    .sort(([, countA], [, countB]) => countB - countA)
    .map(([gymName, count]) => ({ gymName, count }))
}
