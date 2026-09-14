# Feature 02 — Sales pipeline / deal stages

> **Why it beats other CRMs:** most tools show "customers" as a flat list and
> leave funnel-math to you. HasapNama shows the *money shape* of your book: how
> many deals each stage holds and what the expected value is — without needing a
> separate quoting add-on.

## Data model (new — `Deal`)

```csharp
public class Deal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? OwnerId { get; set; }          // multi-app safety
    public Guid CustomerId { get; set; }        // 1:N — a deal hangs off a customer
    public Guid? ProductId { get; set; }        // optional product reference
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }          // Money! decimal, not float
    public string Stage { get; set; } = "new";   // new|contacted|qualified|proposal|won|lost
    public DateOnly ExpectedClose { get; set; }
    public decimal? Probability { get; set; }    // 0..100
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

## Server

1. `DbSet<Deal> Deals` + migration; `HasPrecision(18,2)` on `Amount`.
2. Endpoints (JWT-scoped, like customers):
   - `GET /api/customers/{id}/deals`
   - `POST /api/customers/{id}/deals`
   - `PATCH /api/deals/{id}` (change `Stage` only — that's the "move through funnel" operation)
3. **Funnel query** — server-side, no client math:
   ```
   SELECT stage, count(*) AS count, round(sum(amount)) AS value
   FROM deals WHERE "OwnerId" = @owner AND stage != 'lost'
   GROUP BY stage ORDER BY min(weight)
   ```

## App (pipeline page)

```
    new        contacted      qualified      proposal      won
   [  3 ]      [  5 ]         [  2 ]         [  1 ]       [ 9 ]
   $1,200      $4,900         $8,300         $6,000       $21k
```

Wide screens render a horizontal Kanban; phones stack cards vertically. A single
tap on a card opens the deal side sheet with **Move pipeline** dropdown.
Customers that have deals get a small ₼/usd badge on the list.

## Acceptance

- [ ] Moving a deal `proposal → won` adds it to the won KPI automatically
- [ ] A lost deal is *kept* (with reason), never deleted — so you can learn
- [ ] Expected-value column updates live as stages change
