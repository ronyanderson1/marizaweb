using System.DirectoryServices;
using System.Text;
using System.Text.Json;
using marizaweb;
using Oracle.ManagedDataAccess.Client;
using System.Data;

var policyName = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
                      policy =>
                      {
                          policy.WithOrigins("https://marizaweb.netlify.app");
                       });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
    c.SwaggerDoc(
        "v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Version = "v1",
            Title = "ASP.NEt Web Api para autenticar usuário no Active Directory",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "José Iramar Martins Cândido",
                Url = new Uri("https://example.com/contact"),
            },
            License = new Microsoft.OpenApi.Models.OpenApiLicense
            {
                Name = "Licença de uso",
                Url = new Uri("https://example.com/License"),
            }
        }
        )
    );

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "APP API v1");
});


app.MapGet("/ObterAutenticacaoUsuario/{usuario}/{senha}", async (string usuario, string senha) =>
{
    string? nomeUsuario = "";
    bool retorno = false;

    try
    {
        DirectoryEntry obj_de = new DirectoryEntry("LDAP://192.168.1.2", usuario, senha);
        DirectorySearcher obj_dsearch = new DirectorySearcher(obj_de);
        SearchResult _sResult = null;
        var sResult = obj_dsearch.FindOne();
       
        nomeUsuario = "Autenticado";
    }
    catch (Exception ex)
    {
        nomeUsuario = ex.Message;
    }

    var login = new Login
    {
        nomeusuario = nomeUsuario
    };

    return JsonSerializer.Serialize(login);
});
app.MapGet("/ObtemDadosPedidosPendentesComposicaoFrete/{dtinicial}" +
                                                     "/{dtfinal}" +
                                                     "/{empresa}" +
                                                     "/{fragcliente}" +
                                                     "/{fragcidade}", async (string dtinicial,
                                                                             string dtfinal,
                                                                             string empresa,
                                                                             string fragcliente,
                                                                             string fragcidade) =>
                                                     {
                                                         string jsonString = "";
                                                         string erro = "";
                                                         OracleConnection connection = new OracleConnection();

                                                         try
                                                         {
                                                             //Pedido pedido = null;
                                                             List<Pedido> pedido = new List<Pedido>();


                                                             string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.254)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = SAIB)));User ID=SAIB2000;Password=M4R1Z4; ";
                                                             //string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.21)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = HOMOLOG)));User ID=SAIB2000;Password=M4R1Z4; ";
                                                             connection.ConnectionString = connString;
                                                             connection.Open();
                                                             if (connection != null)
                                                             {
                                                                 erro = "consegui conectar";
                                                                 StringBuilder str = new StringBuilder();
                                                                 str.Remove(0, str.Length);
                                                                 str.Append("SELECT TO_CHAR(mpf.PEDF_DTA_EMIS,'DD/MM/YYYY') AS datafat, ");
                                                                 str.Append("'Localizar' AS gerente, ");
                                                                 str.Append("'Localizar' AS rca, ");
                                                                 str.Append("mpf.pedf_liqu_id liquidacao, ");
                                                                 str.Append("mpf.pedf_nr_nf AS notafiscal, ");
                                                                 str.Append("mpf.PEDF_ID numped, ");
                                                                 str.Append("mpf.PEDF_CLI_ID codcli, ");
                                                                 str.Append("c.CLI_FANTASIA  razaosocial, ");
                                                                 str.Append("G.GEN_ID idcidade, ");
                                                                 str.Append("g.GEN_DESCRICAO || ' - ' || g1.GEN_DESCRICAO nomecidade, ");
                                                                 str.Append("g1.GEN_DESCRICAO uf, ");
                                                                 str.Append("mpf.PEDF_PEDF_ID_VENDA pedvenda, ");
                                                                 str.Append("mpf.PEDF_ID_LIBERADO pedfat, ");
                                                                 str.Append("g2.GEN_DESCRICAO Tipo, ");
                                                                 str.Append("g2.GEN_ID cdtipo, ");
                                                                 str.Append("OP.SIT_DESCRICAO DESCRICAOSITUACAO, ");

                                                                 str.Append("(SELECT REPLACE(sum(mpfp.PEDF_QTDE * p.PROF_PESO_B),',','.')");
                                                                 str.Append("FROM MA_PEDIDO_FAT_P mpfp, ");
                                                                 str.Append("PRODUTO_FAT p ");
                                                                 str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                                 str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID AND ");
                                                                 str.Append("mpfp.PEDF_PROD_EMP_ID = p.PROF_PROD_EMP_ID  AND ");
                                                                 str.Append("mpfp.PEDF_PROD_ID = p.PROF_PROD_ID) AS peso, ");

                                                                 str.Append("(SELECT sum(mpfp.PEDF_QTDE) ");
                                                                 str.Append("FROM MA_PEDIDO_FAT_P mpfp, ");
                                                                 str.Append("PRODUTO_FAT p ");
                                                                 str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                                 str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID AND ");
                                                                 str.Append("mpfp.PEDF_PROD_EMP_ID = p.PROF_PROD_EMP_ID  AND ");
                                                                 str.Append("mpfp.PEDF_PROD_ID = p.PROF_PROD_ID) AS qtdcaixa, ");

                                                                 str.Append("(SELECT ROUND((sum(p.PROF_PESO_B)/1060),0) ");
                                                                 str.Append("FROM MA_PEDIDO_FAT_P mpfp, ");
                                                                 str.Append(" PRODUTO_FAT p ");
                                                                 str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                                 str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID AND ");
                                                                 str.Append("mpfp.PEDF_PROD_EMP_ID = p.PROF_PROD_EMP_ID  AND ");
                                                                 str.Append("mpfp.PEDF_PROD_ID = p.PROF_PROD_ID) AS palet, ");

                                                                 str.Append("(SELECT REPLACE((ROUND(sum(mpfp.PEDF_VLR_TOT),2)),',','.') ");
                                                                 str.Append("FROM MA_PEDIDO_FAT_P mpfp ");
                                                                 str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                                 str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID) AS vlrpedido, ");

                                                                 str.Append("(SELECT L.LIQU_PLACA_TRANSP ");
                                                                 str.Append("FROM LIQUIDACAO l ");
                                                                 str.Append("WHERE L.LIQU_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                                 str.Append("L.LIQU_ID = mpf.pedf_liqu_id AND ");
                                                                 str.Append("mpf.pedf_liqu_id >1) AS placa, ");

                                                                 str.Append("(SELECT REPLACE(PFL.PEDL_VLR_FRETE_CONT,',','.') ");
                                                                 str.Append("FROM PEDIDO_FAT_LOGISTICA pfl ");
                                                                 str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                                 str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID ) AS vlrfrete, ");


                                                                 str.Append("(SELECT CASE maf.AGPF_TIPO_VEICULO ");
                                                                 str.Append("WHEN 'C' ");
                                                                 str.Append("THEN 'Carreta' ");
                                                                 str.Append("WHEN 'T' ");
                                                                 str.Append("THEN 'Truck' ");
                                                                 str.Append("END ");
                                                                 str.Append("FROM MA_AGRUPAMENTO_FRETE maf ");
                                                                 str.Append("WHERE MAF.AGPF_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                                 str.Append("MAF.AGPF_ID = MPF.PEDF_AGRF_ID) AS tipoveiculo, ");

                                                                 str.Append("(SELECT SUM(maf.AGPF_NUMERO_ENTREGAS) ");
                                                                 str.Append("FROM MA_AGRUPAMENTO_FRETE maf ");
                                                                 str.Append("WHERE MAF.AGPF_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                                 str.Append("MAF.AGPF_ID = MPF.PEDF_AGRF_ID) AS qtdentregas, ");

                                                                 str.Append("(SELECT F.FRO_DESC ");
                                                                 str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                                 str.Append("FROTA f ");
                                                                 str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                                 str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                                 str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                                 str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS transportador, ");

                                                                 str.Append("(SELECT PFL.PEDL_NOME_MOTORISTA ");
                                                                 str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                                 str.Append("FROTA f ");
                                                                 str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                                 str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                                 str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                                 str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS motorista, ");

                                                                 str.Append("(SELECT NVL(TO_CHAR(PFL.PEDL_DTA_COLETA,'DD/MM/YYYY'),'0')  ");
                                                                 str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                                 str.Append("FROTA f ");
                                                                 str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                                 str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                                 str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID ");
                                                                 str.Append("AND PFL.PEDL_FRO_ID = F.FRO_ID) AS carregamento, ");

                                                                 str.Append("(SELECT PFL.PEDL_HORA_COLETA ");
                                                                 str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                                 str.Append("FROTA f ");
                                                                 str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                                 str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                                 str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                                 str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS horario, ");

                                                                 str.Append("(SELECT DISTINCT(TRUNC(sysdate) - TRUNC(SPF.PEDF_DTA_EMIS)) + 1 ");
                                                                 str.Append("FROM PEDIDO_FAT SPF ");
                                                                 str.Append("WHERE MPF.PEDF_LIQU_EMP_ID = SPF.PEDF_LIQU_EMP_ID AND ");
                                                                 str.Append("MPF.PEDF_LIQU_ID = SPF.PEDF_LIQU_ID AND ");
                                                                 str.Append("SPF.PEDF_NR_NF >0) as prazonota,");

                                                                 str.Append("(SELECT DISTINCT 'AGUA' ");
                                                                 str.Append("FROM PRODUTO P, ");
                                                                 str.Append("PRODUTO_C PC, ");
                                                                 str.Append("GENER MC, ");
                                                                 str.Append("PRODUTO_TP TP, ");
                                                                 str.Append("GENER CT, ");
                                                                 str.Append("MA_PEDIDO_FAT_P mpfp ");
                                                                 str.Append("WHERE mpfp.PEDF_PROD_EMP_ID = P.PROD_EMP_ID ");
                                                                 str.Append("AND mpfp.PEDF_PROD_ID = P.PROD_ID ");
                                                                 str.Append("AND     P.PROD_EMP_ID = PC.PROC_PROD_EMP_ID AND P.PROD_ID = PC.PROC_PROD_ID ");
                                                                 str.Append("AND     PC.PROC_GEN_TGEN_ID_MARCA_DE = MC.GEN_TGEN_ID ");
                                                                 str.Append("AND     PC.PROC_GEN_EMP_ID_MARCA_DE = MC.GEN_EMP_ID ");
                                                                 str.Append("AND     PC.PROC_GEN_ID_MARCA_DE = MC.GEN_ID ");
                                                                 str.Append("AND     P.PROD_ID = TP.PROT_PROD_ID AND P.PROD_EMP_ID = TP.PROT_PROD_EMP_ID ");
                                                                 str.Append("AND     TP.PROT_GEN_TGEN_ID = CT.GEN_TGEN_ID AND TP.PROT_GEN_ID = CT.GEN_ID ");
                                                                 str.Append("AND     CT.GEN_ID = 3 ");
                                                                 str.Append("AND     MC.GEN_ID = 3 ");
                                                                 str.Append("AND     mpfp.PEDF_PEDF_EMP_ID = MPF.PEDF_EMP_ID ");
                                                                 str.Append("AND     mpfp.PEDF_PEDF_ID = MPF.PEDF_ID) AS categoria, ");

                                                                 str.Append("CASE mpf.PEDF_RETIRAR ");
                                                                 str.Append("WHEN 1 ");
                                                                 str.Append("THEN 'GESTÃO' ");
                                                                 str.Append("WHEN 2 ");
                                                                 str.Append("THEN 'INDÚSTRIA' ");
                                                                 str.Append("END AS RETIRAR, ");
                                                                 str.Append("CASE mpf.PEDF_ENTREGA ");
                                                                 str.Append("WHEN 1 ");
                                                                 str.Append("THEN 'AGENDAR' ");
                                                                 str.Append("WHEN 2 ");
                                                                 str.Append("THEN 'IMEDIATO' ");
                                                                 str.Append("WHEN 3 ");
                                                                 str.Append("THEN 'AGENDADO' ");
                                                                 str.Append("END AS ENTREGA, ");
                                                                 str.Append("TO_CHAR(mpf.PEDF_DATA_ENTREGA_AGENDADA,'DD/MM/YYYY') AS DTAENTREGAAGENDADA, ");
                                                                 str.Append("mpf.PEDF_OBS_LOGISTICA OBSLOGISTICA, ");
                                                                 str.Append("mpf.PEDF_FIFO FIFO, ");
                                                                 str.Append("CASE mpf.PEDF_TP_FRETE ");
                                                                 str.Append("WHEN '0' ");
                                                                 str.Append("THEN 'CIF' ");
                                                                 str.Append("WHEN '1' ");
                                                                 str.Append("THEN 'FOB' ");
                                                                 str.Append("END AS TPFRETE ");

                                                                 str.Append("FROM MA_PEDIDO_FAT mpf, ");
                                                                 str.Append("CLIENTE c, ");
                                                                 str.Append("CLIENTE_E ce, ");
                                                                 str.Append("GENER g, ");
                                                                 str.Append("GENER g1, ");
                                                                 str.Append("gener g2, ");
                                                                 str.Append("OPERACAO_FAT of2, ");
                                                                 str.Append("MA_SITUACAO_PEDIDO op ");

                                                                 str.Append("WHERE mpf.PEDF_EMP_ID = c.CLI_EMP_ID AND ");
                                                                 str.Append("mpf.PEDF_CLI_ID = c.CLI_ID AND ");
                                                                 str.Append("mpf.PEDF_EMP_ID = OP.SIT_EMP_ID AND ");
                                                                 str.Append("mpf.PEDF_SITUACAO = OP.SIT_ID AND ");
                                                                 str.Append("ce.CLIE_CLI_EMP_ID = c.CLI_EMP_ID AND ");
                                                                 str.Append("ce.CLIE_CLI_ID = c.CLI_ID AND ");
                                                                 str.Append("ce.CLIE_GEN_tgen_ID_CIDADE_DE = g.GEN_TGEN_ID  AND ");
                                                                 str.Append("ce.CLIE_GEN_EMP_ID = g.GEN_EMP_ID AND ");
                                                                 str.Append("ce.CLIE_GEN_ID_CIDADE_DE = g.GEN_ID AND ");
                                                                 str.Append("ce.CLIE_GEN_tgen_ID_CIDADE_DE = 5001 AND ");
                                                                 str.Append("mpf.pedf_emp_id = of2.oper_emp_id AND ");
                                                                 str.Append("mpf.pedf_oper_id = of2.oper_id AND ");
                                                                 str.Append("of2.oper_gen_emp_id_tp_operacao_de = g2.gen_emp_id AND  ");
                                                                 str.Append("of2.oper_gen_id_tp_operacao_de = g2.gen_id AND  ");
                                                                 str.Append("g2.gen_tgen_id = 9013 AND ");
                                                                 str.Append("SUBSTR(g.GEN_TEXT1,1,2) = g1.gen_number3 AND ");
                                                                 str.Append("g1.GEN_TGEN_ID = 5006 AND ");
                                                                 str.Append("TO_CHAR(mpf.PEDF_DTA_EMIS,'YYYYMMDD') BETWEEN " + dtinicial + " AND " + dtfinal);
                                                                 str.Append(" and mpf.PEDF_SITUACAO in(9) ");
                                                                 str.Append("and mpf.PEDF_EMP_ID = " + empresa);
                                                                 if (fragcliente != "@")
                                                                 {
                                                                     str.Append(" AND c.CLI_FANTASIA like '%" + fragcliente + "%'");
                                                                 }
                                                                 if (fragcidade != "@")
                                                                 {
                                                                     str.Append(" AND g.GEN_DESCRICAO like '%" + fragcidade + "%'");
                                                                 }
                                                                 str.Append(" ORDER BY mpf.PEDF_DTA_EMIS, mpf.PEDF_CLI_ID, mpf.PEDF_ID ASC ");
                                                                 using (OracleCommand cmd = new OracleCommand(str.ToString(), connection))
                                                                 {
                                                                     using (OracleDataReader reader = cmd.ExecuteReader())
                                                                     {
                                                                         if (reader.HasRows)
                                                                         {
                                                                             while (reader.Read())
                                                                             {
                                                                                 string categoriaAux = "";

                                                                                 if (reader["categoria"].ToString().Trim().Length == 0)
                                                                                 {
                                                                                     categoriaAux = "NA";
                                                                                 }
                                                                                 else
                                                                                 {
                                                                                     categoriaAux = reader["categoria"].ToString();
                                                                                 }
                                                                                 pedido.Add(new Pedido()
                                                                                 {
                                                                                     datafat = reader["datafat"].ToString(),
                                                                                     gerente = reader["gerente"].ToString(),
                                                                                     rca = reader["rca"].ToString(),
                                                                                     liquidacao = reader["liquidacao"].ToString(),
                                                                                     notafiscal = reader["notafiscal"].ToString(),
                                                                                     numped = reader["numped"].ToString(),
                                                                                     codcli = reader["codcli"].ToString(),
                                                                                     razaosocial = reader["razaosocial"].ToString(),
                                                                                     idcidade = reader["idcidade"].ToString(),
                                                                                     nomecidade = reader["nomecidade"].ToString(),
                                                                                     uf = reader["uf"].ToString(),
                                                                                     pedvenda = reader["pedvenda"].ToString(),
                                                                                     pedfat = reader["pedfat"].ToString(),
                                                                                     tipo = reader["tipo"].ToString(),
                                                                                     peso = reader["peso"].ToString(),
                                                                                     qtdcaixa = reader["qtdcaixa"].ToString(),
                                                                                     palet = reader["palet"].ToString(),
                                                                                     vlrpedido = reader["vlrpedido"].ToString(),
                                                                                     placa = reader["placa"].ToString(),
                                                                                     vlrfrete = reader["vlrfrete"].ToString(),
                                                                                     tipoveiculo = reader["tipoveiculo"].ToString(),
                                                                                     qtdentregas = reader["qtdentregas"].ToString(),
                                                                                     transportador = reader["transportador"].ToString(),
                                                                                     motorista = reader["motorista"].ToString(),
                                                                                     carregamento = reader["carregamento"].ToString(),
                                                                                     horario = reader["horario"].ToString(),
                                                                                     prazonota = reader["prazonota"].ToString(),
                                                                                     descricaosituacao = reader["descricaosituacao"].ToString(),
                                                                                     categoria = categoriaAux,
                                                                                     retirar = reader["retirar"].ToString(),
                                                                                     entrega = reader["entrega"].ToString(),
                                                                                     dtaentregaagendada = reader["dtaentregaagendada"].ToString(),
                                                                                     obslogistica = reader["obslogistica"].ToString(),
                                                                                     tpfrete = reader["tpfrete"].ToString(),
                                                                                     fifo = reader["fifo"].ToString(),
                                                                                     cdtipo = reader["cdtipo"].ToString(),
                                                                                 });
                                                                             }
                                                                             reader.Close();
                                                                             jsonString = JsonSerializer.Serialize(pedido);
                                                                         }
                                                                     }
                                                                 }
                                                             }
                                                         }
                                                         catch (Exception ex)
                                                         {
                                                             var erroretorno = new ErroRetorno
                                                             {
                                                                 mensagem = ex.Message
                                                             };
                                                             jsonString = JsonSerializer.Serialize(erroretorno);
                                                         }
                                                         finally
                                                         {
                                                             if (connection != null)
                                                             {
                                                                 connection.Close();
                                                             }
                                                         }
                                                         return jsonString;
                                                     });

