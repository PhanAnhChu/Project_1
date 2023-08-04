using DAL;
using Persistence;

namespace BLL
{
    public class CustomerBLL
    {
        readonly CustomerDAL cdal = new();

        public Customer? GetCustomerById(int id) => cdal.GetCustomerById(id);

        public bool AddCustomer(Customer customer) => cdal.AddCustomer(customer);
    }
}