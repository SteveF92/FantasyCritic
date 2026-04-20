import type { SiteAnnouncement } from '@/models/SiteAnnouncement';

function getAnnouncementSortTime(item: SiteAnnouncement): number {
  if (item.sortDate) {
    const t = Date.parse(item.sortDate);
    if (!Number.isNaN(t)) {
      return t;
    }
  }
  if (item.postedAt) {
    const t = Date.parse(item.postedAt);
    if (!Number.isNaN(t)) {
      return t;
    }
  }
  return 0;
}

/** Newest first: `sortDate` or parseable `postedAt`, then later entries in the source array win ties. */
export function sortSiteAnnouncementsNewestFirst(items: SiteAnnouncement[]): SiteAnnouncement[] {
  return [...items]
    .map((item, index) => ({ item, index }))
    .sort((a, b) => {
      const ta = getAnnouncementSortTime(a.item);
      const tb = getAnnouncementSortTime(b.item);
      if (tb !== ta) {
        return tb - ta;
      }
      return b.index - a.index;
    })
    .map((x) => x.item);
}
