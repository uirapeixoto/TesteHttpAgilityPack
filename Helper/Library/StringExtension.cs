using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Automacao.Core.Helper.Library
{
    public static class StringExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void CopyTo(this Stream source, Stream destination)
        {
            // TODO: Argument validation
            byte[] buffer = new byte[16384]; // For example...
                                             // byte[] buffer = new byte[(int)source.Length]; // For example...
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
            }
        }
        // This is the extension method.
        // The first parameter takes the "this" modifier
        // and specifies the type for which the method is defined.
        public static List<string> ToLines(this string str)
        {
            var linhas = new List<string>();
            using (StringReader reader = new StringReader(str))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        linhas.Add(line);
                    }

                } while (line != null);
            }
            return linhas;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="characterCount"></param>
        /// <returns></returns>
        public static string Limit(this string str, int characterCount)
        {
            if (str.Length <= characterCount) return str;
            else return str.Substring(0, characterCount).TrimEnd() + "...";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimX(this string value)
        {
            if (value != null)
            {
                return value.Trim();
            }
            return "";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsHtmlEncoded(string text)
        {
            return (HttpUtility.HtmlDecode(text) != text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string InnerText(this string inputString)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(inputString);
            var texto = HttpUtility.HtmlDecode(doc.DocumentNode.InnerText).Trim();
            var isEncoded = IsHtmlEncoded(texto);
            while (isEncoded)
            {
                texto = HttpUtility.HtmlDecode(texto).Trim();
                isEncoded = IsHtmlEncoded(texto);
            }

            return texto;
        }
    }
}
