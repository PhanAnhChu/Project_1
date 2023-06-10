using System;
using System.Collections.Generic;

namespace Persistence {
    public class Bill {
        public uint Id { get; set; }
        public uint Cashier_Id { get; set; }
        public DateTime Created_Date { get; set; }
        public List<uint> Order_Id { get; set; }

        public override bool Equals(object obj) {
            if (obj is Bill bill)
                return bill.Id == Id;

            return false;
        }

        public override int GetHashCode() {
            return Id.GetHashCode();
        }
    }
}