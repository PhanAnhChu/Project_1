using System.Collections.Generic;
using Persistence;
using DAL;

namespace BLL
{
    public class BillBLL {
        readonly BillDAL bdal = new();

        public Bill? GetBillById(int id) => bdal.GetBillById(id);

        public List<Bill> GetBills(int page) => bdal.GetBills(page);

        public bool AddBill(Bill bill, List<Order> orders) => bdal.AddBill(bill, orders);
    }
}