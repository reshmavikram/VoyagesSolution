using System.ComponentModel.DataAnnotations;

namespace Data.Solution.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        public string username { get; set; }

    }
    public class VerificationCodeViewModel
    {
        public string username { get; set; }
        public string email { get; set; }
        public string code { get; set; }

    }
}
