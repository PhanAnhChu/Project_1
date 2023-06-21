using System.Collections.Generic;
using Persistence;
using DAL;

namespace BLL
{
    public class OrderBLL
    {
        readonly OrderDAL odal = new();

        public Order? GetOrderById(int id)
        {
            return odal.GetOrderById(id);
        }

        public List<Order> GetOrdersFromBill(Bill bill)
        {
            return odal.GetOrdersFromBill(bill);
        }
    }
}