export interface RoyaleYearQuarter extends YearQuarter {
  openForPlay: boolean;
  finished: boolean;
}

export interface YearQuarter {
  year: number;
  quarter: number;
}

export function yearQuarter2026Q1AndQ2FeatureSupported(yearQuarterToCheck: YearQuarter): boolean {
  return yearQuarterToCheck.year === 2026 && yearQuarterToCheck.quarter <= 2;
}

export function yearQuarter2026Q3FeatureSupported(yearQuarterToCheck: YearQuarter): boolean {
  const yearQuarterToCheckAgainst: YearQuarter = { year: 2026, quarter: 3 };

  return compareYearQuarter(yearQuarterToCheck, yearQuarterToCheckAgainst) >= 0;
}

export function compareYearQuarter(a: YearQuarter, b: YearQuarter): number {
  if (a.year !== b.year) {
    return a.year - b.year;
  }

  return a.quarter - b.quarter;
}
