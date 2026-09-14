# Feature 03 — Follow-up tasks & reminders (nag system done right)

> **Why it beats other CRMs:** reminder pings die in the notification tray. The
> winners re-surface the task *where the work happens* — on the customer's page,
> on the dashboard, and as one-tap "mark done". HasapNama ties the reminder to
> the customer so "call Arslan today" is never a floating orphan again.

## Data model (new — `Todo`)

```csharp
public class Todo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? CustomerId { get; set; }   // 1:N — reminder lives on a customer
    public Guid? OwnerId { get; set; }      // who owns it (multi-app safety)
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? DueAt { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## Server

1. `DbSet<Todo> Todos` in `AppDbContext` + migration.
2. Endpoints (all scoped by `OwnerId` via the JWT claim):
   - `POST /api/todos` (create)
   - `GET /api/todos?open=true` (open items, newest first)
   - `PATCH /api/todos/{id}` → `{ isCompleted: true }`
   - `DELETE /api/todos/{id}`
3. The **Dashboard returns "today's follow-ups"**: due today + overdue, open,
   joined to customer name for display.

## App

* Customers page: a small **"Due ⏰"** badge on a customer who has open follow-ups.
* Dashboard: numbered list of today's follow-ups with quick `✓ done`.
* Add button on the customer page: "Schedule follow-up" (title + date).

## Acceptance

- [ ] Reminder counts as "due today" and shows customer name even before opening it
- [ ] Marking done via dashboard removes it from the list without leaving the page
- [ ] Device-tested: Android notification fires for due items (time-permitting)
