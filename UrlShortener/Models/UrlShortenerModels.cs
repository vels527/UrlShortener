using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Models
{
    public class ShortenUrlModel
    {
        [Required]
        [Display(Name = "Full Url")]
        public string FullUrl { get; set; }
     }

    public class ExpandUrlModel
    {
        [Required]
        [Display(Name = "Shortened Url")]
        public string ShortUrl { get; set; }
    }

}
