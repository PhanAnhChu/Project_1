using System;
using System.Collections.Generic;
using DAL;
namespace Persistence
{
    public class Bill
    {
        public int Id { get; set; }
        public int Cashier_id { get; set; }
        public DateTime Created_date { get; set; }
        // public string Customer_name { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Bill bill)
                return bill.Id == Id;

            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();
        public void PrintInvoice(){
            OrderDAL odal = new();
            List<Order> orders = odal.GetOrdersFromBill(this);
            foreach (Order o in orders)
            {
                Console.WriteLine(o.Id);
            }
        }
    }
}