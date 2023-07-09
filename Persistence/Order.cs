namespace Persistence {
    public class Order {
        public int Id { get; set; }
        public int Bill_id { get; set; }
        public int Good_id { get; set; }
        public int Quantity { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Order order)
                return order.Id == Id;
            
            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}