using System.Data.Common;
using MinimalsApi.Dominio.Entidades;
using MinimalsApi.Dominio.Interfaces;
using MinimalsApi.DTOs;
using MinimalsApi.Infraestrutura.Db;

namespace MinimalsApi.Dominio.Servicos;


public class AdministradorServico : IAdministradorServico
{
    private readonly DbContexto _contexto;
    public AdministradorServico(DbContexto contexto)
    {
        _contexto = contexto;
    }
    public Administrador? Login(LoginDTO loginDTO)
    {
        return _contexto.Administradores.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
    }

    public Administrador Incluir(Administrador administrador)
    {
        _contexto.Administradores.Add(administrador);
        _contexto.SaveChanges();
        return administrador;
    }

    public List<Administrador> Todos(int pagina = 1)
    {
        var query = _contexto.Administradores.AsQueryable();

        int itensPorPagina = 10;
        query = query.Skip((pagina - 1) * itensPorPagina).Take(itensPorPagina);
        return query.ToList();
    }

    public Administrador? BuscarPorId(int id)
    {
        return _contexto.Administradores.Where(v => v.Id == id).FirstOrDefault();
    }
}