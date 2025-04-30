using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.EF
{
    [Table("User")]
    public partial class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Không được để trống")]
        [StringLength(50, ErrorMessage = "Tên không được quá 50 ký tự")]
        [DisplayName("Tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Không được để trống")]
        [StringLength(100, ErrorMessage = "Địa chỉ không được quá 100 ký tự")]
        [DisplayName("Địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Không được để trống")]
        [DisplayName("Số điện thoại")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ được chứa số")]

        public int? Phone { get; set; }

        [Required(ErrorMessage = "Không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được quá 50 ký tự")]
        [DisplayName("Tên đăng nhập")]
        public string Username { get; set; }


        [StringLength(32)]
        [DisplayName("Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        [DisplayName("Email")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "Nhóm người dùng không được quá 50 ký tự")]
        [DisplayName("Nhóm người dùng")]
        public string GroupId { get; set; }

        [DisplayName("Trạng thái tài khoản")]
        public bool Status { get; set; }
    }
}
