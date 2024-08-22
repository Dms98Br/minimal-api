using MinimalsApi;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalsApi.Dominio.Entidades;
using MinimalsApi.Dominio.Enuns;
using MinimalsApi.Dominio.Interfaces;
using MinimalsApi.Dominio.ModelView;
using MinimalsApi.Dominio.ModelViews;
using MinimalsApi.Dominio.Servicos;
using MinimalsApi.DTOs;
using MinimalsApi.Infraestrutura.Db;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        key = Configuration?.GetSection("Jwt")?.ToString() ?? "123456";

    }
    private string key = "";
    public IConfiguration Configuration { get; set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

        services.AddAuthorization();

        services.AddScoped<IAdministradorServico, AdministradorServico>();
        services.AddScoped<IVeiculoServico, VeiculoServico>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira aqui seu token JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
            });
        });

        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
            );
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
            #endregion

            #region Administrador

            string GerarTokenJwt(Administrador administrador)
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>(){
        new Claim("Email",administrador.Email),
        new Claim("Perfil", administrador.Perfil),
        new Claim(ClaimTypes.Role, administrador.Perfil),
    };

                var token = new JwtSecurityToken(
                claims: claims,
                  expires: DateTime.Now.AddDays(1),
                  signingCredentials: credentials
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }

            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) =>
            {
                var adm = administradorServico.Login(loginDTO);
                if (adm != null)
                {
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministradorLogadoModelView
                    {
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token,
                    });
                }
                else
                {
                    return Results.Unauthorized();
                }
            }).AllowAnonymous().WithTags("Administradores");

            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) =>
            {
                var validacao = new ErrosDeValidacao
                {
                    Mensagens = new List<string>()
                };

                if (string.IsNullOrEmpty(administradorDTO.Email))
                    validacao.Mensagens.Add("E-mail é obrigatório");

                if (string.IsNullOrEmpty(administradorDTO.Senha))
                    validacao.Mensagens.Add("Senha é obrigatória");

                if (validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao);



                var adm = new Administrador
                {
                    Email = administradorDTO.Email,
                    Senha = administradorDTO.Senha,
                    Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Editor.ToString(),
                };

                administradorServico.Incluir(adm);
                return Results.Created($"/administrador/{adm.Id}", new AdministradorModelView
                {
                    Id = adm.Id,
                    Email = adm.Email,
                    Perfil = adm.Perfil
                });

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapGet("/administradores", ([FromQuery] int pagina, IAdministradorServico administradorServico) =>
            {
                var adms = new List<AdministradorModelView>();
                var administradores = administradorServico.Todos(pagina);

                foreach (var adm in administradores)
                {
                    adms.Add(new AdministradorModelView
                    {
                        Id = adm.Id,
                        Email = adm.Email,
                        Perfil = adm.Perfil
                    });
                }

                return Results.Ok(adms);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            endpoints.MapGet("/administradores/{id}", ([FromRoute] int Id, IAdministradorServico administradorServico) =>
            {
                var adm = administradorServico.BuscarPorId(Id);
                if (adm == null) return Results.NotFound();
                return Results.Ok(new AdministradorModelView
                {
                    Id = adm.Id,
                    Email = adm.Email,
                    Perfil = adm.Perfil
                });
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Administradores");

            #endregion

            #region Veiculos

            ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
            {
                var validacao = new ErrosDeValidacao
                {
                    Mensagens = new List<string>()
                };
                if (string.IsNullOrEmpty(veiculoDTO.Nome))
                    validacao.Mensagens.Add("O nome não pode ser vazio");

                if (string.IsNullOrEmpty(veiculoDTO.Marca))
                    validacao.Mensagens.Add("A marca não pode ficar em branco");

                if (veiculoDTO.Ano < 1886)
                    validacao.Mensagens.Add("Não foram fábricados veículos nesse período");

                return validacao;
            }

            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculosDTO, IVeiculoServico veiculoServico) =>
            {
                var validacao = validaDTO(veiculosDTO);
                if (validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao.Mensagens);

                var veiculo = new Veiculo
                {
                    Nome = veiculosDTO.Nome,
                    Marca = veiculosDTO.Marca,
                    Ano = veiculosDTO.Ano,
                };
                veiculoServico.Incluir(veiculo);
                return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endpoints.MapGet("/veiculos", ([FromQuery] int pagina, IVeiculoServico veiculoServico) =>
            {
                var veiculos = veiculoServico.Todos(pagina);

                return Results.Ok(veiculos);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endpoints.MapGet("/veiculos/{id}", ([FromRoute] int Id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(Id);
                if (veiculo == null) return Results.NotFound();
                return Results.Ok(veiculo);
            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm,Editor" })
            .WithTags("Veiculos");

            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int Id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(Id);

                if (veiculo == null) return Results.NotFound();

                var validacao = validaDTO(veiculoDTO);
                if (validacao.Mensagens.Count > 0)
                    return Results.BadRequest(validacao.Mensagens);

                veiculo.Nome = veiculoDTO.Nome;
                veiculo.Marca = veiculoDTO.Marca;
                veiculo.Ano = veiculoDTO.Ano;

                veiculoServico.Atualizar(veiculo);

                return Results.Ok(veiculo);

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculos");

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int Id, IVeiculoServico veiculoServico) =>
            {
                var veiculo = veiculoServico.BuscarPorId(Id);

                if (veiculo == null) return Results.NotFound();

                veiculoServico.Apagar(veiculo);

                return Results.NoContent();

            })
            .RequireAuthorization()
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
            .WithTags("Veiculos");

            #endregion
        });
    }

}