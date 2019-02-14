using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProjetoTeste.Domain.Model.Asc
{
    [Serializable()]
    [DataContract]
    public class TipoPessoa
    {
        [DataMember] public int iCod { get; set; }
        [DataMember] public string sDescricao { get; set; }
    }
}
