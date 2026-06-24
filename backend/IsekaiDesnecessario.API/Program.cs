using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using IsekaiDesnecessario.API;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<XpService>();
builder.Services.AddScoped<LootboxService>();
builder.Services.AddScoped<MissaoService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NotificacaoService>();
builder.Services.AddScoped<PontosAtributoService>();
builder.Services.AddHostedService<DiarioLimpezaService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.MapInboundClaims = false; // preserva "sub" sem remap para NameIdentifier
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });
builder.Services.AddAuthorization();

// Origens permitidas vêm do config (Cors:AllowedOrigins). Em produção, defina
// as URLs do app; sem configuração cai para permissivo (útil em dev).
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
    {
        if (corsOrigins is { Length: > 0 })
            p.WithOrigins(corsOrigins).AllowAnyMethod().AllowAnyHeader();
        else
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    }));

builder.Services.Configure<FormOptions>(opt => opt.MultipartBodyLengthLimit = Limites.TamanhoMaxFotoBytes);

var app = builder.Build();

// Aplica migrations pendentes automaticamente na inicialização
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("healthy"));
app.Run();
