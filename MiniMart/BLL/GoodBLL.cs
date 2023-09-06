using DAL;
using Persistence;

namespace BLL {
    public class GoodBLL {
        readonly GoodDAL gdal = new();

        public Good? GetGoodById(int id) => gdal.GetGoodsById(id);

        public int GetCurrentQuantity(int good_id) => gdal.GetCurrentQuantity(good_id);
    }
}