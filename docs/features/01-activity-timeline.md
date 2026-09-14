# Feature 01 — Activity timeline per customer

> **Why it beats other CRMs:** most small-biz CRMs store a customer's *current state*
> but forget *what happened*. Real sales people need "who called when, what they
> said, what they promised" — a diary, not just a table. This turns every
> customer record into a living history.

## What we build

Every customer gets an **activity feed** (like a chat log): notes, phone calls,
emails, meetings, and status changes — newest first, with author + timestamp.

```
id        uuid
customerId uuid   (FK -> customers)
ownerId    uuid   (for the multi-app / per-user isolation)
type       text   ("note" | "call" | "email" | "meeting" | "status-change" | "created")
author     text   (who logged it)
summary    text   (what happened)
created_at timestamptz
```

## Model

```csharp
// HasapNama.Shared/Models/Models.cs
public class Activity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Guid OwnerId { get; set; }              // multi-app isolation
    public string Type { get; set; } = "note";     // note|call|email|meeting|status-change|created
    public string Author { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

## Server

1. `DbSet<Activity> Activities` in `AppDbContext` (FK to Customers, `HasMaxLength` on Summary).
2. Automatic entries: when a customer is **created**, insert an activity of type
   `created` ("New customer created").
3. New endpoint, auth-scoped to the customer owner:

```csharp
// Program.cs — inside the customers group
customers.MapPost("/{id:guid}/activities", async (Guid id, ActivityRequest req, ClaimsPrincipal user, AppDbContext db) =>
{
    var ownerId = user.GetUserId();
    var customer = await db.Customers.SingleOrDefaultAsync(c => c.Id == id && c.OwnerId == ownerId);
    if (customer is null) return Results.NotFound();
    var activity = new Activity { CustomerId = id, OwnerId = ownerId, Type = req.Type, Summary = req.Summary.Trim(), Author = req.Author };
    db.Activities.Add(activity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/customers/{id}/activities/{activity.Id}", activity);
});
```

## App

* In `Components/Pages/CustomerEdit.razor`, add a **"History"** tab showing the feed.
* Quick-log buttons: one tap adds a `call` ("Called — busy, try again Friday") or
  `note`. Minimal typing wins on mobile.

## Acceptance

- [ ] Creating a customer auto-logs a `created` activity
- [ ] Log buttons write to the server and the feed refreshes instantly
- [ ] Only the owning user can read/write activities for their customer
