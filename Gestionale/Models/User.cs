namespace ClickClok.Models
{
    public class User
    {
        public int UserId { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string Salt { get; set; } = Guid.NewGuid().ToString(); // Genera un salt univoco

        // Relazione con gli appuntamenti
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
