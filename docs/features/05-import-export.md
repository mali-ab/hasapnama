# Feature 05 — CSV import & export (+ a downloadable template)

> **Why it beats other CRMs:** nobody type-type-types hundreds of rows into a
> CRM. The best tools make the *leap* from "my messy Excel sheet" to "clean,
> merged customer list" near-instant, and let you take your data back anytime
> (no lock-in). HasapNama gives you both with one dial button.

## Server

```
POST /api/customers/import     (multipart: template fields below, .csv UTF-8)
GET  /api/customers/export     (same columns, one row per customer)
GET  /api/customers/import/template   (download an empty header-only .csv)
```

Import rules (devil's in the detail — where CRMs slip):

| Column      | Required | Notes                                    |
|-------------|----------|------------------------------------------|
| `name`      | ✅       | trimmed, empty→skip row                  |
| `company`   |          |                                          |
| `email`     |          | lower-cased, empty→null                  |
| `phone`     |          | kept as-is — never reformat a real number|
| `address`   |          |                                          |
| `notes`     |          |                                          |

* `Link header` + `{ imported, skipped, failed }` result — you *see* what you
  didn't import, with row numbers.
* Matches on `email` first, then `name` → *update* if found (no duplicates),
  else *create*.

## App

* Customers page gets an **`⬇ Import`** button → opens the file picker
  (`.csv`), streams to the server, then a result summary toast.
* **`⬆ Export`** always available — even with zero customers (empty template),
  so you can start from a blank slate on any new device/app.

## Acceptance

- [ ] Import of 1,000 rows completes in < 3s (streamed, not buffered)
- [ ] Re-running the same file updates instead of duplicating (idempotent)
- [ ] Export round-trips: import the export => 0 changes
