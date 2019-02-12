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
            
            var document = webclient.GetStringPage("https://loja.uira.com.br/loja/index.php") ;
            if (document.Contains("homefeatured"))
            {
                Console.WriteLine("achou");
            }

            var htmlDoc = webclient.GetHmlDocumento();
            var texto = htmlDoc.DocumentNode.Descendants("ul")
                .Where(div => div.GetAttributeValue("id", "").Equals("homefeatured"))
                .Select(ul => new
                {
                    ul.Descendants("li").FirstOrDefault().InnerText
                }).FirstOrDefault();
                

            Console.WriteLine(texto);
            
            Console.WriteLine(document);
            Console.ReadKey();
        }
    }
}
