using Automacao.Core.Helper;
using Automacao.Domain.Model.ASC;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Automacao.Core.Asc
{
    public class AscPage : IDisposable
    {

        private string _baseurl = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/";

        public bool Authenticated { get; set; }
        public string User { get; private set; }
        public string Pass { get; private set; }

        private BrowserSession bs = new BrowserSession();
        private Uri _uri;

        public AscPage(string user, string pass)
        {
            User = user;
            Pass = pass;

            _uri = new Uri($@"{_baseurl}");

            WebProxy p = new WebProxy("localhost", 8888);
            WebRequest.DefaultWebProxy = p;
        }
        /// <summary>
        /// Realiza a autenticação no sistema
        /// </summary>
        /// <returns></returns>
        public bool Autenticar()
        {
            try
            {
                Authenticated = false;
                bs.UseCredentials = true;
                bs.Credentials = new NetworkCredential(User, Pass);
                var loged = bs.Get("http://dynamics.caixaseguros.intranet:5555/CRMCAD/main.aspx");
                if (loged.Contains("Importante:"))
                    Authenticated = true;
                return Authenticated;
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao tentar autenticar usuário " + User + ". Message:" + ex.Message + ".  StackTrace: " + ex.StackTrace);
            }
        }

        public void Autenticar2()
        {
            try
            {
                    //initial "Login-procedure"
                    bs.UseCredentials = true;
                    bs.Credentials = new NetworkCredential(User, Pass);
                    var loged = bs.Get2("http://dynamics.caixaseguros.intranet:5555/CRMCAD/main.aspx");

                    if (loged.Contains("Importante:"))
                        Authenticated = true;
             
            }
            catch (Exception e)
            {
                Authenticated = false;
            }
            
        }

        public string GetPage(string url)
        {
            try
            {
                if (!Authenticated)
                    Autenticar();

                return bs.Get(url);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// Retorna a lista de itens da fila de trabalho 
        /// </summary>
        /// <returns></returns>
        public List<Ocorrencia> GetOcorrencias()
        {
            using (var asc = new ASCSession(User, Pass))
            {
                List<LinhaGridWebService> lista = new List<LinhaGridWebService>();
                var oid = string.Empty;
                var ocorencias = asc.GetOcorrencias();
                Authenticated = asc.Autenticad;
                return null;
            }
        }

        public async Task<string> GetIframe(string url)
        {
            try
            {
                var asc = new ASCSession(User, Pass);
                await asc.GetIframe();
                return "";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<HtmlDocument> GetHtml()
        {
            using (var asc = new ASCSession(User, Pass))
            {
                return await asc.GetHtmlDocument();
            }
        }
        
        public void Dispose()
        {
        }
    }
}
