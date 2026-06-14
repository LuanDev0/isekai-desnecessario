using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using IsekaiDesnecessario.API.Data;
using IsekaiDesnecessario.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<XpService>();
builder.Services.AddScoped<GachaService>();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Limite de 5MB para upload
builder.Services.Configure<FormOptions>(opt => opt.MultipartBodyLengthLimit = 5 * 1024 * 1024);

var app = builder.Build();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles(); // serve wwwroot/uploads/
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
