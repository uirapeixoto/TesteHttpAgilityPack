using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Comunicado")]
    [XmlRoot(ElementName = "comunicado")]
    public class ComunicadoXML : ComunicadoMain
    {
        [Category("Comunicado")]
        [XmlElement(ElementName = "empresa")]
        override public Empresa Empresa { get; set; }
        [Category("Comunicado")]
        [XmlElement(ElementName = "segurado")]
        override public SeguradoComunicado Segurado { get; set; }
        [Category("Comunicado")]
        [XmlElement(ElementName = "comunicante")]
        override public Comunicante Comunicante { get; set; }
        [Category("Comunicado")]
        [XmlElement(ElementName = "reclamante")]
        override public Reclamante Reclamante { get; set; }
        [Category("Comunicado")]
        [XmlElement(ElementName = "sinistro")]
        override public SinistroComunicado Sinistro { get; set; }
        [Category("Comunicado")]
        [XmlElement(ElementName = "produtos")]
        override public Produtos Produtos { get; set; }
        [Category("Comunicado")]
        [XmlElement(ElementName = "contrato")]
        override public Contrato Contrato { get; set; }
        [Category("Comunicado")]
        [XmlElement(ElementName = "finalizacao")]
        override public Finalizacao Finalizacao { get; set; }
        [Category("Comunicado")]
        [XmlAttribute(AttributeName = "id")]
        override public string Id { get; set; }
        [XmlIgnore]
        override public bool StSinistroOnline { get; set; }

        public static explicit operator ComunicadoXML(Comunicado v)
        {
            var c = new ComunicadoXML
            {
                Comunicante = v.Comunicante,
                Contrato = v.Contrato,
                Empresa = v.Empresa,
                Finalizacao = v.Finalizacao,
                Id = v.Id,
                Produtos = v.Produtos,
                Reclamante = v.Reclamante,
                Segurado = v.Segurado,
                Sinistro = v.Sinistro,
                StSinistroOnline = v.StSinistroOnline
            };
            return c;
        }
        public ComunicadoDC P()
        {
            ComunicadoDC s = new ComunicadoDC
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
