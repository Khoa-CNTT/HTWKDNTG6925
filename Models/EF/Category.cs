using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
    [Table("Category")]
    public partial class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required(ErrorMessage = "Mã loại sản phẩm không được để trống")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên phải có độ dài từ 3 đến 50 ký tự")]
        [RegularExpression(@"^[A-Za-z0-9\s\p{L}]+$", ErrorMessage = "Tên chỉ cho phép ký tự chữ, số, khoảng trắng")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tiêu đề phải có độ dài từ 3 đến 50 ký tự")]
        [RegularExpression(@"^[A-Za-z0-9-]+$", ErrorMessage = "Tiêu đề chỉ cho phép ký tự chữ, số và dấu gạch ngang")]
        public string MetaTitle { get; set; }

        public int ParId { get; set; } // Không nullable, mặc định là 0 nếu không chọn
    }
}
