using HtmlAgilityPack;
using ProjetoTeste.Page;
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

            var page = new AutomobilePage();
            var result = page.GetCars2();
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
