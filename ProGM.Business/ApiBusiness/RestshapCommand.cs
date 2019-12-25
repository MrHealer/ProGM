using Newtonsoft.Json;
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
             //var client = new RestClient("http://40.74.77.139/api/?key=computerList&companyId=cf09c7b5-254e-11ea-b536-005056b97a5d&groupId=ALL");
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
                var client = new RestClient(url + "?key=computer-detail&_strMacAddress=" + mac);
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
        
    }
}
