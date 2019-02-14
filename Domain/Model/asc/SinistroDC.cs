using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "sinistro")]
    public class SinistroDC
    {
        [XmlElement(ElementName = "codcobertura")]
        public string Codcobertura { get; set; }
        [XmlElement(ElementName = "cobertura")]
        public string Cobertura { get; set; }
        [XmlElement(ElementName = "outrosinistro")]
        public string Outrosinistro { get; set; }
        [XmlElement(ElementName = "dataacidente")]
        public string Dataacidente { get; set; }
        [XmlElement(ElementName = "datasinistro")]
        public string Datasinistro { get; set; }
        [XmlElement(ElementName = "conjugeoufilho")]
        public string Conjugeoufilho { get; set; }
        [XmlElement(ElementName = "cpf")]
        public string Cpf { get; set; }
        [XmlElement(ElementName = "nascimento")]
        public string Nascimento { get; set; }
        [XmlElement(ElementName = "relato")]
        public string Relato { get; set; }
        [XmlElement(ElementName = "empregadocaixa")]
        public string Empregadocaixa { get; set; }
        [XmlElement(ElementName = "empregadofuncef")]
        public string Empregadofuncef { get; set; }
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public SinistroComunicado P()
        {
            SinistroComunicado s = new SinistroComunicado
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
