using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ClickClok.Models;
using ClickClok.Helpers;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using Gestionale.Models;

namespace AppointmentManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        // Costruttore del controller
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            try
            {
                Debug.WriteLine("Inizio del metodo Register");

                // Logga i dati ricevuti
                Debug.WriteLine($"Username ricevuto: '{model.Username}'");
                Debug.WriteLine($"Password ricevuta: '{model.Password}'");

                // Verifica che i campi username e password non siano vuoti
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                {
                    Debug.WriteLine("Username o password vuoti");
                    return Json(new { success = false, message = "I campi username e password sono obbligatori." });
                }

                // Verifica se l'utente esiste già nel database
                var existingUser = _context.Users.FirstOrDefault(u => u.Username.ToLower() == model.Username.ToLower());
                if (existingUser != null)
                {
                    Debug.WriteLine("Username già in uso");
                    return Json(new { success = false, message = "Username già in uso." });
                }

                // Genera un salt univoco per l'utente
                var salt = Guid.NewGuid().ToString();
                Debug.WriteLine($"Salt generato: {salt}");

                // Calcola l'hash della password combinata con il salt
                var hashedPassword = PasswordHelper.HashPassword(model.Password, salt);
                Debug.WriteLine($"Password hashata: {hashedPassword}");

                // Crea un nuovo utente
                var newUser = new User
                {
                    Username = model.Username,
                    Salt = salt,
                    Password = hashedPassword
                };

                // Aggiungi il nuovo utente al database
                _context.Users.Add(newUser);
                _context.SaveChanges();
                Debug.WriteLine("Nuovo utente salvato nel database");

                // Restituisce una risposta positiva in formato JSON
                return Json(new { success = true, message = "Registrazione avvenuta con successo. Puoi ora effettuare il login." });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Errore durante la registrazione: {ex.Message}");
                return Json(new { success = false, message = "Si è verificato un errore durante la registrazione. Riprovare." });
            }
        }




        // Metodo GET per visualizzare la pagina di login
        public IActionResult Login()
        {
            // Se l'utente è già loggato, reindirizza alla home
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            // Verifica se esiste un token di sessione nei cookie
            var sessionToken = Request.Cookies["SessionToken"];
            if (!string.IsNullOrEmpty(sessionToken))
            {
                // Verifica se il token di sessione è valido
                var session = _context.Sessions
                    .Include(s => s.User)
                    .FirstOrDefault(s => s.SessionToken == sessionToken);

                // Se la sessione è valida, imposta i dati della sessione
                if (session != null)
                {
                    HttpContext.Session.SetInt32("UserId", session.UserID);
                    HttpContext.Session.SetString("Username", session.User.Username);
                    return RedirectToAction("Index", "Home");
                }
            }

            // Se l'utente non è loggato, mostra la pagina di login
            return View();
        }


        // Metodo POST per il login
        [HttpPost]
        public IActionResult Login(string name, string password)
        {
            try
            {
                // Verifica che i campi username e password non siano vuoti
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                {
                    Debug.WriteLine($"[DEBUG] Tentativo di login con campi vuoti: Username = {name}, Password = {password}");
                    ViewBag.ErrorMessage = "I campi username e password sono obbligatori.";
                    return View("Login");
                }

                // Log dei dati in ingresso
                Debug.WriteLine($"[DEBUG] Tentativo di login: Username = {name}, Password = {password}");

                // Cerca l'utente nel database, ignorando maiuscole/minuscole nel nome utente
                var user = _context.Users.FirstOrDefault(u =>
                    u.Username.ToLower() == name.ToLower());

                // Log dell'utente trovato (o null)
                if (user != null)
                {
                    Debug.WriteLine($"[DEBUG] Utente trovato: {user.Username}, Salt = {user.Salt}");
                }
                else
                {
                    Debug.WriteLine("[DEBUG] Utente non trovato.");
                }

                // Se l'utente esiste, controlla la password
                if (user != null)
                {
                    // Hash la password inserita usando il salt dell'utente
                    var hashedPassword = PasswordHelper.HashPassword(password, user.Salt);

                    // Log dell'hash calcolato
                    Debug.WriteLine($"[DEBUG] Hash della password inserita: {hashedPassword}");

                    // Confronta l'hash della password inserita con quello nel database
                    if (hashedPassword == user.Password)
                    {
                        Debug.WriteLine($"[DEBUG] Password corretta per l'utente: {user.Username}");

                        // Se l'hash corrisponde, crea una sessione
                        var session = new Session
                        {
                            UserID = user.UserId,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Sessions.Add(session);
                        _context.SaveChanges();

                        // Imposta il cookie di sessione
                        Response.Cookies.Append("SessionToken", session.SessionToken, new CookieOptions
                        {
                            HttpOnly = true,
                            IsEssential = true,
                            Expires = DateTime.UtcNow.AddDays(1)
                        });

                        // Imposta i dati della sessione
                        HttpContext.Session.SetInt32("UserId", user.UserId);
                        HttpContext.Session.SetString("Username", user.Username);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        Debug.WriteLine("[DEBUG] Hash non corrisponde. Credenziali errate.");

                        // Se l'hash non corrisponde, mostra un messaggio di errore
                        ViewBag.ErrorMessage = "Credenziali errate!";
                        return View("Login");
                    }
                }

                // Se l'utente non esiste, mostra un messaggio di errore
                Debug.WriteLine("[DEBUG] L'utente non esiste nel database.");
                ViewBag.ErrorMessage = "Credenziali errate!";
                return View("Login");
            }
            catch (Exception ex)
            {
                // Log dell'eccezione se c'è un errore
                Debug.WriteLine($"[ERROR] Errore durante il login: {ex.Message}");

                // Se si verifica un errore, mostra un messaggio di errore generico
                ViewBag.ErrorMessage = "Si è verificato un errore. Riprovare.";
                return View("Login");
            }
        }



        // Metodo per il logout dell'utente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            try
            {
                // Recupera il token di sessione dal cookie
                var sessionToken = Request.Cookies["SessionToken"];
                if (!string.IsNullOrEmpty(sessionToken))
                {
                    // Rimuove la sessione dal database
                    var session = _context.Sessions.FirstOrDefault(s => s.SessionToken == sessionToken);
                    if (session != null)
                    {
                        _context.Sessions.Remove(session);
                        _context.SaveChanges();
                    }

                    // Elimina il cookie di sessione
                    Response.Cookies.Delete("SessionToken");
                }

                // Pulisce la sessione dell'utente
                HttpContext.Session.Clear();
            }
            catch
            {
                // Ignora eventuali errori durante il logout
            }

            // Reindirizza alla pagina di login
            return RedirectToAction("Login", "Account");
        }
    }
}
