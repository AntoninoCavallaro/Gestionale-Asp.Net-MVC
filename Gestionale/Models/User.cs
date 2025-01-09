namespace ClickClok.Models
{
    public class User
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public bool IsPaid { get; set; }

        // Aggiungi questa proprietà per navigare attraverso gli appuntamenti associati
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>(); // Assicurati che sia una collezione
    }
}
