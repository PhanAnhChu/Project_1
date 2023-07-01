using System;

namespace Persistence {
    public class Shift {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Shift shift)
                return shift.Id == Id;
            
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}