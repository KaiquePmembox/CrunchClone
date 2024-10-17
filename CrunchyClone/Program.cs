using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o servi�o de autentica��o com cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login"; // Define o caminho de redirecionamento para login
    options.LogoutPath = "/Account/Logout"; // Define o caminho de redirecionamento para logout
    options.ExpireTimeSpan = TimeSpan.FromHours(1); // Define o tempo de expira��o do cookie
    options.SlidingExpiration = true; // Renova o cookie se a sess�o estiver ativa
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure o pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ativa a autentica��o e autoriza��o
app.UseAuthentication(); // Importante: esse middleware deve ser chamado antes do UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
