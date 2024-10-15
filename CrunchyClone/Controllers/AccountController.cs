namespace CrunchyClone.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using MySql.Data.MySqlClient;
    using CrunchyClone.Models;
    using CrunchyClone.Services;

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
        public IActionResult Login(string username, string password)
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
                        // Sessão de autenticação (pode usar cookies ou qualquer outra forma de session)
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ViewBag.Message = "Usuário ou senha inválidos!";
            return View();
        }
    }

}
