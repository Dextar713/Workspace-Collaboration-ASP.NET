using System.ComponentModel.DataAnnotations;

namespace Discord2.Models
{
    public class Profile
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Bio")]
        public string? Bio { get; set; }

        // Property for displaying the current avatar
        public string? CurrentAvatarPath { get; set; }

        [Display(Name = "Upload New Avatar")]
        public IFormFile? Avatar { get; set; }
    }

}
