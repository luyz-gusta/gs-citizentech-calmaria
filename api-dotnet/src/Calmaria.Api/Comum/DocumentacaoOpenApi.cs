using System.Text.Json.Nodes;
using Calmaria.Api.Comunidade;
using Calmaria.Api.Privacidade;
using Calmaria.Api.Sensorial;
using Calmaria.Privacidade.Taint;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Calmaria.Api.Comum;

/// <summary>
/// Ajustes do documento OpenAPI que o Scalar mostra: os tipos de texto da privacidade aparecem como texto, e os
/// pedidos e respostas principais têm um exemplo pronto para testar dali mesmo. Os exemplos são os do contrato
/// (docs/disciplinas) e todos os dados são fictícios.
/// </summary>
internal static class DocumentacaoOpenApi
{
    private static readonly Dictionary<Type, string> Exemplos = new()
    {
        [typeof(NovoRelatoDto)] = """
            { "usuarioId": "3f2b7c1e-8a4d-4c55-9e0b-1d2a6b7c8e90", "lugarId": 1, "nota": 4,
              "gatilhos": ["BARULHO", "MULTIDAO"], "comentario": "Muito cheio às 18h",
              "visitadoEm": "2026-11-16T18:00:00-03:00" }
            """,
        [typeof(RelatoCriadoDto)] = """
            { "id": 31, "comentario": "Muito cheio às 18h", "removidos": [] }
            """,
        [typeof(NovoUsuarioDto)] = """{ "apelido": "Ma" }""",
        [typeof(PerfilSensorialDto)] = """
            { "sensibilidadeSom": 5, "sensibilidadeLuz": 3, "sensibilidadeMultidao": 4,
              "contatoApoioNome": "Mãe", "contatoApoioTelefone": "11900000000" }
            """,
        [typeof(LeituraRecebidaDto)] = """
            { "dispositivoId": "esp32-consolacao-01", "lugarId": 1, "somDb": 78.5, "luzLux": 1450,
              "movimentosMin": 12, "indice": 53, "nivel": "MODERADO" }
            """,
        [typeof(LugarEntradaDto)] = """
            { "nome": "Mercado da Rua Frei Caneca", "tipo": "MERCADO", "latitude": -23.5531, "longitude": -46.6545,
              "distrito": "CONSOLACAO", "endereco": "Rua Frei Caneca, 500", "horarioFuncionamento": "08:00-22:00" }
            """,
        [typeof(LugarResumoDto)] = """
            { "id": 1, "nome": "Estação Consolação (Linha 2)", "tipo": "ESTACAO", "latitude": -23.5575,
              "longitude": -46.6609, "distrito": "CONSOLACAO", "ehRefugio": false, "distanciaM": 640,
              "indiceAtual": 53, "nivelAtual": "MODERADO", "ultimaLeituraEm": "2026-11-16T18:05:00-03:00" }
            """,
        [typeof(RefugioMaisProximoDto)] = """
            { "lugar": { "id": 5, "nome": "Parque Trianon", "tipo": "REFUGIO", "latitude": -23.5617,
                         "longitude": -46.6573, "descricao": "Bancos na sombra e pouco barulho" },
              "distanciaM": 210, "minutosAPe": 3,
              "linkTrajeto": "https://www.google.com/maps/dir/?api=1&destination=-23.5617,-46.6573&travelmode=walking" }
            """,
        [typeof(PrevisaoDto)] = """
            { "distrito": "CONSOLACAO", "data": "2026-11-21", "reclamacoesPrevistas": 9.4, "nivel": "ALTO" }
            """,
        [typeof(TextoParaAnaliseDto)] = """{ "texto": "Meu CPF é 529.982.247-25 e meu filho é autista." }""",
        [typeof(AnaliseDto)] = """
            { "temDadoPessoal": true,
              "achados": [ { "categoria": "CPF", "trecho": "529.***.***-25" }, { "categoria": "SAUDE", "trecho": "<SAUDE>" } ],
              "textoSanitizado": "Meu CPF é [removido] e meu filho é <SAUDE>." }
            """,
        [typeof(FluxoDto)] = """
            { "origem": "relato.comentario", "destino": "RESPOSTA_PUBLICA", "categoria": "CPF", "peso": 4,
              "bloqueado": true, "ocorridoEm": "2026-11-17T12:10:03-03:00" }
            """
    };

    public static void Configurar(OpenApiOptions opcoes) =>
        opcoes.AddSchemaTransformer((schema, contexto, _) =>
        {
            var tipo = contexto.JsonTypeInfo.Type;
            if (tipo == typeof(TextoNaoConfiavel) || tipo == typeof(TextoSanitizado))
            {
                // No JSON, os dois são texto; a diferença só existe dentro da API (o taint pelo tipo).
                schema.Type = JsonSchemaType.String;
                schema.Properties?.Clear();
            }

            if (Exemplos.TryGetValue(tipo, out var exemplo))
                schema.Examples = [JsonNode.Parse(exemplo)!];
            return Task.CompletedTask;
        });
}
