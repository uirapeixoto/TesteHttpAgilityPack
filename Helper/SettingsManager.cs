using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoTeste.Helper
{
    /// <summary>
    /// Author: Felipe Guarneri
    /// Configura asessão do navegador
    /// </summary>
    public static class SettingsManager
    {

        public static string GetSessionUserAgent()
        {
            return @"Mozilla/5.0 (Windows NT 10.0; WOW64; rv:54.0) Gecko/20100101 Firefox/54.0";
        }

        public static string GetSessionAcceptLanguage()
        {
            return "pt-BR,pt;q=0.8,en-US;q=0.5,en;q=0.3";
        }

        public static string GetSessionAcceptCharset()
        {
            return "ISO-8859-1,utf-8;q=0.8,*;q=0.8";
        }

        public static string GetSessionTransferEncoding()
        {
            return "gzip,deflate";
        }

        public static string GetConnection()
        {
            return "keep-alive";
        }
    }
}
