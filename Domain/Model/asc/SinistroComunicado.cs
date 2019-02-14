using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Sinistro")]
    [XmlRoot(ElementName = "sinistro")]
    public class SinistroComunicado
    {
        [Category("Sinistro")]
        [XmlElement(ElementName = "codcobertura")]
        public string Codcobertura { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "cobertura")]
        public string Cobertura { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "outrosinistro")]
        public string Outrosinistro { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "dataacidente")]
        public string Dataacidente { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "datasinistro")]
        public string Datasinistro { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "conjugeoufilho")]
        public string Conjugeoufilho { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "cpf")]
        public string Cpf { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "nascimento")]
        public string Nascimento { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "relato")]
        public string Relato { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "empregadocaixa")]
        public string Empregadocaixa { get; set; }
        [Category("Sinistro")]
        [XmlElement(ElementName = "empregadofuncef")]
        public string Empregadofuncef { get; set; }
        [Category("Sinistro")]
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public SinistroDC P()
        {
            SinistroDC s = new SinistroDC
            {
                Cobertura = this.Cobertura,
                Codcobertura = this.Codcobertura,
                Conjugeoufilho = this.Conjugeoufilho,
                Cpf = this.Cpf,
                Dataacidente = this.Dataacidente,
                Datasinistro = this.Datasinistro,
                Empregadocaixa = this.Empregadocaixa,
                Empregadofuncef = this.Empregadofuncef,
                Nascimento = this.Nascimento,
                Outrosinistro = this.Outrosinistro,
                Ref = this.Ref,
                Relato = this.Relato
            };
            return s;
        }
    }
}
