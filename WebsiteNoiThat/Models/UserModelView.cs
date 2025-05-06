using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteNoiThat.Models
{
    public class UserModelView
    {
        public int UserId { get; set; }

        [StringLength(30, ErrorMessage = "Tên không được quá 30 ký tự.")]
        [Required(ErrorMessage = "Tên không được để trống.")]
        public string Name { get; set; }

        [StringLength(50, ErrorMessage = "Địa chỉ không được quá 50 ký tự.")]
        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        
        public string Address { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [DisplayName("Số điện thoại")]
        public int? Phone { get; set; }

        [StringLength(25, ErrorMessage = "Tên đăng nhập không được quá 25 ký tự.")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string Username { get; set; }

        [StringLength(32, ErrorMessage = "Mật khẩu không được quá 32 ký tự.")]
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        public string Password { get; set; }

        [Display(Name = "Địa chỉ email")]
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [StringLength(50, ErrorMessage = "GroupId không được quá 50 ký tự.")]
        public string GroupId { get; set; }
        
        [Required(ErrorMessage = "Kích hoạt tài khoản không được để trống.")]
        public bool Status { get; set; }

        [Range(10000000, 99999999, ErrorMessage = "Số thẻ không hợp lệ.")]
        public int? NumberCard { get; set; }

        [Range(100000000, 999999999, ErrorMessage = "Số CCCD không hợp lệ.")]
        public int? Indentification { get; set; }
    }
}
