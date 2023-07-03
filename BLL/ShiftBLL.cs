using System;
using DAL;

namespace BLL {
    public class ShiftBLL {
        ShiftDAL sdal = new();
        public bool AddShift(DateTime startTime, DateTime endTime) {
            return sdal.AddShift(startTime, endTime);
        }
    }
}