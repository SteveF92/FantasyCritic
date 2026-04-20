/** Bootstrap-Vue `b-alert` variants we use for site copy. */
export type SiteAnnouncementAlertVariant = 'info' | 'warning' | 'secondary' | 'success' | 'danger';

export interface SiteAnnouncement {
  /** Stable id for dismiss keys, analytics, or future API rows. */
  id: string;
  title: string;
  /** Plain text only (no HTML). */
  body: string;
  /** Optional line shown under the title (any string). */
  postedAt?: string;
  /** Optional ISO 8601 date for ordering when `postedAt` is not parseable or ties need a stable sort. */
  sortDate?: string;
  /** Optional link target (site-relative or absolute URL). */
  href?: string;
  /** Shown with `href`; include both or neither. */
  linkLabel?: string;
  variant?: SiteAnnouncementAlertVariant;
}
