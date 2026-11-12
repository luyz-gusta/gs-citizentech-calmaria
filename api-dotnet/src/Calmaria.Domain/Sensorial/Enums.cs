namespace Calmaria.Domain.Sensorial;

// Na API, os valores saem em maiúsculas com sublinhado (MERCADO, LUZ_FORTE), como manda o contrato.

/// <summary>Tipo do lugar no mapa.</summary>
public enum TipoLugar
{
    Mercado,
    Estacao,
    Parque,
    Refugio,
    Outro
}

/// <summary>Nível do Índice de Sobrecarga Sensorial: 0 a 39, 40 a 69 e 70 a 100.</summary>
public enum NivelSobrecarga
{
    Calmo,
    Moderado,
    Intenso
}

/// <summary>Nível da previsão de reclamações de barulho do distrito, pelos tercis do próprio distrito.</summary>
public enum NivelPrevisao
{
    Baixo,
    Medio,
    Alto
}
