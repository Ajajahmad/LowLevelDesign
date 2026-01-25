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

**Relations -**
User -> Account = Aggregation
User -> Card = Aggregation
Account -> Card = Composition
ATM -> CashDispenser = Composition
ATM -> Transaction = Aggregation




          **Attributes**
=> User
- userId
- name
  
=> ATM
- atmId
- location
- cashDispenser
  
=> Account
- accountId
- accountNumber
- balance
- status
  
=> Card
- cardId
- cardNumber
- hashedPin
- status
  
=> CashDispenser
- dispenserId
- denominationCountMap  // Map<Denomination, Count>
  
=>Transaction
- transactionId
- accountId
- amount
- type        // WITHDRAWAL, BALANCE_INQUIRY
- status      // INITIATED, SUCCESS, FAILED
- timestamp

Q3️⃣: Where does PIN validation logic live and WHY?

Options:
-Card?
-ATM?
-Transaction?
-Separate service?

✅ IDEAL INTERVIEW ANSWER
PIN validation logic belongs to Card
Card
- hashedPin
- status
- failedAttempts

Responsibility:
Card owns PIN state
Card knows when it should block itself
Card validates entered PIN

ATM’s role:
ATM → asks Card to authenticate(pin)
ATM:
collects PIN
delegates validation
reacts to result
🧠 Key principle:
The object that owns the data enforces its rules.


Q4️⃣: How would you design Transaction?
Specifically:
-Should Transaction be:
-concrete class?
-abstract class?
-Do we need different transaction types?
-Who executes the transaction logic?

✅ The Transaction itself
Each transaction subclass executes its own logic.
abstract class Transaction {
    execute()
}

Subclasses:
WithdrawalTransaction.execute():
- check balance
- check limits
- ask CashDispenser to dispense
- update account
- mark status

BalanceInquiryTransaction.execute():
- read account balance
- mark status

 **Role breakdown (VERY IMPORTANT)**
🧠 Transaction
-Owns execution logic
-Knows its own steps
-Updates its own state

🏧 ATM
-Orchestrates flow
-Creates transaction
-Calls transaction.execute()

💰 CashDispenser
-Only dispenses cash
-Knows denominations
-Has zero business rules


Q5️⃣:Where should balance deduction happen?
Options:
-ATM
-Transaction
-Account
-CashDispenser
-- Ans- 
-Balance deduction should happen inside Account, because Account owns the balance and enforces all invariants.
Transaction only coordinates the operation.

Q6️⃣:

Who creates the Transaction object?
Options:
-ATM
-Card
-Account
-Factory class
✔️ ATM creates the transaction via a Factory
- ATM → TransactionFactory → Transaction

Q7️⃣:
Walk me through the full CASH WITHDRAWAL flow
(from card insertion to cash dispense)
⚠️ Rules:
Step-by-step
-Mention who calls whom
-No code
-No hand-waving
-This is usually the final ATM LLD question.


💳 CASH WITHDRAWAL FLOW (Step-by-step)

1️⃣ ATM receives card insertion
-Reads card details
-Prompts user for PIN

2️⃣ ATM delegates authentication to Card
-Calls card.authenticate(pin)
-Card validates hashed PIN
-Updates failed attempts / blocks if needed

3️⃣ ATM resolves Account via Card
-Card returns associated Account

4️⃣ ATM asks user for transaction type and amount
-User selects Withdrawal
-User enters amount

5️⃣ ATM creates Transaction via Factory
-Calls TransactionFactory.create(WITHDRAWAL, account, amount)
-Receives WithdrawalTransaction

6️⃣ ATM executes the Transaction
-Calls transaction.execute()

7️⃣ WithdrawalTransaction execution
-Checks account balance via Account
-Checks transaction limits
-Asks CashDispenser to dispense cash

-- If successful:
-Calls account.debit(amount)
-Marks transaction SUCCESS

--If failure:
-Marks transaction FAILED

8️⃣ ATM displays result and ejects card
