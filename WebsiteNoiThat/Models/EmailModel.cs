using System.ComponentModel.DataAnnotations;

namespace WebsiteNoiThat.Models
{
    public class EmailModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email của bạn.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email của bạn")]
        public string SenderEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [Display(Name = "Tiêu đề")]
        public string EmailSubject { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [Display(Name = "Nội dung")]
        [DataType(DataType.MultilineText)]
        public string EMailBody { get; set; }
    }
}
