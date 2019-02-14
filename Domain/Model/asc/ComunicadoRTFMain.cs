using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    public abstract class ComunicadoRTFMain
    {
        [XmlIgnore]
        abstract public ComunicadoXML Comunicado { get; set; }
        [XmlIgnore]
        abstract public Comunicado ComunicadoObj { get; set; }
        [XmlIgnore]
        abstract public bool FormatoDiferente { get; set; }
        [XmlIgnore]
        abstract public string NU_ASC { get; set; }
        [XmlIgnore]
        abstract public string OID { get; set; }
        [XmlIgnore]
        abstract public string QID { get; set; }

        [XmlIgnore]
        abstract public string Id { get; set; }
        [XmlIgnore]
        abstract public bool ExtensaoDocx { get; set; }
        [XmlIgnore]
        abstract public bool FormatoInvalido { get; set; }
        [XmlIgnore]
        abstract public string LinkArquivo { get; set; }
        [XmlIgnore]
        abstract public string CacheFile { get; set; }
        [XmlIgnore]
        abstract public int CanalEntrada { get; set; } //1- almaviva;2 sinistro online;3 email; 4 andamento- apolice espefica
    }
}
