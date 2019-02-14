using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Comunicante")]
    [XmlRoot(ElementName = "comunicante")]
    public class Comunicante
    {
        [Category("Comunicante")]
        [XmlElement(ElementName = "tipo")]
        public string Tipo { get; set; }
        [Category("Comunicante")]
        [XmlElement(ElementName = "matricula")]
        public string Matricula { get; set; }
        [Category("Comunicante")]
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [Category("Comunicante")]
        [XmlElement(ElementName = "foneddd")]
        public string Foneddd { get; set; }
        [Category("Comunicante")]
        [XmlElement(ElementName = "fonenum")]
        public string Fonenum { get; set; }
        [Category("Comunicante")]
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
    }
}
