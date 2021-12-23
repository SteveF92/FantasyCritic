import Vue from 'vue';
import moment from 'moment';

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

Vue.filter('money', function (value) {
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
});

Vue.filter('dateTime', function(value) {
  if (value) {
    return moment(String(value)).local().format('MMMM Do YYYY, h:mm:ss a');
  }
  return '';
});

Vue.filter('date', function (value) {
  if (value) {
    return moment(String(value)).local().format('YYYY-MM-DD');
  }
  return '';
});

Vue.filter('yesNo', function (value) {
  if (value) {
    return 'Yes';
  }
  return 'No';
});

Vue.filter('percent', function(value, decimals) {
  if (!value) value = 0;
  if (!decimals) decimals = 0;

  value = value * 100;
  return Math.round(value * Math.pow(10, decimals)) / Math.pow(10, decimals) + '%';
});

Vue.filter('thousands', function (value) {
  return new Intl.NumberFormat().format(value);
});


Vue.filter('selectTextFromPossibleOptions', function (value, possibleOptions) {
  let matchingValue = _.filter(possibleOptions, x => x.value === value)[0];
  if (matchingValue) {
    return matchingValue.text;
  }

  return value;
});
