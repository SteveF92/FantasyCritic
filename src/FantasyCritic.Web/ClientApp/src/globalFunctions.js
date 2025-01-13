import moment from 'moment';
import GraphemeBreaker from 'grapheme-breaker-mjs';

export function publisherIconIsValid(publisherIcon) {
  if (!publisherIcon) {
    return true;
  }
  let length = GraphemeBreaker.countBreaks(publisherIcon);
  return length === 1;
}

export function ordinal_suffix_of(i) {
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

export function formatPublisherGameReleaseDate(publisherGame, shorten) {
  if (!publisherGame.masterGame) {
    return 'Unknown';
  }

  return formatMasterGameReleaseDate(publisherGame.masterGame, shorten);
}

export function formatMasterGameReleaseDate(masterGame, shorten) {
  if (masterGame.releaseDate) {
    if (shorten) {
      return moment(masterGame.releaseDate).format('MMM Do, YYYY');
    }
    return moment(masterGame.releaseDate).format('MMMM Do, YYYY');
  }

  if (shorten) {
    return masterGame.estimatedReleaseDate;
  }

  return masterGame.estimatedReleaseDate + ' (Estimated)';
}

export function formatPublisherGameAcquiredDate(publisherGame) {
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
  let date = moment(publisherGame.timestamp).format('MMMM Do, YYYY');
  return type + ' on ' + date;
}

export function formatPublisherGameRemovedDate(publisherGame) {
  if (!publisherGame.removedTimestamp) {
    return '';
  }
  return moment(publisherGame.removedTimestamp).format('MMMM Do, YYYY');
}

export function formatLongDateTime(dateTime) {
  return moment(String(dateTime)).local().format('MMMM Do, YYYY, h:mm:ss a');
}

export function formatLongDate(date) {
  return moment(String(date)).local().format('MMMM Do, YYYY');
}

export function toISODateString(date) {
  const fullString = date.toISOString();
  const dateString = fullString.substring(0, 10);
  return dateString;
}

export function roundNumber(value, decimals) {
  if (value === 0) {
    return 0;
  }
  if (!value) {
    return '--';
  }

  if (!decimals) {
    decimals = 0;
  }

  value = Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals);
  return value;
}

export function maxBy(array, func) {
  const max = Math.max(...array.map(func));
  return array.find((item) => func(item) === max);
}

export function intersection(array, ...args) {
  return array.filter((item) => args.every((arr) => arr.includes(item)));
}

export function except(arrayOne, arrayTwo) {
  return arrayOne.filter((x) => !arrayTwo.includes(x));
}

export function startCase(valueString) {
  return valueString.replace(/([a-z])([A-Z])/g, '$1 $2');
}

export function orderBy(arr, keySelector) {
  return [...arr].sort((a, b) => {
    const valA = keySelector(a);
    const valB = keySelector(b);
    if (valA < valB) return -1;
    if (valA > valB) return 1;
    return 0;
  });
}

export function orderByDescending(arr, keySelector) {
  return [...arr].sort((a, b) => {
    const valA = keySelector(a);
    const valB = keySelector(b);
    if (valA < valB) return 1;
    if (valA > valB) return -1;
    return 0;
  });
}
