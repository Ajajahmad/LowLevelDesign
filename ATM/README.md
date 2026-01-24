                                        Design ATM

Requirement ->

1. Who are the users of the ATM? Is it only bank customers with debit cards?
2. What operations should the ATM support? (withdrawal, balance inquiry, deposit, etc.)
3. How is authentication handled? (PIN-based?)
4. Does one card map to one bank account or multiple accounts?
5. How should cash handling work? Are we tracking denominations?
6. Are there any transaction limits (per transaction / per day)?
7. What is in scope and out of scope? (bank backend, networking, failures)

1️⃣ Who are the users?
-Bank customers
-Each user has:
-Debit card
-Bank account
-No admin, no maintenance user.

2️⃣ What operations are supported?
In scope:
-Cash withdrawal
-Balance inquiry
-PIN authentication

Out of scope:
-Cash deposit
-Fund transfer
-Mini statement
-Network failure handling
-Keep it simple.

3️⃣ Authentication?
-PIN-based authentication
-3 attempts max
-Card is blocked after 3 failures (logic only, no persistence)

4️⃣ Card ↔ Account mapping?
-One card maps to one bank account
-One account belongs to one user

5️⃣ Cash handling?
-ATM has limited cash
-Cash is stored in denominations
-ATM must check if it can dispense requested amount
-(This is a key ATM design point.)

6️⃣ Transaction limits?
-Max withdrawal per transaction (e.g., ₹10,000)
-Daily limit is out of scope

7️⃣ Scope boundaries?
-Assume bank backend always works
-Focus on LLD, not infra
-No DB or concurrency discussion





                **Entities:**
1. User
2. Card
3. Account
4. ATM
5. Transaction
6. CashDispenser
