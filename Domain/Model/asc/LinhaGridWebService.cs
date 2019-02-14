using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoTeste.Domain.Model.Asc
{
    public class LinhaGridWebService
    {
        public string oType;

        public string OID { get; set; }
        public string QueueItemID { get; set; }
        public string ReferenteA { get; set; }
        public string Titulo { get; set; }
        public string NumeroOcorrencia { get; set; }
        public DateTime DataPrevistaConclusao { get; set; }
        public string CanalEntrada { get; set; }
        public string CPFCNPJ { get; set; }
        public string NomeCliente { get; set; }
        public DateTime DataCriacao { get; set; }
        public string Fila { get; set; }
        public string Status { get; set; }
        public string MOid { get; set; }
    }
}
