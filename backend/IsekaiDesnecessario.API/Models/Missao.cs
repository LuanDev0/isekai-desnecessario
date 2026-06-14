namespace IsekaiDesnecessario.API.Models;

public class Missao
{
    public int Id { get; set; }
    public int PerfilId { get; set; }
    public Perfil? Perfil { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int TipoId { get; set; }
    public TipoMissao? Tipo { get; set; }
    public int RecompensaXp { get; set; }
    public int RecompensaMoedas { get; set; }
    public int Streak { get; set; }
    public bool Concluida { get; set; } = false;
    public DateTime? ConcluidaEm { get; set; }
    public DateTime? DataLimite { get; set; }
    public int? MissaoPrincipalId { get; set; }
    public int? AtributoId { get; set; }
    public Atributo? Atributo { get; set; }
}
