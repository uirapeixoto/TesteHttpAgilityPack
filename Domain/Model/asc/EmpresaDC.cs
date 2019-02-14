using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "empresa")]
    public class EmpresaDC
    {
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [XmlElement(ElementName = "cnpj")]
        public string Cnpj { get; set; }
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public Empresa P()
        {
            Empresa s = new Empresa
            {
                Cnpj = this.Cnpj,
                Nome = this.Nome,
                Ref = this.Ref
            };
            return s;
        }
    }
}
