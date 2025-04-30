using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
    [Table("Card")]
    public partial class Card
    {
        [Key]
        public int CardId { get; set; }

        [Display(Name = "Số thẻ tích điểm")]
        public int? NumberCard { get; set; }

        [Display(Name = "Mã số người dùng")]
        public int? UserNumber { get; set; }

        [Display(Name = "Mã người dùng")]
        [Required(ErrorMessage = "Vui lòng nhập mã người dùng")]
        public int? UserId { get; set; }

        [Display(Name = "CCCD")]
        [Required(ErrorMessage = "Vui lòng nhập CCCD")]
        public int? Identification { get; set; }
    }
}
