using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoTeste.Domain.Model.Asc
{
    public class Comunicado : ComunicadoMain
    {
        override public Empresa Empresa { get; set; }

        override public SeguradoComunicado Segurado { get; set; }

        override public Comunicante Comunicante { get; set; }

        override public Reclamante Reclamante { get; set; }

        override public SinistroComunicado Sinistro { get; set; }

        override public Produtos Produtos { get; set; }

        override public Contrato Contrato { get; set; }

        override public Finalizacao Finalizacao { get; set; }

        override public string Id { get; set; }

        override public bool StSinistroOnline { get; set; }
    }
}
