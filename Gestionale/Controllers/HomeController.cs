using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClickClok.Helpers;
using ClickClok.Models;
using System.Linq;

namespace ClickClok.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Azione principale per visualizzare la pagina iniziale
        public IActionResult Index()
        {
            var userId = SessionHelper.GetInt(HttpContext.Session, "UserId");
            _logger.LogInformation("UserId recuperato in Index: " + (userId.HasValue ? userId.Value.ToString() : "null"));

            // Se l'utente non è loggato, reindirizza alla pagina di login
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Effettua il login per visualizzare gli appuntamenti.";
                return RedirectToAction("Login", "Account");
            }

            // Recupera il nome dell'utente dal database
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId.Value);
            if (user != null)
            {
                ViewData["UserName"] = user.Username;
            }

            // Recupera gli appuntamenti dell'utente
            var appointments = _context.Appointments
                .Where(a => a.UserId == userId.Value)
                .OrderBy(a => a.AppointmentTime)
                .ToList();

            _logger.LogInformation("Trovati " + appointments.Count + " appuntamenti per userId: " + userId.Value);
            return View(appointments);
        }

        // Metodo di utilità per verificare se l'utente è loggato
        private bool IsUserLoggedIn(out int userId)
        {
            userId = SessionHelper.GetInt(HttpContext.Session, "UserId") ?? 0;
            _logger.LogInformation("UserId recuperato in IsUserLoggedIn: " + (userId > 0 ? userId.ToString() : "null"));
            return userId > 0;
        }

        // Metodo per creare un nuovo appuntamento
        [HttpPost]
        public JsonResult Create([FromBody] Appointment appointment)
        {
            // Verifica se l'utente è loggato
            if (!IsUserLoggedIn(out int userId))
            {
                return Json(new { success = false, message = "Effettua il login per creare un appuntamento." });
            }

            // Verifica che l'ora dell'appuntamento sia valida
            if (appointment.AppointmentTime == null || appointment.AppointmentTime <= DateTime.Now)
            {
                return Json(new { success = false, message = "Fornisci una data e un'ora valide per il futuro." });
            }

            // Imposta l'UserId dell'appuntamento
            appointment.UserId = userId;

            // Verifica se il modello dell'appuntamento è valido
            if (ModelState.IsValid)
            {
                try
                {
                    // Controlla se esiste già un appuntamento nello stesso orario
                    var existingAppointment = _context.Appointments
                        .FirstOrDefault(a => a.UserId == userId && a.AppointmentTime == appointment.AppointmentTime);

                    if (existingAppointment != null)
                    {
                        return Json(new { success = false, message = "Hai già un appuntamento in questo orario." });
                    }

                    // Salva l'appuntamento nel database
                    _context.Appointments.Add(appointment);
                    _context.SaveChanges();

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Si è verificato un errore durante il salvataggio dell'appuntamento." });
                }
            }

            return Json(new { success = false, message = "Dati non validi forniti." });
        }

        // Metodo per modificare un appuntamento
        [HttpPost]
        public IActionResult EditConfirmed(Appointment updatedAppointment)
        {
            var userId = SessionHelper.GetInt(HttpContext.Session, "UserId");

            // Verifica se l'utente è loggato
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Utente non loggato." });
            }

            // Verifica che l'ora dell'appuntamento sia valida
            if (updatedAppointment.AppointmentTime == null || updatedAppointment.AppointmentTime <= DateTime.Now)
            {
                return Json(new { success = false, message = "Fornisci una data e un'ora valide per il futuro." });
            }

            // Recupera l'appuntamento da modificare
            var appointment = _context.Appointments
                .FirstOrDefault(a => a.AppointmentId == updatedAppointment.AppointmentId && a.UserId == userId.Value);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Appuntamento non trovato." });
            }

            try
            {
                // Modifica i campi dell'appuntamento
                appointment.AppointmentTime = updatedAppointment.AppointmentTime;
                appointment.Description = updatedAppointment.Description;
                appointment.Details = updatedAppointment.Details;

                _context.SaveChanges();
                return Json(new { success = true, message = "Appuntamento aggiornato correttamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Errore durante la modifica dell'appuntamento." });
            }
        }

        // Metodo per eliminare un appuntamento
        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            var userId = SessionHelper.GetInt(HttpContext.Session, "UserId");

            // Verifica se l'utente è loggato
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Utente non loggato." });
            }

            // Recupera l'appuntamento da eliminare
            var appointment = _context.Appointments
                .FirstOrDefault(a => a.AppointmentId == id && a.UserId == userId.Value);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Appuntamento non trovato." });
            }

            try
            {
                // Elimina l'appuntamento
                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
                return Json(new { success = true, message = "Appuntamento eliminato correttamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Errore durante l'eliminazione dell'appuntamento." });
            }
        }
    }
}
