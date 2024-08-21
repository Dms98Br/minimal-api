using MinimalsApi.Dominio.Entidades;
using MinimalsApi.DTOs;

namespace MinimalsApi.Dominio.Interfaces;
public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);
    Administrador Incluir(Administrador administrador);
    List<Administrador> Todos(int pagina);
    Administrador? BuscarPorId(int id);

}