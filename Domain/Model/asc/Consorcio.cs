using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Consorcio")]
    [XmlRoot(ElementName = "consorcio")]
    public class Consorcio
    {
        [XmlAttribute(AttributeName = "grupo")]
        public string Grupo { get; set; }
        [XmlAttribute(AttributeName = "cota")]
        public string Cota { get; set; }
    }
}
