using DAL;
using Persistence;

namespace BLL
{
    public class CashierBLL
    {
        readonly CashierDAL cdal = new();

        public Cashier? GetCashierByLogin(string username, string password)
        {
            return cdal.GetCashierByLogin(username, password);
        }
    }
}