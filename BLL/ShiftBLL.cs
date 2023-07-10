using System;
using DAL;
using Persistence;

namespace BLL {
    public class ShiftBLL {
        readonly ShiftDAL sdal = new();
        
        public bool AddShift(DateTime startTime, DateTime endTime, int id, float expected, float actual) => sdal.AddShift(startTime, endTime, id, expected, actual);

        public static float CheckTotalIncome(Shift shift) => ShiftDAL.CheckTotalIncome(shift);
    }
}