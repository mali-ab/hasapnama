# Feature 07 — Team workspace: several people, one shared customer base

> **Why it beats other CRMs:** every serious CRM now streams toward
> "workspaces" (Slack-for-customers). HasapNama is already multi-app (several
> apps → one server), so the *same ownership model* trivially extends to
> **several people owning one workspace** — without a second sharing concept.

## Model

```csharp
public class Workspace
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;   // e.g. "Ali's shop"
    public Guid OwnerId { get; set; }                  // creator = admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Join table: a user can be a member of many workspaces.
public class WorkspaceMember
{
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "member"; // admin|manager|member
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
```

Customers then gain `WorkspaceId?` — `null` = the user's **personal** bucket
(so a one-person shop is unchanged and simple).

## Server

* `GET /api/workspaces` (my memberships), `POST /api/workspaces`
* `POST /api/workspaces/{id}/members` (admin only) — by username; invite
  resolves to the AppUser.
* `GET /api/workspaces/{id}/customers` — a **shared** list: every member sees
  every customer in that workspace (still `OwnerId`-safe underneath; visible
  only through the workspace).

## App

* Workspace switcher in the top bar (`Ali's shop ▾`).
* Customers page header shows `Shared workspace · 3 members`.
* Role badge on members list (admin/manager/member).

## Acceptance

- [ ] Creating a workspace + inviting a colleague → both users see the same
      shared customer list from TWO different phones
- [ ] A non-member cannot list that workspace's customers (401/403)
- [ ] One-person mode (no workspace) still works exactly like today
