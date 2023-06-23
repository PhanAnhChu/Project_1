using System;

namespace Persistence
{
    public class Bill
    {
        public int Id { get; set; }
        public int Cashier_id { get; set; }
        public DateTime Created_date { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Bill bill)
                return bill.Id == Id;

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}