using Automacao.Domain.Model.ASC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Automacao.Core.Helper.Library
{
    public class Fixer
    {
        public string FileName { get; private set; }
        public Encoding DefailtEncoding { get; set; }
        public ComunicadoRTF ActualComunicado { get; private set; }
        public string ErrorMessage { get; private set; }

        public Fixer()
        {
            DefailtEncoding = Encoding.UTF8;
        }

        public ComunicadoRTF LodComunicadoFromFile(string file)
        {
            var detectEncodeing = TextFileEncodingDetector.DetectTextFileEncoding(file);
            if (detectEncodeing != null)
                this.DefailtEncoding = TextFileEncodingDetector.DetectTextFileEncoding(file);
            var dataFile = File.ReadAllText(file, this.DefailtEncoding);
            this.ActualComunicado = HtmlToComunicado(dataFile);
            //fix fones
            FixFones();
            FixCobertura();

            return this.ActualComunicado;

        }
        public void FixCobertura()
        {
            this.ActualComunicado.Comunicado.Sinistro.Cobertura = this.ActualComunicado.Comunicado.Sinistro.Cobertura.Replace("-", "").Trim();
            this.ActualComunicado.Comunicado.Sinistro.Codcobertura = this.ActualComunicado.Comunicado.Sinistro.Codcobertura.Replace("-", "").Trim();
        }

        internal void DoEveriThing(string file)
        {
            try
            {
                if (!file.Contains(".rtf"))
                {
                    throw new Exception("O arquivo dete ter a extensão .rtf");
                }

                var comunicado = LodComunicadoFromFile(file);
                // var xmlText = ConvertToXml(comunicado);
                var fSemExtensao = Path.GetFileNameWithoutExtension(file);
                var diretorio = Path.GetDirectoryName(file);
                var nFile = diretorio + "\\" + fSemExtensao + "_FIXED.rtf";
                if (File.Exists(nFile))
                    File.Delete(nFile);
                var xcomunicado = (ComunicadoRTFXML)comunicado;
                SaveXML(xcomunicado, nFile, this.DefailtEncoding);
            }
            catch (Exception ex)
            {
                this.ErrorMessage = ex.Message;



            }
        }

        public void FixFones()
        {
            var ddd = RemoveSpecialCharacters(ActualComunicado.Comunicado.Reclamante.Fonecelddd).Trim();
            var dddcom = RemoveSpecialCharacters(ActualComunicado.Comunicado.Reclamante.Fonecomddd).Trim();
            var foneresddd = RemoveSpecialCharacters(ActualComunicado.Comunicado.Reclamante.Foneresddd).Trim();
            var cel = RemoveSpecialCharacters(ActualComunicado.Comunicado.Reclamante.Fonecelnum).Trim();
            var foneCom = RemoveSpecialCharacters(ActualComunicado.Comunicado.Reclamante.Fonecomnum).Trim();
            var fonteresnum = RemoveSpecialCharacters(ActualComunicado.Comunicado.Reclamante.Fonteresnum).Trim();

            ActualComunicado.Comunicado.Reclamante.Fonecelddd = ddd == "" ? "" : float.Parse(ddd) == 0 ? "" : ddd;
            ActualComunicado.Comunicado.Reclamante.Fonecelnum = cel == "" ? "" : float.Parse(cel) == 0 ? "" : cel;
            ActualComunicado.Comunicado.Reclamante.Fonecomddd = dddcom == "" ? "" : float.Parse(dddcom) == 0 ? "" : dddcom;
            ActualComunicado.Comunicado.Reclamante.Fonecomnum = foneCom == "" ? "" : float.Parse(foneCom) == 0 ? "" : foneCom;
            ActualComunicado.Comunicado.Reclamante.Foneresddd = foneresddd == "" ? "" : float.Parse(foneresddd) == 0 ? "" : foneresddd;
            ActualComunicado.Comunicado.Reclamante.Fonteresnum = fonteresnum == "" ? "" : float.Parse(fonteresnum) == 0 ? "" : fonteresnum;

        }

        public static T DeserializeFromXmlString<T>(string xmlString)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static T DeserializeXMLFileToObject<T>(string XmlFilename)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(XmlFilename)) return default(T);

            try
            {
                StreamReader xmlStream = new StreamReader(XmlFilename);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                returnObject = (T)serializer.Deserialize(xmlStream);
            }
            catch (Exception)
            {
                //InternalError = ex.Message;
            }
            return returnObject;
        }

        public ComunicadoRTF HtmlToComunicado(string htmlExtructure)
        {

            string dsStatus = string.Empty;
            string dsXl = "";
            ComunicadoRTF c = new ComunicadoRTF();
            try
            {

                if (RTFUtil.IsRichText(htmlExtructure))
                {
                    var textExtructure = RTFUtil.StripRTF(htmlExtructure);
                    return ParseComunicadoFromText(textExtructure);
                }
                var nTexto = htmlExtructure
                    .Replace("<xml>", Environment.NewLine + "<xml>")
                    .Replace("</xml>", "</xml>" + Environment.NewLine);

                //nTexto = TrimStartZ(nTexto, "<xml>");
                var linhas = nTexto.ToLines();
                for (int i = 0; i < linhas.Count; i++)
                {
                    var linha = linhas[i];
                    if (linha.Contains("<xml>"))
                    {
                        dsXl = "";
                        var linhaEnd = "</xml>";
                        var nl = linha;
                        int conta = i;
                        while (nl != linhaEnd)
                        {
                            nl = linhas[conta];
                            dsXl += nl;
                            conta++;

                        }
                        // break;
                    }
                }
                bool completaXml = false;
                if (nTexto.Contains("<xml id=") || nTexto.Contains("4LIFE"))
                {
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(htmlExtructure);
                    var noxml = doc.DocumentNode.SelectNodes("/html[1]/body[1]/xml");
                    dsXl = noxml[0].InnerHtml;
                    completaXml = true;
                }

                if (completaXml)
                    c = (ComunicadoRTF)DeserializeFromXmlString<ComunicadoRTFXML>("<xml id=\"4LIFE\">" + dsXl + "</xml>");
                else
                    c = (ComunicadoRTF)DeserializeFromXmlString<ComunicadoRTFXML>(dsXl);


            }
            catch (Exception ex)
            {
                return new ComunicadoRTF { Id = ex.Message, FormatoInvalido = true, FormatoDiferente = true };
                // throw;
            }
            return c;

        }

        public ComunicadoRTF ParseComunicadoFromDoc(string textExtructure)
        {
            ComunicadoRTF c = new ComunicadoRTF();
            textExtructure = textExtructure
                                     .Replace("\v", Environment.NewLine)
                                    .Replace("Empregado CAIXA", Environment.NewLine + "Empregado CAIXA")
                                    .Replace("Empregado FUNCEF", Environment.NewLine + "Empregado FUNCEF")
                                    .Replace("Nome do(a) Agente de Relacionamento", Environment.NewLine + "Nome do(a) Agente de Relacionamento")
                                    .Replace("Data do Comunicado", Environment.NewLine + "Data do Comunicado")
                                    .Replace("Certificado(s) / Bilhete(s)", Environment.NewLine + "Certificado(s) / Bilhete(s)")
                                    .Replace("6.2. Consórcio Prestamista  Produto", Environment.NewLine + "6.2. Consórcio Prestamista  Produto")
                                    .Replace("Data do Sinistro/Ocorrência", Environment.NewLine + "Data do Sinistro/Ocorrência")
                                    .Replace("Nome do Cônjuge ou Filho (sinistrado)", Environment.NewLine + "Nome do Cônjuge ou Filho (sinistrado)")
                                    .Replace("É OBRIGATÓRIO O PREENCHIMENTO", Environment.NewLine + "É OBRIGATÓRIO O PREENCHIMENTO")
                                    .Replace("CNPJ", Environment.NewLine + "CNPJ")
                                    .Replace("\t", " ")
                                    .Replace("E-mail ", Environment.NewLine + "E-mail ")
                                    .Replace("DDD ", Environment.NewLine + "DDD ")
                                    .Replace("Bairro Bairro", "Bairro")
                                    .Replace("Cidade Cidade", "Cidade");


            var listaR = textExtructure.ToLines();
            var newXml = "<xml id=\"4LIFE\"><comunicado id=\"" + DateTime.Now.Ticks.ToString() + "\">";
            for (int i = 0; i < listaR.Count; i++)
            {
                var l = listaR[i];
                if (l.Contains("1 - DADOS DA EMPRESA"))
                {
                    newXml += "<empresa ref=\"1\">";
                    //var lEmpresa = listaR[i + 1]
                    //    .Replace("Nome da Empresa", "<nome>")
                    //    .Replace("CNPJ ", "</nome><cnpj>")
                    //    + "</cnpj></empresa>"
                    //    ;
                    newXml += "<nome>" + listaR[i + 2].Trim() + "</nome><cnpj>" + listaR[i + 4].Trim() + "</cnpj></empresa>";

                }
                if (l.Contains("2 - DADOS DO SEGURADO / PARTICIPANTE / CONSORCIADO"))
                {
                    newXml += "<segurado ref=\"2\">";
                    var lSegurado = "<nome>" + listaR[i + 2].Replace("CPF", "</nome>");
                    var cpf = "<cpf>" + listaR[i + 3].Replace(" Data Nascimento", "").Trim() + " </cpf>";

                    var nascimento = "<nascimento>" + listaR[i + 4].Replace("Sexo", "").Trim() + "</nascimento>";


                    var sexo = "<sexo>" + listaR[i + 5].Trim() + "</sexo>";
                    newXml += lSegurado + cpf + nascimento + sexo + "</segurado>";
                }
                if (l.Contains("3 - DADOS DO COMUNICANTE"))
                {
                    newXml += "<comunicante ref=\"3\">";
                    var tipoComunicante = "<tipo>" + listaR[i + 2].Replace("Matrícula", "").Trim() + "</tipo>";
                    var matricula = "<matricula>" + listaR[i + 3].Replace("Nome", "").Trim() + "</matricula>";
                    var nomeComunicante = "<nome>" + listaR[i + 4].Replace("DDD", "").Trim() + "</nome>";
                    var foneddd = "<foneddd>" + listaR[i + 5].Replace("Telefone", "").Trim() + "</foneddd>";
                    var fonenum = "<fonenum>" + listaR[i + 6].Replace("Telefone", "").Trim() + "</fonenum>";
                    newXml += tipoComunicante + matricula + nomeComunicante + foneddd + fonenum + "</comunicante>";

                }
                if (l.Contains("4 - DADOS DO RECLAMANTE"))
                {
                    newXml += "<reclamante ref=\"4\">";
                    var reclamante = "<nome>" + listaR[i + 2].Replace("Nome ", "").Replace("CPF", "").Trim() + "</nome>";

                    var pCPF = "<cpf>" + listaR[i + 3].Replace("Grau de Parentesco", "").Trim() + "</cpf>";

                    var parentesco = "<parentesco>" + listaR[i + 4].Replace("E-mail", "").Trim() + "</parentesco>";

                    var emailpes = "<emailpes>" + listaR[i + 5].Replace("E-mail", "").Trim() + "</emailpes>";
                    var emailcom = "<emailcom>" + listaR[i + 6].Replace("Endereço", "").Trim() + "</emailcom>";

                    var endereco = "<endereco>" + listaR[i + 7].Replace("Bairro", "").Trim() + "</endereco>";
                    var bairro = "<bairro>" + listaR[i + 8].Replace("Cidade", "").Trim() + "</bairro>";
                    var cidade = "<cidade>" + listaR[i + 9].Replace("CEP", "").Trim() + "</cidade>";
                    var cep = "<cep>" + listaR[i + 10].Replace("UF", "").Trim() + "</cep>";
                    var uf = "<uf>" + listaR[i + 12].Replace("DDD", "").Trim() + "</uf>";
                    var foneresddd = "<foneresddd>" + listaR[i + 13].Replace("Telefone Residencial", "").Trim() + "</foneresddd>";
                    var fonteresnum = "<fonteresnum>" + listaR[i + 14].Replace("DDD", "").Trim() + "</fonteresnum>";
                    var fonecomddd = "<fonecomddd>" + listaR[i + 15].Replace("Telefone Comercial", "").Trim() + "</fonecomddd>";
                    var fonecomnum = "<fonecomnum>" + listaR[i + 16].Replace("DDD", "").Trim() + "</fonecomnum>";
                    var fonecelddd = "<fonecelddd>" + listaR[i + 17].Replace("Telefone Celular", "").Trim() + "</fonecelddd>";
                    var fonecelnum = "<fonecelnum>" + listaR[i + 18].Replace("DDD", "").Trim() + "</fonecelnum>";


                    newXml += reclamante + pCPF + parentesco + emailpes + emailcom + endereco + bairro + cidade + cep + uf + foneresddd + fonteresnum + fonecomddd +
                        fonecomnum + fonecelddd + fonecelnum + "</reclamante>";
                }
                if (l.Contains("5 - DADOS DO SINISTRO"))
                {
                    newXml += "<sinistro ref=\"5\">";
                    var codcobertura = "<codcobertura>" + listaR[i + 2].Replace("Seguro AUTO Fácil - 24 Horas", "").Trim() + "</codcobertura>";
                    var cobertura = "<cobertura>" + listaR[i + 2].Replace("Seguro AUTO Fácil - 24 Horas", "").Trim() + "</cobertura>";
                    var dataacidente = "<dataacidente>" + listaR[i + 4].Replace("Data do Acidente", "").Trim() + "</dataacidente>";

                    var datasinistro = "<datasinistro>" + listaR[i + 6].Trim() + "</datasinistro>";
                    var conjugeoufilho = "<conjugeoufilho>" + listaR[i + 8].Replace("CPF", "").Trim() + "</conjugeoufilho>";
                    var conjugeoufilhocpf = "<cpf>" + listaR[i + 9].Replace("Data de Nascimento", "").Trim() + "</cpf>";

                    var nascimento = "<nascimento>" + listaR[i + 10].Replace("Histórico do Sinistro (BREVE RELATO)", "").Trim() + "</nascimento>";

                    var relato = "<relato>" + listaR[i + 11].Replace("Histórico do Sinistro (BREVE RELATO)", "").Trim() + "</relato>";

                    var empregadocaixa = "<empregadocaixa>" + listaR[i + 13].Trim() + "</empregadocaixa>";
                    var empregadofuncef = "<empregadofuncef>" + listaR[i + 15].Trim() + "</empregadofuncef>";

                    newXml += codcobertura + cobertura + dataacidente + datasinistro + conjugeoufilho + conjugeoufilhocpf + nascimento + relato + empregadocaixa + empregadofuncef + "</sinistro>";
                }
                if (l.Contains("6 - DADOS DO PRODUTO/CONTRATO"))
                {
                    // newXml += "<produtos ref=\"6\">";
                    var produtonome = "<produtonome>" + listaR[i + 2].Trim() + "</produtonome>";
                    var certificado = "<certificados><certificado>" + listaR[i + 4].Replace("6.2. Consórcio Prestamista  Produto", "").Trim() + "</certificado></certificados>";
                    var produtoCons = "<produto><produtonome /><consorcios><consorcio grupo=\"\" cota=\"\" /></consorcios></produto><produto><produtonome /><consorcios><consorcio grupo=\"\" cota=\"\" /></consorcios></produto><produto><produtonome /><certificados><certificado /></certificados></produto><produto><produtonome /><certificados><certificado /></certificados></produto>";
                    var contrato = "<contrato ref=\"7\"><numero /><adesao /><cartacredito /><prazo /><consorcio grupo=\"\" cota=\"\" /><enderecoimovel /><bairro /><cidade /><cep /><uf /><mesanori /><danosmateriais><pontorefimovel /><horariovisita /><fonecontato /><enderecoalternativoexiste /><enderecoalternativo /><valorindenizacao /><localchaves /><instututofiliacao /></danosmateriais></contrato>";

                    newXml += "<produtos ref=\"6\"><produto>" + produtonome + certificado + "</produto>" + produtoCons + "</produtos>" + contrato;
                }
                if (l.Contains("9. FINALIZAÇÃO DO COMUNICADO"))
                {
                    newXml += "<finalizacao ref=\"8\">";
                    var datacomunicado = "<datacomunicado>" + listaR[i + 3].Trim() + "</datacomunicado>";
                    var agente = "<nomeagenterelacionamento>" + listaR[i + 5].Trim() + "</nomeagenterelacionamento>";
                    newXml += datacomunicado + agente + "</finalizacao>";
                }

            }
            const string reduceMultiSpace = @"[ ]{2,}";
            var line = Regex.Replace(newXml.Replace("\t", " "), reduceMultiSpace, " ");
            newXml = line + "</comunicado></xml>";
            var b = DeserializeFromXmlString<ComunicadoRTFXML>(newXml);
            c = (ComunicadoRTF)b;
            c.Comunicado.Sinistro.Cobertura = c.Comunicado.Sinistro.Codcobertura;
            c.FormatoDiferente = true;
            return c;
        }
        public static ComunicadoRTF ParseComunicadoFromText(string textExtructure)
        {
            ComunicadoRTF c = new ComunicadoRTF();
            textExtructure = textExtructure
                                    .Replace("E-mail ", Environment.NewLine + "E-mail ")
                                    .Replace("DDD ", Environment.NewLine + "DDD ")
                                    .Replace("Bairro Bairro", "Bairro")
                                    .Replace("Cidade Cidade", "Cidade");

            var listaR = textExtructure.ToLines();
            var newXml = "<xml id=\"4LIFE\"><comunicado id=\"" + DateTime.Now.Ticks.ToString() + "\">";
            for (int i = 0; i < listaR.Count; i++)
            {
                var l = listaR[i];
                if (l.Contains("1 - DADOS DA EMPRESA "))
                {
                    newXml += "<empresa ref=\"1\">";
                    var lEmpresa = listaR[i + 1]
                        .Replace("Nome da Empresa", "<nome>")
                        .Replace("CNPJ ", "</nome><cnpj>")
                        + "</cnpj></empresa>"
                        ;
                    newXml += lEmpresa;

                }
                if (l.Contains("2 - DADOS DO SEGURADO / PARTICIPANTE / CONSORCIADO"))
                {
                    newXml += "<segurado ref=\"2\">";
                    var lSegurado = listaR[i + 1]
                        .Replace("Nome ", "<nome>")
                        .Replace("CPF ", "</nome><cpf>");
                    var linhaNasc = listaR[i + 2];
                    if (listaR[i + 1].Contains("Data Nasciment"))
                    {
                        linhaNasc = lSegurado;
                        lSegurado = "";
                    }
                    var lSegurado2 = linhaNasc
                        .Replace("Data Nascimento ", "</cpf><nascimento>")
                        .Replace("Sexo ", "</nascimento><sexo>")
                        + "</sexo></segurado>"
                        ;
                    newXml += lSegurado + lSegurado2;
                }
                if (l.Contains("3 - DADOS DO COMUNICANTE "))
                {
                    newXml += "<comunicante ref=\"3\">";
                    var lEmpresa = listaR[i + 1]
                        .Replace("Comunicante é ", "<tipo>")
                        .Replace("Matrícula ", "</tipo><matricula>");
                    var lNomeCom = listaR[i + 2]
                        .Replace("Nome ", "</matricula><nome>");
                    var lTelefone = listaR[i + 3]
                        .Replace("DDD ", "</nome><foneddd>")
                        .Replace("Telefone ", "</foneddd><fonenum>")
                        + "</fonenum></comunicante>"
                        ;
                    newXml += lEmpresa + lNomeCom + lTelefone;
                }
                if (l.Contains("4 - DADOS DO RECLAMANTE "))
                {
                    newXml += "<reclamante ref=\"4\">";
                    var lReclamante = listaR[i + 1]
                        .Replace("Nome ", "<nome>")
                        .Replace("CPF ", "</nome><cpf>");
                    var lParentesco = listaR[i + 2]
                        .Replace("Grau de Parentesco ", "</cpf><parentesco>");
                    var lDadosEmailPes = listaR[i + 4]
                        .Replace("E-mail ", "</parentesco><emailpes>");
                    var lDadosEmailcom = listaR[i + 5]
                        .Replace("E-mail ", "</emailpes><emailcom>");
                    var lDadosEndereco = listaR[i + 6]
                        .Replace("Endereço ", "</emailcom><endereco>")
                        .Replace("Bairro ", "</endereco><bairro>")
                        .Replace("Cidade ", "</bairro><cidade>")
                        .Replace("CEP ", "</cidade><cep>");
                    var lDadosEndereco2 = listaR[i + 8]
                        .Replace("UF ", "</cep><uf>");
                    var lDadosEndereco3 = listaR[i + 9]
                        .Replace("DDD ", "</uf><foneresddd>")
                        .Replace("Telefone Residencial ", "</foneresddd><fonteresnum>");
                    var lDadosEndereco4 = listaR[i + 10]
                        .Replace("DDD ", "</fonteresnum><fonecomddd>")
                        .Replace("Telefone Comercial ", "</fonecomddd><fonecomnum>");
                    var lDadosEndereco5 = listaR[i + 11]
                        .Replace("DDD ", "</fonecomnum><fonecelddd>")
                        .Replace("Telefone Celular ", "</fonecelddd><fonecelnum>")
                        + "</fonecelnum></reclamante>"
                        ;
                    newXml += lReclamante + lParentesco + lDadosEmailPes + lDadosEmailcom + lDadosEndereco + lDadosEndereco2 + lDadosEndereco3 + lDadosEndereco4 + lDadosEndereco5;
                }
                if (l.Contains("5 - DADOS DO SINISTRO "))
                {
                    newXml += "<sinistro ref=\"5\">";
                    var lSinistro = listaR[i + 1]
                        .Replace("Cobertura Pleiteada ", "<codcobertura>")//")
                        .Replace("Seguro AUTO Fácil - 24 Horas ", "</codcobertura><cobertura></cobertura><outrosinistro />")
                        .Replace("Data do Acidente ", "<dataacidente>")
                        .Replace("Data do Sinistro/Ocorrência ", "</dataacidente><datasinistro>");
                    var lSinistro2 = listaR[i + 2]
                        .Replace("Nome do Cônjuge ou Filho (sinistrado) ", "</datasinistro><conjugeoufilho>")
                        .Replace("CPF ", "</conjugeoufilho><cpf>")
                        .Replace("Data de Nascimento ", "</cpf><nascimento>");
                    var lSinistro3 = listaR[i + 3]
                        .Replace("Histórico do Sinistro (BREVE RELATO) ", "</nascimento><relato>");
                    var lSinistro4 = listaR[i + 4]
                        .Replace("Empregado CAIXA ", "</relato><empregadocaixa>")
                        .Replace("Empregado FUNCEF ", "</empregadocaixa><empregadofuncef>")
                    + "</empregadofuncef></sinistro>"
                        ;
                    newXml += lSinistro + lSinistro2 + lSinistro3 + lSinistro4;
                }
                if (l.Contains("6 - DADOS DO PRODUTO/CONTRATO "))
                {
                    newXml += "<produtos ref=\"6\"><produto>";
                    var lProduto = listaR[i + 2]
                        .Replace("Produto ", "<produtonome>")
                        .Replace("Certificado(s) / Bilhete(s) ", "</produtonome><certificados><certificado>")
                        + "</certificado>        </certificados>      </produto>      <produto>        <produtonome />        <consorcios>          <consorcio grupo=\"\" cota=\"\" />        </consorcios>      </produto>      <produto>        <produtonome />        <consorcios>          <consorcio grupo=\"\" cota=\"\" />        </consorcios>      </produto>      <produto>        <produtonome />        <certificados>          <certificado />        </certificados>      </produto>      <produto>        <produtonome />        <certificados>          <certificado />        </certificados>      </produto>    </produtos><contrato ref=\"7\">      <numero />      <adesao />      <cartacredito />      <prazo />      <consorcio grupo=\"\" cota=\"\" />      <enderecoimovel />      <bairro />      <cidade />      <cep />      <uf />      <mesanori />      <danosmateriais>        <pontorefimovel />        <horariovisita />        <fonecontato />        <enderecoalternativoexiste />        <enderecoalternativo />        <valorindenizacao />        <localchaves />        <instututofiliacao />      </danosmateriais>    </contrato>"
                        ;
                    newXml += lProduto;
                }
                if (l.Contains("9. FINALIZAÇÃO DO COMUNICADO "))
                {
                    newXml += "<finalizacao ref=\"8\">";
                    var lFinal = listaR[i + 2]
                        .Replace("Data do Comunicado ", "<datacomunicado>")
                        .Replace("Nome do(a) Agente de Relacionamento ", "</datacomunicado><nomeagenterelacionamento>")
                        + "</nomeagenterelacionamento></finalizacao>"
                        ;
                    newXml += lFinal;
                }

            }
            const string reduceMultiSpace = @"[ ]{2,}";
            var line = Regex.Replace(newXml.Replace("\t", " "), reduceMultiSpace, " ");
            newXml = line + "</comunicado></xml>";
            var b = DeserializeFromXmlString<ComunicadoRTFXML>(newXml);
            c = (ComunicadoRTF)b;
            c.Comunicado.Sinistro.Cobertura = c.Comunicado.Sinistro.Codcobertura;
            c.FormatoDiferente = true;
            return c;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        private static string MesclarTemplate(ComunicadoRTFXML cs, string xmlStructure)
        {
            var template = "";// Properties.Resources.TEMPLATE;
            var linhas = template.ToLines();
            foreach (var l in linhas)
            {
                if (l.Contains("[empresanome]")) template = template.Replace("[empresanome]", cs.Comunicado.Empresa.Nome);
                if (l.Contains("[empresacnpj]")) template = template.Replace("[empresacnpj]", cs.Comunicado.Empresa.Cnpj);
                if (l.Contains("[seguradonome]")) template = template.Replace("[seguradonome]", cs.Comunicado.Segurado.Nome);
                if (l.Contains("[seguradocpf]")) template = template.Replace("[seguradocpf]", cs.Comunicado.Segurado.Cpf);
                if (l.Contains("[seguradonascimento]")) template = template.Replace("[seguradonascimento]", cs.Comunicado.Segurado.Nascimento);
                if (l.Contains("[seguradosexo]")) template = template.Replace("[seguradosexo]", cs.Comunicado.Segurado.Sexo);
                if (l.Contains("[reclamantenome]")) template = template.Replace("[reclamantenome]", cs.Comunicado.Reclamante.Nome);
                if (l.Contains("[reclamantecpf]")) template = template.Replace("[reclamantecpf]", cs.Comunicado.Reclamante.Cpf);
                if (l.Contains("[reclamanteparentesco]")) template = template.Replace("[reclamanteparentesco]", cs.Comunicado.Reclamante.Parentesco);
                if (l.Contains("[reclamanteemailpes]")) template = template.Replace("[reclamanteemailpes]", cs.Comunicado.Reclamante.Emailpes);
                if (l.Contains("[reclamanteemailcom]")) template = template.Replace("[reclamanteemailcom]", cs.Comunicado.Reclamante.Emailcom);
                if (l.Contains("[reclamanteendereco]")) template = template.Replace("[reclamanteendereco]", cs.Comunicado.Reclamante.Endereco);
                if (l.Contains("[reclamantebairro]")) template = template.Replace("[reclamantebairro]", cs.Comunicado.Reclamante.Bairro);
                if (l.Contains("[reclamantecidade]")) template = template.Replace("[reclamantecidade]", cs.Comunicado.Reclamante.Cidade);
                if (l.Contains("[reclamantecep]")) template = template.Replace("[reclamantecep]", cs.Comunicado.Reclamante.Cep);
                if (l.Contains("[reclamanteuf]")) template = template.Replace("[reclamanteuf]", cs.Comunicado.Reclamante.Uf);
                if (l.Contains("[reclamantefoneresddd]")) template = template.Replace("[reclamantefoneresddd]", cs.Comunicado.Reclamante.Foneresddd);
                if (l.Contains("[reclamantefonteresnum]")) template = template.Replace("[reclamantefonteresnum]", cs.Comunicado.Reclamante.Fonteresnum);
                if (l.Contains("[reclamantefonecomddd]")) template = template.Replace("[reclamantefonecomddd]", cs.Comunicado.Reclamante.Fonecomddd);
                if (l.Contains("[reclamantefonecomnum]")) template = template.Replace("[reclamantefonecomnum]", cs.Comunicado.Reclamante.Fonecomnum);
                if (l.Contains("[reclamantefonecelddd]")) template = template.Replace("[reclamantefonecelddd]", cs.Comunicado.Reclamante.Fonecelddd);
                if (l.Contains("[reclamantefonecelnum]")) template = template.Replace("[reclamantefonecelnum]", cs.Comunicado.Reclamante.Fonecelnum);
                if (l.Contains("[sinistrocodcobertura]")) template = template.Replace("[sinistrocodcobertura]", cs.Comunicado.Sinistro.Codcobertura);
                if (l.Contains("[sinistrodataacidente]")) template = template.Replace("[sinistrodataacidente]", cs.Comunicado.Sinistro.Dataacidente);
                if (l.Contains("[sinistrodatasinistro]")) template = template.Replace("[sinistrodatasinistro]", cs.Comunicado.Sinistro.Datasinistro);
                if (l.Contains("[sinistroconjugeoufilho]")) template = template.Replace("[sinistroconjugeoufilho]", cs.Comunicado.Sinistro.Conjugeoufilho);
                if (l.Contains("[sinistrocpf]")) template = template.Replace("[sinistrocpf]", cs.Comunicado.Sinistro.Cpf);
                if (l.Contains("[sinistronascimento]")) template = template.Replace("[sinistronascimento]", cs.Comunicado.Sinistro.Nascimento);
                if (l.Contains("[sinistrorelato]")) template = template.Replace("[sinistrorelato]", cs.Comunicado.Sinistro.Relato);
                if (l.Contains("[sinistroempregadocaixa]")) template = template.Replace("[sinistroempregadocaixa]", cs.Comunicado.Sinistro.Empregadocaixa);
                if (l.Contains("[sinistroempregadofuncef]")) template = template.Replace("[sinistroempregadofuncef]", cs.Comunicado.Sinistro.Empregadofuncef);
                if (l.Contains("[produtonome]")) template = template.Replace("[produtonome]", cs.Comunicado.Produtos.Produto.First().Produtonome);
                if (l.Contains("[certificado]")) template = template.Replace("[certificado]", cs.Comunicado.Produtos.Produto.First().Certificados.Certificado);
                if (l.Contains("[finalizacaodatacomunicado]")) template = template.Replace("[finalizacaodatacomunicado]", cs.Comunicado.Finalizacao.Datacomunicado);
                if (l.Contains("[finalizacaonomeagenterelacionamento]")) template = template.Replace("[finalizacaodfinalizacaonomeagenterelacionamentoatacomunicado]", cs.Comunicado.Finalizacao.Nomeagenterelacionamento);
                if (l.Contains("[xmlfile]")) template = template.Replace("[xmlfile]", xmlStructure);
            }
            return template;
        }

        public static Encoding GetEncoding(string filename)
        {
            using (StreamReader sr = new StreamReader(filename, true))
            {
                while (sr.Peek() >= 0)
                {
                    Console.Write((char)sr.Read());
                }

                //Test for the encoding after reading, or at least
                //after the first read.
                return sr.CurrentEncoding;
            }
            // Read the BOM
            //    var bom = new byte[4];
            //using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //{
            //    file.Read(bom, 0, 4);
            //}

            //// Analyze the BOM
            //if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            //if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            //if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            //if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            //if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            //return Encoding.ASCII;
        }

        public string ConvertToXml(ComunicadoRTFXML comunicado)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = this.DefailtEncoding
            };
            XmlSerializer xsSubmit = new XmlSerializer(typeof(ComunicadoRTFXML));

            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww, xmlWriterSettings))
                {
                    xsSubmit.Serialize(writer, comunicado);
                    xml = sww.ToString(); // Your XML                    
                }
            }
            var xmlCoded = Encoding.UTF8.GetString(Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(xml)));
            return xmlCoded;
        }

        public static void SaveXML(ComunicadoRTFXML cs, string outFile, Encoding enc)
        {
            var f = new Fixer();
            f.DefailtEncoding = enc;
            var xmlCoded = f.ConvertToXml(cs);
            string templateMesclado = MesclarTemplate(cs, xmlCoded);

            File.WriteAllText(outFile, templateMesclado, enc);
        }

    }
}
