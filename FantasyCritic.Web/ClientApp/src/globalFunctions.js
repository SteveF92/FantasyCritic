import GraphemeBreaker from 'grapheme-breaker-mjs';

export default {
  publisherIconIsValid(publisherIcon) {
    if (!publisherIcon) {
      return true;
    }
    let length = GraphemeBreaker.countBreaks(publisherIcon);
    return length === 1;
  },
  ordinal_suffix_of(i) {
    var j = i % 10,
      k = i % 100;
    if (j == 1 && k != 11) { 
      return i + "st";
    }
    if (j == 2 && k != 12) {
      return i + "nd";
    }
    if (j == 3 && k != 13) {
      return i + "rd";
    }
    return i + "th";
  }
}