app.MapGet("/ObtemDadosPedidosGerencial/{dtinicial}" +
                                      "/{dtfinal}" +
                                      "/{empresa}" +
                                      "/{fragcliente}" +
                                      "/{fragcidade}" +
                                      "/{dtagendamento}" +
                                      "/{dtcarregamento}", async (string dtinicial,
                                                                  string dtfinal,
                                                                  string empresa,
                                                                  string fragcliente,
                                                                  string fragcidade,
                                                                  string dtagendamento,
                                                                  string dtcarregamento) =>
                                      {
                                          string jsonString = "";
                                          OracleConnection connection = new OracleConnection();

                                          try
                                          {
                                              //Pedido pedido = null;
                                              List<Pedido> pedido = new List<Pedido>();


                                              string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.254)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = SAIB)));User ID=SAIB2000;Password=M4R1Z4; ";
                                              //string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.21)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = HOMOLOG)));User ID=SAIB2000;Password=M4R1Z4; ";
                                              connection.ConnectionString = connString;
                                              connection.Open();
                                              if (connection != null)
                                              {

                                                  StringBuilder str = new StringBuilder();
                                                  str.Remove(0, str.Length);
                                                  str.Append("SELECT TO_CHAR(mpf.PEDF_DTA_EMIS,'DD/MM/YYYY') AS datafat, ");
                                                  str.Append("'Localizar' AS gerente, ");
                                                  str.Append("'Localizar' AS rca, ");
                                                  str.Append("mpf.pedf_liqu_id liquidacao, ");
                                                  str.Append("mpf.pedf_nr_nf AS notafiscal, ");
                                                  str.Append("mpf.PEDF_ID numped, ");
                                                  str.Append("mpf.PEDF_CLI_ID codcli, ");
                                                  str.Append("c.CLI_FANTASIA  razaosocial, ");
                                                  str.Append("G.GEN_ID idcidade, ");
                                                  str.Append("g.GEN_DESCRICAO || ' - ' || g1.GEN_DESCRICAO nomecidade, ");
                                                  str.Append("g1.GEN_DESCRICAO uf, ");
                                                  str.Append("mpf.PEDF_PEDF_ID_VENDA pedvenda, ");
                                                  str.Append("mpf.PEDF_ID_LIBERADO pedfat, ");
                                                  str.Append("g2.GEN_DESCRICAO Tipo, ");
                                                  str.Append("g2.GEN_ID cdtipo, ");
                                                  str.Append("OP.SIT_DESCRICAO DESCRICAOSITUACAO, ");

                                                  str.Append("(SELECT REPLACE(sum(mpfp.PEDF_QTDE * p.PROF_PESO_B),',','.')");
                                                  str.Append("FROM MA_PEDIDO_FAT_P mpfp, ");
                                                  str.Append("PRODUTO_FAT p ");
                                                  str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                  str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID AND ");
                                                  str.Append("mpfp.PEDF_PROD_EMP_ID = p.PROF_PROD_EMP_ID  AND ");
                                                  str.Append("mpfp.PEDF_PROD_ID = p.PROF_PROD_ID) AS peso, ");

                                                  str.Append("(SELECT sum(mpfp.PEDF_QTDE) ");
                                                  str.Append("FROM MA_PEDIDO_FAT_P mpfp, ");
                                                  str.Append("PRODUTO_FAT p ");
                                                  str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                  str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID AND ");
                                                  str.Append("mpfp.PEDF_PROD_EMP_ID = p.PROF_PROD_EMP_ID  AND ");
                                                  str.Append("mpfp.PEDF_PROD_ID = p.PROF_PROD_ID) AS qtdcaixa, ");

                                                  str.Append("(SELECT ROUND((sum(p.PROF_PESO_B)/1060),0) ");
                                                  str.Append("FROM MA_PEDIDO_FAT_P mpfp, ");
                                                  str.Append(" PRODUTO_FAT p ");
                                                  str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                  str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID AND ");
                                                  str.Append("mpfp.PEDF_PROD_EMP_ID = p.PROF_PROD_EMP_ID  AND ");
                                                  str.Append("mpfp.PEDF_PROD_ID = p.PROF_PROD_ID) AS palet, ");

                                                  str.Append("(SELECT REPLACE((ROUND(sum(mpfp.PEDF_VLR_TOT),2)),',','.') ");
                                                  str.Append("FROM MA_PEDIDO_FAT_P mpfp ");
                                                  str.Append("WHERE mpfp.PEDF_PEDF_EMP_ID  = mpf.PEDF_EMP_ID AND ");
                                                  str.Append("mpfp.PEDF_PEDF_ID  = mpf.PEDF_ID) AS vlrpedido, ");

                                                  str.Append("(SELECT L.LIQU_PLACA_TRANSP ");
                                                  str.Append("FROM LIQUIDACAO l ");
                                                  str.Append("WHERE L.LIQU_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                  str.Append("L.LIQU_ID = mpf.pedf_liqu_id AND ");
                                                  str.Append("mpf.pedf_liqu_id >1) AS placa, ");

                                                  str.Append("(SELECT REPLACE(PFL.PEDL_VLR_FRETE_CONT,',','.') ");
                                                  str.Append("FROM PEDIDO_FAT_LOGISTICA pfl ");
                                                  str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                  str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID ) AS vlrfrete, ");


                                                  str.Append("(SELECT CASE maf.AGPF_TIPO_VEICULO ");
                                                  str.Append("WHEN 'C' ");
                                                  str.Append("THEN 'Carreta' ");
                                                  str.Append("WHEN 'T' ");
                                                  str.Append("THEN 'Truck' ");
                                                  str.Append("END ");
                                                  str.Append("FROM MA_AGRUPAMENTO_FRETE maf ");
                                                  str.Append("WHERE MAF.AGPF_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                  str.Append("MAF.AGPF_ID = MPF.PEDF_AGRF_ID) AS tipoveiculo, ");

                                                  str.Append("(SELECT SUM(maf.AGPF_NUMERO_ENTREGAS) ");
                                                  str.Append("FROM MA_AGRUPAMENTO_FRETE maf ");
                                                  str.Append("WHERE MAF.AGPF_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                  str.Append("MAF.AGPF_ID = MPF.PEDF_AGRF_ID) AS qtdentregas, ");

                                                  str.Append("(SELECT F.FRO_DESC ");
                                                  str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                  str.Append("FROTA f ");
                                                  str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                  str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                  str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                  str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS transportador, ");

                                                  str.Append("(SELECT PFL.PEDL_NOME_MOTORISTA ");
                                                  str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                  str.Append("FROTA f ");
                                                  str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                  str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                  str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                  str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS motorista, ");

                                                  str.Append("(SELECT NVL(TO_CHAR(PFL.PEDL_DTA_COLETA,'DD/MM/YYYY'),'0')  ");
                                                  str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                  str.Append("FROTA f ");
                                                  str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                  str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                  str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID ");
                                                  if (dtcarregamento != "@")
                                                  {
                                                      str.Append(" AND TO_CHAR(PFL.PEDL_DTA_COLETA,'YYYYMMDD') = " + dtcarregamento);
                                                  }
                                                  str.Append("AND PFL.PEDL_FRO_ID = F.FRO_ID) AS carregamento, ");

                                                  str.Append("(SELECT PFL.PEDL_HORA_COLETA ");
                                                  str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                  str.Append("FROTA f ");
                                                  str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                  str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                  str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                  str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS horario, ");

                                                  str.Append("(SELECT DISTINCT(TRUNC(sysdate) - TRUNC(SPF.PEDF_DTA_EMIS)) + 1 ");
                                                  str.Append("FROM PEDIDO_FAT SPF ");
                                                  str.Append("WHERE MPF.PEDF_LIQU_EMP_ID = SPF.PEDF_LIQU_EMP_ID AND ");
                                                  str.Append("MPF.PEDF_LIQU_ID = SPF.PEDF_LIQU_ID AND ");
                                                  str.Append("SPF.PEDF_NR_NF >0) as prazonota,");

                                                  str.Append("(SELECT DISTINCT 'AGUA' ");
                                                  str.Append("FROM PRODUTO P, ");
                                                  str.Append("PRODUTO_C PC, ");
                                                  str.Append("GENER MC, ");
                                                  str.Append("PRODUTO_TP TP, ");
                                                  str.Append("GENER CT, ");
                                                  str.Append("MA_PEDIDO_FAT_P mpfp ");
                                                  str.Append("WHERE mpfp.PEDF_PROD_EMP_ID = P.PROD_EMP_ID ");
                                                  str.Append("AND mpfp.PEDF_PROD_ID = P.PROD_ID ");
                                                  str.Append("AND     P.PROD_EMP_ID = PC.PROC_PROD_EMP_ID AND P.PROD_ID = PC.PROC_PROD_ID ");
                                                  str.Append("AND     PC.PROC_GEN_TGEN_ID_MARCA_DE = MC.GEN_TGEN_ID ");
                                                  str.Append("AND     PC.PROC_GEN_EMP_ID_MARCA_DE = MC.GEN_EMP_ID ");
                                                  str.Append("AND     PC.PROC_GEN_ID_MARCA_DE = MC.GEN_ID ");
                                                  str.Append("AND     P.PROD_ID = TP.PROT_PROD_ID AND P.PROD_EMP_ID = TP.PROT_PROD_EMP_ID ");
                                                  str.Append("AND     TP.PROT_GEN_TGEN_ID = CT.GEN_TGEN_ID AND TP.PROT_GEN_ID = CT.GEN_ID ");
                                                  str.Append("AND     CT.GEN_ID = 3 ");
                                                  str.Append("AND     MC.GEN_ID = 3 ");
                                                  str.Append("AND     mpfp.PEDF_PEDF_EMP_ID = MPF.PEDF_EMP_ID ");
                                                  str.Append("AND     mpfp.PEDF_PEDF_ID = MPF.PEDF_ID) AS categoria, ");

                                                  str.Append("CASE mpf.PEDF_RETIRAR ");
                                                  str.Append("WHEN 1 ");
                                                  str.Append("THEN 'GESTÃO' ");
                                                  str.Append("WHEN 2 ");
                                                  str.Append("THEN 'INDÚSTRIA' ");
                                                  str.Append("END AS RETIRAR, ");
                                                  str.Append("CASE mpf.PEDF_ENTREGA ");
                                                  str.Append("WHEN 1 ");
                                                  str.Append("THEN 'AGENDAR' ");
                                                  str.Append("WHEN 2 ");
                                                  str.Append("THEN 'IMEDIATO' ");
                                                  str.Append("WHEN 3 ");
                                                  str.Append("THEN 'AGENDADO' ");
                                                  str.Append("END AS ENTREGA, ");
                                                  str.Append("TO_CHAR(mpf.PEDF_DATA_ENTREGA_AGENDADA,'DD/MM/YYYY') AS DTAENTREGAAGENDADA, ");
                                                  str.Append("mpf.PEDF_OBS_LOGISTICA OBSLOGISTICA, ");
                                                  str.Append("mpf.PEDF_FIFO FIFO, ");
                                                  str.Append("CASE mpf.PEDF_TP_FRETE ");
                                                  str.Append("WHEN '0' ");
                                                  str.Append("THEN 'CIF' ");
                                                  str.Append("WHEN '1' ");
                                                  str.Append("THEN 'FOB' ");
                                                  str.Append("END AS TPFRETE ");

                                                  str.Append("FROM MA_PEDIDO_FAT mpf, ");
                                                  str.Append("CLIENTE c, ");
                                                  str.Append("CLIENTE_E ce, ");
                                                  str.Append("GENER g, ");
                                                  str.Append("GENER g1, ");
                                                  str.Append("gener g2, ");
                                                  str.Append("OPERACAO_FAT of2, ");
                                                  str.Append("MA_SITUACAO_PEDIDO op ");

                                                  str.Append("WHERE mpf.PEDF_EMP_ID = c.CLI_EMP_ID AND ");
                                                  str.Append("mpf.PEDF_CLI_ID = c.CLI_ID AND ");
                                                  str.Append("mpf.PEDF_EMP_ID = OP.SIT_EMP_ID AND ");
                                                  str.Append("mpf.PEDF_SITUACAO = OP.SIT_ID AND ");
                                                  str.Append("ce.CLIE_CLI_EMP_ID = c.CLI_EMP_ID AND ");
                                                  str.Append("ce.CLIE_CLI_ID = c.CLI_ID AND ");
                                                  str.Append("ce.CLIE_GEN_tgen_ID_CIDADE_DE = g.GEN_TGEN_ID  AND ");
                                                  str.Append("ce.CLIE_GEN_EMP_ID = g.GEN_EMP_ID AND ");
                                                  str.Append("ce.CLIE_GEN_ID_CIDADE_DE = g.GEN_ID AND ");
                                                  str.Append("ce.CLIE_GEN_tgen_ID_CIDADE_DE = 5001 AND ");
                                                  str.Append("mpf.pedf_emp_id = of2.oper_emp_id AND ");
                                                  str.Append("mpf.pedf_oper_id = of2.oper_id AND ");
                                                  str.Append("of2.oper_gen_emp_id_tp_operacao_de = g2.gen_emp_id AND  ");
                                                  str.Append("of2.oper_gen_id_tp_operacao_de = g2.gen_id AND  ");
                                                  str.Append("g2.gen_tgen_id = 9013 AND ");
                                                  str.Append("SUBSTR(g.GEN_TEXT1,1,2) = g1.gen_number3 AND ");
                                                  str.Append("g1.GEN_TGEN_ID = 5006 AND ");
                                                  str.Append("TO_CHAR(mpf.PEDF_DTA_EMIS,'YYYYMMDD') BETWEEN " + dtinicial + " AND " + dtfinal);
                                                  if (dtagendamento != "@")
                                                  {
                                                      str.Append(" AND TO_CHAR(mpf.PEDF_DATA_ENTREGA_AGENDADA,'YYYYMMDD') = " + dtagendamento);
                                                  }
                                                  str.Append("and mpf.PEDF_EMP_ID = " + empresa);
                                                  if (fragcliente != "@")
                                                  {
                                                      str.Append(" AND c.CLI_FANTASIA like '%" + fragcliente + "%'");
                                                  }
                                                  if (fragcidade != "@")
                                                  {
                                                      str.Append(" AND g.GEN_DESCRICAO like '%" + fragcidade + "%'");
                                                  }

                                                  str.Append(" ORDER BY mpf.PEDF_DTA_EMIS, mpf.PEDF_CLI_ID, mpf.PEDF_ID ASC ");
                                                  using (OracleCommand cmd = new OracleCommand(str.ToString(), connection))
                                                  {
                                                      using (OracleDataReader reader = cmd.ExecuteReader())
                                                      {
                                                          if (reader.HasRows)
                                                          {
                                                              while (reader.Read())
                                                              {
                                                                  string categoriaAux = "";

                                                                  if (reader["categoria"].ToString().Trim().Length == 0)
                                                                  {
                                                                      categoriaAux = "NA";
                                                                  }
                                                                  else
                                                                  {
                                                                      categoriaAux = reader["categoria"].ToString();
                                                                  }
                                                                  if (dtcarregamento != "@")
                                                                  {
                                                                      if (reader["carregamento"].ToString() != "")
                                                                      {
                                                                          pedido.Add(new Pedido()
                                                                          {
                                                                              datafat = reader["datafat"].ToString(),
                                                                              gerente = reader["gerente"].ToString(),
                                                                              rca = reader["rca"].ToString(),
                                                                              liquidacao = reader["liquidacao"].ToString(),
                                                                              notafiscal = reader["notafiscal"].ToString(),
                                                                              numped = reader["numped"].ToString(),
                                                                              codcli = reader["codcli"].ToString(),
                                                                              razaosocial = reader["razaosocial"].ToString(),
                                                                              idcidade = reader["idcidade"].ToString(),
                                                                              nomecidade = reader["nomecidade"].ToString(),
                                                                              uf = reader["uf"].ToString(),
                                                                              pedvenda = reader["pedvenda"].ToString(),
                                                                              pedfat = reader["pedfat"].ToString(),
                                                                              tipo = reader["tipo"].ToString(),
                                                                              peso = reader["peso"].ToString(),
                                                                              qtdcaixa = reader["qtdcaixa"].ToString(),
                                                                              palet = reader["palet"].ToString(),
                                                                              vlrpedido = reader["vlrpedido"].ToString(),
                                                                              placa = reader["placa"].ToString(),
                                                                              vlrfrete = reader["vlrfrete"].ToString(),
                                                                              tipoveiculo = reader["tipoveiculo"].ToString(),
                                                                              qtdentregas = reader["qtdentregas"].ToString(),
                                                                              transportador = reader["transportador"].ToString(),
                                                                              motorista = reader["motorista"].ToString(),
                                                                              carregamento = reader["carregamento"].ToString(),
                                                                              horario = reader["horario"].ToString(),
                                                                              prazonota = reader["prazonota"].ToString(),
                                                                              descricaosituacao = reader["descricaosituacao"].ToString(),
                                                                              categoria = categoriaAux,
                                                                              retirar = reader["retirar"].ToString(),
                                                                              entrega = reader["entrega"].ToString(),
                                                                              dtaentregaagendada = reader["dtaentregaagendada"].ToString(),
                                                                              obslogistica = reader["obslogistica"].ToString(),
                                                                              tpfrete = reader["tpfrete"].ToString(),
                                                                              fifo = reader["fifo"].ToString(),
                                                                              cdtipo = reader["cdtipo"].ToString(),
                                                                          });
                                                                      }
                                                                  }
                                                                  else
                                                                  {
                                                                      pedido.Add(new Pedido()
                                                                      {
                                                                          datafat = reader["datafat"].ToString(),
                                                                          gerente = reader["gerente"].ToString(),
                                                                          rca = reader["rca"].ToString(),
                                                                          liquidacao = reader["liquidacao"].ToString(),
                                                                          notafiscal = reader["notafiscal"].ToString(),
                                                                          numped = reader["numped"].ToString(),
                                                                          codcli = reader["codcli"].ToString(),
                                                                          razaosocial = reader["razaosocial"].ToString(),
                                                                          idcidade = reader["idcidade"].ToString(),
                                                                          nomecidade = reader["nomecidade"].ToString(),
                                                                          uf = reader["uf"].ToString(),
                                                                          pedvenda = reader["pedvenda"].ToString(),
                                                                          pedfat = reader["pedfat"].ToString(),
                                                                          tipo = reader["tipo"].ToString(),
                                                                          peso = reader["peso"].ToString(),
                                                                          qtdcaixa = reader["qtdcaixa"].ToString(),
                                                                          palet = reader["palet"].ToString(),
                                                                          vlrpedido = reader["vlrpedido"].ToString(),
                                                                          placa = reader["placa"].ToString(),
                                                                          vlrfrete = reader["vlrfrete"].ToString(),
                                                                          tipoveiculo = reader["tipoveiculo"].ToString(),
                                                                          qtdentregas = reader["qtdentregas"].ToString(),
                                                                          transportador = reader["transportador"].ToString(),
                                                                          motorista = reader["motorista"].ToString(),
                                                                          carregamento = reader["carregamento"].ToString(),
                                                                          horario = reader["horario"].ToString(),
                                                                          prazonota = reader["prazonota"].ToString(),
                                                                          descricaosituacao = reader["descricaosituacao"].ToString(),
                                                                          categoria = categoriaAux,
                                                                          retirar = reader["retirar"].ToString(),
                                                                          entrega = reader["entrega"].ToString(),
                                                                          dtaentregaagendada = reader["dtaentregaagendada"].ToString(),
                                                                          obslogistica = reader["obslogistica"].ToString(),
                                                                          tpfrete = reader["tpfrete"].ToString(),
                                                                          fifo = reader["fifo"].ToString(),
                                                                          cdtipo = reader["cdtipo"].ToString(),
                                                                      });
                                                                  }
                                                              }
                                                              reader.Close();
                                                              jsonString = JsonSerializer.Serialize(pedido);
                                                          }
                                                      }
                                                  }
                                              }
                                          }
                                          catch (Exception ex)
                                          {
                                              var erroretorno = new ErroRetorno
                                              {
                                                  mensagem = ex.Message
                                              };
                                              jsonString = JsonSerializer.Serialize(erroretorno);
                                          }
                                          finally
                                          {
                                              if (connection != null)
                                              {
                                                  connection.Close();
                                              }
                                          }

                                          return jsonString;
                                      });
