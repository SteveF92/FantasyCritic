import Vue from 'vue';
import { DateTime } from 'luxon';

Vue.filter('score', function (value, decimals) {
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
});

Vue.filter('money', function (value, decimals) {
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
});

Vue.filter('dateTime', function (value) {
  if (value) {
    return DateTime.fromISO(String(value)).toLocaleString(DateTime.DATETIME_FULL);
  }
  return '';
});

Vue.filter('dateTimeAt', function (value) {
  if (value) {
    return DateTime.fromISO(value).toFormat("MMMM dd, yyyy, 'at' h:mm:ss a");
  }
  return '';
});

Vue.filter('date', function (value) {
  if (value) {
    return DateTime.fromISO(value).toFormat('yyyy-MM-dd');
  }
  return '';
});

Vue.filter('longDate', function (value) {
  if (value) {
    return DateTime.fromISO(value).toFormat('MMMM dd, yyyy');
  }
  return '';
});

Vue.filter('yesNo', function (value) {
  if (value) {
    return 'Yes';
  }
  return 'No';
});

Vue.filter('approvedRejected', function (value) {
  if (value) {
    return 'Approved';
  }
  return 'Rejected';
});

Vue.filter('percent', function (value, decimals) {
  if (!value) value = 0;
  if (!decimals) decimals = 0;

  value = value * 100;
  return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals) + '%';
});

Vue.filter('thousands', function (value) {
  return new Intl.NumberFormat().format(value);
});

Vue.filter('selectTextFromPossibleOptions', function (value, possibleOptions) {
  const matchingValue = possibleOptions.find((x) => x.value === value);
  if (matchingValue) {
    return matchingValue.text;
  }

  return value;
});
