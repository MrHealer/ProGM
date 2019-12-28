using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProGM.Business.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProGM.Business.ApiBusiness
{
    public class RestshapCommand
    {
        const string url = "http://40.74.77.139/api";

        public static responseAccountDetail AccountDetail(string id)
        {
            var client = new RestClient(url+"?key=accountDetails&accountId=" + id);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
            try
            {
                IRestResponse response = client.Execute(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<responseAccountDetail>(response.Content);
                }
            }
            catch (Exception)
            {

            }
            return null;
        }
        public static LoginResponse Login(string username, string password, ref string messeage)
        {
            try
            {
                var restClient = new RestClient(url);
                restClient.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
                request.AddHeader("Content-Type", "multipart/form-data");
                request.AlwaysMultipartFormData = true;
                request.AddParameter("key", "login");
                request.AddParameter("accountName", username);
                request.AddParameter("password", password);
                IRestResponse response = restClient.Execute(request);
                LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(response.Content);
                return loginResponse;
            }
            catch (Exception ex)
            {
                messeage = ex.Message;
                return null;
            }


        }
        public static responseListPC GetAllComputerByCompany(string id)
        {
            //var client = new RestClient("http://40.74.77.139/api/?key=groupComputerList&companyId="+id);
            var client = new RestClient(url + "/?key=computerList&companyId=" + id + "&groupId=ALL");
            var request = new RestRequest(Method.GET);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("accept-encoding", "gzip, deflate");
            request.AddHeader("Host", "40.74.77.139");
            request.AddHeader("Cache-Control", "no-cache");
            request.AddHeader("Accept", "*/*");
            request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
            try
            {
                IRestResponse response = client.Execute(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    responseListPC responseData = JsonConvert.DeserializeObject<responseListPC>(response.Content);
                    return responseData;
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }

        }
        public static ResponseApiComputerDetail ComputerDetail(string mac)
        {
            try
            {
                var client = new RestClient(url + "?key=computeDetail&pcMacAddress=" + mac);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
                IRestResponse response = client.Execute(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    ResponseApiComputerDetail responseData = JsonConvert.DeserializeObject<ResponseApiComputerDetail>(response.Content);
                    return responseData;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return null;
        }
        public static CategoryListResponse ListCategorys(string companyId)
        {
            var client = new RestClient(url + "?key=productCategoryList&categoryId=ALL&companyId=" + companyId);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
            try
            {
                IRestResponse response = client.Execute(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<CategoryListResponse>(response.Content);
                }
            }
            catch (Exception)
            {

            }
            return null;
        }
        public static ProductResponse ListProducts(string categoryId = "ALL")
        {
            var client = new RestClient(url + "?key=productList&categoryId=" + categoryId);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
            try
            {
                IRestResponse response = client.Execute(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<ProductResponse>(response.Content);
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        public static ResonseWalletDetail walletDetail(string idUser)
        {
            var client = new RestClient(url+"?key=walletDetail&accountId="+ idUser);
            client.Timeout = -1;
            var request = new RestRequest(Method.GET);
            request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
            try
            {
                IRestResponse response = client.Execute(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    return JsonConvert.DeserializeObject<ResonseWalletDetail>(response.Content);
                }
            }
            catch (Exception)
            {

            }
            return null;
        }
        /// <summary>
        /// Hàm trừ tiền
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="idManager"></param>
        /// <param name="amount"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static bool walletWithdrawal(string idUser,string idManager,decimal amount, string comment)
        {

            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
            request.AddHeader("Content-Type", "multipart/form-data; boundary=--------------------------716141854746053963220112");
            request.AlwaysMultipartFormData = true;
            request.AddParameter("key", "walletWithdrawal");
            request.AddParameter("amount", amount);
            request.AddParameter("comment", comment);
            request.AddParameter("walletId", walletDetail(idUser).walletDetail[0].strId);
            request.AddParameter("serviceId", "c4cc7bb4-2638-11ea-b536-005056b97a5d");
            request.AddParameter("cashierId", idManager);
            IRestResponse response = client.Execute(request);
            var status = JObject.FromObject(response.Content);
            if (status["result"].Equals("SUCCESS"))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Hàm nạp tiền
        /// </summary>
        /// <param name="idUser"></param>
        /// <param name="idManager"></param>
        /// <param name="amount"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        public static bool walletDeposit(string idUser, string idManager, decimal amount, string comment)
        {
            var client = new RestClient("http://40.74.77.139/api/");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Basic d2ViOjEyMw==");
            request.AddHeader("Content-Type", "multipart/form-data; boundary=--------------------------017743957999191953426396");
            request.AlwaysMultipartFormData = true;
            request.AddParameter("key", "walletDeposit");
            request.AddParameter("amount", amount);
            request.AddParameter("comment", comment);
            request.AddParameter("walletId", walletDetail(idUser).walletDetail[0].strId);
            request.AddParameter("serviceId", "d06bf8fe-2638-11ea-b536-005056b97a5d");
            request.AddParameter("cashierId", idManager);
            IRestResponse response = client.Execute(request);
            var status = JObject.FromObject(response.Content);
            if (status["result"].Equals("SUCCESS"))
            {
                return true;
            }
            return false;
        }
    }
}
