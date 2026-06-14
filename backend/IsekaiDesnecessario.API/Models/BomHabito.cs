namespace IsekaiDesnecessario.API.Models;

public class BomHabito
{
    public int Id { get; set; }
    public int PerfilId { get; set; }
    public Perfil? Perfil { get; set; }
    public string Habito { get; set; } = string.Empty;
    public int Xp { get; set; }
    public string Frequencia { get; set; } = string.Empty;
    public int Streak { get; set; }
    public DateTime? UltimaExecucao { get; set; }
    public int? AtributoId { get; set; }
    public Atributo? Atributo { get; set; }
}
