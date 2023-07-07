using System;
using DAL;

namespace BLL {
    public class ShiftBLL {
        ShiftDAL sdal = new();
        public bool AddShift(DateTime startTime, DateTime endTime, int id, float expected, float actual) {
            return sdal.AddShift(startTime, endTime, id, expected, actual);
        }
    }
}