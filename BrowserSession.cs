using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ProjetoTeste
{
    public class BrowserSession : IDisposable
    {
        private bool _isPost;
        private HtmlDocument _htmlDoc;

        #region Novos Atributos

        public Dictionary<string, string> extraHeader;

        public List<string> toRemoveHeader;

        private CookieContainer _cookies;

        public bool IsXMLPost { get; set; }

        public string XmlToPost { get; set; }

        public string lasUrl;

        public string EncodingTexto { get; set; }

        public string JavaScriptText { get; set; }

        public bool UseCredentials { get; set; }

        public NetworkCredential Credentials { get; set; }

        public bool AutoRedirect { get; set; }

        public string FileName { get; set; }

        public string LastContentType { get; set; }
        public string RTFData { get; private set; }
        public string InternalError { get; private set; }

        #endregion

        /// <summary>
        /// System.Net.CookieCollection. Provides a collection container for instances of Cookie class 
        /// </summary>
        public CookieCollection Cookies { get; set; }

        /// <summary>
        /// Provide a key-value-pair collection of form elements 
        /// </summary>
        public FormElementCollection FormElements { get; set; }

        /// <summary>
        /// Makes a HTTP GET request to the given URL
        /// </summary>
        public string Get(string url)
        {
            _isPost = false;
            _htmlDoc = CreateWebRequestObject().Load(url);
            return _htmlDoc.DocumentNode.InnerHtml;
        }

        /// <summary>
        /// Makes a HTTP POST request to the given URL
        /// </summary>
        public string Post(string url)
        {
            _isPost = true;
            CreateWebRequestObject().Load(url, "POST");
            return _htmlDoc.DocumentNode.InnerHtml;
        }

        public BrowserSession()
        {
            _cookies = new CookieContainer();
        }

        /// <summary>
        /// Limpar os cookies
        /// </summary>
        public void ClearCookies()
        {
            _cookies = new CookieContainer();
        }


        /// <summary>
        /// Event handler for HtmlWeb.PostResponseHandler. Occurs after a HTTP response is received
        /// </summary>
        protected void OnAfterResponse(HttpWebRequest request, HttpWebResponse response)
        {
            SaveCookiesFrom(response); // Save cookies for subsequent requests
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreHandleDocumentHandler. Occurs before a HTML document is handled
        /// </summary>
        protected void OnPreHandleDocument(HtmlDocument document)
        {
            SaveHtmlDocument(document);
        }

        /// <summary>
        /// Creates the HtmlWeb object and initializes all event handlers. 
        /// </summary>
        private HtmlWeb CreateWebRequestObject()
        {
            var dsEnc = "ISO-8859-1";
            if (EncodingTexto != null)
                if (EncodingTexto != string.Empty)
                    dsEnc = EncodingTexto;
            HtmlWeb web = new HtmlWeb
            {
                AutoDetectEncoding = false,
                OverrideEncoding = Encoding.GetEncoding(dsEnc)
            };

            web.UseCookies = true;
            web.PreRequest = new HtmlWeb.PreRequestHandler(OnPreRequest);
            web.PostResponse = new HtmlWeb.PostResponseHandler(OnAfterResponse);
            web.PreHandleDocument = new HtmlWeb.PreHandleDocumentHandler(OnPreHandleDocument);
            return web;
        }

        /// <summary>
        /// Assembles the Post data and attaches to the request object
        /// </summary>
        private void AddPostDataTo(HttpWebRequest request)
        {
            string payload = FormElements.AssemblePostPayload();
            byte[] buff = Encoding.UTF8.GetBytes(payload.ToCharArray());
            request.ContentLength = buff.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            System.IO.Stream reqStream = request.GetRequestStream();
            reqStream.Write(buff, 0, buff.Length);
        }

        /// <summary>
        /// Add cookies to the request object
        /// </summary>
        private void AddCookiesTo(HttpWebRequest request)
        {
            if (Cookies != null && Cookies.Count > 0)
            {
                request.CookieContainer.Add(Cookies);
            }
        }

        /// <summary>
        /// Saves cookies from the response object to the local CookieCollection object
        /// </summary>
        private void SaveCookiesFrom(HttpWebResponse response)
        {
            if (response.Cookies.Count > 0)
            {
                if (Cookies == null) Cookies = new CookieCollection();
                Cookies.Add(response.Cookies);
            }
        }

        /// <summary>
        /// Saves the form elements collection by parsing the HTML document
        /// </summary>
        private void SaveHtmlDocument(HtmlDocument document)
        {
            _htmlDoc = document;
            FormElements = new FormElementCollection(_htmlDoc);
        }

        /// <summary>
        /// Author: Felipe Guaneri
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string PostJs(string url)
        {
            _isPost = true;

            CreateWebRequestObject().Load(url, "POST");

            if (string.IsNullOrEmpty(JavaScriptText))
                return _htmlDoc.DocumentNode.InnerHtml;

            return JavaScriptText;
        }

        /// <summary>
        /// Author: Felipe Guaneri 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HtmlDocument PostAndReturnDocumentNode(string url)
        {
            _isPost = true;

            CreateWebRequestObject().Load(url, "POST");
            return _htmlDoc;
        }

        /// <summary>
        /// Event handler for HtmlWeb.PreRequestHandler. Occurs before an HTTP request is executed.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected bool OnPreRequest(HttpWebRequest request)
        {
            request.UserAgent = SettingsManager.GetSessionUserAgent();
            WebHeaderCollection myWebHeaderCollection = request.Headers;

            if (UseCredentials)
            {
                request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                request.Credentials = Credentials;
            }
            request.AllowAutoRedirect = this.AutoRedirect;
            myWebHeaderCollection.Add("Accept-Language", SettingsManager.GetSessionAcceptLanguage());
            myWebHeaderCollection.Add("AcceptCharset", SettingsManager.GetSessionAcceptCharset());
            myWebHeaderCollection.Add("TransferEncoding", SettingsManager.GetSessionTransferEncoding());
            myWebHeaderCollection.Add("Connection", SettingsManager.GetConnection());

            if (extraHeader != null)
            {
                foreach (var kv in extraHeader)
                {
                    if (myWebHeaderCollection.Get(kv.Key) == null)
                        myWebHeaderCollection.Add(kv.Key, kv.Value);
                }
            }
            if (toRemoveHeader != null)
            {
                foreach (var r in toRemoveHeader)
                {
                    if (myWebHeaderCollection.Get(r) != null)
                        myWebHeaderCollection.Remove(r);
                }
            }

            request.Referer = lasUrl;


            AddCookiesTo(request);               // Add cookies that were saved from previous requests

            if (_isPost) AddPostDataTo(request); // We only need to add post data on a POST request
            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetRTF(string url)
        {
            _isPost = false;
            try
            {


                CreateWebRequestObject().Load(url);

                if (string.IsNullOrEmpty(this.RTFData))
                    return _htmlDoc.DocumentNode.InnerHtml;
            }
            catch (Exception ex)
            {
                this.InternalError = ex.ToString();
            }
            return this.RTFData;
        }


        /// <summary>
        /// Append a url parameter to a string builder, url-encodes the value
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void AppendParameter(StringBuilder sb, string name, string value)
        {
            string encodedValue = HttpUtility.UrlEncode(value);
            sb.AppendFormat("{0}={1}&", name, encodedValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public HttpWebResponse SendDataToService(string url, Dictionary<string, string> dados)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in dados)
            {
                AppendParameter(sb, item.Key, item.Value);
            }

            byte[] buff = Encoding.UTF8.GetBytes(sb.ToString());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.Credentials = CredentialCache.DefaultNetworkCredentials; // ??

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(buff, 0, buff.Length);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            HtmlDocument doc = new HtmlDocument();

            doc.Load(stream);

            FileName = doc.DocumentNode.InnerHtml;

            return response;

            // do something with response
        }

        /// <summary>
        /// Retorna o documento html da pagina
        /// </summary>
        /// <returns></returns>
        public async Task<HtmlDocument> GetHtmlDocument(string url)
        {
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            return htmlDocument;
        }

        #region Cookies
        /// <summary>
        /// Criar uma requisição considerando os cookies
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string Get2(string url)
        {
            HtmlWeb web = new HtmlWeb();
            web.UseCookies = true;
            web.PreRequest = new HtmlWeb.PreRequestHandler(OnPreRequest2);
            web.PostResponse = new HtmlWeb.PostResponseHandler(OnAfterResponse2);
            HtmlDocument doc = web.Load(url);
            return doc.DocumentNode.InnerHtml;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool OnPreRequest2(HttpWebRequest request)
        {
            request.CookieContainer = _cookies;
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        protected void OnAfterResponse2(HttpWebRequest request, HttpWebResponse response)
        {
            //do nothing
        }
        /// <summary>
        /// Salva o cookie da pagina
        /// </summary>
        /// <param name="response"></param>
        private void SaveCookiesFrom2(HttpWebResponse response)
        {
            if ((response.Cookies.Count > 0))
            {
                if (Cookies == null)
                {
                    Cookies = new CookieCollection();
                }
                Cookies.Add(response.Cookies);
                _cookies.Add(Cookies);     //-> add the Cookies
            }
        }
        #endregion

        public void Dispose()
        {
        }
    }
}
