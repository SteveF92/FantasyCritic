# Support Ticket Resolved History for Users

## Goal

Let users see admin resolution notes for closed support tickets. Today admins enter `ResolutionNotes` when closing a ticket in the admin console, but the user-facing Support Ticket page never loads or displays that field—and closed tickets disappear entirely once `ClosedAt` is set.

## Constraints

- No schema changes (`ResolutionNotes` and `ClosedAt` already exist on `tbl_user_supportticket`)
- No new API endpoints or NSwag regen (user data stays server-rendered on the existing Razor page)
- No email notifications or mid-ticket reply threading
- No admin UI changes (close flow and resolution-notes textarea stay as-is)

## Approach

Extend the existing Account → Support Ticket Razor page (`/account/manage/supportticket`) with a **Resolved tickets** section below the active-ticket UI.

### Data layer

Add one read method through the existing user-store stack:

| Layer | Change |
| --- | --- |
| `IReadOnlyFantasyCriticUserStore` | `GetClosedSupportTickets(Guid userID, int limit)` |
| `IFantasyCriticUserStore` | Same (if split from read-only interface pattern) |
| `MySQLFantasyCriticUserStore` | Query implementation |
| `FantasyCriticUserManager` | Pass-through |

SQL:

```sql
SELECT * FROM tbl_user_supportticket
WHERE UserID = @userID AND ClosedAt IS NOT NULL
ORDER BY ClosedAt DESC
LIMIT @limit
```

Default limit: **20**. Map rows to `SupportTicket` domain objects the same way `GetActiveSupportTicket` does (load user, `entity.ToDomain(user)`).

### User-facing UI

**Page layout (top to bottom):**

1. **Active ticket section** — unchanged (verification code, edit/close when `OpenedByUser`).
2. **Resolved tickets section** — new; rendered only when the user has at least one closed ticket.

Each resolved ticket card displays:

| Field | Source |
| --- | --- |
| Opened | `OpenedAt` |
| Closed | `ClosedAt` |
| Your issue | `IssueDescription` |
| Resolution | `ResolutionNotes`, or *"No resolution notes provided."* if null/blank |

Tickets opened by admin (`OpenedByUser = false`) appear in this list. The section is hidden when there are no closed tickets (no empty-state placeholder).

### Edge cases

- **Closed with no notes:** Ticket still appears; resolution line shows the empty-state message.
- **Limit of 20:** Older closed tickets are not shown. Acceptable given low ticket volume.
- **Active + resolved:** Both sections can appear on the same page when the user has an open ticket and prior closed tickets.

## Testing

- **Manual smoke test:** User opens ticket → admin closes with resolution notes → user page shows the ticket in the resolved list with notes visible.
- **Manual edge case:** Close a ticket with empty resolution notes → user sees the empty-state resolution message.
- Automated unit test optional (`FantasyCritic.FakeRepo` does not implement the user store today); no integration test required unless added during implementation for confidence.

## Out of scope

- Email notification when a ticket is closed
- Banner highlighting recently closed tickets
- User ability to reopen closed tickets
- Admin replies while a ticket is still open (threading)
- Ticket history beyond the most recent 20 closed tickets
