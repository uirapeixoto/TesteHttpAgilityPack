using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoTeste.Domain.Model.Asc
{
    public class ComunicadoRTF : ComunicadoRTFMain
    {
        override public Comunicado ComunicadoObj { get; set; }
        override public ComunicadoXML Comunicado { get; set; }
        override public bool FormatoDiferente { get; set; }

        override public string NU_ASC { get; set; }
        override public string QID { get; set; }

        override public string OID { get; set; }

        override public string Id { get; set; }

        override public bool ExtensaoDocx { get; set; }

        override public bool FormatoInvalido { get; set; }

        override public string LinkArquivo { get; set; }

        override public string CacheFile { get; set; }
        override public int CanalEntrada { get; set; }


        public static explicit operator ComunicadoRTF(ComunicadoRTFXML v)
        {
            ComunicadoRTF c = new ComunicadoRTF
            {
                CacheFile = v.CacheFile,
                Comunicado = v.Comunicado,
                ComunicadoObj = v.ComunicadoObj,
                ExtensaoDocx = v.ExtensaoDocx,
                FormatoDiferente = v.FormatoDiferente,
                FormatoInvalido = v.FormatoInvalido,
                Id = v.Id,
                LinkArquivo = v.LinkArquivo,
                NU_ASC = v.NU_ASC,
                OID = v.OID
            };
            return c;
        }
        public ComunicadoDoc P()
        {
            ComunicadoDoc c = new ComunicadoDoc
            {
                Comunicado = this.Comunicado.P(),
                Id = this.Id
            };
            return c;
        }
    }
}
