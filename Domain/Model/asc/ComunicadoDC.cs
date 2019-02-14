using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "comunicado")]
    public class ComunicadoDC
    {
        [XmlElement(ElementName = "empresa")]
        public EmpresaDC Empresa { get; set; }
        [XmlElement(ElementName = "segurado")]
        public SeguradoDC Segurado { get; set; }
        [XmlElement(ElementName = "reclamante")]
        public ReclamanteDC Reclamante { get; set; }
        [XmlElement(ElementName = "sinistro")]
        public SinistroDC Sinistro { get; set; }
        [XmlElement(ElementName = "produtos")]
        public ProdutoDC Produtos { get; set; }
        [XmlElement(ElementName = "finalizacao")]
        public FinalizacaoDC Finalizacao { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        public ComunicadoXML P()
        {
            ComunicadoXML s = new ComunicadoXML
            {
                Empresa = Empresa.P(),
                Finalizacao = Finalizacao.P(),
                Id = Id,
                Reclamante = Reclamante.P(),
                //Produtos = Produtos.P(),
                Segurado = Segurado.P(),
                Sinistro = Sinistro.P(),
            };
            return s;
        }
    }
}
