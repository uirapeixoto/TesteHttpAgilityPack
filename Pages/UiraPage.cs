using ProjetoTeste.Domain.Model;
using System;
using System.Collections.Generic;

namespace ProjetoTeste.Pages
{
    public class UiraPage : IDisposable
    {
        private string _baseurl;
        private string _urlHome;
        private string _user;
        private string _pswd;
        private bool _logetd;
        private MyWebClient _web;

        public UiraPage()
        {
            _logetd = false;
            _baseurl = "https://loja.uira.com.br/loja/";
            _urlHome = $"{_baseurl}index.php";
            _user = "uira.peixoto@gmail.com";
            _pswd = "uira2099!";
            _web = new MyWebClient();
        }

        public bool Login()
        {
            bool sendDataStatus = false;
            string url = $"{_baseurl}index.php?controller=authentication";
            
            try
            {
                Dictionary<string, string> sendData = new Dictionary<string, string>()
                {
                    { "email",_user },
                    { "passwd",_pswd }
                };

                _web.SendDataToService(url, sendData);
                sendDataStatus = true;

                return sendDataStatus;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetHome()
        {
            if (!_logetd)
                Login();

            var response = _web.GetPageResponse(_urlHome);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return _web.GetResponseString(response);

            return string.Empty;
        }

        public List<ProductModel> GetProducts()
        {
            return new List<ProductModel>();
        }

        public void Dispose()
        {
        }
    }
}
