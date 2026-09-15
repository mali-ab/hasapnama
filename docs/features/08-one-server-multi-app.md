# Feature 08 — Multi-app ONE SERVER: the CRM your other apps feed into

> **Why it beats other CRMs:** store CRMs lock customers inside a single vendor
> app. HasapNama's pitch is literal — **two or more separate apps (different
> phones, different jobs, even different owners) all write to ONE server and ONE
> database**, so a sale logged in the t-shirt app today is the same person your
> wholesale app meets next week (no kopy-paste, no duplicate).

```
 phone A (t-shirt shop)  ─┐
 phone B (wholesale)     ─┼─► API @ :5080 ─► PostgreSQL (shared!)
   Windows app           ─┘        │
                                   └─ JWT issuer + per-user OwnerId scoping
```

## Principles (the 3 rules that make it *one* system, not N copies)

1. **Every row carries `OwnerId`** — set from the JWT `sub` claim, never from
   the body. One gone row can never cross into another app's data, even by
   mistake.
2. **The server owns identity** (register/login/me). Apps only ever hold a JWT.
   Add a 3rd app tomorrow: it just calls the same endpoints — zero changes to
   the DB.
3. **Merge by `email`, never by guess.** An API `POST /api/customers/merge`
   finds candidates by normalized email across *all* apps and hands the union
   list back; the user picks the winner (no silent drama data).

## Already live in this repo

- [x] `src/HasapNama.Shared` — the DTO + entity contracts both apps compile
      against (single source of truth)
- [x] `src/HasapNama.Server` — JWT auth, EF Core + Npgsql, customer CRUD all
      scoped by `OwnerId`; multi-app-safe by construction
- [x] `src/HasapNama.App` — MAUI Blazor Hybrid, Android + Windows, talks to the
      one server

## This feature, new

```
POST /api/customers/merge   { keep: Guid, absorb: Guid }
```
moves every `Deal`, `Todo`, `Activity` from `absorb` onto `keep`, then deletes
the loser row. Returns the merged customer. Undo = `POST /api/customers/merge`
backwards (keep/absorb swapped) within the same session.

## Acceptance

- [ ] Log in the SAME user on phone A and Windows app → both show the identical
      customer list (they share `OwnerId`, so "shared" is free)
- [ ] Two DIFFERENT users/owners → invisible to each other, even live side by
      side in one server process
- [ ] Merging keeps the audit trail readable: `absorb → keep` activity entry
