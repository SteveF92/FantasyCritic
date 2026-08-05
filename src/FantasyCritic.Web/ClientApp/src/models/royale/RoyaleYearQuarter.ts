export interface RoyaleYearQuarter extends YearQuarter {
  openForPlay: boolean;
  finished: boolean;
}

export interface YearQuarter {
  year: number;
  quarter: number;
}

export function compareYearQuarter(a: YearQuarter, b: YearQuarter): number {
  if (a.year !== b.year) {
    return a.year - b.year;
  }

  return a.quarter - b.quarter;
}
