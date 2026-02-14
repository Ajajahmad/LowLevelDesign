Requirement Clarification

✅ Actors

Customer using vending machine

No admin / maintenance user in scope

✅ Supported Actions

View available products

Select product

Insert money

Dispense product if payment sufficient

Return change if extra money inserted

✅ Authentication

No authentication needed (public machine)

✅ Product & Inventory

Machine stores multiple product types

Each product has:

price

quantity

Limited stock per product

✅ Payment Scope

Cash only (notes + coins)

No card / UPI (keep LLD focused)

✅ Constraints

Must handle:

insufficient funds

out of stock

exact change logic (basic)

✅ Out of Scope

Networking

Remote inventory sync

Expiry dates

Maintenance mode

Concurrency


ENTITIES :
VendingMachine
Product
Slot (or InventorySlot)
Money
Payment
Transaction


ATTRIBUTES:
---Product
- productId
- name
- price
--------Slot
- slotId
- product
- quantity
-------Money
- denomination
- type
-------CashInventory
- denominationCountMap
-----Payment
- insertedAmount
- insertedDenominations
------Transaction
- transactionId
- product
- amountPaid
- status
------VendingMachine
- slots
- cashInventory
