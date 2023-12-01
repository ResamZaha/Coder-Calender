using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace CoderCalender.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required, MaxLength(256)]

        public string name { get; set; }
        [Required, DataType(DataType.Password)]

        public string passwordHash { get; set; }

        public void convertPassword()
        {
            using var hash = SHA256.Create();
            var byteArray = hash.ComputeHash(Encoding.UTF8.GetBytes(passwordHash));
            passwordHash = Convert.ToHexString(byteArray).ToLower();
        }
        public UserModel(){
            name = "";
            passwordHash = "";
        }
        public UserModel(RegisterViewModel registerViewModel){
            name = registerViewModel.Username!;
            passwordHash = registerViewModel.Password!;
            convertPassword();
        }
    }
    public class RegisterViewModel
    {
        [Required, MaxLength(256)]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        public RegisterViewModel(){
            Username = "";
            Password = "";
            ConfirmPassword = "";
        }
    }
}