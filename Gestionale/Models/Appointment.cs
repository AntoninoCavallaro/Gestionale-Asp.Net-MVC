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

        public int UserId { get; set; }
        public virtual User? User { get; set; }

    }


}
