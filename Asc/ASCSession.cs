using Automacao.Core.Helper;
using Automacao.Core.Helper.Library;
using Automacao.Domain.Model.ASC;
using Automacao.Domain.Model.ASC.Client;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static Automacao.Domain.Model.ASC.Ocorrencia;

namespace Automacao.Core.Asc
{
    public class ASCSession : IDisposable
    {

        private string _baseurl;
        private bool _authenticated;
        private string _user;
        private string _password;
        private string _encondingText;
        private bool _userCredential;
        private bool _autoRedirect;

        public BrowserSession bs = new BrowserSession();
        public bool Autenticad { get; private set; }
        public List<string> IgnoredIds { get; set; }

        public ASCSession(string user, string password)
        {
            _user = user;
            _password = password;
            _baseurl = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/";

            bs = new BrowserSession();
            bs.EncodingTexto = "utf-8";

            Autenticar();
            
        }
         
        /// <summary>
        /// Faz a autenticação do ASC
        /// </summary>
        /// <returns></returns>
        public bool Autenticar()
        {
            _authenticated = false;
            try
            {
                bs.UseCredentials = true;
                bs.Credentials = new System.Net.NetworkCredential(_user, _password);
                var loged = bs.Get($@"{_baseurl}main.aspx");
                if (loged.Contains("Importante:"))
                    _authenticated = true;
            }
            catch (Exception ex)
            {

                throw new Exception($@"Falha ao tentar autenticar usuário {_user}. Erro: {ex.Message}");
            }
            return _authenticated;

        }


