using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Reclamante")]
    [XmlRoot(ElementName = "reclamante")]
    public class Reclamante
    {
        [Category("Reclamante")]
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "cpf")]
        public string Cpf { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "parentesco")]
        public string Parentesco { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "emailpes")]
        public string Emailpes { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "emailcom")]
        public string Emailcom { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "endereco")]
        public string Endereco { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "bairro")]
        public string Bairro { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "cidade")]
        public string Cidade { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "cep")]
        public string Cep { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "uf")]
        public string Uf { get; set; }

        //[XmlIgnore]
        //[Category("Reclamante")]
        //public string UfSigla { get; set; }

        [Category("Reclamante")]
        [XmlElement(ElementName = "foneresddd")]
        public string Foneresddd { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "fonteresnum")]
        public string Fonteresnum { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "fonecomddd")]
        public string Fonecomddd { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "fonecomnum")]
        public string Fonecomnum { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "fonecelddd")]
        public string Fonecelddd { get; set; }
        [Category("Reclamante")]
        [XmlElement(ElementName = "fonecelnum")]
        public string Fonecelnum { get; set; }
        [Category("Reclamante")]
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public ReclamanteDC P()
        {
            ReclamanteDC s = new ReclamanteDC
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
