using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ClickClok.Models;
using ClickClok.Helpers;
using Microsoft.AspNetCore.Http;

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
                    ViewBag.ErrorMessage = "I campi username e password sono obbligatori.";
                    return View("Login");
                }

                // Cerca l'utente nel database
                var user = _context.Users.FirstOrDefault(u =>
                    u.Username.ToLower() == name.ToLower() && u.Password == password);

                // Se l'utente esiste, crea una sessione
                if (user != null)
                {
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

                // Se le credenziali sono errate, mostra un messaggio di errore
                ViewBag.ErrorMessage = "Credenziali errate!";
                return View("Login");
            }
            catch
            {
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
