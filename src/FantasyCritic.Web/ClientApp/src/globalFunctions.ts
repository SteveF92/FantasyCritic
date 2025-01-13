import GraphemeBreaker from 'grapheme-breaker-mjs';
import { DateTime } from 'luxon';

export function publisherIconIsValid(publisherIcon: string): boolean {
  if (!publisherIcon) {
    return true;
  }
  let length = GraphemeBreaker.countBreaks(publisherIcon);
  return length === 1;
}

export function ordinal_suffix_of(i: number): string {
  const j = i % 10,
    k = i % 100;
  if (j == 1 && k != 11) {
    return i + 'st';
  }
  if (j == 2 && k != 12) {
    return i + 'nd';
  }
  if (j == 3 && k != 13) {
    return i + 'rd';
  }
  return i + 'th';
}

export function formatPublisherGameReleaseDate(publisherGame, shorten: boolean): string {
  if (!publisherGame.masterGame) {
    return 'Unknown';
  }

  return formatMasterGameReleaseDate(publisherGame.masterGame, shorten);
}

export function formatMasterGameReleaseDate(masterGame, shorten: boolean): string {
  if (masterGame.releaseDate) {
    if (shorten) {
      return DateTime.fromISO(masterGame.releaseDate).toFormat('MMM dd, yyyy');
    }
    return DateTime.fromISO(masterGame.releaseDate).toFormat('MMMM dd, yyyy');
  }

  if (shorten) {
    return masterGame.estimatedReleaseDate;
  }

  return masterGame.estimatedReleaseDate + ' (Estimated)';
}

export function formatPublisherGameAcquiredDate(publisherGame): string {
  let type = '';
  if (publisherGame.overallDraftPosition) {
    type = `Drafted ${ordinal_suffix_of(publisherGame.overallDraftPosition)}`;
  } else if (publisherGame.bidAmount || publisherGame.bidAmount === 0) {
    type = 'Picked up for $' + publisherGame.bidAmount;
  } else if (publisherGame.acquiredInTradeID) {
    type = 'Acquired in a trade';
  } else {
    type = 'Acquired';
  }

  let date = DateTime.fromISO(publisherGame.timestamp).toFormat('MMMM dd, yyyy');
  return type + ' on ' + date;
}

export function formatPublisherGameRemovedDate(publisherGame): string {
  if (!publisherGame.removedTimestamp) {
    return '';
  }
  return DateTime.fromISO(publisherGame.removedTimestamp).toFormat('MMMM dd, yyyy');
}

export function formatLongDateTime(dateTime: string): string {
  return DateTime.fromISO(dateTime).toFormat('MMMM dd, yyyy, h:mm:ss a');
}

export function formatLongDate(date: string) {
  return DateTime.fromISO(date).toFormat('MMMM dd, yyyy');
}

export function roundNumber(value: number, decimals: number): string {
  if (value === 0) {
    return '0';
  }
  if (!value) {
    return '--';
  }

  if (!decimals) {
    decimals = 0;
  }

  value = Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
  return value.toString();
}

export function maxBy<T>(array: T[], func: (item: T) => number): T | undefined {
  const max = Math.max(...array.map(func));
  return array.find((item) => func(item) === max);
}

export function intersection<T>(array: T[], ...args: T[][]): T[] {
  return array.filter((item) => args.every((arr) => arr.includes(item)));
}

export function except<T>(arrayOne: T[], arrayTwo: T[]): T[] {
  return arrayOne.filter((x) => !arrayTwo.includes(x));
}

export function startCase(valueString: string): string {
  return valueString.replace(/([a-z])([A-Z])/g, '$1 $2');
}

export function orderBy<T, K>(arr: T[], keySelector: (item: T) => K): T[] {
  return [...arr].sort((a, b) => {
    const valA = keySelector(a);
    const valB = keySelector(b);
    if (valA < valB) return -1;
    if (valA > valB) return 1;
    return 0;
  });
}

export function orderByDescending<T, K>(arr: T[], keySelector: (item: T) => K): T[] {
  return [...arr].sort((a, b) => {
    const valA = keySelector(a);
    const valB = keySelector(b);
    if (valA < valB) return 1;
    if (valA > valB) return -1;
    return 0;
  });
}
