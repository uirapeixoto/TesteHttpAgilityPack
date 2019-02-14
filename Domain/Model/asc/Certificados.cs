using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Certificados")]
    [XmlRoot(ElementName = "certificados")]
    public class Certificados
    {
        [Category("Certificados")]
        [XmlElement(ElementName = "certificado")]
        public string Certificado { get; set; }
        public CertificadosDC P()
        {
            CertificadosDC s = new CertificadosDC
            {
                Certificado = this.Certificado,
            };
            return s;
        }
    }
}
