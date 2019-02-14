using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Serializable()]
    [DataContract()]
    public class OcorrenciaOuvidoria
    {
        [DataMember] public PessoaOuvidoria kpInformante { get; set; }
        [DataMember] public PessoaOuvidoria kpCliente { get; set; }

        //[DataMember]  public string sClienteCPFCNPJ { get; set; }
        //[DataMember] public string sClienteNome { get; set; }


        //[DataMember] public string sInformanteCPFCNPJ { get; set; }
        //[DataMember] public string sInformanteNome { get; set; }


        //[DataMember]
        //public string sTipoPessoa { get; set; }
        [DataMember]
        public string sTrabalhadoPor { get; set; }

        [DataMember]
        public DateTime dModificacao { get; set; }
        [DataMember]
        public string sRazaoStatus { get; set; }

        [DataMember] public string sInformantePF { get; set; }
        [DataMember] public string sClientePF { get; set; }

        [DataMember] public string sInformantePJ { get; set; }
        [DataMember] public string sClientePJ { get; set; }


        [DataMember]
        public string oType { get; set; }
        [DataMember]
        public string oID { get; set; }
        [DataMember] public string oQueueItemID { get; set; }
        public DateTime dInseridoNaFila { get; set; }

        [DataMember] public string nOcorrencia { get; set; }


        [DataMember] public DateTime dCadastroOcorrencia { get; set; }
        [DataMember] public DateTime dRecebimento { get; set; }

        [DataMember]
        public TipoOcorrenciaOuvidoria kpTipoOcorrencia { get; set; }
        [DataMember] public int iIdTipoOcorrencia { get; set; }

        [DataMember]
        public TipoContatoOuvidoria kpFormaContatoOuvidoria { get; set; }
        [DataMember] public int iIdFormaContato { get; set; }


        [DataMember]
        public OrigemOcorrenciaOuvidoria kpOrigemOcorrencia { get; set; }
        [DataMember] public int iIdOrigemOcorrencia { get; set; }

        [DataMember]
        public List<SubisidioOvidoria> kpSubisidios { get; set; }

        [DataMember] public string sProtocolo { get; set; }

        [DataMember] public DateTime dVencimentoPrevisto { get; set; }


        [DataMember] public DateTime dMulta { get; set; }
        [DataMember] public string sEmail { get; set; }
        [DataMember] public string sModificadoPor { get; set; }

        [DataMember] public string sPertenceACaixa { get; set; }
        [DataMember] public string sProduto { get; set; }
        [DataMember] public string sOcorrenciaComCliente { get; set; }
        // public DateTime dCriadoEm { get; set; }
        [DataMember] public string sControleStatus { get; set; }
        [DataMember] public string sStatus { get; set; }
        [DataMember] public string sStatusOcorrencia { get; set; }

        [DataMember] public string MensagemDeErro { get; set; }




        [DataMember] public DateTime dCriacaoASC { get; set; }
        [DataMember] public DateTime dVencimentoLegal { get; set; }
        [DataMember] public int CodErro { get; set; }
        //[DataMember] public int iCodTipoPessoa { get; set; }
    }
}
