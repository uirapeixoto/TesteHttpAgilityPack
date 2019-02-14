using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "reclamante")]
    public class ReclamanteDC
    {
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [XmlElement(ElementName = "cpf")]
        public string Cpf { get; set; }
        [XmlElement(ElementName = "parentesco")]
        public string Parentesco { get; set; }
        [XmlElement(ElementName = "emailpes")]
        public string Emailpes { get; set; }
        [XmlElement(ElementName = "emailcom")]
        public string Emailcom { get; set; }
        [XmlElement(ElementName = "endereco")]
        public string Endereco { get; set; }
        [XmlElement(ElementName = "bairro")]
        public string Bairro { get; set; }
        [XmlElement(ElementName = "cidade")]
        public string Cidade { get; set; }
        [XmlElement(ElementName = "cep")]
        public string Cep { get; set; }
        [XmlElement(ElementName = "uf")]
        public string Uf { get; set; }
        [XmlElement(ElementName = "foneresddd")]
        public string Foneresddd { get; set; }
        [XmlElement(ElementName = "fonteresnum")]
        public string Fonteresnum { get; set; }
        [XmlElement(ElementName = "fonecomddd")]
        public string Fonecomddd { get; set; }
        [XmlElement(ElementName = "fonecomnum")]
        public string Fonecomnum { get; set; }
        [XmlElement(ElementName = "fonecelddd")]
        public string Fonecelddd { get; set; }
        [XmlElement(ElementName = "fonecelnum")]
        public string Fonecelnum { get; set; }
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public Reclamante P()
        {
            Reclamante s = new Reclamante
            {
                Cpf = this.Cpf,
                Parentesco = this.Parentesco,
                Emailpes = this.Emailpes,
                Emailcom = this.Emailcom,
                Endereco = this.Endereco,
                Bairro = this.Bairro,
                Cidade = this.Cidade,
                Cep = this.Cep,
                Uf = this.Uf,
                Foneresddd = this.Foneresddd,
                Fonecelddd = this.Fonecelddd,
                Fonecelnum = this.Fonecelnum,
                Fonecomddd = this.Fonecomddd,
                Fonecomnum = this.Fonecomnum,
                Fonteresnum = this.Fonteresnum,
                Nome = this.Nome,
                Ref = this.Ref
            };
            return s;
        }
    }
}
