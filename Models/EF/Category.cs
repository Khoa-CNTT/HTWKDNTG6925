using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
    [Table("Category")]
    public partial class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required(ErrorMessage = "Ma loaisp khong duoc de trong")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Ten khong duoc de trong")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Ten phai co do dai tu 3 den 50 ky tu")]
        [RegularExpression(@"^[A-Za-z0-9\s\p{L}]+$", ErrorMessage = "Ten chi cho phep ky tu chu, so, khoang trang")]
        public string Name { get; set; }

        [Required(ErrorMessage = "MetaTitle khong duoc de trong")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "MetaTitle phai co do dai tu 3 den 50 ky tu")]
        [RegularExpression(@"^[A-Za-z0-9-]+$", ErrorMessage = "MetaTitle chi cho phep ky tu chu, so va dau gach ngang")]
        public string MetaTitle { get; set; }

        // Định nghĩa ParId
        [CustomValidation(typeof(Category), nameof(ValidateParentCategoryId))]
        public int? ParId { get; set; }

        // Phương thức xác thực cho ParId
        public static ValidationResult ValidateParentCategoryId(int? parId, ValidationContext context)
        {
            // Kiểm tra nếu ParId không có giá trị, thì nó hợp lệ
            if (!parId.HasValue)
            {
                return ValidationResult.Success;
            }

            // Thực hiện kiểm tra logic khác nếu cần, ví dụ: kiểm tra ParId có hợp lệ hay không
            // Giả sử bạn có một phương thức nào đó để kiểm tra xem ParId có tồn tại trong cơ sở dữ liệu không
            var db = new DBNoiThat(); // Tạo mới DB context
            var category = db.Categories.Find(parId);
            if (category == null)
            {
                return new ValidationResult("ParId không tồn tại trong cơ sở dữ liệu.");
            }

            return ValidationResult.Success; // Trả về thành công nếu tất cả các điều kiện đều đúng
        }
    }
}
