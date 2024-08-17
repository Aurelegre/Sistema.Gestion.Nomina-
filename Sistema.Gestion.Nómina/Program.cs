using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Helpers;
using Sistema.Gestion.Nómina.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(op =>
{
    op.LoginPath = "/Login/Login";
    op.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    op.AccessDeniedPath = "/Login/Login";
});

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
