using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JCAM_CONNECT.Models
{
    public class Devotional
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string? BibleVerse { get; set; }

        [ForeignKey("Author")]
        public int? AuthorId { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
        public bool IsPublished { get; set; } = true;

        public virtual User? Author { get; set; }
    }
}