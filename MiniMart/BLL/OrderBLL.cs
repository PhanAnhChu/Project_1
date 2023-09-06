using System.Collections.Generic;
using Persistence;
using DAL;

namespace BLL
{
    public class OrderBLL
    {
        readonly OrderDAL odal = new();

        public List<Order> GetOrdersFromBill(Bill bill) => odal.GetOrdersFromBill(bill);
    }
}