using System;
using System.Collections.Generic;
using BLL;

namespace Persistence
{
    public class Bill
    {
        public int Id { get; set; }
        public int Cashier_id { get; set; }
        public DateTime Created_date { get; set; }
        public int? Customer_id { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Bill bill)
                return bill.Id == Id;

            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public bool PrintInvoice(List<Order> alternative_orders) {
            Console.Clear();
            CashierBLL cBLL = new();
            Cashier? cashier = cBLL.GetCashierById(Cashier_id);

            if (cashier != null) {
                GoodBLL gBLL = new();
                Console.WriteLine($"{" ", 33}VTC MART");
                Console.WriteLine($"     Address: No. 18 Tam Trinh Street, Minh Khai Ward,\n     Hai Ba Trung District, Hanoi");
                Console.WriteLine($"     Time: {Created_date}");
                Console.WriteLine($"     Cashier: {cashier.Name}");
                Console.WriteLine($"============================================================================");
                    
                string str = $"          +----+{new string('-', 13)}+{new string('-', 11)}+{new string('-', 15)}+{new string('-', 15)}+";

                Console.WriteLine(str);
                Console.WriteLine($"          | Id | {"Name", -12}| {"Quantity", -10}|     {"Price", -10}|     {"Total", -10}|");
                Console.WriteLine(str);
                float total = 0;
                int item = 0;
                OrderBLL obll = new();
                List<Order> orders = obll.GetOrdersFromBill(this);

                if (orders.Count == 0)
                    orders = alternative_orders;

                foreach (Order o in orders)
                {
                    Good? good = gBLL.GetGoodById(o.Good_id);
                    if (good != null) {
                        Console.WriteLine($"          | {good.Id, -3}| {good.Name, -12}| {o.Quantity, -10}|{good.Price, 10} VND |{good.Price * o.Quantity, 10} VND |");
                        total += good.Price * o.Quantity;
                        item += o.Quantity;
                    }
                    else return false;
                }

                Console.WriteLine(str);
                Console.WriteLine($"----------                                                        ----------");
                Console.WriteLine($"\n          item:{" ", 20}(VAT Included){" ", 14}{item}");
                Console.WriteLine($"\n          Cash:{" ", 45}{total} VND");
                Console.WriteLine($"\n          {" ", 9}Invoices are only export within the day");
                Console.WriteLine($"\n          {" ", 14}Thank you for your purchase!");


                Console.ReadKey();
                return true;
            }
            return false;
        }
    }
}