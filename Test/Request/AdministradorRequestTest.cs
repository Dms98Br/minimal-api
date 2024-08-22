using System.Text;
using System.Text.Json;
using MinimalsApi.DTOs;
using Test.Helpers;
using System.Net;
using MinimalsApi.Dominio.ModelView;

namespace Test.Request.AdministradorTest;

[TestClass]
public class AdministradorTest
{
    [ClassInitialize]
    public static void ClassInit(TestContext testContext)
    {
        Setup.ClassInit(testContext);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        Setup.ClassCleanup();
    }
    [TestMethod]
    public async Task TestarGetSetPropriedadesAsync()
    {
        // Arrange
        var loginDTO = new LoginDTO
        {
            Email = "adm@teste.com",
            Senha = "123456"
        };

        var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

        // Act
        var response = await Setup.client.PostAsync("/administradores/login", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsStringAsync();
        var admLogado = JsonSerializer.Deserialize<AdministradorLogadoModelView>(result, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.IsNotNull(admLogado?.Email ?? "");
        Assert.IsNotNull(admLogado?.Perfil ?? "");
        Assert.IsNotNull(admLogado?.Token ?? "");
    }
}