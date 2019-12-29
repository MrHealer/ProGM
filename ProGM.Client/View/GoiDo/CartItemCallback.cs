using ProGM.Business.Model;
using ProGM.Client.Model;

namespace ProGM.Client.View.GoiDo
{
    public interface CartItemCallback
    {
        void deleteItem(int index, Food food);
    }
}
