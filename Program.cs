using ProjetoTeste.Pages;
using System;

namespace ProjetoTeste
{
    class Program
    {
        static void Main(string[] args)
        {

            var page = new UiraPage();
            var result = page.Login();
            var pageHome = page.GetHome();
            
            Console.WriteLine(pageHome);
            Console.ReadKey();
        }
    }
}
