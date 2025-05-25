// Models/EF/Review.cs
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
    [Table("Review")]
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }

        [Required]
        [DisplayName("Mã sản phẩm")]
        public int ProductId { get; set; }

        [Required]
        [DisplayName("Mã người dùng")]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        [DisplayName("Số sao")]
        public int Rating { get; set; }

        [StringLength(500)]
        [DisplayName("Bình luận")]
        public string Comment { get; set; }

        [DisplayName("Ngày đánh giá")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
