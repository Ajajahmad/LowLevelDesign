using System;
using System.Net.Http.Headers;
using System.Transactions;

namespace VendingMaching
{

    public enum TransactionStatus
    {
        Initiated,
        Success,
        Failed
    }
    public class Product
    {
        public string productId { get; set; }
        public string productName { get; set; }
        public decimal price { get; set; }
        public Product(string id, string name, decimal price)
        {
            this.productId = id;
            this.productName = name;
            this.price = price;
        }
    }
    public class Slot
    {
        public string slotId { get; set; }
        public Product product { get; set; }
        public int quantity;
        public Slot(string slotId, Product product, int quantity)
        {
            this.slotId = slotId;
            this.product = product;
            this.quantity = quantity;
        }

        public bool isAvailale()
        {
            return quantity > 0;
        }
        public void dispenseOne()
        {
            if (quantity <= 0)
                throw new InvalidOperationException("Out of Stock");
            this.quantity--;
        }
    }
    public class CashInventory
    {
        private Dictionary<int, int> cashMap;
        public CashInventory(Dictionary<int, int> initialCashMap)
        {
            this.cashMap = new Dictionary<int, int>(initialCashMap);
        }   
        public void AddCash(int denom, int count)
        {
            if (!cashMap.ContainsKey(denom))
                cashMap[denom] = 0;
            cashMap[denom] += count;
        }
        public int Total()
        {
            int sum = 0;
            foreach(var kv in cashMap)
            {
                sum += kv.Key * kv.Value;
            }
            return sum;
        }
        public bool CanReturnChange(int amount)
        {
            return amount <= Total();
        }
        public void DeductChange(int amount)
        {
            var keys = cashMap.Keys.OrderByDescending(x => x).ToList();
            foreach (var d in keys)
            {
                int need = amount / d;
                int use = Math.Min(need, cashMap[d]);
                if (use > 0)
                {
                    cashMap[d] -= use;
                    amount -= use * d;
                }
                if (amount == 0) break;
            }

            if (amount != 0)
                throw new InvalidOperationException("Cannot return exact change");
        }
    }
    public class Payment
    {
        private Dictionary<int, int> inserted = new();
        public void Insert(int denom, int count = 1)
        {
            if (!inserted.ContainsKey(denom))
                inserted[denom] = 0;
            inserted[denom] += count;
        }
        public int Total()
        {
            int sum = 0;
            foreach (var kv in inserted)
                sum += kv.Key * kv.Value;
            return sum;
        }
        public Dictionary<int, int> GetAll() => inserted;
    }

    public class Transaction
    {
        public string id { get;  }= Guid.NewGuid().ToString();
       public Product Product { get; }
        public int Paid { get; }
        public TransactionStatus Status { get; private set; }
        public Transaction(Product product, int Paid)
        {
            this.Product = product;
            this.Paid = Paid;
            this.Status = TransactionStatus.Initiated;
        }
        public void MarkSuccess() => this.Status = TransactionStatus.Success;
        public void MarkFailed() => this.Status = TransactionStatus.Failed;

    }
    public class VendingMachine
    {
        private List<Slot> slots;
        private CashInventory cashInventory;
        
        public VendingMachine(List<Slot> slots, CashInventory cashInventory)
        {
            this.slots = slots;
            this.cashInventory = cashInventory;
        }
        public Slot FindSlot(string productId)
        {
            return slots.FirstOrDefault(s => s.product.productId == productId);
        }
        public void Purchase(string productId, Payment payment)
        {
            var slot = FindSlot(productId);
            if (slot == null || !slot.isAvailale())
            {
                throw new Exception("Product not available");
            }
            var price = (int)slot.product.price;
            var paid = payment.Total();
            if (paid < price)
                throw new Exception("Insufficient Amount");
            int change = paid - price;
            if (!cashInventory.CanReturnChange(change))
                throw new Exception("Cannot return change");

            slot.dispenseOne();
            foreach(var kv in payment.GetAll())
                cashInventory.AddCash(kv.Key, kv.Value);
            cashInventory.DeductChange(change);
            var tx = new Transaction(slot.product, paid);
            tx.MarkSuccess();

            Console.WriteLine($"Dispensed {slot.product.productName}, change = {change}");
        }
    }
    class Program
    {
        static void Main()
        {
            var coke = new Product("p1", "Coke", 50);
            var chips = new Product("p2", "chips", 20);
            var slots = new List<Slot>
            {
                new Slot("s1", coke, 5),
                new Slot("s2", chips, 3)
            };
            var cash = new CashInventory(new Dictionary<int, int>
            {
                {10,10},{20,10},{50,10}
            });
            var vm = new VendingMachine(slots, cash);
            var payment = new Payment();
            payment.Insert(60);
            try
            {
                vm.Purchase("p1", payment);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Done, Press Key");
            Console.ReadKey();
        }
    }
}

