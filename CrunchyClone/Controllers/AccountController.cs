namespace CrunchyClone.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using MySql.Data.MySqlClient;
    using CrunchyClone.Models;
    using CrunchyClone.Services;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;

    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public IActionResult Register(User user, string password)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string hashedPassword = AuthService.HashPassword(password);

                var query = "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@Username, @Email, @PasswordHash)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Login");
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var query = "SELECT * FROM Users WHERE Username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string storedPasswordHash = reader.GetString("PasswordHash");

                    if (AuthService.VerifyPassword(password, storedPasswordHash))
                    {
                        // Cria as claims do usuário
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, username)
                        };

                        // Cria o identity do usuário
                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // Cria o principal (com as claims) para a sessão de autenticação
                        var principal = new ClaimsPrincipal(identity);

                        // Define as propriedades do cookie
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = true, // Cookie persistente
                            ExpiresUtc = DateTime.UtcNow.AddHours(1) // Duração do cookie
                        };

                        // Autentica o usuário e salva o cookie
                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.Message = "Usuário ou senha inválidos!";
            return View();
        }

        // GET: /Account/MinhaConta
        [Authorize] // Certifique-se de que o usuário está autenticado
        public IActionResult MinhaConta()
        {
            string username = User.Identity.Name;
            string email = string.Empty;

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                var query = "SELECT Email FROM Users WHERE Username = @Username";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    email = reader.GetString("Email");
                }
            }

            // Passa os dados para a view
            ViewBag.Username = username;
            ViewBag.Email = email;

            return View();
        }

        // POST: /Account/UpdateAccount
        [HttpPost]
        [Authorize] // Certifique-se de que o usuário está autenticado
        public IActionResult UpdateAccount(string newName, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "As senhas não coincidem!";
                return RedirectToAction("MinhaConta"); // Redireciona para a view "MinhaConta"
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                string username = User.Identity.Name; // Obtém o nome do usuário autenticado

                // Verifica se há uma nova senha e faz o hash
                string hashedPassword = null;
                if (!string.IsNullOrEmpty(newPassword))
                {
                    hashedPassword = AuthService.HashPassword(newPassword);
                }

                // Atualiza o nome e a senha, se necessário
                var query = "UPDATE Users SET Username = @Username";
                if (!string.IsNullOrEmpty(hashedPassword))
                {
                    query += ", PasswordHash = @PasswordHash";
                }
                query += " WHERE Username = @OldUsername";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", newName);
                if (!string.IsNullOrEmpty(hashedPassword))
                {
                    cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                }
                cmd.Parameters.AddWithValue("@OldUsername", username);
                cmd.ExecuteNonQuery();
            }

            ViewBag.Message = "Conta atualizada com sucesso!";
            return RedirectToAction("MinhaConta"); // Redireciona para a view "MinhaConta"
        }

        // POST: /Account/Logout
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
