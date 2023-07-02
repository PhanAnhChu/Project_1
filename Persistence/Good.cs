namespace Persistence {
    public class Good {
        public int Id { get; set; }
        public string Name { get; set; }
        public float Price { get; set; }
        public int Quantity { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Good good)
                return good.Id == Id;
            
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}