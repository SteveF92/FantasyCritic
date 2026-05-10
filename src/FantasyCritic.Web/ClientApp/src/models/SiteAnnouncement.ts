export interface SiteAnnouncement {
  /** Stable id for dismiss keys, analytics, or future API rows. */
  id: string;
  htmlId: string;
  title: string;
  body: string;
  /** NodaTime `Instant` serialized as ISO-8601 (e.g. end of JSON pipeline). */
  postedAt: string;
  linkAddress?: string;
  linkLabel?: string;
}
