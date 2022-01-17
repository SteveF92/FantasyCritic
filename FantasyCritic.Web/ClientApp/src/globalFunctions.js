import GraphemeBreaker from 'grapheme-breaker-mjs';

export default {
  publisherIconIsValid(publisherIcon) {
    if (!publisherIcon) {
      return false;
    }
    let length = GraphemeBreaker.countBreaks(publisherIcon);
    return length === 1;
  }
}
