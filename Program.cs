using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoTeste
{
    class Program
    {
        static void Main(string[] args)
        {
            var webclient = new MyWebClient();
            
            HtmlDocument document = webclient.GetPage("https://loja.uira.com.br/loja/index.php", true);

            var response = webclient.GetResponse();

            if (webclient.GetResponse().StatusCode == HttpStatusCode.OK)
            {
                using (var streamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    document.OptionFixNestedTags = true;
                    document.Load(streamReader);

                    Console.WriteLine(streamReader);
                }
            }

            Console.WriteLine(document);
            Console.ReadKey();
        }
    }
}
