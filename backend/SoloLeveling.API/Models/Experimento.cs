using System.Text.Json.Serialization;

namespace SoloLeveling.API.Models;

public class Experimento
{
    public int      Id           { get; set; }
    public int      PerfilId     { get; set; }
    [JsonIgnore] public Perfil? Perfil { get; set; }
    public string   Titulo       { get; set; } = string.Empty;
    public string   Descricao    { get; set; } = string.Empty;
    public int      DuracaoDias  { get; set; } = 21;
    public DateTime DataInicio   { get; set; } = DateTime.Today;
    public bool     Ativo        { get; set; } = true;
    public bool     Convertido   { get; set; } = false;
    public List<ExperimentoDia> Dias { get; set; } = [];
}

public class ExperimentoDia
{
    public int      Id             { get; set; }
    public int      ExperimentoId  { get; set; }
    [JsonIgnore] public Experimento? Experimento { get; set; }
    public DateTime Data           { get; set; } = DateTime.Today;
}
