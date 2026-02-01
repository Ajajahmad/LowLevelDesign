using System;

namespace ATMDesign
{
    // enums
    public enum TransactionType
    {
        Withdrawn,
        BalanceInquiry
    }

    public enum TransactionStatus
    {
        Initiated,
        Success,
        Failed
    }

    public enum CardStatus
    {
        Blocked,
        Active
    }
    
    public enum AccountStatus
    {
        Blocked,
        Active
    }


    public class User
    {
        public int id { get; }
        public string Name { get; }
        public User(int id, string name)
        {
            this.id = id;
            this.Name = name;   
        }
    }
    public class ATM
    {
        private CashDispenser dispenser;
        public ATM(CashDispenser cashDispenser) 
        {
            this.dispenser = cashDispenser;
        }
        public void start(Card card, string pin, TransactionType type, decimal amount)
        {
            if (!card.Authenticate( pin))
                throw new InvalidOperationException("Authentication failed");
            Account account = card.GetAccount();
            Transaction transaction = TransactionFactory.CreateTransaction(type, account, amount, this.dispenser);
            transaction.Execute();
            EjectCard();
        }
        public void EjectCard()
        {
            Console.WriteLine("Please collect your card");
        }
    }
    public class Account
    {
        public int AccountId { get; }   
        public string AccountNumber { get; } 
        public decimal Balance { get; set; }
        public AccountStatus accountStatus { get; } 
        public Account(int accountId, string accountNumber, decimal initialBalance)
        {
            this.AccountId = accountId;
            this.AccountNumber = accountNumber;
            this.Balance = initialBalance;
            this.accountStatus = AccountStatus.Active;
        }
        public bool HasSufficientAmount(decimal amount)
        {
            return this.Balance >= amount;
        }
        public void Debit(decimal amount)
        {
            if (this.Balance < amount)
                throw new InvalidOperationException("InSufficient balance");
            this.Balance -= amount;
        }
        public decimal GetBalance()
        {
            return this.Balance;
        }
    }
    public class Card
    {
        public int CardId { get; }
        public string CardNumber { get; }
        public string HashedPin { get; }
        public CardStatus CardStatus { get; private set; }
        private Account Account;
        public Card(int cardId, string cardNumber, string hashedPin, Account account)
        {
            this.CardId = cardId;
            this.CardNumber = cardNumber;
            this.HashedPin = hashedPin;
            this.Account = account;
            this.CardStatus = CardStatus.Active;
        }
        public bool Authenticate(string pin)
        {
            if (this.HashedPin == pin) return true;
            return false;
        }
        public Account GetAccount()
        {
            return this.Account;
        }
    }
    public class CashDispenser
    {
        private Dictionary<int , int > DenominationCash { get; } 
        public CashDispenser( Dictionary<int, int>cash )
        {
            this.DenominationCash = cash;
        }
        public bool CanDispense(decimal amount)
        {
            foreach (var item in DenominationCash)
            {
                int note = item.Key;
                int count = item.Value;
                decimal t = amount / note;
                amount -= t * note;
            }
            if (amount > 0)
            {
                return false;
            }
            return true;
        }
        public void Dispense(decimal amount)
        {
            var notes = DenominationCash.Keys
                                        .OrderByDescending(x => x)
                                        .ToList();

            foreach (var note in notes)
            {
                int available = DenominationCash[note];

                int needed = (int)(amount / note);
                int used = Math.Min(needed, available);

                if (used > 0)
                {
                    DenominationCash[note] -= used;
                    amount -= used * note;
                }

                if (DenominationCash[note] == 0)
                    DenominationCash.Remove(note);

                if (amount == 0)
                    break;
            }

            if (amount != 0)
                throw new InvalidOperationException("Cannot dispense exact amount");

            Console.WriteLine("Amount dispensed successfully");
        }
    }


    //// strategy 
    abstract class Transaction
    {
        public string TransactionId { get; }
        protected decimal amount;
        public TransactionType transactionType;
        public TransactionStatus transactionStatus { get;  set; }

        protected Account account;

        protected Transaction( Account account, decimal amount)
        {
            this.TransactionId = Guid.NewGuid().ToString();
            this.account = account;
            this.amount = amount;
            this.transactionStatus = TransactionStatus.Initiated;
        }
        public abstract void Execute();
    }

     class WithdrawalTransaction : Transaction
    {
        private CashDispenser cashDispenser;
        public WithdrawalTransaction(Account account, decimal amount, CashDispenser cash) : base(account, amount)
        {
            this.cashDispenser = cash;
        }
        public override void Execute()
        {
            if(!account.HasSufficientAmount(amount))
            {
                transactionStatus = TransactionStatus.Failed;
                throw new InvalidOperationException("Transaction Failed");
            }
            if (!cashDispenser.CanDispense(amount))
            {
                transactionStatus = TransactionStatus.Failed;
                throw new InvalidOperationException("ATM does not have enough money.");
            }
            cashDispenser.Dispense(amount);
            account.Debit(amount);
            transactionStatus = TransactionStatus.Success;
        }
    }

     class BalanceInquiryTransaction : Transaction
    {
        public BalanceInquiryTransaction(Account account) : base(account, 0)
        {

        }
        public override void Execute()
        {
            Console.WriteLine($"Current Balance: ₹{account.GetBalance()}");
            transactionStatus = TransactionStatus.Success;
        }
    }


    // Transation Factory ///
    class TransactionFactory
    {
        public static Transaction CreateTransaction(TransactionType type, Account account, decimal amount, CashDispenser cashDispenser)
        {
            return type switch
            {
                TransactionType.Withdrawn => new WithdrawalTransaction(account, amount, cashDispenser),
                TransactionType.BalanceInquiry => new BalanceInquiryTransaction(account),
                _ => throw new ArgumentException("Invalid Tranaction Type")
            }; ;

        }
    }

    class Program
    {
        static void Main()
        {
            Dictionary<int, int> cash = new Dictionary<int, int> { [1000] = 10, [500] = 15, [100] = 20, [50] = 10 };
            CashDispenser dispenser = new CashDispenser(cash);
            ATM atm = new ATM(dispenser);
            Account account1 = new Account(101, "101", 10000);
            Card card = new Card(1011, "101_1", "account123", account1);

            atm.start(card, "account123", TransactionType.Withdrawn, 5000);
            atm.start(card, "account123", TransactionType.BalanceInquiry, 0);


            atm.start(card, "account123", TransactionType.Withdrawn, 50);
            atm.start(card, "account123", TransactionType.BalanceInquiry, 0);
            Console.ReadKey();


        }
    }
}
