using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "produto")]
    public class ProdutoDC
    {
        [XmlElement(ElementName = "produtonome")]
        public string Produtonome { get; set; }
        [XmlElement(ElementName = "certificados")]
        public CertificadosDC Certificados { get; set; }
        public Produto P()
        {
            Produto s = new Produto
            {
                Certificados = Certificados.P(),
                Produtonome = Produtonome
            };
            return s;
        }
    }
}
