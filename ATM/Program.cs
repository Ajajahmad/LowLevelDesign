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
    }
    public class Account
    {
        public int AccountId { get; }   
        public string AccountNumber { get; } 
        public decimal Balance { get; }
        public AccountStatus accountStatus { get; } 
        public Account(int accountId, string accountNumber, decimal initialBalance)
        {
            this.AccountId = accountId;
            this.AccountNumber = accountNumber;
            this.Balance = initialBalance;
            this.accountStatus = AccountStatus.Active;
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
    }
    public class CashDispenser
    {
        private Dictionary<int , int > DenominationCash { get; } 
        public CashDispenser( Dictionary<int, int>cash )
        {
            this.DenominationCash = cash;
        }
    }
    abstract class Transaction
    {
        public string TransactionId { get; }
        protected decimal amount;
        public TransactionType transactionType;
        public TransactionStatus transactionStatus { get; private set; }

        protected Account account;

        public Transaction( Account account, decimal amount)
        {
            this.TransactionId = Guid.NewGuid().ToString();
            this.account = account;
            this.amount = amount;
            this.transactionStatus = TransactionStatus.Initiated;
        }
    }
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello");
        }
    }
}
