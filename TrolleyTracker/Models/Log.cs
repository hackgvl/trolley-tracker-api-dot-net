namespace TrolleyTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;



    public partial class Log
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Log()
        {
        }


        public int ID { get; set; }
        public DateTime Logged { get; set; }
        [StringLength(50)]
        [Required]
        public string Level { get; set; }
        [Required]
        public string Message { get; set; }
        [StringLength(250)]
        public string Username { get; set; }
        [StringLength(100)]
        public string RemoteAddress { get; set; }
        public string Callsite { get; set; }
        public string Exception { get; set; }

    }
}
