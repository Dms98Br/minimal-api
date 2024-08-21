using MinimalsApi.Dominio.Enuns;

namespace MinimalsApi.Dominio.ModelView;

public record AdministradorLogadoModelView
{
    public string Email { get; set; } = default!;
    public string Perfil { get; set; } = default!;
    public string Token { get; set; } = default!;
}