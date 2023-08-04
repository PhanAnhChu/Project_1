using System;
using DAL;
using Persistence;

namespace BLL {
    public class ShiftBLL {
        readonly ShiftDAL sdal = new();
        
        public bool AddShift(DateTime startTime, DateTime endTime, int id, float expected, float actual, int confirmer_id) => sdal.AddShift(startTime, endTime, id, expected, actual, confirmer_id);

        public static float CheckTotalIncome(Shift shift) => ShiftDAL.CheckTotalIncome(shift);
    }
}