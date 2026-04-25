using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parcial_Vilchez_Cristopher.Data;
using Parcial_Vilchez_Cristopher.Models;
using Parcial_Vilchez_Cristopher.Services;

// 1. AQUÍ NACE EL BUILDER (Todo debe ir debajo de esto)
var builder = WebApplication.CreateBuilder(args);

// --- SERVICIOS ---
builder.Services.AddControllersWithViews();

// Servicios de la Pregunta 3
builder.Services.AddScoped<IMatriculaService, MatriculaService>();
builder.Services.AddHttpContextAccessor(); // Permite usar Sesiones en el Layout

// --- PREGUNTA 4: REDIS Y SESIONES ---
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");

// Si no hay conexión configurada, usamos la memoria local (Ideal para probar en tu PC sin instalar Redis)
if (string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddDistributedMemoryCache();
}
// Si hay conexión, usamos Redis (Ideal para cuando lo subamos a Render)
else
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "PortalAcademico_";
    });
}

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// -------------------------------------

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=app.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

// --- SEED DATA ---
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.Migrate();

    if (!await roleManager.RoleExistsAsync("Coordinador"))
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));

    var adminEmail = "admin@universidad.edu.pe";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Coordinador");
    }

    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso { Codigo = "TALL01", Nombre = "Taller de Liderazgo", Creditos = 2, CupoMaximo = 1, HorarioInicio = new TimeSpan(16,0,0), HorarioFin = new TimeSpan(18,0,0), Activo = true },
            new Curso { Codigo = "CS101", Nombre = "Programación .NET 9", Creditos = 5, CupoMaximo = 10, HorarioInicio = new TimeSpan(8,0,0), HorarioFin = new TimeSpan(10,0,0), Activo = true },
            new Curso { Codigo = "CS102", Nombre = "Arquitectura Cloud", Creditos = 4, CupoMaximo = 20, HorarioInicio = new TimeSpan(10,0,0), HorarioFin = new TimeSpan(12,0,0), Activo = true },
            new Curso { Codigo = "DB101", Nombre = "Base de Datos I", Creditos = 4, CupoMaximo = 15, HorarioInicio = new TimeSpan(9,0,0), HorarioFin = new TimeSpan(11,0,0), Activo = true },
            new Curso { Codigo = "MAT201", Nombre = "Cálculo II", Creditos = 4, CupoMaximo = 25, HorarioInicio = new TimeSpan(14,0,0), HorarioFin = new TimeSpan(16,0,0), Activo = true },
            new Curso { Codigo = "FIS301", Nombre = "Física III", Creditos = 4, CupoMaximo = 15, HorarioInicio = new TimeSpan(15,0,0), HorarioFin = new TimeSpan(17,0,0), Activo = true }
        );
        context.SaveChanges();
    }
}

// --- MIDDLEWARE ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// PREGUNTA 4: ACTIVAR SESIONES (Debe ir entre Authorization y MapControllerRoute)
app.UseSession(); 

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();