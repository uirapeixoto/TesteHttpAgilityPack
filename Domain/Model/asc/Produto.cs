using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Produto")]
    [XmlRoot(ElementName = "produto")]
    public class Produto
    {
        [Category("Produto")]
        [XmlElement(ElementName = "produtonome")]
        public string Produtonome { get; set; }
        [Category("Produto")]
        [XmlElement(ElementName = "certificados")]
        public Certificados Certificados { get; set; }
        [Category("Produto")]
        [XmlElement(ElementName = "consorcios")]
        public Consorcios Consorcios { get; set; }
        public ProdutoDC P()
        {
            ProdutoDC s = new ProdutoDC
            {
                Certificados = this.Certificados.P(),
                Produtonome = this.Produtonome
            };
            return s;
        }
    }
}
