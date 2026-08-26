# VendoraPOS User Guide

Plain-language instructions for using each feature as it ships. This is written for the person running
the store, not for developers — see [architecture.md](architecture.md) for the technical design.

A new section is added here every time a feature is completed, so this file always reflects what the app
can currently do.

## Products

### Viewing products

Go to **Products** in the top navigation. You'll see every product with its SKU, name, price, quantity on
hand, expiry date (if it has one), and whether it's active. A quantity at or below the store's low-stock
threshold is shown in red with a ⚠ **Low** flag, and an expiry date that's already passed or is coming up
within a week is highlighted the same way — see **Stock Alerts** below for a dedicated page listing just
these.

The row actions (far right of each row) are icons to keep the table compact: a pencil for **Edit**, a
clock for **Manage stock** (view history and record restock/waste/adjustment), and a circle/checkmark
toggle for **Activate**/**Deactivate**. Hover or tap-and-hold any icon to see what it does.

### Adding a product

Click **+ New Product**, fill in SKU, name, price, initial quantity, and an optional description, then
**Save**. The initial quantity is automatically recorded as the product's first stock history entry (see
below), so it's never a mystery number later.

A few more fields are optional: **Barcode** (the number printed on the product's real-world barcode), a
product **Photo**, and an **Expiry date**. None are required, but Barcode and Photo make **Checkout**
faster and easier to browse — see below. Click **Select Image** to choose a photo from your device's own
photo gallery/camera roll (on phone or tablet) or file browser (on desktop) — the picture is saved and
shown as a preview with a **Remove** option if you want to change it. There's no need to host the image
anywhere yourself.

**Expiry date** is entirely optional — leave it blank for anything that doesn't expire (most retail and
café items). For a pharmacy, or anything else with a shelf life, set it and the product automatically
shows up on the **Stock Alerts** page once it's within a week of that date, so it doesn't get missed.

### Editing a product

Click the pencil icon (**Edit**) on a product's row. You can change name, price, description, barcode,
photo, and expiry date. SKU and quantity can't be edited here directly — quantity changes go through
**Manage stock** instead, so there's always a record of *why* it changed. Click **Save** and confirm in
the dialog that appears — every saved edit is recorded in the product's **Activity Log** (see below), so
there's always a record of what changed and when.

### Deactivating a product

Click the toggle icon (**Deactivate**) on a product's row, then confirm in the dialog that appears, to
mark it inactive (its Status column changes to "Inactive") without deleting it or losing its stock
history. Click the same icon (now **Activate**) on an inactive product to bring it back immediately — no
confirmation needed, since re-activating isn't risky. Use this for a product you've stopped carrying but
don't want to erase.

> There is no way to permanently delete a product. Almost every product picks up stock history the moment
> it's created (any starting quantity above zero), so deletion was never actually usable in practice —
> **Deactivate** is the supported way to retire a product while keeping its full history intact.

## Stock History

Click the clock icon (**Manage stock**) on any product's row. This one page covers both recording a
change (Restock, Waste, or Adjustment - see below) and viewing the full stock ledger afterward, so there's
one place to go for anything stock-related on that product.

### Why three reasons, not one?

| Reason | What it means | Direction |
|---|---|---|
| **Restock** | New stock arrived — a delivery, a supplier order | Always **adds** stock |
| **Waste** | Stock was lost for a bad reason — expired, damaged, spoiled | Always **removes** stock |
| **Adjustment** | A correction after finding the recorded count is wrong (e.g. a stocktake found more or fewer than the system says) | Can go **either way** |

You don't need to think about plus/minus signs for Restock or Waste — just enter how many units were
received or lost, and the app applies the correct direction. Adjustment is the one case where you enter a
signed number yourself, since a correction can go up or down.

There's a fourth entry you'll see in the history table, **Initial Stock** — this is recorded automatically
the moment a product is created with a starting quantity. You never select it manually.

You'll also see a **Sale** entry appear automatically whenever a product is sold through **Checkout** (a
negative amount) or when that sale is voided (a positive amount reversing it) — see below. Like Initial
Stock, this isn't something you select manually; it's recorded for you as a side effect of checkout.

**Sale** entries don't have a **Reverse** button here, on purpose — reversing one from this page would
change the product's stock without changing anything about the sale itself, leaving the two disagreeing
with each other. If a Sale entry needs undoing, do it from the sale itself in **Sales** (void it, or
restore a sale you voided by mistake — see below), which keeps the stock and the sale record in sync
automatically.

### Recording a stock change

On a product's History page, fill in the reason, the quantity, and an optional note (e.g. "Delivery from
ABC Supplier" or "Found broken on shelf"), then **Record movement**. The product's quantity on hand
updates immediately, and the new entry appears at the top of the history table.

### Fixing a mistake

Stock history entries can't be edited or deleted once recorded — this is intentional, so the record of
what happened can't be quietly changed later. If you entered the wrong number, click **Reverse** on that
row: it pre-fills the form with the exact opposite quantity as an Adjustment, with a note explaining what
it's correcting. Review it and click **Record movement** to confirm — this cancels out the mistake while
keeping both entries visible in the history, so nothing is hidden.

> If a stock change would make the quantity go negative, you'll see an error message and the change is
> rejected — the product's quantity is never allowed to go below zero.

## Stock Alerts

Click **⚠ Stock Alerts** in the top navigation for a focused list of exactly what needs
attention — no need to scan the whole product table yourself:

- **Low stock** — every active product at or below the store's low-stock threshold (10 units by
  default), sorted so the most urgent is at the top. Click **Restock** on any row to jump straight to
  recording a Restock movement for it.
- **Expired** — every active product whose expiry date has already passed.
- **Expiring soon** — every active product expiring within the next 7 days.

This is especially useful for a pharmacy or anything else carrying stock with a shelf life, but works for
any store — products without an expiry date set just never appear in the expiry sections. The list
updates live if a sale happens on another screen while you have this page open — see "Live everywhere"
further down — and deactivated products are left out entirely, since they're not being sold anyway.

> The low-stock threshold (10 units) is currently a single store-wide number, not set per product. If
> your store needs different thresholds for different products (e.g. fast-moving syrup vs. a slow-moving
> supplement), let your VendoraPOS contact know — that's a small, deliberate limitation for now.

## Activity Log

Below the stock history on a product's **History** page is its **Activity Log** — a separate record of
changes to the product's own details (not its quantity): every saved edit (showing exactly which fields
changed, old value to new value) and every activate/deactivate. Like stock history, entries here can't be
edited or removed — it's a permanent record of what happened to the product over time.

> **Live everywhere:** Stock numbers you see anywhere in the app — Products, Checkout, Stock History —
> update automatically the instant they change on *any* open screen, including another cashier's tablet
> or register running at the same time. There's nothing to click and no reload needed; if a sale, void,
> restock, waste, or adjustment happens on one screen, every other open screen reflects the new quantity
> within a second or two. If the connection to the server is briefly unavailable, everything keeps
> working normally off the last-known numbers — it just quietly reconnects once the server's reachable
> again.

## Checkout

Go to **Checkout** to ring up a sale. Products are shown as a grid of tappable cards — each shows its
picture (or a placeholder if none was set), name, SKU, price, quantity in stock, and its barcode if it has
one. Tap a card to add it to the cart — a small confirmation ("Product added") pops up briefly at the
bottom of the screen so you know it registered. Only active products appear here — deactivated products
can't be sold. The grid is paginated the same way the product list is (10–100 per page), so it stays
usable with a large catalog, and sized for comfortable tapping on a tablet.

In the cart, click into a line's quantity to change it — its current value is auto-selected, so you can
just type the new number without deleting the old one first (same behavior as Amount tendered, below).
Press **Enter** to confirm the new quantity, or just tap/click elsewhere. Clearing the box (e.g. to type a
fresh number) never removes the product — it's treated as "1" if you click away without entering anything.
The only way to remove a line is the **✕** button.

The **Current Transaction** panel is always visible — even before you've added anything — and never
scrolls out of view. The item list scrolls on its own if it gets long, while the heading, payment section,
and **Complete Sale** button all stay fixed in place. On a tablet or desktop held sideways (landscape),
this panel sits on the right with the product grid on the left; on a phone or a tablet held upright
(portrait), it's pinned near the bottom of the screen instead, with a capped height and its own internal
scroll. New items always go to the *top* of the list, so the one you just added is immediately visible.
The whole Checkout page is sized to fit the screen — it never grows a scrollbar of its own, no matter how
many products or cart items there are.

The search box is focused automatically the moment the page loads, again right after completing a sale,
and — since a barcode scanner has no idea what's currently focused — automatically **any time you click
anything that isn't a field you'd deliberately type into** (a pagination button, a product card, an empty
area of the page). So no matter what you last clicked, the search box is always ready for the next scan
without you needing to click back into it yourself. It also filters by name or SKU as you type, and
matches an exact barcode too. If the search narrows things down to exactly one product, pressing
**Enter** adds that one.

**Keyboard shortcuts** are shown right on the relevant buttons/fields (not just written here), so a busy
checkout rarely needs the mouse:
- **Ctrl+F** — jump to the search box from anywhere on the page (badge shown in the search box)
- **Enter** (in the search box) — add the matching product (exact barcode match, or the one remaining
  result) and clear the search for the next scan
- **Esc** (in the search box) — clear the search
- **Ctrl+Q** — jump to the top cart line's quantity field (optional — tapping/clicking works just
  as well; badge shown next to "Current Transaction")
- **Ctrl+M** — jump to Amount tendered, when paying by Cash (badge shown next to the field)
- **Ctrl+Enter** — same as clicking "Complete Sale" (badge shown on the button)
- **Enter** — once the confirmation dialog below is open, confirms it (Esc cancels it)

Choose a payment method:
- **Cash** — enter the amount tendered; the change due is calculated for you automatically. The field's
  contents are auto-selected when you click/tab into it (or jump to it with Ctrl+M), so you can just type
  over the "0" without deleting it first. You can't complete the sale until the tendered amount covers the
  total — a message appears immediately if it doesn't.
- **Card** — no tendered amount needed; this assumes payment already happened on your card terminal and
  just records that the sale was paid by card.

Click **Complete Sale** (or press Ctrl+Enter) to bring up a confirmation showing the item count, total,
and payment method — confirm to finish. This immediately reduces the sold products' stock on hand (and
records a **Sale** entry in each product's Stock History). You **stay on the Checkout page** afterward —
the cart clears and a brief confirmation ("Sale completed — total ...") appears, ready for the next
customer right away. The finished sale's full receipt is always available afterward from **Sales**.

## Sales

Go to **Sales** to see every past sale — date, total, payment method, and status — paginated the same way
the product list is. Click **View** on any row to open its receipt: the line items, total, and (for cash
sales) amount tendered and change due.

Every date/time shown anywhere in the app (here, Stock History, Activity Log) is shown in your own
device's local time zone — a sale rung up at 3:49pm your time shows as 3:49pm, not the server's internal
clock time.

### Returning one item from a larger sale

A customer usually doesn't return the *entire* order — often it's just one product out of several.
Open the sale's receipt and click **Return…** next to that specific line, enter how many units are being
returned (it can't exceed how many of that line are still active) and an optional reason, then confirm.
This restocks exactly that quantity, records a compensating **Sale** entry in that product's history, and
leaves every other line in the sale completely untouched. The receipt then shows that line's returned
count alongside its original quantity, plus a **Refunded** and **Net total** line in the summary.

You can return part of a line more than once (e.g. 1 today, another 1 next week) as long as some of it is
still active — once a line's full quantity has been returned, its **Return…** button disappears.

Recorded a return by mistake? Click **Undo** next to the "(N returned)" note on that line, enter how many
units to undo (up to how many were returned) and an optional reason, then confirm. This re-deducts the
stock and reduces that line's returned count back down — the exact reverse of Return…, mirroring how
Restore reverses a whole-sale Void.

### Voiding a sale

If the *whole* sale needs to be undone — rung up by mistake, wrong items across the board, etc. — open it
and click **Void this sale**, then confirm. This restores whatever stock is still active across every line
(any quantity already returned individually isn't restocked twice) and marks the sale as voided. A voided
sale is never deleted — it stays visible with a "Voided" status and the reason you gave, so there's a full
record of what happened.

### Undoing a void

Voided the wrong sale, or changed your mind? Open it and click **Restore this sale**, then confirm. This
flips the sale back to active and re-deducts the stock — the exact reverse of Void, done as one step so
the sale and the stock always agree. It can fail if the stock is no longer available (e.g. that item was
sold to someone else in the meantime) — you'll see a clear error message if so.

> Undo (on a return) and Restore (on a void) both require the stock still being available - if items
> were sold again in the meantime, you'll see a clear error and the undo won't go through.
