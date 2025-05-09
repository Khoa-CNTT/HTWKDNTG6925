namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class News
    {
        // Sử dụng Identity để SQL Server tự động sinh giá trị cho NewsId
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NewsId { get; set; }

        public string Title { get; set; }

        public string Detail { get; set; }

        public string Photo { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateUpdate { get; set; }
    }
}
