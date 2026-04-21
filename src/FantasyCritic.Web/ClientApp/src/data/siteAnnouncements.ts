import type { SiteAnnouncement } from '@/models/SiteAnnouncement';

/**
 * Local site-wide announcements. Replace or merge with API data later.
 * Order is not significant: the UI sorts by `sortDate`, then parseable `postedAt`, then file order (later = newer).
 */
export const SITE_ANNOUNCEMENTS: SiteAnnouncement[] = [
  {
    id: 'royale-groups-and-charts',
    title: 'Royale Groups, Charts, and More!',
    body: "The biggest site update in months! Critic's Royale received a lot of attention this time, with new features for forming groups, seeing a player's past performance, and charts to view a quarter in more detail.",
    postedAt: '2026-04-19',
    sortDate: '2026-04-19',
    variant: 'secondary',
    href: 'https://www.youtube.com/watch?v=wOjiejePfRU',
    linkLabel: 'Watch the DevLog!'
  }
];
