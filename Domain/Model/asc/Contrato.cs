using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Contrato")]
    [XmlRoot(ElementName = "contrato")]
    public class Contrato
    {
        [Category("Contrato")]
        [XmlElement(ElementName = "numero")]
        public string Numero { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "adesao")]
        public string Adesao { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "cartacredito")]
        public string Cartacredito { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "prazo")]
        public string Prazo { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "consorcio")]
        public Consorcio Consorcio { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "enderecoimovel")]
        public string Enderecoimovel { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "bairro")]
        public string Bairro { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "cidade")]
        public string Cidade { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "cep")]
        public string Cep { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "uf")]
        public string Uf { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "mesanori")]
        public string Mesanori { get; set; }
        [Category("Contrato")]
        [XmlElement(ElementName = "danosmateriais")]

        public Danosmateriais Danosmateriais { get; set; }
        [Category("Contrato")]
        [XmlAttribute(AttributeName = "ref")]

        public string Ref { get; set; }
    }
}
