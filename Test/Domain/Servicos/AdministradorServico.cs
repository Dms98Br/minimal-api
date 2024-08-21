using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MinimalsApi.Dominio.Entidades;
using MinimalsApi.Dominio.Servicos;
using MinimalsApi.Infraestrutura.Db;

namespace Test.Domain.Servicos;

[TestClass]
public class AdministradorServicoTest
{
    private DbContext CriarContextoDeTeste()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.GetFullPath(Path.Combine(assemblyPath ?? "", "..", "..", ".."));
        var builder = new ConfigurationBuilder()
        .SetBasePath(path ?? Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

        var configuration = builder.Build();

        return new DbContexto(configuration);

    }
    [TestMethod]
    public void TestandoSalvarAdministrador()
    {
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");

        //Arrange
        var adm = new Administrador();

        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";
        var admServico = new AdministradorServico((DbContexto)context);
        //Act
        admServico.Incluir(adm);
        var admResult = admServico.BuscarPorId(adm.Id);
        //Assert
        Assert.AreEqual(1, admServico.Todos(1).Count());

    }
    [TestMethod]
    public void TestandoBuscaPorId()
    {
        var context = CriarContextoDeTeste();
        context.Database.ExecuteSqlRaw("TRUNCATE TABLE administradores");

        //Arrange
        var adm = new Administrador();

        adm.Email = "teste@teste.com";
        adm.Senha = "teste";
        adm.Perfil = "Adm";
        var admServico = new AdministradorServico((DbContexto)context);
        //Act
        admServico.Incluir(adm);
        var admResult = admServico.BuscarPorId(adm.Id);
        //Assert
        Assert.AreEqual(1, admResult.Id);

    }
}



