using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoTeste
{
    public class MyWebClient
    {
        public string   _url;
        public string   _baseurl;
        public bool     _authenticated;
        public string   _user;
        public string   _passwd;
        public bool     _isPost;
        public string   _encodingText;
        public bool     _useCredential;
        public bool     _autoRedirect;
        public string   _lasUrl;
        public string   _responseString;
        public HtmlDocument   _responsetHtmlDocument;
        private HttpWebResponse _response;
        public HttpWebRequest _request;

        public List<string> _toRemoveHeader;
        public Dictionary<string, string> _extraHeader;

        public NetworkCredential    _credentials;
        private HtmlDocument        _htmlDoc;
        public WebProxy             _proxy;
        public CookieCollection     _cookies;
        public CookieContainer      _cookieContainer;
        public FormElementCollection FormElements { get; set; }
        public BrowserSession _bs;

        public MyWebClient()
        {

            _baseurl = "http://dynamics.caixaseguros.intranet:5555";
            _url = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/main.aspx";
            _user = "ter69726@rootbrasil";
            _passwd = "Caixa123";
            _credentials = new NetworkCredential(_user, _passwd);
            _authenticated = false;
            _isPost = false;
            _toRemoveHeader = new List<string>();
            _extraHeader = new Dictionary<string, string>();
            _cookies = new CookieCollection();
            _cookieContainer = new CookieContainer();
            _bs = new BrowserSession();

            _proxy = new WebProxy("localhost", 8888);
            WebRequest.DefaultWebProxy = _proxy;
            _cookieContainer = new CookieContainer();
        }

        public HttpWebResponse GetResponse()
        {
            return _response;
        }

        public HtmlDocument GetHmlDocumento()
        {
            return _responsetHtmlDocument;
        }

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
        /// Creates the HtmlWeb object and initializes all event handlers. 
        /// </summary>
        private HtmlWeb CreateWebRequestObject()
        {
            var dsEnc = "ISO-8859-1";
            if (_encodingText != null)
                if (_encodingText != string.Empty)
                    dsEnc = _encodingText;

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

        public List<string> GetHtmlPage(string strURL)
        {
            // the html retrieved from the page

            WebResponse objResponse;
            WebRequest objRequest = System.Net.HttpWebRequest.Create(strURL);
            objResponse = objRequest.GetResponse();
            // the using keyword will automatically dispose the object 
            // once complete
            using (StreamReader sr =
            new StreamReader(objResponse.GetResponseStream()))
            {//*[@id="atfResults"]
                string strContent = sr.ReadToEnd();
                // Close and clean up the StreamReader
                sr.Close();
                /*Regex regex = new Regex("<body>((.|\n)*?)</body>", RegexOptions.IgnoreCase);

                //Here we apply our regular expression to our string using the 
                //Match object. 
                Match oM = regex.Match(strContent);
                Result = oM.Value;*/

                HtmlDocument doc = new HtmlDocument();
                doc.Load(new StringReader(strContent));
                HtmlNode root = doc.DocumentNode;
                List<string> itemTags = new List<string>();



                string listingtag = "//*[@id=homefeatured]";

                foreach (HtmlNode link in root.SelectNodes(listingtag))
                {
                    string att = link.OuterHtml;

                    itemTags.Add(att);
                }

                return itemTags;
            }

        }

        public string GetStringPage(string url)
        {
            WebRequest req = WebRequest.Create(url);
            WebResponse res = req.GetResponse();

            _responsetHtmlDocument = new HtmlDocument();
            _responsetHtmlDocument.Load(res.GetResponseStream());

            StreamReader reader = new StreamReader(res.GetResponseStream());
            var result = reader.ReadToEnd();
            reader.Close();
            res.Close();

            return result;
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
        /// Event handler for HtmlWeb.PreRequestHandler. Occurs before an HTTP request is executed.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected bool OnPreRequest(HttpWebRequest request)
        {
            WebHeaderCollection myWebHeaderCollection = request.Headers;

            if (_useCredential)
            {
                request.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                request.Credentials = _credentials;
            }
            request.AllowAutoRedirect = _autoRedirect;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:65.0) Gecko/20100101 Firefox/65.0";
            myWebHeaderCollection.Add("Accept-Language", "pt-BR,pt;q=0.8,en-US;q=0.5,en;q=0.3");
            myWebHeaderCollection.Add("AcceptCharset", "ISO-8859-1,utf-8;q=0.8,*;q=0.8");
            myWebHeaderCollection.Add("TransferEncoding", "gzip,deflate");

            if (_extraHeader != null)
            {
                foreach (var kv in _extraHeader)
                {
                    if (myWebHeaderCollection.Get(kv.Key) == null)
                        myWebHeaderCollection.Add(kv.Key, kv.Value);
                }
            }
            if (_toRemoveHeader != null)
            {
                foreach (var r in _toRemoveHeader)
                {
                    if (myWebHeaderCollection.Get(r) != null)
                        myWebHeaderCollection.Remove(r);
                }
            }

            request.Referer = _lasUrl;

            

            AddCookiesTo(request);               // Add cookies that were saved from previous requests

            if (_isPost) AddPostDataTo(request); // We only need to add post data on a POST request
            return true;
        }

        /// <summary>
        /// Saves cookies from the response object to the local CookieCollection object
        /// </summary>
        private void SaveCookiesFrom(HttpWebResponse response)
        {
            if (response.Cookies.Count > 0)
            {
                if (_cookies == null)
                    _cookies = new CookieCollection();

                _response = response;

                _cookies.Add(response.Cookies);
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
        /// Assembles the Post data and attaches to the request object
        /// </summary>
        private void AddPostDataTo(HttpWebRequest request)
        {
            string payload = FormElements.AssemblePostPayload();
            byte[] buff = Encoding.UTF8.GetBytes(payload.ToCharArray());
            request.ContentLength = buff.Length;
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(buff, 0, buff.Length);
        }

        /// <summary>
        /// Add cookies to the request object
        /// </summary>
        private void AddCookiesTo(HttpWebRequest request)
        {
            if (_cookies != null && _cookies.Count > 0)
            {
                request.CookieContainer.Add(_cookies);
            }
        }

        /// <summary>
        /// Faz a autenticação do ASC
        /// </summary>
        /// <returns></returns>
        public bool Autenticar()
        {
            _authenticated = false;
            try
            {
                _useCredential = true;
                _credentials = new NetworkCredential(_user, _passwd);

                var loged = Get($@"{_baseurl}/CRMCAD/main.aspx");

                if (loged.Contains("Importante:"))
                    _authenticated = true;

                Console.WriteLine("1. Login efetuado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Falha ao tentar autenticar usuário {_user}. {ex.Message}");
                Console.ReadLine();
            }
            return _authenticated;

        }

        //In case you need to clear the cookies
        public void ClearCookies()
        {
            _cookieContainer = new CookieContainer();
        }

        public HtmlDocument GetPage(string url = "", bool external_site = false)
        {

            if (!_authenticated && !external_site)
                Autenticar();

            var goTo = string.IsNullOrEmpty(url) ? $@"{_baseurl}/CRMCAD/AppWebServices/AppGridWebService.ashx?operation=Refresh" : url;
            Console.WriteLine(goTo);
            
            var html = Get(goTo);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            return htmlDocument;
        }
    }
}
