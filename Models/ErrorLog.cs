using System.ComponentModel.DataAnnotations;

namespace Api.Web.Dynamics365.Models
{
    public class ErrorLog
    {
        [Key]
        public int ErrorId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [Required]
        [StringLength(50)]
        public string Level { get; set; }
        [Required]
        public string Message { get; set; }
        public string ExceptionDetails { get; set; }
        [StringLength(255)]
        public string Source { get; set; }
        [StringLength(2048)]
        public string Url { get; set; }
        [StringLength(255)]
        public string UserId { get; set; }
        [StringLength(45)]
        public string IPAddress { get; set; }
        public string StackTrace { get; set; }
    }
}
