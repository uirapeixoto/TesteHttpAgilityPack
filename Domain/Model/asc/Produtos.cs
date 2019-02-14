using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Category("Produtos")]
    [XmlRoot(ElementName = "produtos")]
    public class Produtos
    {
        [Category("Produtos")]
        [XmlElement(ElementName = "produto")]
        public List<Produto> Produto { get; set; }
        [Category("Produtos")]
        [XmlAttribute(AttributeName = "ref")]
        public string Ref { get; set; }
        public ProdutosDC P()
        {
            ProdutosDC s = new ProdutosDC();
            s.Ref = Ref;
            s.Produto = new List<ProdutoDC>();
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
