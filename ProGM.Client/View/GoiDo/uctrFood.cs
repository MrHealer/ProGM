using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProGM.Client.View.Custom;
using ProGM.Business.Model;
using ProGM.Business.ApiBusiness;
using System.Net;
using ProGM.Client.Model;
using System.IO;
using System.Threading;

namespace ProGM.Client.View.GoiDo
{
    public partial class uctrFood : UserControl, FoodItemCallback, CartItemCallback, CategoryItemCallback
    {
        private List<Food> foods = new List<Food>();
        private List<Food> foodsInCart = new List<Food>();
        private List<Productcategorylist> categorys;
        List<Food> fillterList = new List<Food>();
        List<uctrItem> uctrFoodItems = new List<uctrItem>();



        public uctrFood(String companyId)
        {
            
            InitializeComponent();
            ////
            loadData(companyId);
            //fakeData();
            ////
            fillterList.AddRange(foods);
            // load grid food item
            loadFoodsView(foods);
            ////
            loadCategory();
        

        }

        private void loadData(String companyId)
        {
            //fix bug: Could not create SSL/TLS secure channel when load image
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            categorys = RestshapCommand.ListCategorys(companyId).productCategoryList.ToList();
            List<Productlist> productlist = RestshapCommand.ListProducts().productList.ToList();

           
            foreach (Productlist item in productlist)
            {
                Food food = new Food(item);
                //Stream stream = client.OpenRead(item.strThumbnail);
                //Image image = Image.FromStream(stream);
                food.ImageThumbnail = Image.FromFile("../../Resources/spinner.gif");
                this.foods.Add(food);
            }
            new Thread(lazyLoadingFoodImage).Start();

        }

        private void lazyLoadingFoodImage()
        {
            WebClient client = new WebClient();

            for (int i=0; i<this.foods.Count; i++)
            {
                Stream stream = client.OpenRead(foods[i].strThumbnail);
                Image image = Image.FromStream(stream);
                foods[i].ImageThumbnail = image;
                flpFoods.Invoke((MethodInvoker)delegate
                {
                    // Running on the UI thread
                    uctrFoodItems[i].updateImage(image);
                });
            }
        }

        private void loadCategory()
        {
            foreach (Productcategorylist item in categorys)
            {
                ButtonCustom button = new ButtonCustom();
                button.Text = item.strName;
                button.CategoryItemCallback = this;
                button.AutoSize = false;
                button.Size = new Size(240, 35);
                button.Margin = new Padding(0, 0, 0, 5);
                button.Padding = new Padding(20, 0, 0, 0);
                flpCategorys.Controls.Add(button);
                button.ForeColor = Color.White;
                button.Font = new Font(button.Font.FontFamily, 12);
                button.ObjectData = item;
            }
            flpCategorys.Invalidate();
        }

        public void buyNow(Food food)
        {
            //TODO
            MessageBox.Show("Đã mua hàng", food.strName);
        }

        public void addToCart(Food food)
        {
            foodsInCart.Add(food);
            loadCartView();
            int totalAmount = 0;
            foreach (Food item in foodsInCart)
            {
                totalAmount += item.iPrice;
            }
            lbTotalAmount.Text = addDotToAmount(totalAmount);

        }

        public void deleteItem(int index, Food food)
        {
            foodsInCart.Remove(food);
            loadCartView();
            int totalAmount = 0;
            foreach (Food item in foodsInCart)
            {
                totalAmount += item.iPrice;
            }
            lbTotalAmount.Text = addDotToAmount(totalAmount);
        }

        private void loadCartView()
        {
            flpCart.Controls.Clear();
            foreach(Food item in foodsInCart)
            {
                uctrCartItem cartItem = new uctrCartItem(item);
                cartItem.Visible = true;
                cartItem.Show();
                cartItem.setCallback(this);
                flpCart.Controls.Add(cartItem);
            }
            flpCart.Invalidate();
        }
        private String addDotToAmount(int value)
        {
            String sValue = value.ToString();
            if (sValue.Length > 3)
            {
                sValue = sValue.Insert(sValue.Length - 3, ".");
            }
            return sValue;
        }

        private void btnClearCart_Click(object sender, EventArgs e)
        {
            foodsInCart.Clear();
            loadCartView();
            lbTotalAmount.Text = "";
        }

        public void onCategoryItem_Click(object objData)
        {
            if (objData is Productcategorylist)
            {
                Productcategorylist category = (Productcategorylist)objData;
                fillterList.Clear();
                foreach (Food item in foods)
                {
                    if (item.strCategoryId.Equals(category.strId))
                        fillterList.Add(item);
                }
                loadFoodsView(fillterList);
            }
        }

        private void buttonCustom2_Click(object sender, EventArgs e)
        {
            loadFoodsView(foods);
            fillterList.Clear();
            fillterList.AddRange(foods);
        }

        private void loadFoodsView(List<Food> listFood)
        {
            flpFoods.Controls.Clear();
            uctrFoodItems.Clear();
            for (int i = 0; i < listFood.Count; i++)
            {
                uctrItem item = new uctrItem(listFood[i]);
                item.Visible = true;
                item.Show();
                item.SetCallback(this);
                flpFoods.Controls.Add(item);
                uctrFoodItems.Add(item);
            }
            flpFoods.Invalidate();
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
             
            List<Food> softList = fillterList.OrderBy(o => o.iPrice).ToList();
            loadFoodsView(softList);


        }

        private void btnSearch_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            List<Food> searchList = new List<Food>();
            foreach (Food item in fillterList)
            {
                if (item.strName.Contains(btnSearch.Text))
                    searchList.Add(item);
            }
            fillterList.Clear();
            fillterList.AddRange(searchList);
            loadFoodsView(fillterList);
        }

        private void btnHotFood_Click(object sender, EventArgs e)
        {
            //List<Food> hotList = new List<Food>();
            //foreach (Food item in fillterList)
            //{
            //    if (item.IsHot)
            //        hotList.Add(item);
            //}
            //fillterList.Clear();
            //fillterList.AddRange(hotList);
            //loadFoodsView(fillterList);
        }

        private void btnBuyCart_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Trong giỏ có "+foodsInCart.Count,"Đã mua hàng");
            foodsInCart.Clear();
            loadCartView();
            lbTotalAmount.Text = "";
        }
    }
}
