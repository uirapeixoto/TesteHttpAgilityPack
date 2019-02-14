using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoTeste.Domain.Model.Asc
{
    public class Anexo
    {

        public string ContentType { get; set; }
        public string Url { get; set; }
        public string AlteradoPor { get; set; }
        // public byte[] Raw { get; internal set; }
        public string RTFData { get; set; }
        public ComunicadoRTF Comunicado { get; set; }
        public string NU_ASC { get; set; }
        public string OID { get; set; }
        public string Origem { get; set; }
    }
}
