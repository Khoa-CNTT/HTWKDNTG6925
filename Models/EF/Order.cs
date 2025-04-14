namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Order")]
    public partial class Order
    {
        [DisplayName("Mã hoá đơn")]
        public int OrderId { get; set; }

        [StringLength(50, ErrorMessage = "Tên khách hàng không được vượt quá 50 ký tự.")]
        [DisplayName("Tên khách hàng")]
        [Required(ErrorMessage = "Tên khách hàng không được để trống.")]
        public string ShipName { get; set; }

        [DisplayName("Mã khách hàng")]
        [Required(ErrorMessage = "Mã khách hàng là bắt buộc.")]
        public int? UserId { get; set; }

        [DisplayName("SĐT Người nhận")]
        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ được chứa số")]

        public int? ShipPhone { get; set; }

        [DisplayName("Email người nhận")]
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]

        public string ShipEmail { get; set; }

        [Column(TypeName = "date")]
        [DisplayName("Ngày cập nhật")]
        [DataType(DataType.Date, ErrorMessage = "Ngày cập nhật không hợp lệ.")]
        public DateTime? UpdateDate { get; set; }

        [DisplayName("Địa chỉ nhận hàng")]
        [StringLength(100, ErrorMessage = "Địa chỉ không được vượt quá 100 ký tự.")]
        [Required(ErrorMessage = "Địa chỉ nhận hàng không được để trống.")]
        public string ShipAddress { get; set; }

        [DisplayName("Trạng thái đơn hàng")]
        [Required(ErrorMessage = "Trạng thái đơn hàng không được để trống.")]
        public int StatusId { get; set; }
    }
}
