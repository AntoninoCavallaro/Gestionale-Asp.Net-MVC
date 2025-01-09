using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ClickClok.Models;

var builder = WebApplication.CreateBuilder(args);

// Aggiungi il logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders(); // Rimuove i log predefiniti
    logging.AddConsole(); // Aggiungi il logger per la console
    logging.AddDebug(); // Aggiungi il logger per la finestra di debug
});

// Configura SQLite come database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=appointments.db"));

// Configura sessioni
builder.Services.AddDistributedMemoryCache(); // Aggiungi la cache distribuita
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Timeout di sessione
    options.Cookie.HttpOnly = true; // Il cookie di sessione sarà visibile solo al server, non a JavaScript
    options.Cookie.IsEssential = true; // Il cookie è essenziale per il funzionamento del sito
});


// Registra i servizi per i controller con viste
builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseSession();
app.UseStaticFiles();


// Crea e popola il database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate(); // Esegui la migrazione per assicurarti che il DB sia aggiornato.

    // Aggiungi gli utenti se non esistono
    if (!context.Users.Any())
    {
        var users = new List<User>
        {
            new User { Username = "mario", Password = "mario1", IsPaid = true },
            new User { Username = "lucia", Password = "lucia2", IsPaid = false },
            new User { Username = "marco", Password = "marco3", IsPaid = true }
        };
        context.Users.AddRange(users);
        context.SaveChanges();
    }

    // Aggiungi gli appuntamenti se non esistono
    if (!context.Appointments.Any())
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var appointments = new List<Appointment>
{
// User 1 - 10 appointments
    new Appointment { AppointmentTime = today.AddHours(9), Description = "Meeting with team", Details = "Discuss project updates", UserId = 1 },
    new Appointment { AppointmentTime = today.AddHours(10).AddMinutes(30), Description = "Doctor appointment", Details = "Routine check-up", UserId = 1 },
    new Appointment { AppointmentTime = today.AddHours(13), Description = "Lunch with Sarah", Details = "At central park cafe", UserId = 1 },
    new Appointment { AppointmentTime = today.AddHours(15), Description = "Call with supplier", Details = "Discuss shipment details", UserId = 1 },
    new Appointment { AppointmentTime = today.AddHours(17), Description = "Gym session", Details = "Leg day workout", UserId = 1 },
    new Appointment { AppointmentTime = tomorrow.AddHours(8), Description = "Morning jog", Details = "At riverside park", UserId = 1 },
    new Appointment { AppointmentTime = tomorrow.AddHours(10), Description = "Client meeting", Details = "Presentation on new features", UserId = 1 },
    new Appointment { AppointmentTime = tomorrow.AddHours(12).AddMinutes(30), Description = "Lunch with team", Details = "Discuss team-building event", UserId = 1 },
    new Appointment { AppointmentTime = tomorrow.AddHours(14), Description = "Workshop", Details = "Agile methodology", UserId = 1 },
    new Appointment { AppointmentTime = tomorrow.AddHours(16).AddMinutes(30), Description = "Evening yoga", Details = "Relaxation session", UserId = 1 },

    // User 2 - 10 appointments
    new Appointment { AppointmentTime = today.AddHours(9), Description = "Project review", Details = "Evaluate project progress", UserId = 2 },
    new Appointment { AppointmentTime = today.AddHours(11), Description = "Doctor appointment", Details = "Routine check-up", UserId = 2 },
    new Appointment { AppointmentTime = today.AddHours(13).AddMinutes(15), Description = "Lunch with partner", Details = "At downtown restaurant", UserId = 2 },
    new Appointment { AppointmentTime = today.AddHours(15).AddMinutes(30), Description = "Supplier call", Details = "Follow-up on delivery", UserId = 2 },
    new Appointment { AppointmentTime = today.AddHours(17).AddMinutes(45), Description = "Gym session", Details = "Chest and arms workout", UserId = 2 },
    new Appointment { AppointmentTime = tomorrow.AddHours(7), Description = "Morning run", Details = "Around the neighborhood", UserId = 2 },
    new Appointment { AppointmentTime = tomorrow.AddHours(9), Description = "Client meeting", Details = "Review new proposal", UserId = 2 },
    new Appointment { AppointmentTime = tomorrow.AddHours(11).AddMinutes(30), Description = "Lunch with CEO", Details = "Business discussion", UserId = 2 },
    new Appointment { AppointmentTime = tomorrow.AddHours(13), Description = "Team meeting", Details = "Project timeline review", UserId = 2 },
    new Appointment { AppointmentTime = tomorrow.AddHours(15).AddMinutes(45), Description = "Evening relaxation", Details = "Spa visit", UserId = 2 },

    // User 3 - 10 appointments
    new Appointment { AppointmentTime = today.AddHours(8), Description = "Morning yoga", Details = "Stretch and relax", UserId = 3 },
    new Appointment { AppointmentTime = today.AddHours(10).AddMinutes(15), Description = "Doctor appointment", Details = "Eye check-up", UserId = 3 },
    new Appointment { AppointmentTime = today.AddHours(12).AddMinutes(30), Description = "Lunch with colleagues", Details = "At the office", UserId = 3 },
    new Appointment { AppointmentTime = today.AddHours(14).AddMinutes(45), Description = "Team brainstorming session", Details = "Innovative project ideas", UserId = 3 },
    new Appointment { AppointmentTime = today.AddHours(16), Description = "Gym session", Details = "Leg day workout", UserId = 3 },
    new Appointment { AppointmentTime = tomorrow.AddHours(7), Description = "Morning jog", Details = "At the park", UserId = 3 },
    new Appointment { AppointmentTime = tomorrow.AddHours(9), Description = "Client call", Details = "Discuss project updates", UserId = 3 },
    new Appointment { AppointmentTime = tomorrow.AddHours(11).AddMinutes(15), Description = "Lunch with mentor", Details = "Career advice", UserId = 3 },
    new Appointment { AppointmentTime = tomorrow.AddHours(13).AddMinutes(30), Description = "Workshop", Details = "Data analysis techniques", UserId = 3 },
    new Appointment { AppointmentTime = tomorrow.AddHours(15).AddMinutes(45), Description = "Evening walk", Details = "Relaxing stroll", UserId = 3 }
};


        context.Appointments.AddRange(appointments);
        context.SaveChanges();
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
