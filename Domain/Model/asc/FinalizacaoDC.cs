using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "finalizacao")]
    public class FinalizacaoDC
    {
        [XmlElement(ElementName = "datacomunicado")]
        public string Datacomunicado { get; set; }
        [XmlElement(ElementName = "nomeagenterelacionamento")]
        public string Nomeagenterelacionamento { get; set; }
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public Finalizacao P()
        {
            Finalizacao s = new Finalizacao
            {
                Datacomunicado = this.Datacomunicado,
                Nomeagenterelacionamento = this.Nomeagenterelacionamento,
                Ref = this.Ref
            };
            return s;
        }
    }
}
