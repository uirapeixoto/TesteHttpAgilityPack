using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "segurado")]
    public class SeguradoDC
    {
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [XmlElement(ElementName = "cpf")]
        public string Cpf { get; set; }
        [XmlElement(ElementName = "nascimento")]
        public string Nascimento { get; set; }
        [XmlElement(ElementName = "sexo")]
        public string Sexo { get; set; }
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public SeguradoComunicado P()
        {
            SeguradoComunicado s = new SeguradoComunicado
            {
                Sexo = this.Sexo,
                Cpf = this.Cpf,
                Nascimento = this.Nascimento,
                Nome = this.Nome,
                Ref = this.Ref
            };
            return s;
        }
    }
}