app.MapGet("/ObtemDadosLiquidacaoGerencial/{dtinicial}" +
                                         "/{dtfinal}" +
                                         "/{empresa}" +
                                         "/{fragcliente}" +
                                         "/{fragcidade}" +
                                         "/{dtagendamento}" +
                                         "/{dtcarregamento}", async (string dtinicial,
                                                                     string dtfinal,
                                                                     string empresa,
                                                                     string fragcliente,
                                                                     string fragcidade,
                                                                     string dtagendamento,
                                                                     string dtcarregamento) =>
                                         {
                                             string jsonString = "";
                                             OracleConnection connection = new OracleConnection();

                                             try
                                             {
                                                 //Pedido pedido = null;
                                                 List<Pedido> pedido = new List<Pedido>();


                                                 string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.254)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = SAIB)));User ID=SAIB2000;Password=M4R1Z4; ";
                                                 //string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.21)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = HOMOLOG)));User ID=SAIB2000;Password=M4R1Z4; ";
                                                 connection.ConnectionString = connString;
                                                 connection.Open();
                                                 if (connection != null)
                                                 {

                                                     StringBuilder str = new StringBuilder();
                                                     str.Remove(0, str.Length);
                                                     str.Append("SELECT TO_CHAR(mpf.PEDF_DTA_EMIS,'DD/MM/YYYY') AS datafat,  ");
                                                     str.Append("'Localizar' AS gerente,  ");
                                                     str.Append("'Localizar' AS rca,  ");
                                                     str.Append("mpf.pedf_liqu_id liquidacao,  ");
                                                     str.Append("mpf.PEDF_CLI_ID codcli,  ");
                                                     str.Append("c.CLI_FANTASIA  razaosocial, ");
                                                     str.Append("G.GEN_ID idcidade,  ");
                                                     str.Append("g.GEN_DESCRICAO || ' - ' || g1.GEN_DESCRICAO nomecidade,  ");
                                                     str.Append("g1.GEN_DESCRICAO uf,  ");
                                                     str.Append("g2.GEN_DESCRICAO Tipo,  ");
                                                     str.Append("g2.GEN_ID cdtipo,  ");
                                                     str.Append("NVL(tb1.pedf_nr_nf,'0') AS notafiscal, ");
                                                     str.Append("OP.SIT_DESCRICAO DESCRICAOSITUACAO,  ");

                                                     str.Append("REPLACE(sum(mpfp.PEDF_QTDE * p.PROF_PESO_B),',','.') AS peso,  ");
                                                     str.Append("sum(mpfp.PEDF_QTDE) QTDCAIXA, ");
                                                     str.Append("ROUND((sum(p.PROF_PESO_B)/1060),0) PALET, ");
                                                     str.Append("REPLACE((ROUND(sum(mpfp.PEDF_VLR_TOT+nvl(mpfp.PEDF_VLR_IPI,'0')),2)),',','.') VLRPEDIDO, ");

                                                     str.Append("(SELECT L.LIQU_PLACA_TRANSP ");
                                                     str.Append("FROM LIQUIDACAO l ");
                                                     str.Append("WHERE L.LIQU_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                     str.Append("L.LIQU_ID = mpf.pedf_liqu_id AND ");
                                                     str.Append("mpf.pedf_liqu_id >1) AS placa, ");

                                                     str.Append("(SELECT REPLACE(PFL.PEDL_VLR_FRETE_CONT,',','.') ");
                                                     str.Append("FROM PEDIDO_FAT_LOGISTICA pfl ");
                                                     str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                     str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID ) AS vlrfrete, ");

                                                     str.Append("(SELECT CASE maf.AGPF_TIPO_VEICULO ");
                                                     str.Append("WHEN 'C' ");
                                                     str.Append("THEN 'Carreta' ");
                                                     str.Append("WHEN 'T' ");
                                                     str.Append("THEN 'Truck' ");
                                                     str.Append("END ");
                                                     str.Append("FROM MA_AGRUPAMENTO_FRETE maf ");
                                                     str.Append("WHERE MAF.AGPF_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                     str.Append("MAF.AGPF_ID = MPF.PEDF_AGRF_ID) AS tipoveiculo, ");

                                                     str.Append("(SELECT SUM(maf.AGPF_NUMERO_ENTREGAS) ");
                                                     str.Append("FROM MA_AGRUPAMENTO_FRETE maf ");
                                                     str.Append("WHERE MAF.AGPF_EMP_ID = MPF.PEDF_EMP_ID AND ");
                                                     str.Append("MAF.AGPF_ID = MPF.PEDF_AGRF_ID) AS qtdentregas, ");

                                                     str.Append("(SELECT F.FRO_DESC ");
                                                     str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                     str.Append("FROTA f ");
                                                     str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                     str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                     str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                     str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS transportador, ");

                                                     str.Append("(SELECT PFL.PEDL_NOME_MOTORISTA ");
                                                     str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                     str.Append("FROTA f ");
                                                     str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                     str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                     str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                     str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS motorista, ");


                                                     str.Append("(SELECT NVL(TO_CHAR(PFL.PEDL_DTA_COLETA,'DD/MM/YYYY'),'0')  ");
                                                     str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                     str.Append("FROTA f ");
                                                     str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                     str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                     str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID ");
                                                     if (dtcarregamento != "@")
                                                     {
                                                         str.Append(" AND TO_CHAR(PFL.PEDL_DTA_COLETA,'YYYYMMDD') = " + dtcarregamento);
                                                     }
                                                     str.Append("AND PFL.PEDL_FRO_ID = F.FRO_ID) AS carregamento, ");



                                                     str.Append("(SELECT PFL.PEDL_HORA_COLETA ");
                                                     str.Append("FROM PEDIDO_FAT_LOGISTICA pfl, ");
                                                     str.Append("FROTA f ");
                                                     str.Append("WHERE PFL.PEDL_LIQU_EMP_ID = MPF.PEDF_LIQU_EMP_ID AND ");
                                                     str.Append("PFL.PEDL_LIQU_ID = MPF.PEDF_LIQU_ID AND ");
                                                     str.Append("MPF.PEDF_EMP_ID  = F.FRO_EMP_ID  AND ");
                                                     str.Append("PFL.PEDL_FRO_ID = F.FRO_ID) AS horario, ");

                                                     str.Append("(SELECT DISTINCT(TRUNC(sysdate) - TRUNC(SPF.PEDF_DTA_EMIS)) + 1 ");
                                                     str.Append("FROM PEDIDO_FAT SPF ");
                                                     str.Append("WHERE MPF.PEDF_LIQU_EMP_ID = SPF.PEDF_LIQU_EMP_ID AND ");
                                                     str.Append("MPF.PEDF_LIQU_ID = SPF.PEDF_LIQU_ID AND ");
                                                     str.Append("SPF.PEDF_NR_NF >0) as prazonota, ");

                                                     str.Append("CASE mpf.PEDF_RETIRAR  ");
                                                     str.Append("WHEN 1  ");
                                                     str.Append("THEN 'GESTÃO'  ");
                                                     str.Append("WHEN 2  ");
                                                     str.Append("THEN 'INDÚSTRIA'  ");
                                                     str.Append("END AS RETIRAR,  ");
                                                     str.Append("CASE mpf.PEDF_ENTREGA  ");
                                                     str.Append("WHEN 1  ");
                                                     str.Append("THEN 'AGENDAR'  ");
                                                     str.Append("WHEN 2  ");
                                                     str.Append("THEN 'IMEDIATO'  ");
                                                     str.Append("WHEN 3  ");
                                                     str.Append("THEN 'AGENDADO'  ");
                                                     str.Append("END AS ENTREGA,  ");
                                                     str.Append("TO_CHAR(mpf.PEDF_DATA_ENTREGA_AGENDADA,'DD/MM/YYYY') AS DTAENTREGAAGENDADA,  ");
                                                     str.Append("mpf.PEDF_OBS_LOGISTICA OBSLOGISTICA,  ");
                                                     str.Append("mpf.PEDF_FIFO FIFO,  ");
                                                     str.Append("CASE mpf.PEDF_TP_FRETE  ");
                                                     str.Append("WHEN '0'  ");
                                                     str.Append("THEN 'CIF'  ");
                                                     str.Append("WHEN '1'  ");
                                                     str.Append("THEN 'FOB'  ");
                                                     str.Append("END AS TPFRETE  ");
                                                     str.Append("FROM MA_PEDIDO_FAT mpf,  ");
                                                     str.Append("MA_PEDIDO_FAT_P mpfp, ");
                                                     str.Append("PRODUTO_FAT p , ");
                                                     str.Append("CLIENTE c,  ");
                                                     str.Append("CLIENTE_E ce,  ");
                                                     str.Append("GENER g,  ");
                                                     str.Append("GENER g1,  ");
                                                     str.Append("gener g2,  ");
                                                     str.Append("OPERACAO_FAT of2,  ");
                                                     str.Append("MA_SITUACAO_PEDIDO op, ");

                                                     str.Append("(SELECT MPF1.PEDF_LIQU_ID AS LIQUI, mpf1.PEDF_CLI_ID AS CLIID, LISTAGG(MPF1.pedf_nr_nf, ',') WITHIN GROUP(ORDER BY MPF1.pedf_nr_nf) AS pedf_nr_nf");
                                                     str.Append(" FROM MA_PEDIDO_FAT mpf1 ");
                                                     str.Append(" WHERE MPF1.PEDF_LIQU_EMP_ID = " + empresa);
                                                     //str.Append(" AND TO_CHAR(mpf1.PEDF_DTA_EMIS,'YYYYMMDD') BETWEEN " + dtinicial + " AND " + dtfinal);
                                                     str.Append(" AND (SYSDATE - mpf1.PEDF_DTA_EMIS)-1 <= 365");
                                                     str.Append(" GROUP BY MPF1.PEDF_LIQU_ID, mpf1.PEDF_CLI_ID) tb1 ");

                                                     str.Append("WHERE MPF.PEDF_EMP_ID = MPFP.PEDF_PEDF_EMP_ID AND ");
                                                     str.Append("MPF.PEDF_ID = MPFP.PEDF_PEDF_ID AND ");
                                                     str.Append("MPFP.PEDF_PROD_EMP_ID = P.PROF_PROD_EMP_ID AND ");
                                                     str.Append("MPFP.PEDF_PROD_ID = P.PROF_PROD_ID AND ");
                                                     str.Append("mpf.PEDF_EMP_ID = c.CLI_EMP_ID AND  ");
                                                     str.Append("mpf.PEDF_CLI_ID = c.CLI_ID AND  ");
                                                     str.Append("mpf.PEDF_EMP_ID = OP.SIT_EMP_ID AND ");
                                                     str.Append("mpf.PEDF_SITUACAO = OP.SIT_ID AND  ");
                                                     str.Append("ce.CLIE_CLI_EMP_ID = c.CLI_EMP_ID AND  ");
                                                     str.Append("ce.CLIE_CLI_ID = c.CLI_ID AND  ");
                                                     str.Append("ce.CLIE_GEN_tgen_ID_CIDADE_DE = g.GEN_TGEN_ID  AND  ");
                                                     str.Append("ce.CLIE_GEN_EMP_ID = g.GEN_EMP_ID AND  ");
                                                     str.Append("ce.CLIE_GEN_ID_CIDADE_DE = g.GEN_ID AND  ");
                                                     str.Append("ce.CLIE_GEN_tgen_ID_CIDADE_DE = 5001 AND  ");
                                                     str.Append("mpf.pedf_emp_id = of2.oper_emp_id AND  ");
                                                     str.Append("mpf.pedf_oper_id = of2.oper_id AND  ");
                                                     str.Append("of2.oper_gen_emp_id_tp_operacao_de = g2.gen_emp_id AND   ");
                                                     str.Append("of2.oper_gen_id_tp_operacao_de = g2.gen_id AND   ");
                                                     str.Append("g2.gen_tgen_id = 9013 AND  ");
                                                     str.Append("SUBSTR(g.GEN_TEXT1,1,2) = g1.gen_number3 AND  ");
                                                     str.Append("g1.GEN_TGEN_ID = 5006 AND  ");
                                                     str.Append("tb1.LIQUI = MPF.PEDF_LIQU_ID and ");
                                                     str.Append("tb1.CLIID = MPF.PEDF_CLI_ID and ");
                                                     str.Append("TO_CHAR(mpf.PEDF_DTA_EMIS,'YYYYMMDD') BETWEEN " + dtinicial + " AND " + dtfinal + " ");
                                                     if (dtagendamento != "@")
                                                     {
                                                         str.Append(" AND TO_CHAR(mpf.PEDF_DATA_ENTREGA_AGENDADA,'YYYYMMDD') = " + dtagendamento);
                                                     }
                                                     str.Append("AND mpf.PEDF_EMP_ID = " + empresa);
                                                     if (fragcliente != "@")
                                                     {
                                                         str.Append(" AND c.CLI_FANTASIA like '%" + fragcliente + "%'");
                                                     }
                                                     if (fragcidade != "@")
                                                     {
                                                         str.Append(" AND g.GEN_DESCRICAO like '%" + fragcidade + "%'");
                                                     }
                                                     str.Append("GROUP BY mpf.PEDF_DTA_EMIS, ");
                                                     str.Append("mpf.pedf_liqu_id,  ");
                                                     str.Append("mpf.PEDF_CLI_ID,  ");
                                                     str.Append("c.CLI_FANTASIA,  ");
                                                     str.Append("G.GEN_ID,  ");
                                                     str.Append("g.GEN_DESCRICAO, ");
                                                     str.Append("g1.GEN_DESCRICAO,  ");
                                                     str.Append("g1.GEN_DESCRICAO,  ");
                                                     str.Append("g2.GEN_DESCRICAO,  ");
                                                     str.Append("g2.GEN_ID,  ");
                                                     str.Append("OP.SIT_DESCRICAO,  ");
                                                     str.Append("mpf.PEDF_OBS_LOGISTICA,  ");
                                                     str.Append("mpf.PEDF_FIFO,  ");
                                                     str.Append("mpf.PEDF_RETIRAR, ");
                                                     str.Append("mpf.PEDF_ENTREGA, ");
                                                     str.Append("mpf.PEDF_DATA_ENTREGA_AGENDADA, ");
                                                     str.Append("mpf.PEDF_TP_FRETE, ");
                                                     str.Append("MPF.PEDF_EMP_ID, ");
                                                     str.Append("MPF.PEDF_LIQU_EMP_ID, ");
                                                     str.Append("MPF.PEDF_AGRF_ID, ");
                                                     str.Append("tb1.pedf_nr_nf ");
                                                     str.Append("ORDER BY mpf.PEDF_DTA_EMIS, mpf.pedf_liqu_id ASC ");
                                                     using (OracleCommand cmd = new OracleCommand(str.ToString(), connection))
                                                     {
                                                         using (OracleDataReader reader = cmd.ExecuteReader())
                                                         {
                                                             if (reader.HasRows)
                                                             {
                                                                 while (reader.Read())
                                                                 {
                                                                     if (dtcarregamento != "@")
                                                                     {
                                                                         if (reader["carregamento"].ToString() != "")
                                                                         {
                                                                             pedido.Add(new Pedido()
                                                                             {
                                                                                 datafat = reader["datafat"].ToString(),
                                                                                 gerente = reader["gerente"].ToString(),
                                                                                 rca = reader["rca"].ToString(),
                                                                                 liquidacao = reader["liquidacao"].ToString(),
                                                                                 notafiscal = reader["notafiscal"].ToString(),
                                                                                 codcli = reader["codcli"].ToString(),
                                                                                 razaosocial = reader["razaosocial"].ToString(),
                                                                                 idcidade = reader["idcidade"].ToString(),
                                                                                 nomecidade = reader["nomecidade"].ToString(),
                                                                                 uf = reader["uf"].ToString(),
                                                                                 tipo = reader["tipo"].ToString(),
                                                                                 peso = reader["peso"].ToString(),
                                                                                 qtdcaixa = reader["qtdcaixa"].ToString(),
                                                                                 palet = reader["palet"].ToString(),
                                                                                 vlrpedido = reader["vlrpedido"].ToString(),
                                                                                 placa = reader["placa"].ToString(),
                                                                                 vlrfrete = reader["vlrfrete"].ToString(),
                                                                                 tipoveiculo = reader["tipoveiculo"].ToString(),
                                                                                 qtdentregas = reader["qtdentregas"].ToString(),
                                                                                 transportador = reader["transportador"].ToString(),
                                                                                 motorista = reader["motorista"].ToString(),
                                                                                 carregamento = reader["carregamento"].ToString(),
                                                                                 horario = reader["horario"].ToString(),
                                                                                 prazonota = reader["prazonota"].ToString(),
                                                                                 descricaosituacao = reader["descricaosituacao"].ToString(),
                                                                                 retirar = reader["retirar"].ToString(),
                                                                                 entrega = reader["entrega"].ToString(),
                                                                                 dtaentregaagendada = reader["dtaentregaagendada"].ToString(),
                                                                                 obslogistica = reader["obslogistica"].ToString(),
                                                                                 tpfrete = reader["tpfrete"].ToString(),
                                                                                 fifo = reader["fifo"].ToString(),
                                                                                 cdtipo = reader["cdtipo"].ToString(),
                                                                             });

                                                                         }
                                                                     }
                                                                     else
                                                                     {
                                                                         pedido.Add(new Pedido()
                                                                         {
                                                                             datafat = reader["datafat"].ToString(),
                                                                             gerente = reader["gerente"].ToString(),
                                                                             rca = reader["rca"].ToString(),
                                                                             liquidacao = reader["liquidacao"].ToString(),
                                                                             notafiscal = reader["notafiscal"].ToString(),
                                                                             codcli = reader["codcli"].ToString(),
                                                                             razaosocial = reader["razaosocial"].ToString(),
                                                                             idcidade = reader["idcidade"].ToString(),
                                                                             nomecidade = reader["nomecidade"].ToString(),
                                                                             uf = reader["uf"].ToString(),
                                                                             tipo = reader["tipo"].ToString(),
                                                                             peso = reader["peso"].ToString(),
                                                                             qtdcaixa = reader["qtdcaixa"].ToString(),
                                                                             palet = reader["palet"].ToString(),
                                                                             vlrpedido = reader["vlrpedido"].ToString(),
                                                                             placa = reader["placa"].ToString(),
                                                                             vlrfrete = reader["vlrfrete"].ToString(),
                                                                             tipoveiculo = reader["tipoveiculo"].ToString(),
                                                                             qtdentregas = reader["qtdentregas"].ToString(),
                                                                             transportador = reader["transportador"].ToString(),
                                                                             motorista = reader["motorista"].ToString(),
                                                                             carregamento = reader["carregamento"].ToString(),
                                                                             horario = reader["horario"].ToString(),
                                                                             prazonota = reader["prazonota"].ToString(),
                                                                             descricaosituacao = reader["descricaosituacao"].ToString(),
                                                                             retirar = reader["retirar"].ToString(),
                                                                             entrega = reader["entrega"].ToString(),
                                                                             dtaentregaagendada = reader["dtaentregaagendada"].ToString(),
                                                                             obslogistica = reader["obslogistica"].ToString(),
                                                                             tpfrete = reader["tpfrete"].ToString(),
                                                                             fifo = reader["fifo"].ToString(),
                                                                             cdtipo = reader["cdtipo"].ToString(),
                                                                         });
                                                                     }
                                                                 }
                                                                 reader.Close();
                                                                 jsonString = JsonSerializer.Serialize(pedido);
                                                             }
                                                         }
                                                     }
                                                 }
                                             }
                                             catch (Exception ex)
                                             {
                                                 var erroretorno = new ErroRetorno
                                                 {
                                                     mensagem = ex.Message
                                                 };
                                                 jsonString = JsonSerializer.Serialize(erroretorno);
                                             }
                                             finally
                                             {
                                                 if (connection != null)
                                                 {
                                                     connection.Close();
                                                 }
                                             }

                                             return jsonString;
                                         });

