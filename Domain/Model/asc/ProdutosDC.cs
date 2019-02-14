using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [XmlRoot(ElementName = "produtos")]
    public class ProdutosDC
    {
        [XmlElement(ElementName = "produto")]
        public List<ProdutoDC> Produto { get; set; }
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public Produtos P()
        {
            Produtos s = new Produtos();
            s.Ref = this.Ref;
            s.Produto = new List<Produto>();
            foreach (var d in this.Produto)
            {
                s.Produto.Add(
                    d.P()
                    );
            }
            return s;

        }
    }
}
