using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Segurado")]
    [XmlRoot(ElementName = "segurado")]
    public class SeguradoComunicado
    {
        [Category("Segurado")]
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [Category("Segurado")]
        [XmlElement(ElementName = "cpf")]
        public string Cpf { get; set; }
        [Category("Segurado")]
        [XmlElement(ElementName = "nascimento")]
        public string Nascimento { get; set; }
        [Category("Segurado")]
        [XmlElement(ElementName = "sexo")]
        public string Sexo { get; set; }
        [Category("Segurado")]
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public SeguradoDC P()
        {
            SeguradoDC s = new SeguradoDC
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
