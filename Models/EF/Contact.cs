namespace Models.EF
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Contact")]
    public partial class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [StringLength(100)]
        public string SenderEmail { get; set; }

        [StringLength(200)]
        public string EmailSubject { get; set; }

        public string Content { get; set; }

        public bool? Status { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
