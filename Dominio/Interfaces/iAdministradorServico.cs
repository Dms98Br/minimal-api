using MinimalsApi.Dominio.Entidades;
using MinimalsApi.DTOs;

namespace MinimalsApi.Dominio.Interfaces;
public interface IAdministradorServico
{
    Administrador? Login(LoginDTO loginDTO);

}