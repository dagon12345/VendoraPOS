# VendoraPOS User Guide

Plain-language instructions for using each feature as it ships. This is written for the person running
the store, not for developers — see [architecture.md](architecture.md) for the technical design.

A new section is added here every time a feature is completed, so this file always reflects what the app
can currently do.

## Products

### Viewing products

Go to **Products** in the top navigation. You'll see every product with its SKU, name, price, quantity on
hand, and whether it's active.

### Adding a product

Click **+ New Product**, fill in SKU, name, price, initial quantity, and an optional description, then
**Save**. The initial quantity is automatically recorded as the product's first stock history entry (see
below), so it's never a mystery number later.

### Editing a product

Click **Edit** on a product's row. You can change name, price, and description. SKU and quantity can't be
edited here directly — quantity changes go through **Stock History** instead, so there's always a record
of *why* it changed. Click **Save** and confirm in the dialog that appears — every saved edit is recorded
in the product's **Activity Log** (see below), so there's always a record of what changed and when.

### Deactivating a product

Click **Deactivate** on a product's row, then confirm in the dialog that appears, to mark it inactive (its
Status column changes to "Inactive") without deleting it or losing its stock history. Click **Activate**
on an inactive product to bring it back immediately — no confirmation needed, since re-activating isn't
risky. Use this for a product you've stopped carrying but don't want to erase.

> There is no way to permanently delete a product. Almost every product picks up stock history the moment
> it's created (any starting quantity above zero), so deletion was never actually usable in practice —
> **Deactivate** is the supported way to retire a product while keeping its full history intact.

## Stock History

Click **History** on any product's row to see its full stock ledger and record new stock changes.

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

## Activity Log

Below the stock history on a product's **History** page is its **Activity Log** — a separate record of
changes to the product's own details (not its quantity): every saved edit (showing exactly which fields
changed, old value to new value) and every activate/deactivate. Like stock history, entries here can't be
edited or removed — it's a permanent record of what happened to the product over time.
