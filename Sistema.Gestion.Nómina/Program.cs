using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.N�mina;
using Sistema.Gestion.N�mina.Entitys;
using Sistema.Gestion.N�mina.Helpers;
using Sistema.Gestion.N�mina.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SistemaGestionNominaContext>(op => op.UseSqlServer("name=DefaultConnection"));//configuracion de la cadena de coneccion en la clase DBContext
builder.Services.AddTransient<LoginService>();
builder.Services.AddTransient<Hasher>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