app.MapGet("/AtualizaDadosComposicaoFrete/{empresa}/" +
                                         "{vlrfrete}/" +
                                         "{ciffob}/" +
                                         "{volume}/" +
                                         "{peso}/" +
                                         "{usuario}/" +
                                         "{tpveiculo}/" +
                                         "{qtdentregas}/" +
                                         "{aliquotaimposto}/" +
                                         "{placa}/" +
                                         "{pedidos}",
                                         async (string empresa,
                                                string vlrfrete,
                                                string ciffob,
                                                string volume,
                                                string peso,
                                                string usuario,
                                                string tpveiculo,
                                                string qtdentregas,
                                                string aliquotaimposto,
                                                string placa,
                                                string pedidos) =>
                                         {
                                             string jsonString = "";
                                             OracleConnection connection = new OracleConnection();

                                             try
                                             {
                                                 string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.254)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = SAIB)));User ID=SAIB2000;Password=M4R1Z4; ";
                                                 //string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.21)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = HOMOLOG)));User ID=SAIB2000;Password=M4R1Z4; ";
                                                 connection.ConnectionString = connString;
                                                 connection.Open();
                                                 string idDisponivel = "";
                                                 string idProximaEtapa = "";
                                                 StringBuilder str = new StringBuilder();
                                                 OracleCommand cmd = null;
                                                 OracleDataReader reader = null;

                                                 if (connection != null)
                                                 {
                                                     OracleCommand command = connection.CreateCommand();
                                                     OracleTransaction transaction;

                                                     //Obtem o id disponível para ser usado na atualização da tabela de agrupamento(MA_AGRUPAMENTO_FRETE)

                                                     str.Remove(0, str.Length);
                                                     str.Append("SELECT MA_AGRUPAMENTO_FRETE_ID_2_SQ.NEXTVAL AS NEXT_ID FROM DUAL");
                                                     using (cmd = new OracleCommand(str.ToString(), connection))
                                                     {
                                                         using (reader = cmd.ExecuteReader())
                                                         {
                                                             if (reader.HasRows)
                                                             {
                                                                 while (reader.Read())
                                                                 {
                                                                     idDisponivel = reader["NEXT_ID"].ToString();
                                                                 }
                                                                 reader.Close();
                                                                 // Inicia a transação local
                                                                 transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                                                                 //Atribuir objeto de transação para uma transação local pendente
                                                                 command.Transaction = transaction;
                                                                 try
                                                                 {
                                                                     //Atualiza a tabela de agrupamentos
                                                                     str.Remove(0, str.Length);
                                                                     str.Append("INSERT INTO MA_AGRUPAMENTO_FRETE(");
                                                                     str.Append("AGPF_EMP_ID,");
                                                                     str.Append("AGPF_ID,");
                                                                     str.Append("AGPF_VLR_FRETE,");
                                                                     str.Append("AGPF_FRETE_FOB,");
                                                                     str.Append("AGPF_VOLUME,");
                                                                     str.Append("AGPF_PESO,");
                                                                     str.Append("AGPF_FRO_EMP_ID,");
                                                                     str.Append("AGPF_FRO_ID,");
                                                                     str.Append("AGPF_USR_CAD,");
                                                                     str.Append("AGPF_DATA_CAD,");
                                                                     str.Append("AGPF_TIPO_VEICULO,");
                                                                     str.Append("AGPF_NUMERO_ENTREGAS,");
                                                                     str.Append("AGPF_VLR_FRETE_PESO_UN,");
                                                                     str.Append("AGPF_TIPO_FRETE_PESO,");
                                                                     str.Append("AGPF_ALIQUOTA_IMPOSTO,");
                                                                     str.Append("AGPF_PLACA) ");
                                                                     str.Append("VALUES(" + empresa + ",");
                                                                     str.Append(idDisponivel + ",");
                                                                     str.Append(vlrfrete + ",'");
                                                                     str.Append(ciffob + "',");
                                                                     str.Append(volume + ",");
                                                                     str.Append(peso + ",");
                                                                     str.Append("null,");
                                                                     str.Append("null,");
                                                                     str.Append(usuario + ",");
                                                                     str.Append("SYSDATE,'");
                                                                     str.Append(tpveiculo + "',");
                                                                     str.Append(qtdentregas + ",");
                                                                     str.Append("null,");
                                                                     str.Append("null,");
                                                                     str.Append(aliquotaimposto + ",'");
                                                                     str.Append(placa + "')");
                                                                     command.CommandText = str.ToString();
                                                                     command.ExecuteNonQuery();

                                                                     //Obtem o id da situação referente ao próximo etapa
                                                                     str.Remove(0, str.Length);
                                                                     str.Append("SELECT MSP.SIT_IDDESTINO IDDESTINO FROM MA_SITUACAO_PEDIDO MSP WHERE MSP.SIT_DESCORIGEM = 'calculafrete'");
                                                                     using (cmd = new OracleCommand(str.ToString(), connection))
                                                                     {
                                                                         using (reader = cmd.ExecuteReader())
                                                                         {
                                                                             if (reader.HasRows)
                                                                             {
                                                                                 while (reader.Read())
                                                                                 {
                                                                                     idProximaEtapa = reader["IDDESTINO"].ToString();
                                                                                 }
                                                                                 reader.Close();
                                                                             }
                                                                         }
                                                                     }
                                                                     if (idProximaEtapa.Trim().Length > 0) //Verifica se encontrou o id da próxima etapa seguinte ao processamento atual
                                                                     {
                                                                         //Atualiza a tabela de pedidos

                                                                         str.Remove(0, str.Length);

                                                                         str.Append("UPDATE MA_PEDIDO_FAT SET PEDF_SITUACAO = " + idProximaEtapa);
                                                                         str.Append(",PEDF_AGRF_ID = " + idDisponivel);
                                                                         str.Append(",PEDF_AGRF_EMP_ID = " + empresa);
                                                                         str.Append(",PEDF_DTA_COMPOSICAO_FRETE = SYSDATE");
                                                                         str.Append(",PEDF_FRETE_FOB = '" + ciffob + "'");
                                                                         if (ciffob == "S")
                                                                         {
                                                                             str.Append(",PEDF_TP_FRETE = 1");
                                                                         }
                                                                         else
                                                                         {
                                                                             str.Append(",PEDF_TP_FRETE = 0");
                                                                         }
                                                                         str.Append(" WHERE PEDF_ID IN (" + pedidos + ")");
                                                                         str.Append(" AND PEDF_EMP_ID = " + empresa);
                                                                         command.CommandText = str.ToString();
                                                                         command.ExecuteNonQuery();
                                                                     }
                                                                     transaction.Commit();
                                                                 }
                                                                 catch (Exception ex)
                                                                 {
                                                                     transaction.Rollback();

                                                                     var erroretorno = new ErroRetorno
                                                                     {
                                                                         mensagem = ex.Message
                                                                     };
                                                                     jsonString = JsonSerializer.Serialize(erroretorno);
                                                                 }
                                                                 finally
                                                                 {
                                                                     if (connection != null)
                                                                     {
                                                                         connection.Close();
                                                                     }
                                                                 }
                                                             }
                                                         }
                                                     }
                                                 }
                                             }
                                             catch (Exception ex)
                                             {
                                                 var erroretorno = new ErroRetorno
                                                 {
                                                     mensagem = ex.Message
                                                 };
                                                 jsonString = JsonSerializer.Serialize(erroretorno);
                                             }
                                             finally
                                             {
                                                 if (connection != null)
                                                 {
                                                     connection.Close();
                                                 }
                                             }

                                             return jsonString;
                                         });
