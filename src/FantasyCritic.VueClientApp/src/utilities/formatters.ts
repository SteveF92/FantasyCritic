import { DateTime } from 'luxon';

export function formatScore(value: number | null | undefined, decimals?: number): number | string {
  if (value === 0) {
    return 0;
  }
  if (!value) {
    return '--';
  }

  if (!decimals) {
    decimals = 0;
  }

  return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
}

export function formatCurrency<T>(value: T, decimals?: number): T | string {
  if (typeof value !== 'number') {
    return value;
  }

  if (decimals !== 0 && !decimals) {
    decimals = 2;
  }

  const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals
  });
  return formatter.format(value);
}

export function formatDateTime(value: string | null | undefined): string {
  if (value) {
    return DateTime.fromISO(String(value)).toLocaleString(DateTime.DATETIME_FULL);
  }
  return '';
}

export function formatDateTimeAt(value: string | null | undefined): string {
  if (value) {
    return DateTime.fromISO(value).toFormat("MMMM dd, yyyy, 'at' h:mm:ss a");
  }
  return '';
}

export function formatDateIso(value: string | null | undefined): string {
  if (value) {
    return DateTime.fromISO(value).toFormat('yyyy-MM-dd');
  }
  return '';
}

export function formatDateLong(value: string | null | undefined): string {
  if (value) {
    return DateTime.fromISO(value).toFormat('MMMM dd, yyyy');
  }
  return '';
}

export function formatYesNo(value: unknown): string {
  return value ? 'Yes' : 'No';
}

export function formatApprovedRejected(value: unknown): string {
  return value ? 'Approved' : 'Rejected';
}

export function formatPercent(value: number | null | undefined, decimals?: number): string {
  if (!value) value = 0;
  if (!decimals) decimals = 0;

  value = value * 100;
  return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals) + '%';
}

export function formatThousands(value: number): string {
  return new Intl.NumberFormat().format(value);
}

export function selectTextFromPossibleOptions<T>(value: T, possibleOptions: { value: unknown; text: string }[]): T | string {
  const matchingValue = possibleOptions.find((x) => x.value === value);
  if (matchingValue) {
    return matchingValue.text;
  }

  return value;
}
