using System;
using System.Collections.Generic;
using BLL;
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

        public void PrintInvoice(List<Order> orders){
            GoodBLL gBLL = new();
            foreach (Order o in orders)
            {
                Good good = gBLL.GetGoodById(o.Good_id);
                Console.Write(o.Id + " ");
                Console.Write(good.Name + " ");
                Console.Write(o.Quantity + " ");
                Console.Write(good.Price + "\n");
                
            }
        }
    }
}