app.MapGet("/ObtemStatusPedidos/{pednumber}/{empresa}", async (string pednumber, string empresa) =>
{
    string jsonString = "";
    OracleConnection connection = new OracleConnection();

    try
    {
        List<Status> status = new List<Status>();


        string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.254)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = SAIB)));User ID=SAIB2000;Password=M4R1Z4; ";
        // string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.21)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = HOMOLOG)));User ID=SAIB2000;Password=M4R1Z4; ";
        connection.ConnectionString = connString;
        connection.Open();
        if (connection != null)
        {

            StringBuilder str = new StringBuilder();
            str.Remove(0, str.Length);
            str.Append("SELECT mhsp.PEDF_SITUACAO AS situacao, ");
            str.Append("to_char(mhsp.HISP_DTA_MOV,'YYYY-MM-DD HH24:mi:ss') AS datamov, ");
            str.Append("to_char(MHSP.HISP_DTA_MOV,'HH24:mi:ss') AS horamov, ");
            str.Append("sp.sit_descricao as descricao ");
            str.Append("FROM MA_HISTORICO_STATUS_PEDIDOS mhsp, ");
            str.Append("     MA_SITUACAO_PEDIDO sp ");
            str.Append("WHERE mhsp.PEDF_EMP_ID = SP.SIT_EMP_ID ");
            str.Append("AND   mhsp.PEDF_SITUACAO = SP.SIT_ID ");
            str.Append("AND   mhsp.PEDF_EMP_ID = " + empresa);
            str.Append(" AND mhsp.PEDF_ID = " + pednumber);
            str.Append(" ORDER BY MHSP.HISP_DTA_MOV ");
            using (OracleCommand cmd = new OracleCommand(str.ToString(), connection))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            status.Add(new Status()
                            {
                                datamov = reader["datamov"].ToString(),
                                horamov = reader["horamov"].ToString(),
                                descricao = reader["descricao"].ToString(),
                            });
                        }
                        reader.Close();
                        jsonString = JsonSerializer.Serialize(status);
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        var erroretorno = new ErroRetorno
        {
            mensagem = ex.Message
        };
        jsonString = JsonSerializer.Serialize(erroretorno);
    }
    finally
    {
        if (connection != null)
        {
            connection.Close();
        }
    }

    return jsonString;
});
app.MapGet("/ObtemNotasFiscais/{numliquidacao}/{empresa}", async (string numliquidacao, string empresa) =>
{
    string jsonString = "";
    OracleConnection connection = new OracleConnection();

    try
    {
        List<Notasfiscais> notasfiscais = new List<Notasfiscais>();


        string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.254)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = SAIB)));User ID=SAIB2000;Password=M4R1Z4; ";
        // string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.21)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = HOMOLOG)));User ID=SAIB2000;Password=M4R1Z4; ";
        connection.ConnectionString = connString;
        connection.Open();
        if (connection != null)
        {

            StringBuilder str = new StringBuilder();
            str.Remove(0, str.Length);

            str.Append("SELECT ");
            str.Append("mpf.PEDF_NR_NF AS numnf, ");
            str.Append("TO_CHAR(mpf.PEDF_DTA_EMIS, 'DD/MM/YYYY') AS dtnf, ");
            str.Append("mpf.PEDF_CLI_ID AS idcli, ");
            str.Append("c.CLI_FANTASIA razaosocial, ");
            str.Append("REPLACE(mpf.PEDF_VLR_TOT_PED, ',', '.') AS vlrtotalnota, ");
            str.Append("REPLACE(sum(mpfp.PEDF_QTDE * p.PROF_PESO_B), ',', '.') AS peso, ");
            str.Append("sum(mpfp.PEDF_QTDE) qtdcaixa, ");
            str.Append("MPF.PEDF_ID AS numped ");
            str.Append("FROM ");
            str.Append("MA_PEDIDO_FAT mpf, ");
            str.Append("MA_PEDIDO_FAT_P mpfp, ");
            str.Append("PRODUTO_FAT p, ");
            str.Append("CLIENTE c ");
            str.Append("WHERE ");
            str.Append("mpf.PEDF_EMP_ID = mpfp.PEDF_PEDF_EMP_ID ");
            str.Append("AND ");
            str.Append("mpf.PEDF_ID = mpfp.PEDF_PEDF_ID ");
            str.Append("AND ");
            str.Append("MPFP.PEDF_PROD_EMP_ID = P.PROF_PROD_EMP_ID ");
            str.Append("AND ");
            str.Append("MPFP.PEDF_PROD_ID = P.PROF_PROD_ID ");
            str.Append("AND ");
            str.Append("MPF.PEDF_CLI_EMP_ID = C.CLI_EMP_ID ");
            str.Append("AND ");
            str.Append("MPF.PEDF_CLI_ID = C.CLI_ID ");
            str.Append("AND ");
            str.Append("mpf.PEDF_LIQU_EMP_ID = " + empresa);
            str.Append("AND mpf.PEDF_LIQU_ID = " + numliquidacao);
            str.Append("GROUP BY ");
            str.Append("mpf.PEDF_NR_NF, ");
            str.Append("mpf.PEDF_CLI_ID, ");
            str.Append("TO_CHAR(mpf.PEDF_DTA_EMIS, 'DD/MM/YYYY'), ");
            str.Append("REPLACE(mpf.PEDF_VLR_TOT_PED, ',', '.'), ");
            str.Append("MPF.PEDF_ID, ");
            str.Append("c.CLI_FANTASIA ");
            using (OracleCommand cmd = new OracleCommand(str.ToString(), connection))
            {
                using (OracleDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            notasfiscais.Add(new Notasfiscais()
                            {
                                numnf = reader["numnf"].ToString(),
                                idcli = reader["idcli"].ToString(),
                                vlrtotalnota = reader["vlrtotalnota"].ToString(),
                                dtnf = reader["dtnf"].ToString(),
                                razaosocial = reader["razaosocial"].ToString(),
                                peso = reader["peso"].ToString(),
                                qtdcaixa = reader["qtdcaixa"].ToString(),
                                numped = reader["numped"].ToString(),
                            });
                        }
                        reader.Close();
                        jsonString = JsonSerializer.Serialize(notasfiscais);
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        var erroretorno = new ErroRetorno
        {
            mensagem = ex.Message
        };
        jsonString = JsonSerializer.Serialize(erroretorno);
    }
    finally
    {
        if (connection != null)
        {
            connection.Close();
        }
    }

    return jsonString;
});
app.MapGet("/ObtemDadosProdutosPedidosTransicao/{numped}" +
                                              "/{vlrtotalcard4}" +
                                              "/{aliquotacreditoimposto}" +
                                              "/{r1}" +
                                              "/{r2}" +
                                              "/{r3}" +
                                              "/{r4}" +
                                              "/{vlrporkg}", async (string numped,
                                                                    string vlrtotalcard4,
                                                                    string aliquotacreditoimposto,
                                                                    int r1,
                                                                    int r2,
                                                                    int r3,
                                                                    int r4,
                                                                    string vlrporkg) =>
                                              {
                                                  string jsonString = "";
                                                  OracleConnection connection = new OracleConnection();
                                                  string v1 = "";
                                                  string v2 = "";
                                                  decimal numVal = 0;
                                                  decimal numVal1 = 0;
                                                  try
                                                  {

                                                      //Pedido pedido = null;
                                                      List<Produto> produto = new List<Produto>();


                                                      string connString = "Data Source=(DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = 192.168.1.254)(PORT = 1521)))(CONNECT_DATA = (SERVICE_NAME = SAIB)));User ID=SAIB2000;Password=M4R1Z4; ";
                                                      connection.ConnectionString = connString;
                                                      connection.Open();
                                                      if (connection != null)
                                                      {

                                                          StringBuilder str = new StringBuilder();
                                                          str.Remove(0, str.Length);
                                                          str.Append("SELECT prod.PROD_ID produto, ");
                                                          str.Append("prod.PROD_DESC descricao, ");
                                                          str.Append("REPLACE(prodf.PROF_PESO_B,',','.') pesoun, ");
                                                          str.Append("REPLACE(sum(mpfp.PEDF_QTDE * prodf.PROF_PESO_B),',','.') totaldopeso, ");
                                                          if (r3 == 1)
                                                          {
                                                              str.Append("REPLACE((round((((" + vlrtotalcard4 + "-(" + vlrtotalcard4 + "*" + aliquotacreditoimposto + "/100)) / tb2.totalgeralcaixas) * sum(mpfp.PEDF_QTDE)) / sum(mpfp.PEDF_QTDE),2)),',','.') vlrfreteun, ");
                                                              str.Append("REPLACE(round((((" + vlrtotalcard4 + "-(" + vlrtotalcard4 + "*" + aliquotacreditoimposto + "/100)) / tb2.totalgeralcaixas) * sum(mpfp.PEDF_QTDE)),2),',','.') vlrfretetotal, ");
                                                              str.Append("REPLACE(   round( (" + vlrtotalcard4 + " * " + aliquotacreditoimposto + "/100) ,2),',','.') as totaldeducaoimp, ");
                                                              str.Append("REPLACE(" + vlrtotalcard4 + ",',','.') as totalfretebruto, ");
                                                          }
                                                          else if (r1 == 1)
                                                          {
                                                              str.Append("REPLACE((round( (((" + vlrporkg + "* sum(mpfp.PEDF_QTDE * prodf.PROF_PESO_B)))  - ( ( (" + vlrporkg + "* sum(mpfp.PEDF_QTDE * prodf.PROF_PESO_B)))*" + aliquotacreditoimposto + "/100 )) / sum(mpfp.PEDF_QTDE),3)),',','.') vlrfreteun, ");
                                                              str.Append("REPLACE(round((((" + vlrporkg + "* sum(mpfp.PEDF_QTDE * prodf.PROF_PESO_B)))  - ( ( (" + vlrporkg + "* sum(mpfp.PEDF_QTDE * prodf.PROF_PESO_B)))*" + aliquotacreditoimposto + "/100 )),2),',','.') vlrfretetotal, ");
                                                              str.Append("REPLACE( round(  ( (" + vlrporkg + " * tb3.totalgeralpeso) * " + aliquotacreditoimposto + "/100) ,2),',','.') as totaldeducaoimp, ");
                                                              str.Append("REPLACE(round( (" + vlrporkg + " * tb3.totalgeralpeso),2),',','.') as totalfretebruto, ");
                                                          }
                                                          else if (r4 == 1)
                                                          {
                                                              str.Append("0.00 vlrfreteun, ");
                                                              str.Append("0.00 vlrfretetotal, ");
                                                              str.Append("0.00 as totaldeducaoimp, ");
                                                              str.Append("0.00 as totalfretebruto, ");
                                                          }

                                                          str.Append("sum(mpfp.PEDF_QTDE) totaldecaixas, ");
                                                          str.Append("REPLACE(sum(mpfp.PEDF_QTDE * mpfp.PEDF_VLR_UNIT),',','.') valortotal, ");
                                                          str.Append("REPLACE(tb1.vlrtotalgeral,',','.') vlrtotalgeral, ");
                                                          str.Append("tb2.totalgeralcaixas AS totalgeralcaixas, ");
                                                          str.Append("REPLACE(tb3.totalgeralpeso,',','.') as totalgeralpeso ");

                                                          str.Append("FROM MA_PEDIDO_FAT mpf, ");
                                                          str.Append("MA_PEDIDO_FAT_P mpfp, ");
                                                          str.Append("PRODUTO prod , ");
                                                          str.Append("PRODUTO_FAT prodf, ");

                                                          str.Append("(SELECT sum(mpfp.PEDF_QTDE * mpfp.PEDF_VLR_UNIT) AS vlrtotalgeral ");
                                                          str.Append("FROM MA_PEDIDO_FAT mpf, ");
                                                          str.Append("MA_PEDIDO_FAT_P mpfp, ");
                                                          str.Append("PRODUTO prod , ");
                                                          str.Append("PRODUTO_FAT prodf ");
                                                          str.Append("WHERE mpf.PEDF_EMP_ID = mpfp.PEDF_PEDF_EMP_ID and ");
                                                          str.Append("mpf.PEDF_ID = mpfp.PEDF_PEDF_ID and ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prod.PROD_EMP_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prod.PROD_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prodf.PROF_PROD_EMP_ID  AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prodf.PROF_PROD_ID and ");
                                                          str.Append("mpf.PEDF_ID in(" + numped + ") ) tb1, ");


                                                          str.Append("(SELECT sum(mpfp.PEDF_QTDE) AS totalgeralcaixas ");
                                                          str.Append("FROM MA_PEDIDO_FAT mpf, ");
                                                          str.Append("MA_PEDIDO_FAT_P mpfp, ");
                                                          str.Append("PRODUTO prod , ");
                                                          str.Append("PRODUTO_FAT prodf ");
                                                          str.Append("WHERE mpf.PEDF_EMP_ID = mpfp.PEDF_PEDF_EMP_ID and ");
                                                          str.Append("mpf.PEDF_ID = mpfp.PEDF_PEDF_ID and ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prod.PROD_EMP_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prod.PROD_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prodf.PROF_PROD_EMP_ID  AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prodf.PROF_PROD_ID and ");
                                                          str.Append("mpf.PEDF_ID in(" + numped + ") ) tb2, ");

                                                          str.Append("(SELECT sum(mpfp.PEDF_QTDE * prodf.PROF_PESO_B) AS totalgeralpeso ");
                                                          str.Append("FROM MA_PEDIDO_FAT mpf, ");
                                                          str.Append("MA_PEDIDO_FAT_P mpfp, ");
                                                          str.Append("PRODUTO prod , ");
                                                          str.Append("PRODUTO_FAT prodf ");
                                                          str.Append("WHERE mpf.PEDF_EMP_ID = mpfp.PEDF_PEDF_EMP_ID and ");
                                                          str.Append("mpf.PEDF_ID = mpfp.PEDF_PEDF_ID and ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prod.PROD_EMP_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prod.PROD_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prodf.PROF_PROD_EMP_ID  AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prodf.PROF_PROD_ID and ");
                                                          str.Append("mpf.PEDF_ID in(" + numped + ") ) tb3 ");

                                                          str.Append("WHERE mpf.PEDF_EMP_ID = mpfp.PEDF_PEDF_EMP_ID and ");
                                                          str.Append("mpf.PEDF_ID = mpfp.PEDF_PEDF_ID and ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prod.PROD_EMP_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prod.PROD_ID AND ");
                                                          str.Append("mpfp.PEDF_PROD_EMP_ID = prodf.PROF_PROD_EMP_ID  AND ");
                                                          str.Append("mpfp.PEDF_PROD_ID = prodf.PROF_PROD_ID and ");
                                                          str.Append("mpf.PEDF_ID in(" + numped + ") ");
                                                          str.Append("GROUP BY prod.PROD_ID, ");
                                                          str.Append("prod.PROD_DESC, ");
                                                          str.Append("prodf.PROF_PESO_B, ");
                                                          str.Append("tb1.vlrtotalgeral, ");
                                                          str.Append("tb2.totalgeralcaixas, ");
                                                          str.Append("tb3.totalgeralpeso ");
                                                          str.Append("ORDER BY prod.PROD_ID ");

                                                          using (OracleCommand cmd = new OracleCommand(str.ToString(), connection))
                                                          {
                                                              using (OracleDataReader reader = cmd.ExecuteReader())
                                                              {
                                                                  if (reader.HasRows)
                                                                  {
                                                                      while (reader.Read())
                                                                      {
                                                                          produto.Add(new Produto()
                                                                          {
                                                                              produto = reader["produto"].ToString(),
                                                                              descricao = reader["descricao"].ToString(),
                                                                              pesoun = reader["pesoun"].ToString(),
                                                                              totalpeso = reader["totaldopeso"].ToString(),
                                                                              vlrfreteun = reader["vlrfreteun"].ToString(),
                                                                              vlrfretetotal = reader["vlrfretetotal"].ToString(),
                                                                              totaldecaixas = reader["totaldecaixas"].ToString(),
                                                                              vlrtotal = reader["valortotal"].ToString(),
                                                                              totalfretebruto = reader["totalfretebruto"].ToString(),
                                                                              totalgeralcaixas = reader["totalgeralcaixas"].ToString(),
                                                                              totalgeralpeso = reader["totalgeralpeso"].ToString(),
                                                                              vlrtotalgeral = reader["vlrtotalgeral"].ToString(),
                                                                              totaldeducaoimp = reader["totaldeducaoimp"].ToString(),
                                                                              vlrfretetotalgeral = "0", //reader["vlrfretetotalgeral"].ToString(),
                                                                          });
                                                                      }
                                                                      reader.Close();
                                                                      jsonString = JsonSerializer.Serialize(produto);
                                                                  }
                                                              }
                                                          }
                                                      }
                                                  }
                                                  catch (Exception ex)
                                                  {
                                                      var erroretorno = new ErroRetorno
                                                      {
                                                          mensagem = ex.Message
                                                      };
                                                      jsonString = JsonSerializer.Serialize(erroretorno);
                                                  }
                                                  finally
                                                  {
                                                      if (connection != null)
                                                      {
                                                          connection.Close();
                                                      }
                                                  }

                                                  return jsonString;
                                              });

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
//
//
app.UseCors(policyName);

//app.UseCors(x => x
//    .AllowAnyMethod()
//    .AllowAnyHeader()
//    .SetIsOriginAllowed(origin => true) // allow any origin
//    .AllowCredentials()); // allow credentials


app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//app.MapControllers();
app.UseSwagger();
app.Run();
