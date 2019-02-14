using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Serializable()]
    [DataContract]
    public class SubisidioOvidoria
    {
        [DataMember] public string oID;

        [DataMember] public int iDiasDeAtraso { get; set; }
        [DataMember] public string sGerenciaArea { get; set; }
        [DataMember] public string sUltimaProgramacao { get; set; }
        [DataMember] public string sProprietario { get; set; }
        [DataMember] public string sClassificacaoDaOcorrencia { get; set; }
        [DataMember] public string sAvaliacaoReprovada { get; set; }
        [DataMember] public bool bDevolverPedidoAOuvidoria { get; set; }
        [DataMember] public string sConclusao { get; set; }
        [DataMember] public DateTime dResposta { get; set; }
        [DataMember] public DateTime dConclusao { get; set; }
        [DataMember] public DateTime dInicio { get; set; }
        [DataMember] public string sElaboradorSubsidio { get; set; }
        [DataMember] public string sElaboradorOuvidoria { get; set; }
        [DataMember] public DateTime dInicioReal { get; set; }
        [DataMember] public string sPrioridade { get; set; }
        [DataMember] public string sRazaoStatus { get; set; }
        [DataMember] public string sRespostaDoSubsidio { get; set; }
        [DataMember] public string sStatusAtividade { get; set; }
        [DataMember] public string sSubsidioReprovado { get; set; }

        [DataMember] public string Numero { get; set; }
        [DataMember] public DateTime dCriacao { get; set; }
        [DataMember] public string iStatusDoSubsidio { get; set; }
        [DataMember] public DateTime dPrazoDeTratamento { get; set; }

        [DataMember] public string MensagemDeErro { get; set; }
        [DataMember] public int CodErro { get; set; }
        [DataMember] public string Type { get; set; }
    }
}
