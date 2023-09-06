using DAL;
using Persistence;

namespace BLL
{
    public class CashierBLL
    {
        readonly CashierDAL cdal = new();

        public Cashier? GetCashierByLogin(string username, string password) => cdal.GetCashierByLogin(username, password);

        public Cashier? GetCashierById(int id) => cdal.GetCashierById(id);
    }
}