using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "xml")]
    public class ComunicadoDoc
    {
        [XmlElement(ElementName = "comunicado")]
        public ComunicadoDC Comunicado { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlIgnore]
        public bool FormatoDiferente { get; set; }

        public ComunicadoRTF P()
        {
            ComunicadoRTF c = new ComunicadoRTF
            {
                Comunicado = this.Comunicado.P(),
                Id = this.Id
            };
            return c;
        }

        public static ComunicadoDoc Parse(ComunicadoRTF comunicados)
        {
            throw new NotImplementedException();
        }
    }
}
