namespace ClickClok.Models
{
    public class Session
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public string SessionToken { get; set; } = Guid.NewGuid().ToString();

        public virtual User User { get; set; }
    }

}
