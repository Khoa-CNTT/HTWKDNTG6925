namespace Models.EF
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Provider")]
    public partial class Provider
    {
        public int ProviderId { get; set; }

        [Required(ErrorMessage = "Ten nha cung cap khong duoc de trong")]
        [StringLength(50, ErrorMessage = "Ten nha cung cap khong duoc vuot qua 50 ky tu")]
        [RegularExpression(@"^[A-Za-z0-9\s\p{L}]+$", ErrorMessage = "Ten chi cho phep ky tu chu, so, khoang trang")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Dia chi khong duoc de trong")]
        [StringLength(100, ErrorMessage = "Dia chi khong duoc vuot qua 100 ky tu")]
        public string Address { get; set; }

        [Required(ErrorMessage = "So dien thoai khong duoc de trong")]
        [StringLength(10, ErrorMessage = "So dien thoai phai co 10 ky tu")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "So dien thoai khong hop le. Vui long nhap 10 so.")]
        [Phone(ErrorMessage = "So dien thoai khong hop le.")]
        public string Phone { get; set; }
    }
}
