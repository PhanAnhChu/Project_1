using System.Collections.Generic;
using Persistence;
using DAL;
using System;

namespace BLL
{
    public class BillBLL {
        readonly BillDAL bdal = new();

        public Bill? GetBillById(int id) => bdal.GetBillById(id);

        public List<Bill> GetBills(int page) => bdal.GetBills(page);

        public bool AddBill(Bill bill, List<Order> orders) => bdal.AddBill(bill, orders);

        public List<Bill> GetBillsFromInterval(DateTime startTime, DateTime endTime) => bdal.GetBillsFromInterval(startTime, endTime);

        public List<Bill> GetBillsFromShift(Shift shift) => bdal.GetBillsFromShift(shift);

        public float CheckTotalPrice(Bill bill) => bdal.CheckTotalPrice(bill);

        public float CheckTotalIncome(List<Bill> bills) {
            float total = 0;
            foreach (Bill bill in bills)
                total += CheckTotalPrice(bill);
            
            return total;
        }
    }
}