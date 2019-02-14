using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "empresa")]
    public class Empresa
    {
        [Category("Empresa")]
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [Category("Empresa")]
        [XmlElement(ElementName = "cnpj")]
        public string Cnpj { get; set; }
        [Category("Empresa")]
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public EmpresaDC P()
        {
            EmpresaDC s = new EmpresaDC
            {
                Cnpj = this.Cnpj,
                Nome = this.Nome,
                Ref = this.Ref
            };
            return s;
        }
    }
}
