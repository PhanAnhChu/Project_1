namespace Persistence {
    public class Customer {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string? Phone { get; set; }

        public int Reward_Point { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Customer Customer)
                return Customer.Id == Id;

            return false;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}