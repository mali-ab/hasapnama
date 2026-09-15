# Feature 04 — Live dashboard (the "at a glance" home screen)

> **Why it beats other CRMs:** most dashboards are vanity vanity-vanity nonsense
> ("you have 137 contacts!"). Ours answers the question a trader actually wakes
> up with: **"Who do I call today, and is the month going better than last?"**

## Server — single aggregation endpoint (no client-side math)

```
GET /api/stats/summary?days=30
```
returns, all computed in **one** SQL pass over the owner's rows:

```jsonc
{
  "customersTotal": 137,
  "customersAdded": 12,           // last 30 days
  "dealsOpen": 8,  "dealsOpenValue": 12400,
  "dealsWon30": 3, "dealsWonValue30": 3100,
  "topCustomers": [               // by total won value (shared-app aware)
     { "name": "AhalDokuz", "value": 2050 },
     { "name": "Kara Märt",  "value": 890 }
  ],
  "dueFollowups": 4               // today + overdue open, joined to names
}
```

`GROUP BY`-free single pass: `SUM(CASE WHEN …) FILTER(WHERE …)` — so the whole
thing stays a round trip even at 10k customers.

## App — `/dashboard` is the app's HOME after login

```
  [137]  customers     [8 · $12.4k]  open deals     [4]  due today ⏰
  ─────────────────────────────────────────────────────────
  Won last 30 days:  3  ($3.1k)     ┃  Top this month:
  Added last 30 d.:  12             ┃  AhalDokuz  $2 050
                                    ┃  Kara Märt   $890
  ▓▓▓▓▓▓▓▓▓░░░ 82% ↑ vs last 30d     ┃  Myqtyn      $420
  ─────────────────────────────────────────────
  ⏰ DUE TODAY
  • 09:00 call Arslan (AhalDokuz)        [✓]
  • 15:30 renew contract TurkmenistanPC  [✓]
```

- Green/red Δ arrows vs the previous 30-day window on every KPI.
- Tapping any KPI card jumps to the real page (customers / pipeline / follow-ups).
- Top list is truncated to 3 on phones, 5 on wide screens.

## Acceptance

- [ ] All five numbers render after login without a single spinner wait (>200ms
      → fail; it must feel instant from the JWT that's already cached)
- [ ] Two apps (phone + desktop) on the same account show the **same board**
- [ ] No number on the board requires scrolling to reach a second one