        public Ocorrencia GetOcorrenciaByOID(string oID)
        {
            bs.UseCredentials = true;
            bs.Credentials = new System.Net.NetworkCredential(_user, _password);
            if (!_authenticated)
                Autenticar();
            Ocorrencia a = new Ocorrencia();
            a.OID = oID;
            var produto = string.Empty;
            try
            {
                bs.EncodingTexto = "utf-8";
                string urlEdit = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/main.aspx?etc=112&extraqs=%3fetc%3d112%26id%3d%257b" + oID + "%257d%26pagemode%3diframe%26preloadcache%3d1502217951380&pagetype=entityrecord";
                // string urlEdit = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/userdefined/edit.aspx?etc=112&id=%7b"+ oID +"7d&pagemode=iframe&preloadcache=1499719951927";
                //string urlEdit = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/userdefined/edit.aspx?_CreateFromId=%7bC76C3CE8-7011-E311-8B5D-00155D0102C6%7d&_CreateFromType=2&_gridType=4200&etc=4212&id=%7b" + oID + "%7d&pagemode=iframe&preloadcache=1406234987296&rskey=776556412";

                var pgDetalhe = bs.Get(urlEdit);
                if (pgDetalhe.Contains("Object moved"))
                {
                    return null;
                }
                var pgDetalheFL = pgDetalhe.Replace("<label ", Environment.NewLine);
                pgDetalheFL = pgDetalheFL.Replace("ms-crm-LookupItem-Name", Environment.NewLine);

                a.OID = oID;
                a.NumeroOcorrencia = bs.FormElements["f_003"];
                a.CPFCNPJ = bs.FormElements["f_005"];
                a.CanalEntrada = bs.FormElements["f_004"].InnerText();
                var coStatusAtendimento = bs.FormElements["gcs_statusatendimento"];//, 1]}
                switch (coStatusAtendimento)
                {
                    case "0": { a.StatusAtividade = "No Prazo"; break; }
                    case "1": { a.StatusAtividade = "Fora do Prazo"; break; }
                    case "2":
                        {
                            a.StatusAtividade = "Vencida";
                            break;
                        }
                    default:
                        {
                            a.StatusAtividade = "No Prazo";
                            break;
                        }
                }


                var pgLinhas = pgDetalheFL.ToLines();
                for (int i = 0; i < pgLinhas.Count; i++)
                {
                    var l = pgLinhas[i];
                    if (l.Contains("<title"))
                    {
                        var arrTitulo = l.Split(':');
                        //<title>Tarefa: Análise/Resolução da Ocorrência </title>
                        a.Titulo = arrTitulo[1].Replace("</title>", "").Trim();
                        //  a.TipoAtividade = arrTitulo[0].Replace("<title>", "").Trim();
                    }
                    if (l.Contains("Referente a</label>"))
                    {
                        a.ReferenteA = pgLinhas[i + 1].Replace("' wrapper='true'>", "").Replace("</span></span></li></ul></div>", "").Trim();

                    }
                    if (l.Contains("gcs_produto\">Produto"))
                    {
                        //' wrapper='true'>VIDA EXCLUSIVO</span></span></li></ul></div>
                        //  a.Produto = ("<label " + pgLinhas[i + 1]).InnerText();
                    }
                    if (l.Contains("Data de Criação</label>"))
                    {
                        var datax = pgLinhas[i + 2].Split('"');

                        // a.DataCriacao = TextoToDate(bs.FormElements["f_017"] + " " + datax[21]);
                    }
                    if (l.Contains("Data Prevista de Conclusão</label>"))
                    {
                        var datax = pgLinhas[i + 2].Split('"');
                        // a.DataPrevistaConclusao = TextoToDate(bs.FormElements["f_019"] + " " + datax[21]);
                    }
                    if (l.Contains("Data de Conclusão</label>"))
                    {
                        var datax = pgLinhas[i + 2].Split('"');
                        var hora = datax[21];
                        // if (hora.Contains(":"))
                        //   a.DataConclusao = TextoToDate(bs.FormElements["f_021"] + " " + datax[21]);
                    }
                    if (l.Contains("Modificado por</label>"))
                    {//' wrapper='true'>Viviane Moura</span></span></li></ul></div>
                     // a.ModificadoPor = pgLinhas[i + 1].Replace("' wrapper='true'>", "").Replace("</span></span></li></ul></div>", "").Trim();

                    }
                    if (l.Contains("Descrição</label>"))
                    {//' wrapper='true'>Viviane Moura</span></span></li></ul></div>
                     //   a.RespostaAoCliente = ("<label " + l).InnerText();

                    }
                    if (l.Contains("for=\"ownerid\">Proprietário"))
                    {//' wrapper='true'>Viviane Moura</span></span></li></ul></div>
                     //   a.Proprietario = pgLinhas[i + 1].Replace("' wrapper='true'>", "").Replace("</span></span></li></ul></div>", "").Trim();

                    }
                    //for="ownerid">Proprietário
                    //Descrição</label>
                    //[20] = {[f_021, 30/07/2014]}
                }
                // a.Anotacaos = GetAnotacoes_WEB(pgDetalhe, oID, a.NumeroOcorrencia);

            }
            catch (Exception)
            {

                //   throw;
            }
            return a;

        }
        public List<Anexo> GetAnexos(string oID, bool somenteDados = false)
        {
            List<Anexo> anexos = new List<Anexo>();
            try
            {


                if (!_authenticated)
                    Autenticar();
                _authenticated = false;
                bs.UseCredentials = true;
                bs.EncodingTexto = "ISO-8859-1";
                bs.Credentials = new System.Net.NetworkCredential(_user, _user);

                bs.IsXMLPost = true;
                bs.XmlToPost = GetXmlQyeryComplementos(oID);
                var xmlData = bs.PostJs("http://dynamics.caixaseguros.intranet:5555/CRMCAD/AppWebServices/AppGridWebService.ashx?operation=Refresh");
                var htmlData = xmlData
                    .Replace("<tr", Environment.NewLine + "<tr")
                    .Replace("</tr>", Environment.NewLine + "</tr>")
                    .Replace("<td", Environment.NewLine + "<td")
                    .Replace("</td>", Environment.NewLine + "</td>")
                    .Replace("<SPAN", Environment.NewLine + "<SPAN")
                    .Replace("</SPAN>", Environment.NewLine + "</SPAN>")
                    .Replace("<a href", Environment.NewLine + "<a href")
                    .Replace("</a>", Environment.NewLine + "</a>");


                var linhas = htmlData.ToLines();
                var fx = new Fixer();
                for (int i = 0; i < linhas.Count; i++)
                {
                    var l = linhas[i].ToLower();
                    if (l.Contains("5555/upload"))
                    {
                        Anexo a = new Anexo();
                        a.ContentType = "";
                        a.Url = l.InnerText();
                        a.AlteradoPor = linhas[i + 16].InnerText();
                        a.NU_ASC = linhas[i + 32].InnerText();
                        if (string.IsNullOrEmpty(a.NU_ASC))
                            a.NU_ASC = linhas[i + 41].InnerText();
                        a.OID = oID;
                        // a.Raw = bs.Download(a.Url,"");
                        a.RTFData = bs.GetRTF(a.Url);
                        a.ContentType = bs.LastContentType;
                        if (a.ContentType.Contains("application/msword"))
                        {
                            a.Comunicado = fx.ParseComunicadoFromDoc(a.RTFData);
                        }
                        else
                        {
                            a.Comunicado = fx.HtmlToComunicado(a.RTFData); //HtmlToComunicado(a.RTFData);
                        }
                        a.Comunicado.CanalEntrada = 1;
                        if (somenteDados)
                            a.RTFData = null;
                        if (a.Comunicado != null)
                        {
                            a.Comunicado.LinkArquivo = a.Url;
                            a.Comunicado.NU_ASC = a.NU_ASC;
                            a.Comunicado.OID = a.OID;

                        }
                        anexos.Add(a);
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Falha ao recuperar anexos da ocorrencia ID:" + oID + " " + ex.Message);
            }

            return anexos;

        }
        public static String TrimStartZ(string inp, string chars)
        {
            while (chars.Contains(inp[0]))
            {
                inp = inp.Substring(1);
            }

            return inp;
        }

        public List<ASCDetails> GetASCDataDetail(int maxData = 0)
        {
            List<Anexo> lista = new List<Anexo>();
            var oid = string.Empty;

            var ocorrencias = GetOcorrencias();
            var nlista = ocorrencias.Where(s => s.ReferenteA.Contains("Comunicado")).ToList();

            //var primeira = ocorrencias.FirstOrDefault(); nlista.Count
            int maxIterations = maxData > 0 ? maxData : nlista.Count;
            for (int i = 0; i < maxIterations; i++)
            {
                var com = nlista[i];
                oid = com.OID;

                var anexos = GetAnexos(oid);
                foreach (var a in anexos)
                {
                    a.Comunicado.NU_ASC = com.NumeroOcorrencia;
                    a.Comunicado.QID = com.QueueItemID;
                    a.Comunicado.OID = oid;
                    a.Origem = com.EnunReferenteA.ToString();

                }
                lista.AddRange(anexos);
            }
            var joined = from c in nlista
                         join a in lista
                         on c.OID equals a.Comunicado.OID
                         select new ASCDetails
                         {
                             Fila = c.Fila,
                             ASC = c.NumeroOcorrencia,
                             Titulo = c.Titulo,
                             ReferenteA = c.ReferenteA,
                             Status = c.Status,
                             StatusAtividade = c.StatusAtividade,
                             CanalEntrada = c.CanalEntrada,
                             CPFCNPJ = c.CPFCNPJ,
                             NomeCliente = c.NomeCliente,
                             DataConclusao = c.DataConclusao,
                             DataCriacao = c.DataCriacao,
                             DataPrevistaConclusao = c.DataPrevistaConclusao,
                             AnexoAlteradoPor = a.AlteradoPor,
                             LinkArquivoComunicado = a.Url,
                             Empresa = a.Comunicado.Comunicado.Empresa.Nome,//.DS_NOME_EMPRESA,
                             CNPJEmpresa = a.Comunicado.Comunicado.Empresa.Cnpj,//a.Comunicado.NU_CNPJ_EMPRESA,
                             NomeSegurado = a.Comunicado.Comunicado.Segurado.Nome,// a.Comunicado.DS_NOME_SEGURADO,
                             CPFSegurado = a.Comunicado.Comunicado.Segurado.Cpf,//a.Comunicado.NU_CPF_SEGURADO,
                             DataNascimentoSegurado = a.Comunicado.Comunicado.Segurado.Nascimento,//a.Comunicado.DT_NASCIMENTO_SEGURADO,
                             SexoSegurado = a.Comunicado.Comunicado.Segurado.Sexo,// a.Comunicado.DS_SEXO_SEGURADO,
                             TipoComunicante = a.Comunicado.Comunicado.Comunicante.Tipo,// a.Comunicado.DS_TIPO_COMUNICANTE,
                             MatriculaComunicante = a.Comunicado.Comunicado.Comunicante.Matricula,// a.Comunicado.NU_MATRICULA_COMUNICANTE,
                             NomeComunicante = a.Comunicado.Comunicado.Comunicante.Nome,// a.Comunicado.DS_NOME_COMUNICANTE,

                             FoneComunicante = a.Comunicado.Comunicado.Comunicante.Foneddd != null ? a.Comunicado.Comunicado.Comunicante.Foneddd + " " + a.Comunicado.Comunicado.Comunicante.Fonenum : "",

                             NomeReclamante = a.Comunicado.Comunicado.Reclamante.Nome,// a.Comunicado.DS_NOME_RECLAMANTE,
                             CPFReclamante = a.Comunicado.Comunicado.Reclamante.Cpf,// a.Comunicado.NU_CPF_RECLAMANTE,
                             ParentescoReclamante = a.Comunicado.Comunicado.Reclamante.Parentesco,// a.Comunicado.DS_PARENTESCO_RECLAMANTE,
                             EmailPessoalReclamante = a.Comunicado.Comunicado.Reclamante.Emailpes,// a.Comunicado.DS_EMAIL_PESSOAL_RECLAMANTE,
                             EmailComercialReclamante = a.Comunicado.Comunicado.Reclamante.Emailcom,// a.Comunicado.DS_EMAIL_COMERCIAL_RECLAMANTE,
                             EnderecoReclamante = a.Comunicado.Comunicado.Reclamante.Endereco,// a.Comunicado.DS_ENDERECO_RECLAMANTE,
                             BairroReclamante = a.Comunicado.Comunicado.Reclamante.Bairro,// a.Comunicado.DS_BAIRRO_RECLAMANTE,
                             CidadeReclamante = a.Comunicado.Comunicado.Reclamante.Cidade,// a.Comunicado.DS_CIDADE_RECLAMANTE,
                             CEPReclamante = a.Comunicado.Comunicado.Reclamante.Cep,// a.Comunicado.NU_CEP_RECLAMANTE,
                             UFReclamante = a.Comunicado.Comunicado.Reclamante.Uf,// a.Comunicado.DS_UF_RECLAMANTE,
                             FoneResidencialReclamante = a.Comunicado.Comunicado.Reclamante.Foneresddd != null ? a.Comunicado.Comunicado.Reclamante.Foneresddd + " " + a.Comunicado.Comunicado.Reclamante.Fonteresnum : "",
                             FoneComercialReclamante = a.Comunicado.Comunicado.Reclamante.Fonecomddd != null ? a.Comunicado.Comunicado.Reclamante.Fonecomddd + " " + a.Comunicado.Comunicado.Reclamante.Fonecomnum : "",
                             CelularReclamante = a.Comunicado.Comunicado.Reclamante.Fonecelddd != null ? a.Comunicado.Comunicado.Reclamante.Fonecelddd + " " + a.Comunicado.Comunicado.Reclamante.Fonecelnum : "",
                             CodCoberturaSinistro = a.Comunicado.Comunicado.Sinistro.Cobertura,// a.Comunicado.COD_COBERTURA_SINISTRO,
                             CoberturaSinistro = a.Comunicado.Comunicado.Sinistro.Cobertura,// a.Comunicado.DS_COBERTURA_SINISTRO,
                             OutrosSinistros = a.Comunicado.Comunicado.Sinistro.Outrosinistro,// a.Comunicado.DS_OUTROSINISTRO_SINISTRO,
                             DataDoAcidente = a.Comunicado.Comunicado.Sinistro.Dataacidente,//  a.Comunicado.DT_ACIDENTE_SINISTRO,
                             DataDoSinistro = a.Comunicado.Comunicado.Sinistro.Datasinistro,// a.Comunicado.DT_SINISTRO,
                             ConjugeOuFilho = a.Comunicado.Comunicado.Sinistro.Conjugeoufilho,// a.Comunicado.DS_CONJUGEOUFILHO_SINISTRO,
                             CPFSinistrado = a.Comunicado.Comunicado.Sinistro.Cpf,// a.Comunicado.NU_CPF_SINISTRO,
                             NascimentoSinistrado = a.Comunicado.Comunicado.Sinistro.Nascimento,// a.Comunicado.DT_NASCIMENTO_SINISTRO,
                             BreveRelato = a.Comunicado.Comunicado.Sinistro.Relato,// a.Comunicado.DS_RELATO_SINISTRO,
                             EmpregadoCaixa = a.Comunicado.Comunicado.Sinistro.Empregadocaixa,// a.Comunicado.ST_EMPREGADO_CAIXA_SINISTRO,
                             EmpregadoFUNCEF = a.Comunicado.Comunicado.Sinistro.Empregadofuncef,// a.Comunicado.ST_EMPREGADO_FUNCEF_SINISTRO,
                             Produto = a.Comunicado.Comunicado.Produtos.Produto != null ? a.Comunicado.Comunicado.Produtos.Produto.FirstOrDefault().Produtonome : "",
                             Certificado = a.Comunicado.Comunicado.Produtos.Produto != null ? a.Comunicado.Comunicado.Produtos.Produto.FirstOrDefault().Certificados.Certificado : "",
                             DataComunicado = a.Comunicado.Comunicado.Finalizacao.Datacomunicado,
                             AgenteRelacionamento = a.Comunicado.Comunicado.Finalizacao.Nomeagenterelacionamento,
                             ArquivoDOCX = a.Comunicado.ExtensaoDocx,
                             FileData = a.RTFData
                             ,
                             FormatoInvalido = a.Comunicado.FormatoInvalido
                         };
            return joined.ToList();
        }


        public static string UnescapeHex(string data)
        {
            return Encoding.UTF8.GetString(Array.ConvertAll(Regex.Unescape(data).ToCharArray(), c => (byte)c));
        }
        public bool SetWorker(string qID, string WorkerID)
        {
            bool saida = false;
            bs.UseCredentials = true;
            string crmRPCToken = "";
            string timestamp = "";
            bs.Credentials = new System.Net.NetworkCredential(_user, _password;
            if (!_authenticated)
                Autenticar();

            var pgUserToWork = bs.Get("http://dynamics.caixaseguros.intranet:5555/CRMCAD/_grid/cmds/dlg_queueitemworkon.aspx?iObjType=2029&iTotal=1");
            var pgUserLines = pgUserToWork.ToLines();
            foreach (var p in pgUserLines)
            {
                if (p.Contains("x2fDLG_QUEUEITEMWORKON.ASPX"))
                {
                    var vTokens = p.Split('\'');
                    var vTimeS = p.Split('"');
                    crmRPCToken = UnescapeHex(vTokens[3]);
                    timestamp = vTimeS[1];
                    break;

                }
            }

            var urlSetWorker = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/_grid/cmds/dlg_queueitemworkon.aspx?iId=%7b" + qID + "%7d&iIndex=0&iObjType=2029&iTotal=1&isworkerme=0&workerid=%7b" + WorkerID + "%7d&workertype=8";
            bs.extraHeader = new Dictionary<string, string>();
            bs.extraHeader.Add("CRMWRPCToken", crmRPCToken);
            bs.extraHeader.Add("CRMWRPCTokenTimeStamp", timestamp);
            bs.IsXMLPost = true;
            bs.XmlToPost = "<node/>";
            var res = bs.PostJs(urlSetWorker);
            if (res.Contains("<ok"))
                saida = true;
            return saida;

        }
        public bool WorkOn(string Id, bool devolver = false)
        {
            bool saida = false;
            bs.UseCredentials = true;
            string crmRPCToken = "";
            string timestamp = "";
            bs.Credentials = new System.Net.NetworkCredential(_user, _password);
            if (!_authenticated)
                Autenticar();

            var pgUserToWork = bs.Get("http://dynamics.caixaseguros.intranet:5555/CRMCAD/_grid/cmds/dlg_queueitemworkon.aspx?iObjType=2029&iTotal=1");
            var pgUserLines = pgUserToWork.ToLines();
            foreach (var p in pgUserLines)
            {
                if (p.Contains("x2fDLG_QUEUEITEMWORKON.ASPX"))
                {
                    //_aWrpcTokens['\x2fCRMCAD\x2f_GRID\x2fCMDS\x2fDLG_QUEUEITEMWORKON.ASPX']={Token: 'O9NgtaK9EeeXBgAVXdJSjYtS5an9W3U4qd5ykBd4f2p\x2fVqXxKcEspwQdKoSNlMbY', Timestamp: "636426330952993573"};
                    var vTokens = p.Split('\'');
                    var vTimeS = p.Split('"');
                    crmRPCToken = UnescapeHex(vTokens[3]);
                    timestamp = vTimeS[1];
                    break;

                }
            }
            bs.extraHeader = new Dictionary<string, string>();
            bs.extraHeader.Add("CRMWRPCToken", crmRPCToken);
            bs.extraHeader.Add("CRMWRPCTokenTimeStamp", timestamp);
            string urlWok = "";

            if (devolver)
                urlWok = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/_grid/cmds/dlg_queueitemrelease.aspx?iId=%7b" + Id + "%7d&iIndex=0&iObjType=2029&iTotal=1";
            else
                urlWok = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/_grid/cmds/dlg_queueitemworkon.aspx?iId=%7b" + Id + "%7d&iIndex=0&iObjType=2029&iTotal=1&isworkerme=1";
            bs.IsXMLPost = true;
            bs.XmlToPost = "<node/>";
            var res = bs.PostJs(urlWok);
            if (res.Contains("<ok"))
                saida = true;
            return saida;
        }
        public Ocorrencia GetOcorrenciaSinistroOnline(string oid)
        {
            bs.UseCredentials = true;
            bs.Credentials = new System.Net.NetworkCredential(_user, _password);
            //  if (!_authenticated)
            Autenticar();
            Ocorrencia a = new Ocorrencia();
            a.OID = oid;
            var produto = string.Empty;
            try
            {
                bs.EncodingTexto = "utf-8";
                var url = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/userdefined/edit.aspx?etc=112&id=%7b" + oid + "%7d&pagemode=iframe&preloadcache=1462539717002";
                var pgDetalhe = bs.Get(url);
                //  if (!pgDetalhe.Contains("Sinistro Online")) return null;
                if (!bs.FormElements.ContainsKey("crmFormOriginalXml"))
                {

                    return null;
                }
                var xmlDataItem = bs.FormElements["crmFormOriginalXml"];
                xmlDataItem = HttpUtility.HtmlDecode(xmlDataItem).ToString();
                var incidente = Fixer.DeserializeFromXmlString<Incident>(xmlDataItem);

                if (!incidente.Codek_processo_atendimentoid.Name.Contains("DIRVI - SINISTRO - Comunicado de Sinistro - 06 Online"))
                    if (!incidente.Codek_processo_atendimentoid.Name.Contains("DIRVI - SINISTRO - Comunicado de Sinistro - 07 Regulação Manual"))
                    {
                        var o = new Ocorrencia();
                        o.OID = oid;
                        o.CPFCNPJ = incidente.Gcs_cpfcnpjcliente;
                        o.NomeCliente = incidente.Gcs_nomebeneficiario;
                        o.NumeroOcorrencia = incidente.Ticketnumber;

                        o.Anexos = GetAnexos(oid, true);
                        if (o.Anexos == null) o.Anexos = new List<Anexo>();

                        if (o.Anexos.Count > 0)
                        {
                            o.Anexos.First().NU_ASC = incidente.Ticketnumber;
                            o.Anexos.First().OID = oid;
                            o.Anexos.First().Origem = "Sinistro Online";
                        }
                        return o;
                    }

                var cRoot = new ComunicadoRTF();
                var c = new Comunicado();
                c.StSinistroOnline = true;
                c.Id = DateTime.Now.Ticks.ToString();
                c.Comunicante = new Comunicante { Ref = "3", Tipo = "", Nome = "", Foneddd = "", Fonenum = "", Matricula = "" };
                c.Empresa = new Empresa { Ref = "1", Cnpj = "", Nome = "" };
                c.Contrato = new Contrato
                {
                    Ref = "7",
                    Numero = "",
                    Adesao = "",
                    Cartacredito = "",
                    Prazo = "",
                    Consorcio = new Consorcio { Grupo = "", Cota = "" },
                    Enderecoimovel = "",
                    Bairro = "",
                    Cidade = "",
                    Cep = "",
                    Uf = "",
                    Mesanori = "",
                    Danosmateriais = new Danosmateriais
                    {
                        Pontorefimovel = "",
                        Horariovisita = "",
                        Fonecontato = "",
                        Enderecoalternativoexiste = "",
                        Enderecoalternativo = "",
                        Valorindenizacao = "",
                        Localchaves = "",
                        Instututofiliacao = ""
                    }
                };

                var finalizacao = new Finalizacao();
                finalizacao.Ref = "8";
                finalizacao.Datacomunicado = incidente.Createdon.Date;
                finalizacao.Nomeagenterelacionamento = incidente.Gcs_criador_portalid.Name.Trim();
                c.Finalizacao = finalizacao;

                c.Produtos = new Produtos
                {
                    Ref = "6",
                    Produto = new List<Produto>
                     {
                          new Produto  {  Produtonome =incidente.Gcs_produto,  Certificados = new Certificados{  Certificado = incidente.Gcs_certificado  }  },
                          new Produto  {  Produtonome="",  Consorcios = new Consorcios{ Consorcio = new Consorcio{ Cota="", Grupo ="" } }},
                          new Produto  {  Produtonome="",  Consorcios = new Consorcios{ Consorcio = new Consorcio{ Cota="", Grupo ="" } }},
                          new Produto  {  Produtonome="", Certificados = new Certificados{ Certificado="" } },
                          new Produto  {  Produtonome="", Certificados = new Certificados{ Certificado="" } },
                     }
                };

                c.Reclamante = new Reclamante();
                var rec = new Reclamante();
                rec.Ref = "4";
                var endereco = incidente.Gcs_end_beneficiario;
                if (endereco != null)
                {
                    var vend = endereco.Split(',');
                    if (vend.Length >= 7)
                    {
                        rec.Endereco = vend[0].Trim();
                        rec.Bairro = vend[3].Contains(':') ? vend[3].Split(':')[1].Trim() : vend[3].Trim();
                        rec.Cep = vend[6].Contains(':') ? vend[6].Split(':')[1].Trim() : vend[6].Trim();
                        rec.Cidade = vend[4].Contains(':') ? vend[4].Split(':')[1].Trim() : vend[4].Trim();
                        rec.Uf = vend[5].Contains(':') ? vend[5].Split(':')[1].Trim() : vend[5].Trim();
                    }
                    else
                    {
                        rec.Bairro = "";
                        rec.Cidade = "";
                        rec.Uf = "";
                        rec.Endereco = endereco;
                        rec.Cep = vend.Last().Trim();
                    }
                }
                rec.Nome = incidente.Gcs_nomebeneficiario.Trim();
                rec.Cpf = incidente.Gcs_cpf_beneficiario.Trim();
                rec.Parentesco = incidente.Gcs_parentesco.Trim();
                rec.Emailpes = string.IsNullOrEmpty(incidente.Gcs_email) ? "" : incidente.Gcs_email.TrimX();
                rec.Emailcom = "";
                if (incidente.Gcs_telefone != null)
                {
                    rec.Foneresddd = incidente.Gcs_telefone.Substring(0, 2);
                    rec.Fonteresnum = incidente.Gcs_telefone.Substring(2, incidente.Gcs_telefone.Length - 2).TrimX();
                }
                rec.Fonecomddd = "";
                rec.Fonecomnum = "";
                rec.Fonecelddd = "";
                rec.Fonecelnum = "";

                c.Reclamante = rec;

                c.Segurado = new SeguradoComunicado();
                var segurado = new SeguradoComunicado();
                segurado.Ref = "2";
                segurado.Cpf = incidente.Gcs_cpfcnpjcliente;
                segurado.Nascimento = incidente.Gcs_data_nascimento.Date;
                segurado.Nome = incidente.Gcs_nome.TrimX();
                string sexo = GetSexo(incidente.Gcs_nome.TrimX());
                segurado.Sexo = sexo;
                c.Segurado = segurado;

                c.Sinistro = new SinistroComunicado();
                var sinistro = new SinistroComunicado();
                sinistro.Ref = "5";
                sinistro.Cobertura = string.IsNullOrEmpty(incidente.Gcs_cobertura) ? "" : incidente.Gcs_cobertura.TrimX();
                sinistro.Codcobertura = incidente.Gcs_cobertura.TrimX();
                sinistro.Conjugeoufilho = incidente.Gcs_nomebeneficiario.TrimX();
                sinistro.Cpf = incidente.Gcs_cpfcnpjcliente;
                sinistro.Dataacidente = incidente.Gcs_data_sinistro;
                sinistro.Datasinistro = incidente.Gcs_data_sinistro;
                sinistro.Empregadocaixa = "Não";
                sinistro.Empregadofuncef = "Não";
                sinistro.Nascimento = incidente.Gcs_data_nascimento.Date;
                sinistro.Outrosinistro = "";
                sinistro.Relato = incidente.Description.TrimX();

                c.Sinistro = sinistro;

                cRoot.Comunicado = (ComunicadoXML)c;
                cRoot.OID = oid;
                cRoot.CanalEntrada = 2;
                //cRoot.QID = incidente.
                cRoot.NU_ASC = incidente.Ticketnumber;
                var anexoOnline = new Anexo();
                anexoOnline.AlteradoPor = "SINISTRO ONLINE";
                anexoOnline.Comunicado = new ComunicadoRTF();
                anexoOnline.Comunicado = cRoot;
                anexoOnline.ContentType = "plaintext";
                anexoOnline.NU_ASC = incidente.Ticketnumber;
                anexoOnline.OID = oid;
                anexoOnline.RTFData = "";
                anexoOnline.Url = "";

                a.Anexos = new List<Anexo>();
                a.Anexos.Add(anexoOnline);
                a.NumeroOcorrencia = incidente.Ticketnumber;
                a.CPFCNPJ = incidente.Gcs_cpfcnpjcliente;

                
            }
            catch (Exception ex)
            {
                //System.Diagnostics.EventLog.WriteEntry("ASCBotService", "Falha ao ler sinistro online " + ex.ToString(), System.Diagnostics.EventLogEntryType.Warning);
                return null;
            }
            return a;
        }

        private string GetSexo(string nome)
        {
            string sexo = "Masculino";
            try
            {
                var nomes = new List<string>();// Properties.Resources.feminino.ToLines();
                var primeiroNome = nome.Split(' ')[0].ToUpper();
                if (nomes.Any(n => n == primeiroNome))
                    sexo = "Feminino";
            }
            catch (Exception)
            {

                return sexo;
            }

            return sexo;
        }

        public OcorrenciaOuvidoria GetOcorrenciaOuvidoria(string nuOcorrencia, bool fechada = false)
        {
            OcorrenciaOuvidoria oo = new OcorrenciaOuvidoria();

            try
            {

                if (!_authenticated)
                    Autenticar();
                bs.IsXMLPost = true;
                bs.EncodingTexto = "utf-8";
                bs.XmlToPost = GetXmlQyeryOuvidoria(nuOcorrencia, fechada);
                var xmlData = bs.PostJs("http://dynamics.caixaseguros.intranet:5555/CRMCAD/AppWebServices/AppGridWebService.ashx?operation=Refresh");

                var htmlData = xmlData
                    .Replace("<tr", Environment.NewLine + "<tr")
                    .Replace("</tr>", Environment.NewLine + "</tr>")
                    .Replace("<td", Environment.NewLine + "<td");
                if (htmlData.Contains("Não há registros disponíveis nesta exibição"))
                {
                    oo.MensagemDeErro = "Ouvidoria não localizada";
                    oo.CodErro = 1;
                    return oo;
                    //throw new Exception("Ouvidoria não localizada");
                }
                var linhas = htmlData.ToLines();

                if (fechada)
                    oo = ParsePageFechada(linhas);
                else
                    oo = ParsePagePendente(linhas);

            }
            catch (Exception ex)
            {
                oo.CodErro = 2;
                oo.MensagemDeErro = ex.ToString();
                // throw;
            }
            return oo;

        }

        public List<SubisidioOvidoria> GetSubsidios(string nuOcorrencia)
        {
            List<SubisidioOvidoria> subs = new List<SubisidioOvidoria>();
            SubisidioOvidoria oo = new SubisidioOvidoria();

            try
            {

                if (!_authenticated)
                    Autenticar();
                bs.IsXMLPost = true;
                bs.EncodingTexto = "utf-8";
                bs.XmlToPost = GetXmlQyerySubsidio(nuOcorrencia);
                var xmlData = bs.PostJs("http://dynamics.caixaseguros.intranet:5555/CRMCAD/AppWebServices/AppGridWebService.ashx?operation=Refresh");



                var htmlData = xmlData
                    .Replace("<tr", Environment.NewLine + "<tr")
                    .Replace("</tr>", Environment.NewLine + "</tr>")
                    .Replace("<td", Environment.NewLine + "<td");
                if (htmlData.Contains("Não há registros disponíveis nesta exibição"))
                {
                    oo.MensagemDeErro = "Subsidio não localizado";
                    oo.CodErro = 1;
                    subs.Add(oo);
                    return subs;
                    //throw new Exception("Ouvidoria não localizada");
                }
                var linhas = htmlData.ToLines();
                subs = ParseSubsidio(linhas);

            }
            catch (Exception ex)
            {
                oo.CodErro = 2;
                oo.MensagemDeErro = ex.ToString();
                // throw;
            }
            return subs;

        }

        private OcorrenciaOuvidoria ParsePageFechada(List<string> linhas)
        {
            OcorrenciaOuvidoria oo = new OcorrenciaOuvidoria();
            for (int i = 0; i < linhas.Count; i++)
            {
                var l = linhas[i];
                if (l.Contains("class=\"ms-crm-List-Row\" oid="))
                {
                    var idInformanteRoots = linhas[i + 4].InnerText();
                    var idInformanteParsed = FormatCPFCJPJ(idInformanteRoots);
                    oo.kpInformante = new PessoaOuvidoria
                    {
                        sCpfCNPJ = idInformanteParsed,
                        sNome = linhas[i + 13].InnerText(),
                        kpTipoPessoa = GetTipoPessoa(idInformanteParsed)

                    };

                    var idClienteRoots = linhas[i + 33].InnerText();
                    var idClienteParsed = FormatCPFCJPJ(idClienteRoots);
                    oo.kpCliente = new PessoaOuvidoria
                    {
                        sCpfCNPJ = idClienteParsed,
                        sNome = linhas[i + 12].InnerText(),
                        kpTipoPessoa = GetTipoPessoa(idClienteParsed)

                    };


                    var vOid = l.Split('"');

                    oo.oType = vOid[5];
                    oo.oID = vOid[3].Replace("{", "").Replace("}", "");

                    //  oo.oQueueItemID = vOid[9].Replace("{", "").Replace("}", "");
                    oo.dInseridoNaFila = DateTime.Parse(linhas[i + 5].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));

                    oo.nOcorrencia = linhas[i + 3].InnerText();



                    if (!string.IsNullOrEmpty(linhas[i + 5].InnerText()))
                        oo.dCadastroOcorrencia = DateTime.Parse(linhas[i + 5].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    if (!string.IsNullOrEmpty(linhas[i + 6].InnerText()))
                        oo.dRecebimento = DateTime.Parse(linhas[i + 6].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));

                    oo.kpTipoOcorrencia = new TipoOcorrenciaOuvidoria
                    {
                        sDescricao = linhas[i + 7].InnerText(),
                        iCod = GetCodTipoOcorrencia(linhas[i + 7].InnerText())
                    };
                    oo.iIdTipoOcorrencia = GetCodTipoOcorrencia(linhas[i + 7].InnerText());

                    oo.kpFormaContatoOuvidoria = new TipoContatoOuvidoria
                    {
                        sDescricao = linhas[i + 8].InnerText(),
                        iCod = GetCodTipoContato(linhas[i + 8].InnerText())
                    };
                    oo.iIdFormaContato = GetCodTipoContato(linhas[i + 8].InnerText());

                    oo.kpOrigemOcorrencia = new OrigemOcorrenciaOuvidoria
                    {
                        sDescricao = linhas[i + 10].InnerText(),
                        iCod = GetCodOrigemOcorrencia(linhas[i + 10].InnerText())
                    };
                    oo.iIdOrigemOcorrencia = GetCodOrigemOcorrencia(linhas[i + 10].InnerText());

                    oo.sProtocolo = linhas[i + 9].InnerText();


                    if (!string.IsNullOrEmpty(linhas[i + 11].InnerText()))
                        oo.dVencimentoPrevisto = DateTime.Parse(linhas[i + 11].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));


                    if (!string.IsNullOrEmpty(linhas[i + 14].InnerText()))
                        oo.dMulta = DateTime.Parse(linhas[i + 14].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    oo.sTrabalhadoPor = linhas[i + 21].InnerText();
                    oo.sClientePF = linhas[i + 15].InnerText();
                    oo.sClientePJ = linhas[i + 16].InnerText();
                    if (!string.IsNullOrEmpty(linhas[i + 17].InnerText()))
                        oo.dModificacao = DateTime.Parse(linhas[i + 17].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    oo.sEmail = linhas[i + 18].InnerText();
                    oo.sInformantePF = linhas[i + 19].InnerText();
                    oo.sInformantePJ = linhas[i + 20].InnerText();
                    oo.sModificadoPor = linhas[i + 21].InnerText();
                    //numero gcs = linhas[i + 25].InnerText();
                    //oo.sPertenceACaixa = linhas[i + 26].InnerText();
                    oo.sProduto = linhas[i + 24].InnerText();
                    //proprietario = linhas[i + 28].InnerText();
                    oo.sOcorrenciaComCliente = linhas[i + 26].InnerText();
                    // if (!string.IsNullOrEmpty(linhas[i + 30].InnerText()))
                    //     oo.dCriadoEm =  DateTime.Parse(linhas[i + 30].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    oo.sControleStatus = linhas[i + 28].InnerText();
                    oo.sStatus = linhas[i + 29].InnerText();
                    oo.sRazaoStatus = linhas[i + 30].InnerText();
                    oo.sStatusOcorrencia = linhas[i + 31].InnerText();
                    oo.dCriacaoASC = oo.dCadastroOcorrencia;
                    //if (!string.IsNullOrEmpty(linhas[i + 35].InnerText()))
                    //  oo.dCriacaoASC = DateTime.Parse(linhas[i + 35].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));

                    if (!string.IsNullOrEmpty(linhas[i + 32].InnerText()))
                        oo.dVencimentoLegal = DateTime.Parse(linhas[i + 32].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));




                    break;
                }
            }

            return oo;
        }
        private List<SubisidioOvidoria> ParseSubsidio(List<string> linhas)
        {
            //169657
            List<SubisidioOvidoria> subs = new List<SubisidioOvidoria>();
            for (int i = 0; i < linhas.Count; i++)
            {
                var l = linhas[i];
                if (l.Contains("class=\"ms-crm-List-Row\" oid="))
                {
                    SubisidioOvidoria sub = new SubisidioOvidoria();
                    var vOid = l.Split('"');
                    sub.oID = vOid[3].Replace("{", "").Replace("}", "");
                    sub.Type = vOid[5];
                    sub.Numero = linhas[i + 3].InnerText();
                    if (!string.IsNullOrEmpty(linhas[i + 4].InnerText()))
                        sub.dCriacao = DateTime.Parse(linhas[i + 4].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    sub.iStatusDoSubsidio = linhas[i + 5].InnerText();
                    if (!string.IsNullOrEmpty(linhas[i + 6].InnerText()))
                        sub.dPrazoDeTratamento = DateTime.Parse(linhas[i + 6].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    if (!string.IsNullOrEmpty(linhas[i + 7].InnerText()))
                        sub.iDiasDeAtraso = int.Parse(linhas[i + 7].InnerText());
                    sub.sGerenciaArea = linhas[i + 8].InnerText();
                    sub.sUltimaProgramacao = linhas[i + 9].InnerText();
                    sub.sProprietario = linhas[i + 10].InnerText();
                    sub.sClassificacaoDaOcorrencia = linhas[i + 11].InnerText();
                    sub.sAvaliacaoReprovada = linhas[i + 12].InnerText();
                    if (!string.IsNullOrEmpty(linhas[i + 13].InnerText()))
                        sub.bDevolverPedidoAOuvidoria = (linhas[i + 13].InnerText() == "Sim");
                    sub.sConclusao = linhas[i + 14].InnerText();
                    if (!string.IsNullOrEmpty(linhas[i + 15].InnerText()))
                        sub.dResposta = DateTime.Parse(linhas[i + 15].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    if (!string.IsNullOrEmpty(linhas[i + 16].InnerText()))
                        sub.dConclusao = DateTime.Parse(linhas[i + 16].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    if (!string.IsNullOrEmpty(linhas[i + 17].InnerText()))
                        sub.dInicio = DateTime.Parse(linhas[i + 17].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    sub.sElaboradorSubsidio = linhas[i + 18].InnerText();
                    sub.sElaboradorOuvidoria = linhas[i + 19].InnerText();
                    if (!string.IsNullOrEmpty(linhas[i + 20].InnerText()))
                        sub.dInicioReal = DateTime.Parse(linhas[i + 20].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    sub.sPrioridade = linhas[i + 21].InnerText();
                    sub.sRazaoStatus = linhas[i + 22].InnerText();
                    sub.sRespostaDoSubsidio = linhas[i + 23].InnerText();
                    sub.sStatusAtividade = linhas[i + 24].InnerText();
                    sub.sSubsidioReprovado = linhas[i + 25].InnerText();

                    subs.Add(sub);
                }
            }

            return subs;
        }
        private OcorrenciaOuvidoria ParsePagePendente(List<string> linhas)
        {
            OcorrenciaOuvidoria oo = new OcorrenciaOuvidoria();
            for (int i = 0; i < linhas.Count; i++)
            {
                var l = linhas[i];
                if (l.Contains("class=\"ms-crm-List-Row\" oid="))
                {
                    var idInformanteRoots = linhas[i + 6].InnerText();
                    var idInformanteParsed = FormatCPFCJPJ(idInformanteRoots);
                    oo.kpInformante = new PessoaOuvidoria
                    {
                        sCpfCNPJ = idInformanteParsed,
                        sNome = linhas[i + 15].InnerText(),
                        kpTipoPessoa = GetTipoPessoa(idInformanteParsed)

                    };

                    var idClienteRoots = linhas[i + 37].InnerText();
                    var idClienteParsed = FormatCPFCJPJ(idClienteRoots);
                    oo.kpCliente = new PessoaOuvidoria
                    {
                        sCpfCNPJ = idClienteParsed,
                        sNome = linhas[i + 14].InnerText(),
                        kpTipoPessoa = GetTipoPessoa(idClienteParsed)

                    };


                    var vOid = l.Split('"');

                    oo.oType = vOid[5];
                    oo.oID = vOid[3].Replace("{", "").Replace("}", "");

                    oo.oQueueItemID = vOid[9].Replace("{", "").Replace("}", "");
                    oo.dInseridoNaFila = DateTime.Parse(linhas[i + 4].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));

                    oo.nOcorrencia = linhas[i + 5].InnerText();



                    if (!string.IsNullOrEmpty(linhas[i + 7].InnerText()))
                        oo.dCadastroOcorrencia = DateTime.Parse(linhas[i + 7].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    if (!string.IsNullOrEmpty(linhas[i + 8].InnerText()))
                        oo.dRecebimento = DateTime.Parse(linhas[i + 8].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));

                    oo.kpTipoOcorrencia = new TipoOcorrenciaOuvidoria
                    {
                        sDescricao = linhas[i + 9].InnerText(),
                        iCod = GetCodTipoOcorrencia(linhas[i + 9].InnerText())
                    };
                    oo.iIdTipoOcorrencia = GetCodTipoOcorrencia(linhas[i + 9].InnerText());

                    oo.kpFormaContatoOuvidoria = new TipoContatoOuvidoria
                    {
                        sDescricao = linhas[i + 10].InnerText(),
                        iCod = GetCodTipoContato(linhas[i + 10].InnerText())
                    };
                    oo.iIdFormaContato = GetCodTipoContato(linhas[i + 10].InnerText());

                    oo.kpOrigemOcorrencia = new OrigemOcorrenciaOuvidoria
                    {
                        sDescricao = linhas[i + 12].InnerText(),
                        iCod = GetCodOrigemOcorrencia(linhas[i + 12].InnerText())
                    };
                    oo.iIdOrigemOcorrencia = GetCodOrigemOcorrencia(linhas[i + 12].InnerText());

                    oo.sProtocolo = linhas[i + 11].InnerText();


                    if (!string.IsNullOrEmpty(linhas[i + 13].InnerText()))
                        oo.dVencimentoPrevisto = DateTime.Parse(linhas[i + 13].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));


                    if (!string.IsNullOrEmpty(linhas[i + 16].InnerText()))
                        oo.dMulta = DateTime.Parse(linhas[i + 16].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    oo.sTrabalhadoPor = linhas[i + 17].InnerText();
                    oo.sClientePF = linhas[i + 18].InnerText();
                    oo.sClientePJ = linhas[i + 19].InnerText();
                    if (!string.IsNullOrEmpty(linhas[i + 20].InnerText()))
                        oo.dModificacao = DateTime.Parse(linhas[i + 20].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    oo.sEmail = linhas[i + 21].InnerText();
                    oo.sInformantePF = linhas[i + 22].InnerText();
                    oo.sInformantePJ = linhas[i + 23].InnerText();
                    oo.sModificadoPor = linhas[i + 24].InnerText();
                    //numero gcs = linhas[i + 25].InnerText();
                    oo.sPertenceACaixa = linhas[i + 26].InnerText();
                    oo.sProduto = linhas[i + 27].InnerText();
                    //proprietario = linhas[i + 28].InnerText();
                    oo.sOcorrenciaComCliente = linhas[i + 29].InnerText();
                    // if (!string.IsNullOrEmpty(linhas[i + 30].InnerText()))
                    //     oo.dCriadoEm =  DateTime.Parse(linhas[i + 30].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                    oo.sControleStatus = linhas[i + 31].InnerText();
                    oo.sStatus = linhas[i + 32].InnerText();
                    oo.sRazaoStatus = linhas[i + 33].InnerText();
                    oo.sStatusOcorrencia = linhas[i + 34].InnerText();

                    if (!string.IsNullOrEmpty(linhas[i + 35].InnerText()))
                        oo.dCriacaoASC = DateTime.Parse(linhas[i + 35].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));

                    if (!string.IsNullOrEmpty(linhas[i + 36].InnerText()))
                        oo.dVencimentoLegal = DateTime.Parse(linhas[i + 36].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));




                    break;
                }
            }

            return oo;
        }

        private TipoPessoa GetTipoPessoa(string idParsed)
        {
            TipoPessoa tp = new TipoPessoa();
            var cpfcnpjlimpo = idParsed
                          .Replace(".", "")
                          .Replace("/", "")
                          .Replace("-", "");
            if (cpfcnpjlimpo.Length <= 12)
            {
                tp.iCod = 1;
                tp.sDescricao = "FÍSICA";

            }
            else
            {
                tp.iCod = 2;
                tp.sDescricao = "JURÍDICA";
            }
            return tp;

        }

        private string FormatCPFCJPJ(string Id)
        {
            string formatado = string.Empty;
            var cpfcnpjlimpo = Id
                           .Replace(".", "")
                           .Replace("/", "")
                           .Replace("-", "");

            if (cpfcnpjlimpo.Length <= 12)
            {
                formatado = Convert.ToUInt64(cpfcnpjlimpo.PadLeft(11, '0')).ToString(@"000\.000\.000\-00");
            }
            else
            {

                formatado = Convert.ToUInt64(cpfcnpjlimpo.PadLeft(14, '0')).ToString(@"00\.000\.000\/0000\-00");
            }
            return formatado;
        }

        private int GetCodOrigemOcorrencia(string sOrigemOcorrencia)
        {
            Dictionary<int, string> OrigemOcorrencia = new Dictionary<int, string>()
            {
                {7, "Cliente"},
                {1, "PROCON"},
                {2, "PROCON - Audiência" },
                {8, "PROCON - Impugnação" },
                {3, "PROCON - Multa" },
                {12, "PROCON - Eletrônico" },
                {4, "SUSEP" },
                {5, "BACEN" },
                {9, "ANS" },
                {6, "PAC" },
                {11, "Caixa" },
                {10, "Outros" }
            };

            var cod = OrigemOcorrencia.FirstOrDefault(x => x.Value == sOrigemOcorrencia).Key;
            return cod;
        }

        private int GetCodTipoContato(string sTipoContato)
        {
            Dictionary<int, string> TipoContato = new Dictionary<int, string>()
            {
                {1, "Correspondência"},
                {2, "E-mail"},
                {8, "Mobile"},
                {3, "Ouvidoria - 0800"},
                {4 , "Ouvidoria - CAIXA"},
                {5, "Presencial"},
                {6, "RDR"},
                {7, "Telefone"}
            };
            var cod = TipoContato.FirstOrDefault(x => x.Value == sTipoContato).Key;
            return cod;
        }

        private int GetCodTipoOcorrencia(string sTipoOcorrencia)
        {
            Dictionary<int, string> TipoOcorrencia = new Dictionary<int, string>()
            {
                {1, "Reclamação"},
                {2, "PAC"},
                {3, "Sugestão"},
                {4, "Informação"},
                {5, "Elogio"},
                {6, "Denúncia"},
                {7, "PAS"},
                {8, "Recomendações Internas (UC)"},
                {9, "Acompanhamento"},
                {10, "NIP"},
                {11, "Reincidência"}
             };

            var cod = TipoOcorrencia.FirstOrDefault(x => x.Value == sTipoOcorrencia).Key;
            return cod;
        }

        private string GetXmlQyeryOuvidoria(string nuOcorrencia, bool fechada = false)
        {
            string body = "";
            if (!fechada)
                body = @"<grid><sortColumns>enteredon&#58;0</sortColumns><pageNum>1</pageNum><recsPerPage>250</recsPerPage><dataProvider>Microsoft.Crm.Application.Platform.Grid.GridDataProviderQueryBuilder</dataProvider><uiProvider>Microsoft.Crm.Application.Controls.GridUIProvider</uiProvider><cols/><max>-1</max><refreshAsync>False</refreshAsync><pagingCookie>&#60;cookie page&#61;&#34;1&#34;&#62;&#60;enteredon last&#61;&#34;2017-10-09T15&#58;40&#58;39-03&#58;00&#34; first&#61;&#34;2017-11-17T15&#58;54&#58;33-02&#58;00&#34; &#47;&#62;&#60;queueitemid last&#61;&#34;&#123;51ED1F55-21AD-E711-9706-00155DD2528D&#125;&#34; first&#61;&#34;&#123;D534665D-C0CB-E711-94CA-00155D6CF2F8&#125;&#34; &#47;&#62;&#60;&#47;cookie&#62;</pagingCookie><enableMultiSort>true</enableMultiSort><enablePagingWhenOnePage>true</enablePagingWhenOnePage><initStatements>crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridqueueitemworkerid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_clientepfidnew_ocorrencia_ouvidoria_QueueItems&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_clientepjidnew_ocorrencia_ouvidoria_QueueItems&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_informantepfidnew_ocorrencia_ouvidoria_QueueItems&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_informantepjidnew_ocorrencia_ouvidoria_QueueItems&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidoriamodifiedbynew_ocorrencia_ouvidoria_QueueItems&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidoriaowneridnew_ocorrencia_ouvidoria_QueueItems&#39;&#41;&#41;&#59;&#10;</initStatements><refreshCalledFromRefreshButton>1</refreshCalledFromRefreshButton><totalrecordcount>5000</totalrecordcount><allrecordscounted>false</allrecordscounted><returntotalrecordcount>true</returntotalrecordcount><getParameters></getParameters><parameters><autorefresh>1</autorefresh><isGridFilteringEnabled>1</isGridFilteringEnabled><viewid>&#123;86B3CCF3-F8C2-E711-B9C3-00155D0B5C62&#125;</viewid><viewtype>4230</viewtype><RecordsPerPage>250</RecordsPerPage><viewTitle>Ouvidoria Personalizada</viewTitle><layoutXml>&#60;grid name&#61;&#34;resultset&#34; object&#61;&#34;2029&#34; jump&#61;&#34;title&#34; select&#61;&#34;1&#34; icon&#61;&#34;1&#34; preview&#61;&#34;1&#34;&#62;&#60;row name&#61;&#34;result&#34; id&#61;&#34;objectid&#34; multiobjectidfield&#61;&#34;objecttypecode&#34;&#62;&#60;cell name&#61;&#34;enteredon&#34; width&#61;&#34;140&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_numerodaocorrencia&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_cpf_cnpj&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.createdon&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_dataderecebimento&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_tipodeocorrencia&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_formadecontato&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_numerodoprotocolo&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_origemdaocorrencia&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_dtvencimento&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_nome_gcs&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_nome_informante&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_datadamulta&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;workerid&#34; width&#61;&#34;100&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_clientepfid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_clientepjid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.modifiedon&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_email_gcs&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_informantepfid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_informantepjid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.modifiedby&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_numero_gcs&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_ocorrenciapertenceacaixa&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.gcs_produto&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.ownerid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_quem_comunica&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.overriddencreatedon&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.gcs_controlestatus&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.statecode&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.statuscode&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_statusdaocorrencia&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;createdon&#34; width&#61;&#34;100&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_datadevencimentolegal&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09.new_cpf_cnpj_gcs&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;new_ocorrencia_ouvidoria&#34; relatedentityattr&#61;&#34;new_ocorrencia_ouvidoriaid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;e2fcb096-c2a2-4b1a-a67b-91253a81dd5d&#125;&#34; relationshipname&#61;&#34;new_ocorrencia_ouvidoria_QueueItems&#34; &#47;&#62;&#60;cell name&#61;&#34;queueitemid&#34; ishidden&#61;&#34;1&#34; width&#61;&#34;100&#34; &#47;&#62;&#60;&#47;row&#62;&#60;&#47;grid&#62;</layoutXml><otc>2029</otc><otn>queueitem</otn><entitydisplayname>Item da Fila</entitydisplayname><titleformat>&#123;0&#125; &#123;1&#125;</titleformat><entitypluraldisplayname>Itens da Fila</entitypluraldisplayname><qid>&#123;B9AB4B74-F435-E411-8C59-00155D01CC09&#125;</qid><isWorkflowSupported>true</isWorkflowSupported><fetchXmlForFilters>&#60;fetch version&#61;&#34;1.0&#34; output-format&#61;&#34;xml-platform&#34; mapping&#61;&#34;logical&#34; distinct&#61;&#34;false&#34;&#62;&#60;entity name&#61;&#34;queueitem&#34;&#62;&#60;attribute name&#61;&#34;objectid&#34; &#47;&#62;&#60;attribute name&#61;&#34;enteredon&#34; &#47;&#62;&#60;attribute name&#61;&#34;workerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;objecttypecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;order attribute&#61;&#34;enteredon&#34; descending&#61;&#34;true&#34; &#47;&#62;&#60;link-entity name&#61;&#34;new_ocorrencia_ouvidoria&#34; from&#61;&#34;new_ocorrencia_ouvidoriaid&#34; to&#61;&#34;objectid&#34; visible&#61;&#34;false&#34; link-type&#61;&#34;outer&#34; alias&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09&#34;&#62;&#60;attribute name&#61;&#34;new_tipodeocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;overriddencreatedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_quem_comunica&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_produto&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_origemdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ocorrenciapertenceacaixa&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodoprotocolo&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numero_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_informante&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedby&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_formadecontato&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_email_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dtvencimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dataderecebimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadamulta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_controlestatus&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadevencimentolegal&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj_gcs&#34; &#47;&#62;&#60;&#47;link-entity&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXmlForFilters><isFetchXmlNotFinal>False</isFetchXmlNotFinal><effectiveFetchXml>&#60;fetch distinct&#61;&#34;false&#34; no-lock&#61;&#34;false&#34; mapping&#61;&#34;logical&#34; page&#61;&#34;1&#34; count&#61;&#34;250&#34; returntotalrecordcount&#61;&#34;true&#34;&#62;&#60;entity name&#61;&#34;queueitem&#34;&#62;&#60;attribute name&#61;&#34;objectid&#34; &#47;&#62;&#60;attribute name&#61;&#34;enteredon&#34; &#47;&#62;&#60;attribute name&#61;&#34;workerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;objecttypecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;enteredon&#34; &#47;&#62;&#60;attribute name&#61;&#34;workerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;queueitemid&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;queueid&#34; operator&#61;&#34;eq&#34; value&#61;&#34;&#123;B9AB4B74-F435-E411-8C59-00155D01CC09&#125;&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;order attribute&#61;&#34;enteredon&#34; descending&#61;&#34;true&#34; &#47;&#62;&#60;link-entity name&#61;&#34;new_ocorrencia_ouvidoria&#34; to&#61;&#34;objectid&#34; from&#61;&#34;new_ocorrencia_ouvidoriaid&#34; link-type&#61;&#34;outer&#34; alias&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09&#34;&#62;&#60;attribute name&#61;&#34;new_tipodeocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;overriddencreatedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_quem_comunica&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_produto&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_origemdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ocorrenciapertenceacaixa&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodoprotocolo&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numero_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_informante&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedby&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_formadecontato&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_email_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dtvencimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dataderecebimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadamulta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_controlestatus&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadevencimentolegal&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj_gcs&#34; &#47;&#62;&#60;&#47;link-entity&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</effectiveFetchXml><LayoutStyle>GridList</LayoutStyle><enableFilters></enableFilters><quickfind></quickfind><filter></filter><filterDisplay></filterDisplay><maxselectableitems>-1</maxselectableitems><fetchXml>&#60;fetch version&#61;&#34;1.0&#34; output-format&#61;&#34;xml-platform&#34; mapping&#61;&#34;logical&#34; distinct&#61;&#34;false&#34;&#62;&#60;entity name&#61;&#34;queueitem&#34;&#62;&#60;attribute name&#61;&#34;objectid&#34;&#47;&#62;&#60;attribute name&#61;&#34;enteredon&#34;&#47;&#62;&#60;attribute name&#61;&#34;workerid&#34;&#47;&#62;&#60;attribute name&#61;&#34;objecttypecode&#34;&#47;&#62;&#60;attribute name&#61;&#34;createdon&#34;&#47;&#62;&#60;order attribute&#61;&#34;enteredon&#34; descending&#61;&#34;true&#34;&#47;&#62;&#60;link-entity name&#61;&#34;new_ocorrencia_ouvidoria&#34; from&#61;&#34;new_ocorrencia_ouvidoriaid&#34; to&#61;&#34;objectid&#34; alias&#61;&#34;a_0b80c57ecc35e4118c5900155d01cc09&#34; old-link-type&#61;&#34;outer&#34;&#62;&#60;attribute name&#61;&#34;new_tipodeocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_statusdaocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;statecode&#34;&#47;&#62;&#60;attribute name&#61;&#34;overriddencreatedon&#34;&#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_quem_comunica&#34;&#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34;&#47;&#62;&#60;attribute name&#61;&#34;gcs_produto&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_origemdaocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_ocorrenciapertenceacaixa&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_numerodoprotocolo&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_numerodaocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_numero_gcs&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_nome_informante&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_nome_gcs&#34;&#47;&#62;&#60;attribute name&#61;&#34;modifiedby&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_informantepjid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_informantepfid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_formadecontato&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_email_gcs&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_dtvencimento&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_dataderecebimento&#34;&#47;&#62;&#60;attribute name&#61;&#34;modifiedon&#34;&#47;&#62;&#60;attribute name&#61;&#34;createdon&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_datadamulta&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj&#34;&#47;&#62;&#60;attribute name&#61;&#34;gcs_controlestatus&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_clientepjid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_clientepfid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_datadevencimentolegal&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj_gcs&#34;&#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;new_numerodaocorrencia&#34; operator&#61;&#34;eq&#34; gridfilterconditionid&#61;&#34;0b5d65a5221521d425ba46a862b428d5&#34; value&#61;&#34;" + nuOcorrencia + @"&#34;&#47;&#62;&#60;&#47;filter&#62;&#60;&#47;link-entity&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXml></parameters><columns><column width=""140"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Inserido&#32;na&#32;Fila"" fieldname=""enteredon"" entityname=""queueitem"" renderertype=""datetime"">enteredon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""N&#250;mero&#32;da&#32;Ocorr&#234;ncia&#32;&#40;Objeto&#41;"" fieldname=""new_numerodaocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_numerodaocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""CPF&#47;CNPJ&#32;&#40;Informante&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_cpf_cnpj"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_cpf_cnpj</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Data&#32;de&#32;Cria&#231;&#227;o&#32;&#40;Objeto&#41;"" fieldname=""createdon"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.createdon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Data&#32;de&#32;Recebimento&#32;&#40;Objeto&#41;"" fieldname=""new_dataderecebimento"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_dataderecebimento</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Tipo&#32;de&#32;Ocorr&#234;ncia&#32;&#40;Objeto&#41;"" fieldname=""new_tipodeocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_tipodeocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Forma&#32;de&#32;Contato&#32;&#40;Objeto&#41;"" fieldname=""new_formadecontato"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_formadecontato</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""N&#250;mero&#32;do&#32;Protocolo&#32;&#40;Objeto&#41;"" fieldname=""new_numerodoprotocolo"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_numerodoprotocolo</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Origem&#32;da&#32;Ocorr&#234;ncia&#32;&#40;Objeto&#41;"" fieldname=""new_origemdaocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_origemdaocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Data&#32;de&#32;Vencimento&#32;Prevista&#32;&#40;Objeto&#41;"" fieldname=""new_dtvencimento"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_dtvencimento</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Nome&#32;&#40;GCS&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_nome_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_nome_gcs</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Nome&#32;&#40;Informante&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_nome_informante"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_nome_informante</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Data&#32;da&#32;Multa&#32;&#40;Objeto&#41;"" fieldname=""new_datadamulta"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_datadamulta</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Trabalhado&#32;por"" fieldname=""workerid"" entityname=""queueitem"" renderertype=""lookup"">workerid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Cliente&#32;&#40;PF&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_clientepfid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_clientepfid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Cliente&#32;&#40;PJ&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_clientepjid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_clientepjid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Data&#32;de&#32;Modifica&#231;&#227;o&#32;&#40;Objeto&#41;"" fieldname=""modifiedon"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.modifiedon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""E-mail&#32;&#40;GCS&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_email_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_email_gcs</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Informante&#32;&#40;PF&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_informantepfid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_informantepfid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Informante&#32;&#40;PJ&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_informantepjid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_informantepjid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Modificado&#32;Por&#32;&#40;Objeto&#41;"" fieldname=""modifiedby"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.modifiedby</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""N&#250;mero&#32;&#40;GCS&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_numero_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_numero_gcs</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Ocorr&#234;ncia&#32;Pertence&#32;a&#32;Caixa&#63;&#32;&#40;Objeto&#41;"" fieldname=""new_ocorrenciapertenceacaixa"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_ocorrenciapertenceacaixa</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Produto&#32;&#40;Objeto&#41;"" fieldname=""gcs_produto"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.gcs_produto</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Propriet&#225;rio&#32;&#40;Objeto&#41;"" fieldname=""ownerid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""owner"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.ownerid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Quem&#32;est&#225;&#32;comunicando&#32;essa&#32;ocorr&#234;ncia&#32;&#233;&#32;o&#32;Cliente&#63;&#32;&#40;Objeto&#41;"" fieldname=""new_quem_comunica"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_quem_comunica</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Registro&#32;Criado&#32;em&#32;&#40;Objeto&#41;"" fieldname=""overriddencreatedon"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.overriddencreatedon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Controle&#32;Status&#32;&#40;Objeto&#41;"" fieldname=""gcs_controlestatus"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""bit"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.gcs_controlestatus</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Status&#32;&#40;Objeto&#41;"" fieldname=""statecode"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""state"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.statecode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Raz&#227;o&#32;do&#32;Status&#32;&#40;Objeto&#41;"" fieldname=""statuscode"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""status"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.statuscode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Status&#32;da&#32;Ocorr&#234;ncia&#32;&#40;Objeto&#41;"" fieldname=""new_statusdaocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_statusdaocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Cria&#231;&#227;o"" fieldname=""createdon"" entityname=""queueitem"" renderertype=""datetime"">createdon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Data&#32;de&#32;Vencimento&#32;Legal&#32;&#40;Objeto&#41;"" fieldname=""new_datadevencimentolegal"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_datadevencimentolegal</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""CPF&#32;&#47;&#32;CNPJ&#32;&#40;GCS&#41;&#32;&#40;Objeto&#41;"" fieldname=""new_cpf_cnpj_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"" relationshipname=""new_ocorrencia_ouvidoria_QueueItems"">a_0b80c57ecc35e4118c5900155d01cc09.new_cpf_cnpj_gcs</column><column width=""0"" isHidden=""true"" isMetadataBound=""true"" isSortable=""false"" label=""Item&#32;da&#32;Fila"" fieldname=""queueitemid"" entityname=""queueitem"">queueitemid</column></columns></grid>";
            else
                body = @"<grid><sortColumns>new_numerodaocorrencia&#58;1</sortColumns><pageNum>1</pageNum><recsPerPage>250</recsPerPage><dataProvider>Microsoft.Crm.Application.Platform.Grid.GridDataProviderQueryBuilder</dataProvider><uiProvider>Microsoft.Crm.Application.Controls.GridUIProvider</uiProvider><cols/><max>-1</max><refreshAsync>False</refreshAsync><pagingCookie>&#60;cookie page&#61;&#34;1&#34;&#62;&#60;new_numerodaocorrencia last&#61;&#34;10249&#34; first&#61;&#34;10000&#34; &#47;&#62;&#60;new_ocorrencia_ouvidoriaid last&#61;&#34;&#123;6C52AAB6-5F6C-E411-8410-00155D01C830&#125;&#34; first&#61;&#34;&#123;CA68AB8C-5F6C-E411-8410-00155D01C830&#125;&#34; &#47;&#62;&#60;&#47;cookie&#62;</pagingCookie><enableMultiSort>true</enableMultiSort><enablePagingWhenOnePage>true</enablePagingWhenOnePage><initStatements>crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_clientepfid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_clientepjid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_informantepfid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidorianew_informantepjid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidoriamodifiedby&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_ocorrencia_ouvidoriaownerid&#39;&#41;&#41;&#59;&#10;</initStatements><refreshCalledFromRefreshButton>1</refreshCalledFromRefreshButton><totalrecordcount>5000</totalrecordcount><allrecordscounted>false</allrecordscounted><returntotalrecordcount>true</returntotalrecordcount><getParameters></getParameters><parameters><autorefresh>1</autorefresh><isGridFilteringEnabled>1</isGridFilteringEnabled><viewid>&#123;29B4EBAC-46D4-E711-B9C3-00155D0B5C62&#125;</viewid><viewtype>4230</viewtype><RecordsPerPage>250</RecordsPerPage><viewTitle>Ouvidoria Personalizada</viewTitle><layoutXml>&#60;grid name&#61;&#34;resultset&#34; object&#61;&#34;10066&#34; jump&#61;&#34;new_numerodaocorrencia&#34; select&#61;&#34;1&#34; icon&#61;&#34;1&#34; preview&#61;&#34;1&#34;&#62;&#60;row name&#61;&#34;result&#34; id&#61;&#34;new_ocorrencia_ouvidoriaid&#34;&#62;&#60;cell name&#61;&#34;new_numerodaocorrencia&#34; width&#61;&#34;300&#34;&#47;&#62;&#60;cell name&#61;&#34;new_cpf_cnpj&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;createdon&#34; width&#61;&#34;125&#34;&#47;&#62;&#60;cell name&#61;&#34;new_dataderecebimento&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_tipodeocorrencia&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_formadecontato&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_numerodoprotocolo&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_origemdaocorrencia&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_dtvencimento&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_nome_gcs&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_nome_informante&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_datadamulta&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_clientepfid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_clientepjid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;modifiedon&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_email_gcs&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_informantepfid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_informantepjid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;modifiedby&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_numero_gcs&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_ocorrenciapertenceacaixa&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;gcs_produto&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;ownerid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_quem_comunica&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;overriddencreatedon&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;gcs_controlestatus&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;statecode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;statuscode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_statusdaocorrencia&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_datadevencimentolegal&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_cpf_cnpj_gcs&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;&#47;row&#62;&#60;&#47;grid&#62;</layoutXml><otc>10066</otc><otn>new_ocorrencia_ouvidoria</otn><entitydisplayname>Ocorr&#234;ncia de Ouvidoria</entitydisplayname><titleformat>&#123;0&#125; &#123;1&#125;</titleformat><entitypluraldisplayname>Ocorr&#234;ncias de Ouvidoria</entitypluraldisplayname><isWorkflowSupported>true</isWorkflowSupported><fetchXmlForFilters>&#60;fetch version&#61;&#34;1.0&#34; output-format&#61;&#34;xml-platform&#34; mapping&#61;&#34;logical&#34; distinct&#61;&#34;false&#34;&#62;&#60;entity name&#61;&#34;new_ocorrencia_ouvidoria&#34;&#62;&#60;attribute name&#61;&#34;new_ocorrencia_ouvidoriaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_tipodeocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;overriddencreatedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_quem_comunica&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_produto&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_origemdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ocorrenciapertenceacaixa&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodoprotocolo&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numero_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_informante&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedby&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_formadecontato&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_email_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dtvencimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadevencimentolegal&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dataderecebimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadamulta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_controlestatus&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj_gcs&#34; &#47;&#62;&#60;order attribute&#61;&#34;new_numerodaocorrencia&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXmlForFilters><isFetchXmlNotFinal>False</isFetchXmlNotFinal><effectiveFetchXml>&#60;fetch distinct&#61;&#34;false&#34; no-lock&#61;&#34;false&#34; mapping&#61;&#34;logical&#34; page&#61;&#34;1&#34; count&#61;&#34;250&#34; returntotalrecordcount&#61;&#34;true&#34;&#62;&#60;entity name&#61;&#34;new_ocorrencia_ouvidoria&#34;&#62;&#60;attribute name&#61;&#34;new_ocorrencia_ouvidoriaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_tipodeocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;overriddencreatedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_quem_comunica&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_produto&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_origemdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ocorrenciapertenceacaixa&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodoprotocolo&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numero_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_informante&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedby&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_formadecontato&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_email_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dtvencimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadevencimentolegal&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dataderecebimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadamulta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_controlestatus&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dataderecebimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_tipodeocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_formadecontato&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numerodoprotocolo&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_origemdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_dtvencimento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_nome_informante&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadamulta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_clientepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_email_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepfid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_informantepjid&#34; &#47;&#62;&#60;attribute name&#61;&#34;modifiedby&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_numero_gcs&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ocorrenciapertenceacaixa&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_produto&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_quem_comunica&#34; &#47;&#62;&#60;attribute name&#61;&#34;overriddencreatedon&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_controlestatus&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadevencimentolegal&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj_gcs&#34; &#47;&#62;&#60;order attribute&#61;&#34;new_numerodaocorrencia&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</effectiveFetchXml><LayoutStyle>GridList</LayoutStyle><enableFilters></enableFilters><quickfind></quickfind><filter></filter><filterDisplay></filterDisplay><maxselectableitems>-1</maxselectableitems><fetchXml>&#60;fetch version&#61;&#34;1.0&#34; output-format&#61;&#34;xml-platform&#34; mapping&#61;&#34;logical&#34; distinct&#61;&#34;false&#34;&#62;&#60;entity name&#61;&#34;new_ocorrencia_ouvidoria&#34;&#62;&#60;attribute name&#61;&#34;new_ocorrencia_ouvidoriaid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_numerodaocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;createdon&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_tipodeocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_statusdaocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;statecode&#34;&#47;&#62;&#60;attribute name&#61;&#34;overriddencreatedon&#34;&#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_quem_comunica&#34;&#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34;&#47;&#62;&#60;attribute name&#61;&#34;gcs_produto&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_origemdaocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_ocorrenciapertenceacaixa&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_numerodoprotocolo&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_numero_gcs&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_nome_informante&#34;&#47;&#62;&#60;attribute name&#61;&#34;modifiedby&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_informantepjid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_informantepfid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_formadecontato&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_email_gcs&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_dtvencimento&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_datadevencimentolegal&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_dataderecebimento&#34;&#47;&#62;&#60;attribute name&#61;&#34;modifiedon&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_datadamulta&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj&#34;&#47;&#62;&#60;attribute name&#61;&#34;gcs_controlestatus&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_clientepjid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_clientepfid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_nome_gcs&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_cpf_cnpj_gcs&#34;&#47;&#62;&#60;order attribute&#61;&#34;new_numerodaocorrencia&#34; descending&#61;&#34;false&#34;&#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;new_numerodaocorrencia&#34; operator&#61;&#34;eq&#34; gridfilterconditionid&#61;&#34;851377118eea5a59d23584507cc5254d&#34; value&#61;&#34;" + nuOcorrencia + @"&#34;&#47;&#62;&#60;&#47;filter&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXml></parameters><columns><column width=""300"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""N&#250;mero&#32;da&#32;Ocorr&#234;ncia"" fieldname=""new_numerodaocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""Crm.PrimaryField"">new_numerodaocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""CPF&#47;CNPJ&#32;&#40;Informante&#41;"" fieldname=""new_cpf_cnpj"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"">new_cpf_cnpj</column><column width=""125"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Cria&#231;&#227;o"" fieldname=""createdon"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"">createdon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Recebimento"" fieldname=""new_dataderecebimento"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"">new_dataderecebimento</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Tipo&#32;de&#32;Ocorr&#234;ncia"" fieldname=""new_tipodeocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"">new_tipodeocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Forma&#32;de&#32;Contato"" fieldname=""new_formadecontato"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"">new_formadecontato</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""N&#250;mero&#32;do&#32;Protocolo"" fieldname=""new_numerodoprotocolo"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"">new_numerodoprotocolo</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Origem&#32;da&#32;Ocorr&#234;ncia"" fieldname=""new_origemdaocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"">new_origemdaocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Vencimento&#32;Prevista"" fieldname=""new_dtvencimento"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"">new_dtvencimento</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Nome&#32;&#40;GCS&#41;"" fieldname=""new_nome_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"">new_nome_gcs</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Nome&#32;&#40;Informante&#41;"" fieldname=""new_nome_informante"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"">new_nome_informante</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;da&#32;Multa"" fieldname=""new_datadamulta"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"">new_datadamulta</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Cliente&#32;&#40;PF&#41;"" fieldname=""new_clientepfid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"">new_clientepfid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Cliente&#32;&#40;PJ&#41;"" fieldname=""new_clientepjid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"">new_clientepjid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Modifica&#231;&#227;o"" fieldname=""modifiedon"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"">modifiedon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""E-mail&#32;&#40;GCS&#41;"" fieldname=""new_email_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"">new_email_gcs</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Informante&#32;&#40;PF&#41;"" fieldname=""new_informantepfid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"">new_informantepfid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Informante&#32;&#40;PJ&#41;"" fieldname=""new_informantepjid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"">new_informantepjid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Modificado&#32;Por"" fieldname=""modifiedby"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""lookup"">modifiedby</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""N&#250;mero&#32;&#40;GCS&#41;"" fieldname=""new_numero_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"">new_numero_gcs</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Ocorr&#234;ncia&#32;Pertence&#32;a&#32;Caixa&#63;"" fieldname=""new_ocorrenciapertenceacaixa"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"">new_ocorrenciapertenceacaixa</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Produto"" fieldname=""gcs_produto"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"">gcs_produto</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Propriet&#225;rio"" fieldname=""ownerid"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""owner"">ownerid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Quem&#32;est&#225;&#32;comunicando&#32;essa&#32;ocorr&#234;ncia&#32;&#233;&#32;o&#32;Cliente&#63;"" fieldname=""new_quem_comunica"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"">new_quem_comunica</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Registro&#32;Criado&#32;em"" fieldname=""overriddencreatedon"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"">overriddencreatedon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Controle&#32;Status"" fieldname=""gcs_controlestatus"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""bit"">gcs_controlestatus</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Status"" fieldname=""statecode"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""state"">statecode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Raz&#227;o&#32;do&#32;Status"" fieldname=""statuscode"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""status"">statuscode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Status&#32;da&#32;Ocorr&#234;ncia"" fieldname=""new_statusdaocorrencia"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""picklist"">new_statusdaocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Vencimento&#32;Legal"" fieldname=""new_datadevencimentolegal"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""datetime"">new_datadevencimentolegal</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""CPF&#32;&#47;&#32;CNPJ&#32;&#40;GCS&#41;"" fieldname=""new_cpf_cnpj_gcs"" entityname=""new_ocorrencia_ouvidoria"" renderertype=""nvarchar"">new_cpf_cnpj_gcs</column></columns></grid>";

            return body;
        }

        private string GetXmlQyerySubsidio(string nuOcorrencia)
        {
            string body = "";

            //body = @"<grid><sortColumns>subject&#58;1</sortColumns><pageNum>1</pageNum><recsPerPage>250</recsPerPage><dataProvider>Microsoft.Crm.Application.Controls.ActivitiesGridDataProvider</dataProvider><uiProvider>Microsoft.Crm.Application.Controls.GridUIProvider</uiProvider><cols/><max>-1</max><refreshAsync>False</refreshAsync><pagingCookie>&#60;cookie page&#61;&#34;1&#34;&#62;&#60;subject last&#61;&#34;10169.04.00&#34; firstnull&#61;&#34;1&#34; &#47;&#62;&#60;activityid last&#61;&#34;&#123;FFB5110A-A381-E411-8410-00155D01C830&#125;&#34; first&#61;&#34;&#123;41341632-A813-E711-9AC6-00155D0747AB&#125;&#34; &#47;&#62;&#60;&#47;cookie&#62;</pagingCookie><enableMultiSort>true</enableMultiSort><enablePagingWhenOnePage>true</enablePagingWhenOnePage><initStatements>crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_subsidionew_gerenciaareaid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_subsidioownerid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_subsidiogcs_elaboradorouvidoria&#39;&#41;&#41;&#59;&#10;</initStatements><refreshCalledFromRefreshButton>1</refreshCalledFromRefreshButton><totalrecordcount>5000</totalrecordcount><allrecordscounted>false</allrecordscounted><returntotalrecordcount>true</returntotalrecordcount><getParameters></getParameters><parameters><autorefresh>1</autorefresh><isGridFilteringEnabled>1</isGridFilteringEnabled><viewid>&#123;3DB67E79-2B01-E811-ADCC-00155D0B5C62&#125;</viewid><viewtype>4230</viewtype><RecordsPerPage>250</RecordsPerPage><viewTitle>Todas as Subs&#237;dios &#40;Completo&#41;</viewTitle><layoutXml>&#60;grid name&#61;&#34;resultset&#34; object&#61;&#34;10075&#34; jump&#61;&#34;subject&#34; select&#61;&#34;1&#34; icon&#61;&#34;1&#34; preview&#61;&#34;1&#34;&#62;&#60;row name&#61;&#34;result&#34; id&#61;&#34;activityid&#34;&#62;&#60;cell name&#61;&#34;subject&#34; width&#61;&#34;300&#34;&#47;&#62;&#60;cell name&#61;&#34;createdon&#34; width&#61;&#34;125&#34;&#47;&#62;&#60;cell name&#61;&#34;new_statusdosubsidio&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_prazodetratamento&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_diasatraso&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_gerenciaareaid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_ultimaprorrogacao&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;ownerid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_classificacaodaocorrencia&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_avaliacaoreprovada&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_devolverpedidoaouvidoria&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_conclusao&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34;&#47;&#62;&#60;cell name&#61;&#34;new_datadaresposta&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;scheduledend&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;scheduledstart&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_elaboradordosubsidio&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;gcs_elaboradorouvidoria&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;actualstart&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;prioritycode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;statuscode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_respostadosubsidio&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_resultado&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;statecode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_subsidioreprovado&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;gcs_valor&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;&#47;row&#62;&#60;&#47;grid&#62;</layoutXml><otc>10075</otc><otn>new_subsidio</otn><entitydisplayname>Subs&#237;dio</entitydisplayname><titleformat>&#123;0&#125; &#123;1&#125;</titleformat><entitypluraldisplayname>Subs&#237;dios</entitypluraldisplayname><datefilter>All</datefilter><isWorkflowSupported>true</isWorkflowSupported><fetchXmlForFilters>&#60;fetch version&#61;&#34;1.0&#34; output-format&#61;&#34;xml-platform&#34; mapping&#61;&#34;logical&#34; distinct&#61;&#34;false&#34;&#62;&#60;entity name&#61;&#34;new_subsidio&#34;&#62;&#60;attribute name&#61;&#34;activityid&#34; &#47;&#62;&#60;attribute name&#61;&#34;subject&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_valor&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_resultado&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34; &#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_classificacaodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_avaliacaoreprovada&#34; &#47;&#62;&#60;order attribute&#61;&#34;subject&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;not-null&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXmlForFilters><isFetchXmlNotFinal>False</isFetchXmlNotFinal><effectiveFetchXml>&#60;fetch distinct&#61;&#34;false&#34; no-lock&#61;&#34;false&#34; mapping&#61;&#34;logical&#34; page&#61;&#34;1&#34; count&#61;&#34;250&#34; returntotalrecordcount&#61;&#34;true&#34;&#62;&#60;entity name&#61;&#34;new_subsidio&#34;&#62;&#60;attribute name&#61;&#34;activityid&#34; &#47;&#62;&#60;attribute name&#61;&#34;subject&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_valor&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_resultado&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34; &#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_classificacaodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_avaliacaoreprovada&#34; &#47;&#62;&#60;attribute name&#61;&#34;subject&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_classificacaodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_avaliacaoreprovada&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_resultado&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_valor&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;not-null&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;order attribute&#61;&#34;subject&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</effectiveFetchXml><LayoutStyle>GridList</LayoutStyle><enableFilters></enableFilters><quickfind></quickfind><filter></filter><filterDisplay></filterDisplay><maxselectableitems>-1</maxselectableitems><fetchXml>&#60;fetch version&#61;&#34;1.0&#34; output-format&#61;&#34;xml-platform&#34; mapping&#61;&#34;logical&#34; distinct&#61;&#34;false&#34;&#62;&#60;entity name&#61;&#34;new_subsidio&#34;&#62;&#60;attribute name&#61;&#34;activityid&#34;&#47;&#62;&#60;attribute name&#61;&#34;subject&#34;&#47;&#62;&#60;attribute name&#61;&#34;createdon&#34;&#47;&#62;&#60;attribute name&#61;&#34;gcs_valor&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34;&#47;&#62;&#60;attribute name&#61;&#34;statecode&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_resultado&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34;&#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34;&#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34;&#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34;&#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34;&#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34;&#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34;&#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_classificacaodaocorrencia&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_avaliacaoreprovada&#34;&#47;&#62;&#60;order attribute&#61;&#34;subject&#34; descending&#61;&#34;false&#34;&#47;&#62;&#60;filter type&#61;&#34;and&#34; gridfilterid&#61;&#34;5d0d189e6194de436e773e798b9d5e87&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;not-null&#34; gridfilterconditionid&#61;&#34;7a7859e721616eb815db6aa32e6a30bd&#34;&#47;&#62;&#60;condition attribute&#61;&#34;subject&#34; operator&#61;&#34;like&#34; gridfilterconditionid&#61;&#34;e377c6c935ba1ee21e803badb80e83b5&#34; value&#61;&#34;&#37;"+ nuOcorrencia +@".&#37;&#34;&#47;&#62;&#60;&#47;filter&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXml></parameters><columns><column width=""300"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""N&#250;mero&#32;do&#32;Subs&#237;dio"" fieldname=""subject"" entityname=""new_subsidio"" renderertype=""Crm.PrimaryField"">subject</column><column width=""125"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Cria&#231;&#227;o&#32;do&#32;Pedido"" fieldname=""createdon"" entityname=""new_subsidio"" renderertype=""datetime"">createdon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Status&#32;do&#32;Subs&#237;dio"" fieldname=""new_statusdosubsidio"" entityname=""new_subsidio"" renderertype=""picklist"">new_statusdosubsidio</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Prazo&#32;de&#32;Tratamento"" fieldname=""new_prazodetratamento"" entityname=""new_subsidio"" renderertype=""datetime"">new_prazodetratamento</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Dias&#32;de&#32;Atraso"" fieldname=""new_diasatraso"" entityname=""new_subsidio"" renderertype=""int"">new_diasatraso</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Ger&#234;ncia&#47;&#193;rea"" fieldname=""new_gerenciaareaid"" entityname=""new_subsidio"" renderertype=""lookup"">new_gerenciaareaid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Ultima&#32;Prorroga&#231;&#227;o"" fieldname=""new_ultimaprorrogacao"" entityname=""new_subsidio"" renderertype=""datetime"">new_ultimaprorrogacao</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Propriet&#225;rio"" fieldname=""ownerid"" entityname=""new_subsidio"" renderertype=""owner"">ownerid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Classifica&#231;&#227;o&#32;da&#32;Ocorr&#234;ncia"" fieldname=""new_classificacaodaocorrencia"" entityname=""new_subsidio"" renderertype=""picklist"">new_classificacaodaocorrencia</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Avalia&#231;&#227;o&#32;Reprovada"" fieldname=""new_avaliacaoreprovada"" entityname=""new_subsidio"" renderertype=""picklist"">new_avaliacaoreprovada</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Devolver&#32;Pedido&#32;a&#32;Ouvidoria&#63;"" fieldname=""new_devolverpedidoaouvidoria"" entityname=""new_subsidio"" renderertype=""bit"">new_devolverpedidoaouvidoria</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Conclus&#227;o"" fieldname=""new_conclusao"" entityname=""new_subsidio"" renderertype=""ntext"">new_conclusao</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;da&#32;Resposta"" fieldname=""new_datadaresposta"" entityname=""new_subsidio"" renderertype=""datetime"">new_datadaresposta</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Conclus&#227;o"" fieldname=""scheduledend"" entityname=""new_subsidio"" renderertype=""datetime"">scheduledend</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;In&#237;cio"" fieldname=""scheduledstart"" entityname=""new_subsidio"" renderertype=""datetime"">scheduledstart</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Elaborador&#32;do&#32;Subs&#237;dio"" fieldname=""new_elaboradordosubsidio"" entityname=""new_subsidio"" renderertype=""nvarchar"">new_elaboradordosubsidio</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Elaborador&#32;Ouvidoria"" fieldname=""gcs_elaboradorouvidoria"" entityname=""new_subsidio"" renderertype=""lookup"">gcs_elaboradorouvidoria</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""In&#237;cio&#32;Real"" fieldname=""actualstart"" entityname=""new_subsidio"" renderertype=""datetime"">actualstart</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Prioridade"" fieldname=""prioritycode"" entityname=""new_subsidio"" renderertype=""picklist"">prioritycode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Raz&#227;o&#32;do&#32;Status"" fieldname=""statuscode"" entityname=""new_subsidio"" renderertype=""status"">statuscode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Resposta&#32;do&#32;Subs&#237;dio"" fieldname=""new_respostadosubsidio"" entityname=""new_subsidio"" renderertype=""picklist"">new_respostadosubsidio</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Resultado"" fieldname=""new_resultado"" entityname=""new_subsidio"" renderertype=""picklist"">new_resultado</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Status&#32;da&#32;Atividade"" fieldname=""statecode"" entityname=""new_subsidio"" renderertype=""state"">statecode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Subs&#237;dio&#32;Reprovado"" fieldname=""new_subsidioreprovado"" entityname=""new_subsidio"" renderertype=""nvarchar"">new_subsidioreprovado</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Valor"" fieldname=""gcs_valor"" entityname=""new_subsidio"" renderertype=""money"">gcs_valor</column></columns></grid>";
            body = @"<grid><sortColumns>subject&#58;1</sortColumns><pageNum>1</pageNum><recsPerPage>250</recsPerPage><dataProvider>Microsoft.Crm.Application.Controls.ActivitiesGridDataProvider</dataProvider><uiProvider>Microsoft.Crm.Application.Controls.GridUIProvider</uiProvider><cols/><max>-1</max><refreshAsync>False</refreshAsync><pagingCookie>&#60;cookie page&#61;&#34;1&#34;&#62;&#60;subject last&#61;&#34;10169.04.00&#34; firstnull&#61;&#34;1&#34; &#47;&#62;&#60;activityid last&#61;&#34;&#123;FFB5110A-A381-E411-8410-00155D01C830&#125;&#34; first&#61;&#34;&#123;41341632-A813-E711-9AC6-00155D0747AB&#125;&#34; &#47;&#62;&#60;&#47;cookie&#62;</pagingCookie><enableMultiSort>true</enableMultiSort><enablePagingWhenOnePage>true</enablePagingWhenOnePage><initStatements>crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_subsidionew_gerenciaareaid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_subsidioownerid&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridnew_subsidiogcs_elaboradorouvidoria&#39;&#41;&#41;&#59;&#10;</initStatements><refreshCalledFromRefreshButton>1</refreshCalledFromRefreshButton><totalrecordcount>5000</totalrecordcount><allrecordscounted>false</allrecordscounted><returntotalrecordcount>true</returntotalrecordcount><getParameters></getParameters><parameters><autorefresh>1</autorefresh><isGridFilteringEnabled>1</isGridFilteringEnabled><viewid>&#123;93243A1E-930E-427F-8276-2D18FF1DE871&#125;</viewid><viewtype>1039</viewtype><RecordsPerPage>250</RecordsPerPage><viewTitle>Todas as Subs&#237;dios</viewTitle><layoutXml>&#60;grid name&#61;&#34;resultset&#34; object&#61;&#34;10075&#34; jump&#61;&#34;subject&#34; select&#61;&#34;1&#34; icon&#61;&#34;1&#34; preview&#61;&#34;1&#34;&#62;&#60;row name&#61;&#34;result&#34; id&#61;&#34;activityid&#34;&#62;&#60;cell name&#61;&#34;subject&#34; width&#61;&#34;300&#34;&#47;&#62;&#60;cell name&#61;&#34;createdon&#34; width&#61;&#34;125&#34;&#47;&#62;&#60;cell name&#61;&#34;new_statusdosubsidio&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_prazodetratamento&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_diasatraso&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_gerenciaareaid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_ultimaprorrogacao&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;ownerid&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_classificaodaocorrenciaouvidoria&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_avaliacao&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_devolverpedidoaouvidoria&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_conclusao&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34;&#47;&#62;&#60;cell name&#61;&#34;new_datadaresposta&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;scheduledend&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;scheduledstart&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_elaboradordosubsidio&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;gcs_elaboradorouvidoria&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;actualstart&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;prioritycode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;statuscode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_respostadosubsidio&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;statecode&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;cell name&#61;&#34;new_subsidioreprovado&#34; width&#61;&#34;100&#34;&#47;&#62;&#60;&#47;row&#62;&#60;&#47;grid&#62;</layoutXml><otc>10075</otc><otn>new_subsidio</otn><entitydisplayname>Subs&#237;dio</entitydisplayname><titleformat>&#123;0&#125; &#123;1&#125;</titleformat><entitypluraldisplayname>Subs&#237;dios</entitypluraldisplayname><datefilter>All</datefilter><isWorkflowSupported>true</isWorkflowSupported><fetchXmlForFilters>&#60;fetch version&#61;&#34;1.0&#34; mapping&#61;&#34;logical&#34;&#62;&#60;entity name&#61;&#34;new_subsidio&#34;&#62;&#60;attribute name&#61;&#34;subject&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;order attribute&#61;&#34;subject&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;not-null&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34; &#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_classificaodaocorrenciaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_avaliacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;activityid&#34; &#47;&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXmlForFilters><isFetchXmlNotFinal>False</isFetchXmlNotFinal><effectiveFetchXml>&#60;fetch distinct&#61;&#34;false&#34; no-lock&#61;&#34;false&#34; mapping&#61;&#34;logical&#34; page&#61;&#34;1&#34; count&#61;&#34;250&#34; returntotalrecordcount&#61;&#34;true&#34;&#62;&#60;entity name&#61;&#34;new_subsidio&#34;&#62;&#60;attribute name&#61;&#34;subject&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34; &#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_classificaodaocorrenciaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_avaliacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;activityid&#34; &#47;&#62;&#60;attribute name&#61;&#34;subject&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_classificaodaocorrenciaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_avaliacao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34; &#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34; &#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34; &#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34; &#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34; &#47;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;not-null&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;order attribute&#61;&#34;subject&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</effectiveFetchXml><LayoutStyle>GridList</LayoutStyle><enableFilters></enableFilters><quickfind></quickfind><filter></filter><filterDisplay></filterDisplay><maxselectableitems>-1</maxselectableitems><fetchXml>&#60;fetch version&#61;&#34;1.0&#34; mapping&#61;&#34;logical&#34;&#62;&#60;entity name&#61;&#34;new_subsidio&#34;&#62;&#60;attribute name&#61;&#34;subject&#34;&#47;&#62;&#60;attribute name&#61;&#34;createdon&#34;&#47;&#62;&#60;order attribute&#61;&#34;subject&#34; descending&#61;&#34;false&#34;&#47;&#62;&#60;filter type&#61;&#34;and&#34; gridfilterid&#61;&#34;39173e74b5ad2bb962a7ad057c368dde&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;not-null&#34; gridfilterconditionid&#61;&#34;47e3938507b9059a92e907e995a7c285&#34;&#47;&#62;&#60;condition attribute&#61;&#34;subject&#34; operator&#61;&#34;like&#34; gridfilterconditionid&#61;&#34;b994078039ac68819329d7bac875a880&#34; value&#61;&#34;&#37;" + nuOcorrencia + @"&#37;&#34;&#47;&#62;&#60;&#47;filter&#62;&#60;attribute name&#61;&#34;new_ultimaprorrogacao&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_subsidioreprovado&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_statusdosubsidio&#34;&#47;&#62;&#60;attribute name&#61;&#34;statecode&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_respostadosubsidio&#34;&#47;&#62;&#60;attribute name&#61;&#34;statuscode&#34;&#47;&#62;&#60;attribute name&#61;&#34;ownerid&#34;&#47;&#62;&#60;attribute name&#61;&#34;prioritycode&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_prazodetratamento&#34;&#47;&#62;&#60;attribute name&#61;&#34;actualstart&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_gerenciaareaid&#34;&#47;&#62;&#60;attribute name&#61;&#34;gcs_elaboradorouvidoria&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_elaboradordosubsidio&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_diasatraso&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_devolverpedidoaouvidoria&#34;&#47;&#62;&#60;attribute name&#61;&#34;scheduledstart&#34;&#47;&#62;&#60;attribute name&#61;&#34;scheduledend&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_datadaresposta&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_conclusao&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_classificaodaocorrenciaouvidoria&#34;&#47;&#62;&#60;attribute name&#61;&#34;new_avaliacao&#34;&#47;&#62;&#60;attribute name&#61;&#34;activityid&#34;&#47;&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXml></parameters><columns><column width=""300"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""N&#250;mero&#32;do&#32;Subs&#237;dio"" fieldname=""subject"" entityname=""new_subsidio"" renderertype=""Crm.PrimaryField"">subject</column><column width=""125"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Cria&#231;&#227;o&#32;do&#32;Pedido"" fieldname=""createdon"" entityname=""new_subsidio"" renderertype=""datetime"">createdon</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Status&#32;do&#32;Subs&#237;dio"" fieldname=""new_statusdosubsidio"" entityname=""new_subsidio"" renderertype=""picklist"">new_statusdosubsidio</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Prazo&#32;de&#32;Tratamento"" fieldname=""new_prazodetratamento"" entityname=""new_subsidio"" renderertype=""datetime"">new_prazodetratamento</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Dias&#32;de&#32;Atraso"" fieldname=""new_diasatraso"" entityname=""new_subsidio"" renderertype=""int"">new_diasatraso</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Ger&#234;ncia&#47;&#193;rea"" fieldname=""new_gerenciaareaid"" entityname=""new_subsidio"" renderertype=""lookup"">new_gerenciaareaid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Ultima&#32;Prorroga&#231;&#227;o"" fieldname=""new_ultimaprorrogacao"" entityname=""new_subsidio"" renderertype=""datetime"">new_ultimaprorrogacao</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Propriet&#225;rio"" fieldname=""ownerid"" entityname=""new_subsidio"" renderertype=""owner"">ownerid</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Classifica&#231;&#227;o&#32;da&#32;Ocorr&#234;ncia&#32;-&#32;Ouvidoria"" fieldname=""new_classificaodaocorrenciaouvidoria"" entityname=""new_subsidio"" renderertype=""picklist"">new_classificaodaocorrenciaouvidoria</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Avalia&#231;&#227;o"" fieldname=""new_avaliacao"" entityname=""new_subsidio"" renderertype=""picklist"">new_avaliacao</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Devolver&#32;Pedido&#32;a&#32;Ouvidoria&#63;"" fieldname=""new_devolverpedidoaouvidoria"" entityname=""new_subsidio"" renderertype=""bit"">new_devolverpedidoaouvidoria</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""false"" label=""Conclus&#227;o"" fieldname=""new_conclusao"" entityname=""new_subsidio"" renderertype=""ntext"">new_conclusao</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;da&#32;Resposta"" fieldname=""new_datadaresposta"" entityname=""new_subsidio"" renderertype=""datetime"">new_datadaresposta</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;Conclus&#227;o"" fieldname=""scheduledend"" entityname=""new_subsidio"" renderertype=""datetime"">scheduledend</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Data&#32;de&#32;In&#237;cio"" fieldname=""scheduledstart"" entityname=""new_subsidio"" renderertype=""datetime"">scheduledstart</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Elaborador&#32;do&#32;Subs&#237;dio"" fieldname=""new_elaboradordosubsidio"" entityname=""new_subsidio"" renderertype=""nvarchar"">new_elaboradordosubsidio</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Elaborador&#32;Ouvidoria"" fieldname=""gcs_elaboradorouvidoria"" entityname=""new_subsidio"" renderertype=""lookup"">gcs_elaboradorouvidoria</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""In&#237;cio&#32;Real"" fieldname=""actualstart"" entityname=""new_subsidio"" renderertype=""datetime"">actualstart</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Prioridade"" fieldname=""prioritycode"" entityname=""new_subsidio"" renderertype=""picklist"">prioritycode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Raz&#227;o&#32;do&#32;Status"" fieldname=""statuscode"" entityname=""new_subsidio"" renderertype=""status"">statuscode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Resposta&#32;do&#32;Subs&#237;dio"" fieldname=""new_respostadosubsidio"" entityname=""new_subsidio"" renderertype=""picklist"">new_respostadosubsidio</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Status&#32;da&#32;Atividade"" fieldname=""statecode"" entityname=""new_subsidio"" renderertype=""state"">statecode</column><column width=""100"" isHidden=""false"" isMetadataBound=""true"" isSortable=""true"" label=""Subs&#237;dio&#32;Reprovado"" fieldname=""new_subsidioreprovado"" entityname=""new_subsidio"" renderertype=""nvarchar"">new_subsidioreprovado</column></columns></grid>";

            return body;
        }

        /// <summary>
        /// Retorna o documento html da pagina
        /// </summary>
        /// <returns></returns>
        public async Task<HtmlDocument> GetHtmlDocument()
        {
            if (!_authenticated)
                Autenticar();

            var url = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/AppWebServices/AppGridWebService.ashx?operation=Refresh";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            return htmlDocument;
        }

        public async Task GetIframe()
        {
            if (!_authenticated)
                Autenticar();

            var url = "http://dynamics.caixaseguros.intranet:5555/CRMCAD/main.aspx";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(_b);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            HtmlDocument doc = new HtmlDocument();
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//iframe[@src]");

            foreach (var node in nodes)
            {
                HtmlAttribute attr = node.Attributes["src"];
                Console.WriteLine(attr.Value);
            }
        }

        /// <summary>
        /// Retorna a lista de ocorrencias
        /// </summary>
        /// <param name="filterComunicado"></param>
        /// <param name="comANexos"></param>
        /// <param name="somenteComunicado"></param>
        /// <returns></returns>
        public List<Ocorrencia> GetOcorrencias(bool filterComunicado = false, bool comANexos = false, bool somenteComunicado = false)
        {
            if (!_authenticated)
                Autenticar();
            List<Ocorrencia> ocorrencias = new List<Ocorrencia>();
            bs.IsXMLPost = true;
            bs.EncodingTexto = "utf-8";
            int contaPaginas = 1;
            var temOcorrenciaDisponivel = true;
            //  while (temOcorrenciaDisponivel)
            //  {
            bs.EncodingTexto = "utf-8";
            bs.XmlToPost = GetXmlQyery(contaPaginas, 500);
            bool temALgum = false;
            var xmlData = bs.PostJs("http://dynamics.caixaseguros.intranet:5555/CRMCAD/AppWebServices/AppGridWebService.ashx?operation=Refresh");
            var htmlData = xmlData
                .Replace("<tr", Environment.NewLine + "<tr")
                .Replace("</tr>", Environment.NewLine + "</tr>")
                .Replace("<td", Environment.NewLine + "<td")
                .Replace("</td>", Environment.NewLine + "</td>")
                .Replace("<SPAN", Environment.NewLine + "<SPAN")
                .Replace("</SPAN>", Environment.NewLine + "</SPAN>");
            var linhas = htmlData.ToLines();
            for (int i = 0; i < linhas.Count; i++)
            {
                var l = linhas[i];
                if (l.ToLower().Contains("class=\"ms-crm-list-row\" oid="))
                {
                    temALgum = true;
                    List<string> bloco = new List<string>();
                    var final = "</tr>";
                    var texto = "";
                    int contador = 0;
                    while (texto != final)
                    {
                        var linha = linhas[i + contador];
                        bloco.Add(linha);
                        contador++;
                        if (linha.Contains(final))
                            texto = final;
                    }
                    LinhaGridWebService objLinha = ParseLinha(bloco);
                    if (objLinha.Status == "FalhaInterna")
                    {
                        continue;
                    }
                    var o = new Ocorrencia();
                    o.OID = objLinha.OID;
                    o.QueueItemID = objLinha.QueueItemID;
                    o.Titulo = objLinha.Titulo;
                    o.ReferenteA = objLinha.ReferenteA;
                    o.NumeroOcorrencia = objLinha.NumeroOcorrencia;
                    o.DataPrevistaConclusao = objLinha.DataPrevistaConclusao;
                    o.DataCriacao = objLinha.DataCriacao;
                    o.CanalEntrada = objLinha.CanalEntrada;
                    o.CPFCNPJ = objLinha.CPFCNPJ;
                    o.NomeCliente = objLinha.NomeCliente;
                    o.Fila = objLinha.Fila;
                    o.Status = objLinha.Status;

                    //evita carga de objetos já carregados
                    if (ocorrencias.Any(oc => oc.OID == o.OID))
                        continue;
                    if (this.IgnoredIds != null)
                        if (this.IgnoredIds.Any(x => x == o.OID))
                            continue;



                    var objReferenteA = VerificarReferenteA(o.ReferenteA);
                    if (o.CanalEntrada.Contains("Serviços Online"))
                    {
                        objReferenteA = ReferenteAEnum.SinistroOnline;
                        o.EnunReferenteA = objReferenteA;
                    }
                    o.EnunReferenteA = objReferenteA;
                    if (somenteComunicado)
                    {
                        switch (objReferenteA)
                        {
                            case ReferenteAEnum.Comunicado: break;
                            //case ReferenteAEnum.Reanalise: continue;                                    
                            case ReferenteAEnum.Andamento: break;
                            case ReferenteAEnum.Indenizacao: break;
                            case ReferenteAEnum.SinistroOnline: break;
                            case ReferenteAEnum.AlteracaoApoliceEspecifica: break;
                            default: continue;

                        }
                    }

                    o.Anexos = new List<Anexo>();

                    if (string.IsNullOrEmpty(o.NumeroOcorrencia))
                    {
                        continue;
                    }

                    //if (objReferenteA == ReferenteAEnum.Andamento || objReferenteA == ReferenteAEnum.Indenizacao)
                    //{
                    //    o.EnunReferenteA = objReferenteA;
                    //}

                    if (!string.IsNullOrEmpty(o.OID))
                        ocorrencias.Add(o);


                }
            }
            temOcorrenciaDisponivel = temALgum;
            contaPaginas++;
            //  }

            if (comANexos)
            {
                foreach (var o in ocorrencias)
                {
                    if (o.EnunReferenteA == ReferenteAEnum.SinistroOnline)
                    {
                        o.SinistroOnline = true;
                        var aux = GetOcorrenciaSinistroOnline(o.OID);
                        o.Anexos = aux.Anexos;
                        foreach (var a in o.Anexos)
                        {
                            a.Origem = o.EnunReferenteA.ToString();
                        }
                    }
                    if (o.EnunReferenteA == ReferenteAEnum.Comunicado)
                    {
                        o.Anexos = GetAnexos(o.OID, true);
                        foreach (var a in o.Anexos)
                        {
                            a.Origem = o.EnunReferenteA.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(o.NumeroOcorrencia))
                    {
                        var anexoComASC = o.Anexos.FirstOrDefault(s => s.NU_ASC != "");
                        if (anexoComASC != null)
                            o.NumeroOcorrencia = anexoComASC.NU_ASC;
                    }
                }
            }


            return ocorrencias;

        }

        private LinhaGridWebService ParseLinha(List<string> linhas)
        {
            var l = linhas[0];
            var o = new LinhaGridWebService();
            try
            {
                //<tr class="ms-crm-List-Row" oid="{BCA6E6D9-29BF-E711-A81D-00155D1E9A56}" otype="4212" otypename="task" queueitemid="{C8D860D6-29BF-E711-B9C3-00155D0B5C62}">
                var vOid = linhas[15].Split('"');
                var vQID = l.Split('"');

                o.oType = vOid[11];
                o.MOid = vQID[3].Replace("{", "").Replace("}", "");
                o.OID = vOid[9].Replace("{", "").Replace("}", "");
                o.QueueItemID = vQID[9].Replace("{", "").Replace("}", "");
                o.ReferenteA = linhas[16].InnerText();
                o.Titulo = linhas[6].InnerText();
                o.NumeroOcorrencia = linhas[8].InnerText();

                if (linhas[10].InnerText() != "")
                    o.DataPrevistaConclusao = DateTime.Parse(linhas[10].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                o.CanalEntrada = linhas[12].InnerText();

                o.CPFCNPJ = linhas[20].InnerText();
                o.NomeCliente = linhas[22].InnerText();

                if (linhas[24].InnerText() != "")
                    o.DataCriacao = DateTime.Parse(linhas[24].InnerText(), CultureInfo.CreateSpecificCulture("pt-BR"));
                o.Fila = linhas[28].InnerText();

                o.Status = linhas[34].InnerText();

                if (string.IsNullOrEmpty(o.NumeroOcorrencia))
                {
                    Console.Write("cade o ASC?");
                }
            }
            catch (Exception ex)
            {
                var debugLinhas = "";
                foreach (var lx in linhas)
                {
                    debugLinhas = lx + Environment.NewLine;

                }
                //System.Diagnostics.EventLog.WriteEntry("ASCBotService", "Falha no ParseLinha ("+ l + ") " + ex.ToString()+ Environment.NewLine + debugLinhas, System.Diagnostics.EventLogEntryType.Warning);
                //Console.Write(ex.ToString());
                //throw;
                o.Status = "FalhaInterna";
            }
            return o;
        }

        public ReferenteAEnum VerificarReferenteA(string referenteA)
        {
            //Andamento do Processo - Demais Produtos
            //Indenização - Demais Produtos
            ReferenteAEnum saida = ReferenteAEnum.Outros;
            if (referenteA.Contains("Comunicado"))
            {
                saida = ReferenteAEnum.Comunicado;
                return saida;
            }
            if (referenteA.Contains("Reanálise"))
            {
                saida = ReferenteAEnum.Reanalise;
                return saida;
            }
            if (referenteA.Contains("Andamento do Processo - Demais Produtos"))
            {
                saida = ReferenteAEnum.Andamento;
                return saida;
            }
            if (referenteA.Contains("Indenização - Demais Produtos"))
            {
                saida = ReferenteAEnum.Indenizacao;
                return saida;
            }
            if (referenteA.Contains("DIRVI - SINISTRO - Comunicado de Sinistro - 06 Online"))
                saida = ReferenteAEnum.SinistroOnline;
            if (referenteA.Contains("DIRVI - SINISTRO - Comunicado de Sinistro - 07 Regulação Manual"))
                saida = ReferenteAEnum.SinistroOnline;

            if (referenteA.Contains("DIRVI - MANUTENÇÃO DA APÓLICE - Alteração de dados cadastrais - Apólice Específica"))
                saida = ReferenteAEnum.AlteracaoApoliceEspecifica;
            return saida;
        }


        private string GetXmlQyeryComplementos(string oID)
        {
            string xml = "<grid>";
            xml += "<sortColumns>createdon&#58;0</sortColumns>";
            xml += "<pageNum>1</pageNum>";
            xml += "<recsPerPage>4</recsPerPage>";
            xml += "<dataProvider>Microsoft.Crm.Application.Platform.Grid.GridDataProviderQueryBuilder</dataProvider>";
            xml += "<uiProvider>Microsoft.Crm.Application.Controls.GridUIProvider</uiProvider>";
            xml += "<cols/>";
            xml += "<max>-1</max>";
            xml += "<refreshAsync>True</refreshAsync>";
            xml += "<pagingCookie/>";
            xml += "<enableMultiSort>true</enableMultiSort>";
            xml += "<enablePagingWhenOnePage>true</enablePagingWhenOnePage>";
            xml += "<refreshCalledFromRefreshButton>1</refreshCalledFromRefreshButton>";
            xml += "<returntotalrecordcount>true</returntotalrecordcount>";
            xml += "<getParameters>getFetchXmlForFilters</getParameters>";
            xml += "<parameters>";
            xml += "<viewid>&#123;E5D764D6-1D2E-408F-845D-151807B583E1&#125;</viewid>";
            xml += "<RenderAsync>0</RenderAsync>";
            xml += "<LoadOnDemand>0</LoadOnDemand>";
            xml += "<autorefresh>1</autorefresh>";
            xml += "<isGridFilteringEnabled>1</isGridFilteringEnabled>";
            xml += "<viewtype>1039</viewtype>";
            xml += "<RecordsPerPage>4</RecordsPerPage>";
            xml += "<viewTitle>Complementos Ativo&#40;a&#41;</viewTitle>";
            xml += "<layoutXml>&#60;grid name&#61;&#34;resultset&#34; object&#61;&#34;10011&#34; jump&#61;&#34;gcs_name&#34; select&#61;&#34;1&#34; icon&#61;&#34;1&#34; preview&#61;&#34;1&#34;&#62;&#60;row name&#61;&#34;result&#34; id&#61;&#34;gcs_complementoid&#34;&#62;&#60;cell name&#61;&#34;statuscode&#34; width&#61;&#34;125&#34; &#47;&#62;&#60;cell name&#61;&#34;createdon&#34; width&#61;&#34;125&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_name&#34; width&#61;&#34;300&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_local_anexo&#34; width&#61;&#34;300&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_email_enviado&#34; width&#61;&#34;100&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_cliente_sera_notificado&#34; width&#61;&#34;150&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_intermediario_notificado&#34; width&#61;&#34;200&#34; &#47;&#62;&#60;cell name&#61;&#34;createdby&#34; width&#61;&#34;150&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_alterador_portalid_complemento&#34; width&#61;&#34;100&#34; &#47;&#62;&#60;cell name&#61;&#34;a_cc7038b02dfce111ad8900155d02c9d3.subjectid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;incident&#34; relatedentityattr&#61;&#34;incidentid&#34; primaryentityattr&#61;&#34;gcs_ocorrencia_referente&#34; relationshipid&#61;&#34;&#123;3df948c0-6cc4-490f-a98a-6cf44910b7c7&#125;&#34; relationshipname&#61;&#34;gcs_incident_gcs_complemento_Ocorrencia_referente&#34; &#47;&#62;&#60;cell name&#61;&#34;a_cc7038b02dfce111ad8900155d02c9d3.codek_assunto_1_nivelid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;incident&#34; relatedentityattr&#61;&#34;incidentid&#34; primaryentityattr&#61;&#34;gcs_ocorrencia_referente&#34; relationshipid&#61;&#34;&#123;3df948c0-6cc4-490f-a98a-6cf44910b7c7&#125;&#34; relationshipname&#61;&#34;gcs_incident_gcs_complemento_Ocorrencia_referente&#34; &#47;&#62;&#60;cell name&#61;&#34;a_cc7038b02dfce111ad8900155d02c9d3.ticketnumber&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;incident&#34; relatedentityattr&#61;&#34;incidentid&#34; primaryentityattr&#61;&#34;gcs_ocorrencia_referente&#34; relationshipid&#61;&#34;&#123;3df948c0-6cc4-490f-a98a-6cf44910b7c7&#125;&#34; relationshipname&#61;&#34;gcs_incident_gcs_complemento_Ocorrencia_referente&#34; &#47;&#62;&#60;cell name&#61;&#34;a_cc7038b02dfce111ad8900155d02c9d3.codek_assunto_2_nivelid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;incident&#34; relatedentityattr&#61;&#34;incidentid&#34; primaryentityattr&#61;&#34;gcs_ocorrencia_referente&#34; relationshipid&#61;&#34;&#123;3df948c0-6cc4-490f-a98a-6cf44910b7c7&#125;&#34; relationshipname&#61;&#34;gcs_incident_gcs_complemento_Ocorrencia_referente&#34; &#47;&#62;&#60;cell name&#61;&#34;a_cc7038b02dfce111ad8900155d02c9d3.codek_assunto_3_nivelid&#34; width&#61;&#34;100&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;incident&#34; relatedentityattr&#61;&#34;incidentid&#34; primaryentityattr&#61;&#34;gcs_ocorrencia_referente&#34; relationshipid&#61;&#34;&#123;3df948c0-6cc4-490f-a98a-6cf44910b7c7&#125;&#34; relationshipname&#61;&#34;gcs_incident_gcs_complemento_Ocorrencia_referente&#34; &#47;&#62;&#60;&#47;row&#62;&#60;&#47;grid&#62;</layoutXml>";
            xml += "<otc>10011</otc>";
            xml += "<otn>gcs_complemento</otn>";
            xml += "<entitydisplayname>Complemento</entitydisplayname>";
            xml += "<titleformat>&#123;0&#125; &#123;1&#125;</titleformat>";
            xml += "<entitypluraldisplayname>Complementos</entitypluraldisplayname>";
            xml += "<expandable>1</expandable>";
            xml += "<showjumpbar>0</showjumpbar>";
            xml += "<maxrowsbeforescroll>7</maxrowsbeforescroll>";
            xml += "<tabindex>11980</tabindex>";
            xml += "<refreshasynchronous>1</refreshasynchronous>";
            xml += "<subgridAutoExpand>0</subgridAutoExpand>";
            xml += "<relName>gcs_incident_gcs_complemento_Ocorrencia_referente</relName>";
            xml += "<roleOrd>-1</roleOrd>";
            xml += "<oType>112</oType>";
            xml += "<relationshipType>1</relationshipType>";
            xml += "<ribbonContext>SubGridStandard</ribbonContext>";
            xml += "<GridType>SubGrid</GridType>";
            xml += "<isWorkflowSupported>true</isWorkflowSupported>";
            xml += "<LayoutStyle>GridList</LayoutStyle>";
            xml += "<enableFilters></enableFilters>";
            xml += "<oId>&#123;" + oID + "&#125;</oId>";
            xml += "</parameters>";
            xml += "<columns>";
            xml += "<column width=\"125\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Classifica&#231;&#227;o\" fieldname=\"statuscode\" entityname=\"gcs_complemento\">statuscode</column>";
            xml += "<column width=\"125\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Data&#32;de&#32;Cria&#231;&#227;o\" fieldname=\"createdon\" entityname=\"gcs_complemento\">createdon</column>";
            xml += "<column width=\"300\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"T&#237;tulo&#32;do&#32;Complemento\" fieldname=\"gcs_name\" entityname=\"gcs_complemento\" renderertype=\"Crm.PrimaryField\">gcs_name</column>";
            xml += "<column width=\"300\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Local&#32;do&#32;Anexo\" fieldname=\"gcs_local_anexo\" entityname=\"gcs_complemento\">gcs_local_anexo</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Email&#32;Enviado\" fieldname=\"gcs_email_enviado\" entityname=\"gcs_complemento\">gcs_email_enviado</column>";
            xml += "<column width=\"150\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Cliente&#32;Ser&#225;&#32;Notificado&#63;\" fieldname=\"gcs_cliente_sera_notificado\" entityname=\"gcs_complemento\">gcs_cliente_sera_notificado</column>";
            xml += "<column width=\"200\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Intermedi&#225;rio&#32;Ser&#225;&#32;Notificado&#63;\" fieldname=\"gcs_intermediario_notificado\" entityname=\"gcs_complemento\">gcs_intermediario_notificado</column>";
            xml += "<column width=\"150\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Criada&#32;Por\" fieldname=\"createdby\" entityname=\"gcs_complemento\">createdby</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Alterador&#32;Portal\" fieldname=\"gcs_alterador_portalid_complemento\" entityname=\"gcs_complemento\">gcs_alterador_portalid_complemento</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Assunto&#32;&#40;Ocorr&#234;ncia&#32;referente&#41;\" fieldname=\"subjectid\" entityname=\"incident\" relationshipname=\"gcs_incident_gcs_complemento_Ocorrencia_referente\">a_cc7038b02dfce111ad8900155d02c9d3.subjectid</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Assunto&#32;1o&#32;N&#237;vel&#32;&#40;Ocorr&#234;ncia&#32;referente&#41;\" fieldname=\"codek_assunto_1_nivelid\" entityname=\"incident\" relationshipname=\"gcs_incident_gcs_complemento_Ocorrencia_referente\">a_cc7038b02dfce111ad8900155d02c9d3.codek_assunto_1_nivelid</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"N&#250;mero&#32;da&#32;Ocorr&#234;ncia&#32;&#40;Ocorr&#234;ncia&#32;referente&#41;\" fieldname=\"ticketnumber\" entityname=\"incident\" relationshipname=\"gcs_incident_gcs_complemento_Ocorrencia_referente\">a_cc7038b02dfce111ad8900155d02c9d3.ticketnumber</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Assunto&#32;2o&#32;N&#237;vel&#32;&#40;Ocorr&#234;ncia&#32;referente&#41;\" fieldname=\"codek_assunto_2_nivelid\" entityname=\"incident\" relationshipname=\"gcs_incident_gcs_complemento_Ocorrencia_referente\">a_cc7038b02dfce111ad8900155d02c9d3.codek_assunto_2_nivelid</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Assunto&#32;3o&#32;N&#237;vel&#32;&#40;Ocorr&#234;ncia&#32;referente&#41;\" fieldname=\"codek_assunto_3_nivelid\" entityname=\"incident\" relationshipname=\"gcs_incident_gcs_complemento_Ocorrencia_referente\">a_cc7038b02dfce111ad8900155d02c9d3.codek_assunto_3_nivelid</column>";
            xml += "</columns>";
            xml += "</grid>";
            return xml;
        }
        private string GetXmlQyery(int page, int maxPerPage)
        {
            string xml = "<grid>";
            xml += "<sortColumns>gcs_data_prevista_conclusao_tarefa&#58;1&#59;createdon&#58;1</sortColumns>";
            xml += "<pageNum>" + page + "</pageNum>";
            xml += "<recsPerPage>" + maxPerPage + "</recsPerPage>";
            xml += "<dataProvider>Microsoft.Crm.Application.Platform.Grid.GridDataProviderQueryBuilder</dataProvider>";
            xml += "<uiProvider>Microsoft.Crm.Application.Controls.GridUIProvider</uiProvider>";
            xml += "<cols/>";
            xml += "<max>-1</max>";
            xml += "<refreshAsync>False</refreshAsync>";
            xml += "<pagingCookie>&#60;cookie page&#61;&#34;1&#34;&#62;&#60;gcs_data_prevista_conclusao_tarefa last&#61;&#34;2017-07-11T11&#58;00&#58;00-03&#58;00&#34; firstnull&#61;&#34;1&#34; &#47;&#62;&#60;createdon last&#61;&#34;2017-07-07T11&#58;24&#58;28-03&#58;00&#34; first&#61;&#34;2017-07-10T17&#58;50&#58;30-03&#58;00&#34; &#47;&#62;&#60;queueitemid last&#61;&#34;&#123;5F3C22FB-1F63-E711-8DB1-00155D7C548E&#125;&#34; first&#61;&#34;&#123;C925A362-B165-E711-A76D-00155D9F8959&#125;&#34; &#47;&#62;&#60;&#47;cookie&#62;</pagingCookie>";
            xml += "<enableMultiSort>true</enableMultiSort>";
            xml += "<enablePagingWhenOnePage>false</enablePagingWhenOnePage>";
            xml += "<initStatements>crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridactivitypointerregardingobjectidActivityPointer_QueueItem&#39;&#41;&#41;&#59;&#10;crmCreate&#40;Mscrm.FormInputControl.PresenceLookupUIBehavior,&#123;&#125;,null,&#123;&#125;,&#36;get&#40;&#39;lookup_lookupFilterPopupcrmGridqueueitemqueueid&#39;&#41;&#41;&#59;&#10;</initStatements>";
            xml += "<totalrecordcount>259</totalrecordcount>";
            xml += "<allrecordscounted>true</allrecordscounted>";
            xml += "<returntotalrecordcount>false</returntotalrecordcount>";
            xml += "<getParameters></getParameters>";
            xml += "<parameters>";
            xml += "<autorefresh>1</autorefresh>";
            xml += "<isGridFilteringEnabled>1</isGridFilteringEnabled>";
            xml += "<viewid>&#123;707EF51C-436A-E211-AA84-00155D00AF8C&#125;</viewid>";
            xml += "<viewtype>1039</viewtype>";
            xml += "<RecordsPerPage>" + maxPerPage + "</RecordsPerPage>";
            xml += "<viewTitle>ITENS PARA TRABALHAR</viewTitle>";
            xml += "<layoutXml>&#60;grid name&#61;&#34;resultset&#34; object&#61;&#34;2029&#34; jump&#61;&#34;title&#34; select&#61;&#34;1&#34; preview&#61;&#34;1&#34; icon&#61;&#34;1&#34;&#62;&#60;row name&#61;&#34;result&#34; id&#61;&#34;objectid&#34; multiobjectidfield&#61;&#34;objecttypecode&#34;&#62;&#60;cell name&#61;&#34;queueitemid&#34; ishidden&#61;&#34;1&#34; width&#61;&#34;150&#34; &#47;&#62;&#60;cell name&#61;&#34;title&#34; width&#61;&#34;150&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_numerodaocorrencia&#34; width&#61;&#34;150&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_data_prevista_conclusao_tarefa&#34; width&#61;&#34;100&#34; &#47;&#62;&#60;cell name&#61;&#34;a_4317a47620834b68a2f9e7ca23b46f1d.gcs_canaldeentrada&#34; width&#61;&#34;125&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;task&#34; relatedentityattr&#61;&#34;activityid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;ada7188d-db0c-4cbb-9312-bdd5302a0e24&#125;&#34; relationshipname&#61;&#34;Task_QueueItem&#34; &#47;&#62;&#60;cell name&#61;&#34;a_eb56a04c2a8a4c1399e03d0b0a035dac.regardingobjectid&#34; width&#61;&#34;300&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;activitypointer&#34; relatedentityattr&#61;&#34;activityid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;0438fec3-3969-46c9-bf35-7211c6716294&#125;&#34; relationshipname&#61;&#34;ActivityPointer_QueueItem&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_cpfcnpjdocliente&#34; width&#61;&#34;125&#34; &#47;&#62;&#60;cell name&#61;&#34;gcs_nomedocliente&#34; width&#61;&#34;300&#34; &#47;&#62;&#60;cell name&#61;&#34;createdon&#34; width&#61;&#34;125&#34; &#47;&#62;&#60;cell name&#61;&#34;queueid&#34; width&#61;&#34;300&#34; &#47;&#62;&#60;cell name&#61;&#34;objecttypecode&#34; width&#61;&#34;75&#34; &#47;&#62;&#60;cell name&#61;&#34;a_eb56a04c2a8a4c1399e03d0b0a035dac.statecode&#34; width&#61;&#34;125&#34; disableSorting&#61;&#34;1&#34; relatedentityname&#61;&#34;activitypointer&#34; relatedentityattr&#61;&#34;activityid&#34; primaryentityattr&#61;&#34;objectid&#34; relationshipid&#61;&#34;&#123;0438fec3-3969-46c9-bf35-7211c6716294&#125;&#34; relationshipname&#61;&#34;ActivityPointer_QueueItem&#34; &#47;&#62;&#60;&#47;row&#62;&#60;&#47;grid&#62;</layoutXml>";
            xml += "<otc>2029</otc>";
            xml += "<otn>queueitem</otn>";
            xml += "<entitydisplayname>Item da Fila</entitydisplayname>";
            xml += "<titleformat>&#123;0&#125; &#123;1&#125;</titleformat>";
            xml += "<entitypluraldisplayname>Itens da Fila</entitypluraldisplayname>";
            xml += "<qid></qid>";
            xml += "<isWorkflowSupported>true</isWorkflowSupported>";
            xml += "<fetchXmlForFilters>&#60;fetch version&#61;&#34;1.0&#34; output-format&#61;&#34;xml-platform&#34; mapping&#61;&#34;logical&#34; distinct&#61;&#34;false&#34;&#62;&#60;entity name&#61;&#34;queueitem&#34;&#62;&#60;attribute name&#61;&#34;queueitemid&#34; &#47;&#62;&#60;attribute name&#61;&#34;queueid&#34; &#47;&#62;&#60;attribute name&#61;&#34;objecttypecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;title&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_nomedocliente&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_cpfcnpjdocliente&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_data_prevista_conclusao_tarefa&#34; &#47;&#62;&#60;order attribute&#61;&#34;gcs_data_prevista_conclusao_tarefa&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;order attribute&#61;&#34;createdon&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;workerid&#34; operator&#61;&#34;null&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;link-entity name&#61;&#34;queue&#34; from&#61;&#34;queueid&#34; to&#61;&#34;queueid&#34; alias&#61;&#34;ab&#34;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;ownerid&#34; operator&#61;&#34;eq-userteams&#34; &#47;&#62;&#60;condition attribute&#61;&#34;queueid&#34; operator&#61;&#34;ne&#34; uiname&#61;&#34;Tratamento N&#227;o Conformidade&#34; uitype&#61;&#34;queue&#34; value&#61;&#34;&#123;B5E06F48-E6DA-E411-8096-00155DD25210&#125;&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;&#47;link-entity&#62;&#60;link-entity name&#61;&#34;activitypointer&#34; from&#61;&#34;activityid&#34; to&#61;&#34;objectid&#34; alias&#61;&#34;a_eb56a04c2a8a4c1399e03d0b0a035dac&#34;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;regardingobjectid&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;eq&#34; value&#61;&#34;0&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;&#47;link-entity&#62;&#60;link-entity name&#61;&#34;task&#34; from&#61;&#34;activityid&#34; to&#61;&#34;objectid&#34; visible&#61;&#34;false&#34; link-type&#61;&#34;outer&#34; alias&#61;&#34;a_4317a47620834b68a2f9e7ca23b46f1d&#34;&#62;&#60;attribute name&#61;&#34;gcs_canaldeentrada&#34; &#47;&#62;&#60;&#47;link-entity&#62;&#60;attribute name&#61;&#34;objectid&#34; &#47;&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</fetchXmlForFilters>";
            xml += "<isFetchXmlNotFinal>False</isFetchXmlNotFinal>";
            xml += "<effectiveFetchXml>&#60;fetch distinct&#61;&#34;false&#34; no-lock&#61;&#34;false&#34; mapping&#61;&#34;logical&#34; page&#61;&#34;1&#34; count&#61;&#34;50&#34; returntotalrecordcount&#61;&#34;true&#34;&#62;&#60;entity name&#61;&#34;queueitem&#34;&#62;&#60;attribute name&#61;&#34;queueitemid&#34; &#47;&#62;&#60;attribute name&#61;&#34;queueid&#34; &#47;&#62;&#60;attribute name&#61;&#34;objecttypecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;title&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_nomedocliente&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_cpfcnpjdocliente&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_data_prevista_conclusao_tarefa&#34; &#47;&#62;&#60;attribute name&#61;&#34;objectid&#34; &#47;&#62;&#60;attribute name&#61;&#34;queueitemid&#34; &#47;&#62;&#60;attribute name&#61;&#34;title&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_numerodaocorrencia&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_data_prevista_conclusao_tarefa&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_cpfcnpjdocliente&#34; &#47;&#62;&#60;attribute name&#61;&#34;gcs_nomedocliente&#34; &#47;&#62;&#60;attribute name&#61;&#34;createdon&#34; &#47;&#62;&#60;attribute name&#61;&#34;queueid&#34; &#47;&#62;&#60;attribute name&#61;&#34;objecttypecode&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;workerid&#34; operator&#61;&#34;null&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;order attribute&#61;&#34;gcs_data_prevista_conclusao_tarefa&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;order attribute&#61;&#34;createdon&#34; descending&#61;&#34;false&#34; &#47;&#62;&#60;link-entity name&#61;&#34;queue&#34; to&#61;&#34;queueid&#34; from&#61;&#34;queueid&#34; link-type&#61;&#34;inner&#34; alias&#61;&#34;ab&#34;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;ownerid&#34; operator&#61;&#34;eq-userteams&#34; &#47;&#62;&#60;condition attribute&#61;&#34;queueid&#34; operator&#61;&#34;ne&#34; value&#61;&#34;&#123;B5E06F48-E6DA-E411-8096-00155DD25210&#125;&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;&#47;link-entity&#62;&#60;link-entity name&#61;&#34;activitypointer&#34; to&#61;&#34;objectid&#34; from&#61;&#34;activityid&#34; link-type&#61;&#34;inner&#34; alias&#61;&#34;a_eb56a04c2a8a4c1399e03d0b0a035dac&#34;&#62;&#60;attribute name&#61;&#34;statecode&#34; &#47;&#62;&#60;attribute name&#61;&#34;regardingobjectid&#34; &#47;&#62;&#60;filter type&#61;&#34;and&#34;&#62;&#60;condition attribute&#61;&#34;statecode&#34; operator&#61;&#34;eq&#34; value&#61;&#34;0&#34; &#47;&#62;&#60;&#47;filter&#62;&#60;&#47;link-entity&#62;&#60;link-entity name&#61;&#34;task&#34; to&#61;&#34;objectid&#34; from&#61;&#34;activityid&#34; link-type&#61;&#34;outer&#34; alias&#61;&#34;a_4317a47620834b68a2f9e7ca23b46f1d&#34;&#62;&#60;attribute name&#61;&#34;gcs_canaldeentrada&#34; &#47;&#62;&#60;&#47;link-entity&#62;&#60;&#47;entity&#62;&#60;&#47;fetch&#62;</effectiveFetchXml>";
            xml += "<LayoutStyle>GridList</LayoutStyle>";
            xml += "<enableFilters></enableFilters>";
            xml += "<quickfind></quickfind>";
            xml += "<filter></filter>";
            xml += "<filterDisplay></filterDisplay>";
            xml += "<maxselectableitems>-1</maxselectableitems>";
            xml += "</parameters>";
            xml += "<columns>";
            xml += "<column width=\"150\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"T&#237;tulo\" fieldname=\"title\" entityname=\"queueitem\" renderertype=\"Crm.PrimaryField\">title</column>";
            xml += "<column width=\"150\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"N&#250;mero&#32;da&#32;Ocorr&#234;ncia\" fieldname=\"gcs_numerodaocorrencia\" entityname=\"queueitem\" renderertype=\"nvarchar\">gcs_numerodaocorrencia</column>";
            xml += "<column width=\"100\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Data&#32;Prevista&#32;de&#32;Conclus&#227;o\" fieldname=\"gcs_data_prevista_conclusao_tarefa\" entityname=\"queueitem\" renderertype=\"datetime\">gcs_data_prevista_conclusao_tarefa</column>";
            xml += "<column width=\"125\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Canal&#32;de&#32;Entrada&#32;&#40;Objeto&#41;\" fieldname=\"gcs_canaldeentrada\" entityname=\"task\" renderertype=\"nvarchar\" relationshipname=\"Task_QueueItem\">a_4317a47620834b68a2f9e7ca23b46f1d.gcs_canaldeentrada</column>";
            xml += "<column width=\"300\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Referente&#32;a&#32;&#40;Objeto&#41;\" fieldname=\"regardingobjectid\" entityname=\"activitypointer\" renderertype=\"lookup\" relationshipname=\"ActivityPointer_QueueItem\">a_eb56a04c2a8a4c1399e03d0b0a035dac.regardingobjectid</column>";
            xml += "<column width=\"125\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"CPF&#47;CNPJ&#32;do&#32;Cliente\" fieldname=\"gcs_cpfcnpjdocliente\" entityname=\"queueitem\" renderertype=\"nvarchar\">gcs_cpfcnpjdocliente</column>";
            xml += "<column width=\"300\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Nome&#32;do&#32;Cliente\" fieldname=\"gcs_nomedocliente\" entityname=\"queueitem\" renderertype=\"nvarchar\">gcs_nomedocliente</column>";
            xml += "<column width=\"125\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Data&#32;de&#32;Cria&#231;&#227;o\" fieldname=\"createdon\" entityname=\"queueitem\" renderertype=\"datetime\">createdon</column>";
            xml += "<column width=\"300\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Fila\" fieldname=\"queueid\" entityname=\"queueitem\" renderertype=\"lookup\">queueid</column>";
            xml += "<column width=\"75\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"true\" label=\"Tipo\" fieldname=\"objecttypecode\" entityname=\"queueitem\" renderertype=\"picklist\">objecttypecode</column>";
            xml += "<column width=\"125\" isHidden=\"false\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Status&#32;da&#32;Atividade&#32;&#40;Objeto&#41;\" fieldname=\"statecode\" entityname=\"activitypointer\" renderertype=\"state\" relationshipname=\"ActivityPointer_QueueItem\">a_eb56a04c2a8a4c1399e03d0b0a035dac.statecode</column>";
            xml += "<column width=\"0\" isHidden=\"true\" isMetadataBound=\"true\" isSortable=\"false\" label=\"Item&#32;da&#32;Fila\" fieldname=\"queueitemid\" entityname=\"queueitem\">queueitemid</column>";
            xml += "</columns>";
            xml += "</grid>   ";

            return xml;
        }

        public void Dispose()
        {
        }
    }
}
