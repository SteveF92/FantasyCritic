import GraphemeBreaker from 'grapheme-breaker-mjs';

export default {
  publisherIconIsValid(publisherIcon) {
    let length = GraphemeBreaker.countBreaks(publisherIcon);
    return length === 1;
  }
}
