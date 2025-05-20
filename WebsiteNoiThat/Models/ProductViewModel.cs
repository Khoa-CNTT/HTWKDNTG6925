using Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebsiteNoiThat.Models
{
    public class ProductViewModel
    {
        [Key]
        public int ProductId { get; set; }

        [DisplayName("Tên sản phẩm")]
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên sản phẩm không được quá 50 ký tự.")]
        [RegularExpression(@"^[\p{L}0-9\s]+$", ErrorMessage = "Tên sản phẩm không được chứa các ký tự đặc biệt.")]
        public string Name { get; set; }

        [DisplayName("Mô tả sản phẩm")]
        [Required(ErrorMessage = "Nhập mô tả")]
        [StringLength(10000, ErrorMessage = "Mô tả không quá ký tự.")]
        public string Description { get; set; }

        [DisplayName("Đơn giá")]
        [Required(ErrorMessage = "Giá là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Giá không được âm.")]
        public int? Price { get; set; }

        [DisplayName("Số lượng")]
        [Required(ErrorMessage = "Số lượng là bắt buộc.")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int? Quantity { get; set; }

        [DisplayName("Mã nhà cung cấp")]
        public int? ProviderId { get; set; }

        [DisplayName("Mã danh mục sản phẩm")]
        public int? CateId { get; set; }

        [DisplayName("Danh mục sản phẩm")]
        public string CateName { get; set; }

        [DisplayName("Nhà cung cấp")]
        public string ProviderName { get; set; }

        [Required(ErrorMessage = "Hãy chọn ảnh cho sản phẩm.")]
        [DisplayName("Ảnh")]
        public string Photo { get; set; }

        [DisplayName("Ngày bắt đầu KM")]
        [Column(TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("Ngày kết thúc KM")]
        [Column(TypeName = "date")]
        [DateGreaterThan("StartDate", ErrorMessage = "Ngày kết thúc phải muộn hơn ngày bắt đầu")]
        public DateTime? EndDate { get; set; }


        [DisplayName("Giảm giá (%)")]
        [Required(ErrorMessage = "Vui lòng nhập giảm giá")]
        [Range(0, 100, ErrorMessage = "Giảm giá phải trong khoảng từ 0 đến 100.")]
        public int? Discount { get; set; }
        public bool IsVisible { get; set; }
        [DisplayName("Chiều dài")]
        [Required(ErrorMessage = "Nhập chiều dài")]
        public decimal Length { get; set; }
        [DisplayName("Chiều rộng")]
        [Required(ErrorMessage = "Nhập chiều rộng")]
        public decimal Width { get; set; }
        [DisplayName("Chiều cao")]
        [Required(ErrorMessage = "Nhập chiều cao")]
        public decimal Height { get; set; }

    }
}
