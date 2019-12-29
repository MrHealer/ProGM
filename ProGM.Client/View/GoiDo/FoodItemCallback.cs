using ProGM.Business.Model;
using ProGM.Client.Model;

namespace ProGM.Client.View.GoiDo
{
    public interface FoodItemCallback
    {
        void buyNow(Food food);
        void addToCart(Food food);
    }
}
