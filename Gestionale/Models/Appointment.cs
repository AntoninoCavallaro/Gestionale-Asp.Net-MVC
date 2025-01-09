using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ClickClok.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime AppointmentTime { get; set; }

        public string? Description { get; set; }
        public string? Details { get; set; }


        // Rimuovi la validazione del campo UserId
        public int UserId { get; set; }

        // La proprietà di navigazione non deve essere obbligatoria
        [BindNever]
        public virtual User? User { get; set; }
    }


}
