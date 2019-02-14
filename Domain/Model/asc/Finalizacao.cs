using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Finalizacao")]
    [XmlRoot(ElementName = "finalizacao")]
    public class Finalizacao
    {
        [Category("Finalizacao")]
        [XmlElement(ElementName = "datacomunicado")]
        public string Datacomunicado { get; set; }
        [Category("Finalizacao")]
        [XmlElement(ElementName = "nomeagenterelacionamento")]
        public string Nomeagenterelacionamento { get; set; }
        [Category("Finalizacao")]
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public FinalizacaoDC P()
        {
            FinalizacaoDC s = new FinalizacaoDC
            {
                Datacomunicado = this.Datacomunicado,
                Nomeagenterelacionamento = this.Nomeagenterelacionamento,
                Ref = this.Ref
            };
            return s;
        }
    }
}
