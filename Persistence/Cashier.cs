namespace Persistence {
    public class Cashier {
        public int Id { get; set; }

        public string Name;

        public override bool Equals(object? obj)
        {
            if (obj is Cashier cashier)
                return cashier.Id == Id;

            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}