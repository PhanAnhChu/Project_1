using System.Collections.Generic;
using Persistence;
using DAL;

namespace BLL
{
    public class BillBLL {
        readonly BillDAL bdal = new();

        public Bill? GetBillById(int id)
        {
            return bdal.GetBillById(id);
        }

        public List<Bill> GetBills(int page)
        {
            return bdal.GetBills(page);
        }

        public bool AddBill(Bill bill, List<Order> orders)
        {
            return bdal.AddBill(bill, orders);
        }
    }
}