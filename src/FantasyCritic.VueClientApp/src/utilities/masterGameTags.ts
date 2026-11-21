import { type CSSProperties } from 'vue';

import type { MasterGameTagViewModel } from '@/api/clients';
import { useInterLeagueStore } from '@/store/interLeagueStore';

export function getMatchingTag(tagName: string): MasterGameTagViewModel {
  const interLeagueStore = useInterLeagueStore();
  const matchingTag = interLeagueStore.allTags.find((x) => x.name === tagName);
  if (!matchingTag) {
    throw new Error(`Could not find matching tag for: ${tagName}`);
  }

  return matchingTag;
}

export function getMatchingTags(tagNames: string[]): MasterGameTagViewModel[] {
  const interLeagueStore = useInterLeagueStore();
  const matchingTags = tagNames.map((x) => interLeagueStore.allTags.find((y) => y.name === x)).filter((x) => !!x);
  return matchingTags;
}

export function getMultiBadgeColor(tagNames: string[]): CSSProperties {
  let colorString = '';
  const stripeWidth = 100.0 / tagNames.length;
  for (let i = 0; i < tagNames.length; i++) {
    const tag = getMatchingTag(tagNames[i]);
    const badgeColor = tag.badgeColor;

    colorString += `#${badgeColor} ${i * stripeWidth}%, #${badgeColor} ${(i + 1) * stripeWidth}%,`;
  }

  colorString = colorString.substring(0, colorString.length - 1);
  const backGroundString = `linear-gradient(135deg, ${colorString})`;

  return {
    backgroundImage: backGroundString,
    color: 'white'
  };
}
