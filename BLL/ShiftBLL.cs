using DAL;

namespace BLL {
    public class ShiftBLL {
        ShiftDAL sdal = new();
        public bool AddShift() {
            return sdal.AddShift();
        }
    }
}