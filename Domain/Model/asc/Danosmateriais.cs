using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Danos Materiais")]
    [XmlRoot(ElementName = "danosmateriais")]
    public class Danosmateriais
    {
        [XmlElement(ElementName = "pontorefimovel")]
        public string Pontorefimovel { get; set; }
        [XmlElement(ElementName = "horariovisita")]
        public string Horariovisita { get; set; }
        [XmlElement(ElementName = "fonecontato")]
        public string Fonecontato { get; set; }
        [XmlElement(ElementName = "enderecoalternativoexiste")]
        public string Enderecoalternativoexiste { get; set; }
        [XmlElement(ElementName = "enderecoalternativo")]
        public string Enderecoalternativo { get; set; }
        [XmlElement(ElementName = "valorindenizacao")]
        public string Valorindenizacao { get; set; }
        [XmlElement(ElementName = "localchaves")]
        public string Localchaves { get; set; }
        [XmlElement(ElementName = "instututofiliacao")]
        public string Instututofiliacao { get; set; }
    }
}
