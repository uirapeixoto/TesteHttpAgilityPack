using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    public abstract class ComunicadoMain
    {
        [XmlIgnore]
        abstract public Empresa Empresa { get; set; }
        [XmlIgnore]
        abstract public SeguradoComunicado Segurado { get; set; }
        [XmlIgnore]
        abstract public Comunicante Comunicante { get; set; }
        [XmlIgnore]
        abstract public Reclamante Reclamante { get; set; }
        [XmlIgnore]
        abstract public SinistroComunicado Sinistro { get; set; }
        [XmlIgnore]
        abstract public Produtos Produtos { get; set; }
        [XmlIgnore]
        abstract public Contrato Contrato { get; set; }
        [XmlIgnore]
        abstract public Finalizacao Finalizacao { get; set; }
        [XmlIgnore]
        abstract public string Id { get; set; }
        [XmlIgnore]
        abstract public bool StSinistroOnline { get; set; }
    }
}
