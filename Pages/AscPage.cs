using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoTeste.Pages
{
    public class AscPage : IDisposable
    {
        private string _baseurl;
        private string _urlLogin;
        private string _user;
        private string _pswd;
        private bool _authenticated;
        private bool _useCredential;
        public NetworkCredential _credentials;

        private MyWebClient _web;

        public AscPage()
        {
            _authenticated = false;
            _baseurl = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/";
            _urlLogin = $"{_baseurl}index.php";
            _useCredential = true;
            _credentials = new NetworkCredential(_user, _pswd);
            _user = "ter02699@rootbrasil";
            _pswd = "Caixa123";

            _web = new MyWebClient();
        }

        public bool Login()
        {
            _authenticated = false;
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

        public void Dispose()
        {
        }
    }
}
