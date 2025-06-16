using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using QueuingSystemBe.Models;
using QueuingSystemBe.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using QueuingSystemBe.HubForRealTime;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json").Build();

// Add services to the container.
IWebHostEnvironment hostEnvironment = builder.Environment;
hostEnvironment.WebRootPath = Directory.GetCurrentDirectory();

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddSingleton<UserConnectSvc>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
{
    option.RequireHttpsMetadata = false;
    option.SaveToken = true;
    option.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration["Jwt:Audience"],
        ValidIssuer = configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("ConnectionDb")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IServiceSvc, ServiceSvc>();
builder.Services.AddScoped<IUserSvc, UserSvc>();
builder.Services.AddScoped<IStatisticSvc, StatisticSvc>();
builder.Services.AddScoped<IAuthenticationSvc, AuthenticationSvc>();
builder.Services.AddScoped<IAssignmentSvc, AssignmentSvc>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();
app.MapHub<AuthHub>("/authHub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();