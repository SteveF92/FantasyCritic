import moment from 'moment';
import _ from 'lodash';

export function score(value, decimals) {
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

export function money(value) {
  if (typeof value !== 'number') {
    return value;
  }
  var formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
  return formatter.format(value);
}

export function dateTime(value) {
  if (value) {
    return moment(String(value)).local().format('MMMM Do, YYYY, h:mm:ss a');
  }
  return '';
}

export function dateTimeAt(value) {
  if (value) {
    return moment(String(value)).local().format('MMMM Do, YYYY, [at] h:mm:ss a');
  }
  return '';
}

export function date(value) {
  if (value) {
    return moment(String(value)).local().format('YYYY-MM-DD');
  }
  return '';
}

export function longDate(value) {
  if (value) {
    return moment(String(value)).local().format('MMMM Do, YYYY');
  }
  return '';
}

export function yesNo(value) {
  if (value) {
    return 'Yes';
  }
  return 'No';
}

export function approvedRejected(value) {
  if (value) {
    return 'Approved';
  }
  return 'Rejected';
}

export function percent(value, decimals) {
  if (!value) value = 0;
  if (!decimals) decimals = 0;

  value = value * 100;
  return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals) + '%';
}

export function thousands(value) {
  return new Intl.NumberFormat().format(value);
}

export function selectTextFromPossibleOptions(value, possibleOptions) {
  let matchingValue = _.filter(possibleOptions, (x) => x.value === value)[0];
  if (matchingValue) {
    return matchingValue.text;
  }

  return value;
}
