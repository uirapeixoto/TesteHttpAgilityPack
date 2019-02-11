using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Automacao.Core.Helper.Library
{
    class RTFUtil
    {
        public static string GetRTFTHtmlData(string rtf)
        {

            int i1 = rtf.IndexOf(@"{\colortbl");

            if (i1 <= 0) throw new ArgumentException("Bad input RTF.");

            int i2 = rtf.IndexOf(";}", i1);

            if (i2 <= 0) throw new ArgumentException("Bad input RTF.");

            string data = rtf.Substring(i2 + 2, rtf.Length - (i2 + 2) - 1);


            var tw = Format(data);

            return tw;

        }


        // Default color table used by VS's IDE.

        static string[] m_colorTable = new string[]

            {

               // rrGGbb

                "#000000", // default, starts at index 0

                "#000000", // real color table starts at index 1

                "#0000FF",

                "#00ffFF",

                "#00FF00",

                "#FF00FF",

                "#FF0000",

                "#FFFF00",

                "#FFffFF",

                "#000080",

                "#008080",

                "#008000",

                "#800080",

                "#800000",

                "#808000",

                "#808080",

                "#c0c0c0"

            };


        // Escape HTML chars

        static string Escape(string st)

        {

            st = st.Replace("&", "&amp;");

            st = st.Replace("<", "&lt;");

            st = st.Replace(">", "&gt;");

            return st;

        }

        // Convert the RTF data into an HTML stream.

        // This rtf snippet is past the font + color tables, so we're just transfering control words now.

        // Write out HTML to the text writer.

        static string Format(string rtf)
        {
            string tw = "";

            tw += ("<html><pre>");

            tw += ("<span color=black>");

            // Example: \fs20 \cf2 using\cf0  System;

            // root --> ('text' '\' ('control word' | 'escaped char'))+

            // 'control word'  --> (alpha)+ (numeric*) space?

            // 'escaped char' = 'x'. Some characters \, {, } are escaped: '\x' --> 'x'

            // @todo - handle embedded groups (begin with '{')


            int idx = 0;

            while (idx < rtf.Length)

            {

                // Get any text up to a '\'.

                Regex r1 = new Regex(@"(.*?)\\", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                Match m = r1.Match(rtf, idx);

                if (m.Length == 0) break;


                // text will be empty if we have adjacent control words

                string stText = m.Groups[1].ToString();

                tw += (Escape(stText));

                idx += m.Length;


                // check for RTF escape characters. According to the spec, these are the only escaped chars.

                char chNext = rtf[idx];

                if (chNext == '{' || chNext == '}' || chNext == '\\')

                {

                    // Escaped char

                    tw += (chNext);

                    idx++;

                    continue;

                }


                // Must be a control char. @todo- delimeter includes more than just space, right?

                Regex r2 = new Regex(@"([\{a-z]+)([0-9]*) ", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                m = r2.Match(rtf, idx);

                string stCtrlWord = m.Groups[1].ToString();

                string stCtrlParam = m.Groups[2].ToString();


                if (stCtrlWord == "cf")

                {

                    // Set font color.

                    int iColor = Int32.Parse(stCtrlParam);

                    tw += ("</span>"); // close previous span, and start a new one for the given color.

                    tw += ("<span style=\"color: " + m_colorTable[iColor] + "\">");

                }

                else if (stCtrlWord == "fs")

                {

                    // Sets font size. ignore

                }

                else if (stCtrlWord == "par")

                {

                    // This is a newline. ignore

                    // @todo- I think the only reason we can ignore this is because the \par in our input are always followed by

                    // a '\r\n' and we're accidentally writing that.

                }

                else

                {

                    // throw new ArgumentException("Unrecognized control word '" + stCtrlWord + stCtrlParam + "'after:" + stText);

                }

                idx += m.Length;

            }

            tw += (Escape(rtf.Substring(idx))); // rest of string


            tw += ("</pre></html>");
            return tw;

        } // end Format()

        public static bool IsRichText(string testString)
        {
            if ((testString != null) &&
                (testString.Trim().StartsWith("{\\rtf")))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string StripRTF(string rtfString)
        {
            string result = rtfString;

            try
            {
                if (IsRichText(rtfString))
                {
                    // Put body into a RichTextBox so we can strip RTF
                    using (System.Windows.Forms.RichTextBox rtfTemp = new System.Windows.Forms.RichTextBox())
                    {
                        rtfTemp.Rtf = rtfString;
                        result = rtfTemp.Text;
                    }
                }
                else
                {
                    result = rtfString;
                }
            }
            catch
            {
                throw;
            }

            return result;
        }

    }
}
