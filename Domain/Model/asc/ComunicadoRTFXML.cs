using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "xml")]
    public class ComunicadoRTFXML : ComunicadoRTFMain
    {
        [XmlElement(ElementName = "comunicado")]
        override public ComunicadoXML Comunicado { get; set; }
        [XmlAttribute(AttributeName = "id")]
        override public string Id { get; set; }

        [XmlIgnore]
        override public Comunicado ComunicadoObj { get; set; }
        [XmlIgnore]
        override public bool FormatoDiferente { get; set; }
        [XmlIgnore]
        override public string NU_ASC { get; set; }
        [XmlIgnore]
        override public string OID { get; set; }
        [XmlIgnore]
        override public string QID { get; set; }
        [XmlIgnore]
        override public bool ExtensaoDocx { get; set; }
        [XmlIgnore]
        override public bool FormatoInvalido { get; set; }
        [XmlIgnore]
        override public string LinkArquivo { get; set; }
        [XmlIgnore]
        override public string CacheFile { get; set; }
        [XmlIgnore]
        override public int CanalEntrada { get; set; }
        public static explicit operator ComunicadoRTFXML(ComunicadoRTF v)
        {
            ComunicadoRTFXML c = new ComunicadoRTFXML
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
    }

}
