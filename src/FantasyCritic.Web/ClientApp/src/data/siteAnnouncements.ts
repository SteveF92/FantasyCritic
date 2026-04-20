import type { SiteAnnouncement } from '@/models/SiteAnnouncement';

/**
 * Local site-wide announcements. Replace or merge with API data later.
 * Order is not significant: the UI sorts by `sortDate`, then parseable `postedAt`, then file order (later = newer).
 */
export const SITE_ANNOUNCEMENTS: SiteAnnouncement[] = [
  {
    id: 'royale-groups-and-charts',
    title: 'Royale Groups, Charts, and More!',
    body: 'Edit src/data/siteAnnouncements.ts to post messages for all signed-in users. Entries use the SiteAnnouncement model in src/models/SiteAnnouncement.ts.',
    postedAt: '2026-04-19',
    sortDate: '2026-04-19',
    variant: 'secondary',
    href: 'https://www.youtube.com/watch?v=ZFsHZjA2AVg',
    linkLabel: 'Watch the DevLog!'
  }
];
