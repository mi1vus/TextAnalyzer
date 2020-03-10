using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace TextAnalyzer
{
    public class DBHelper
    {
        public class Word
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            [Key(), Required, Column(TypeName = "NVarChar"), MinLength(3), MaxLength(15)]
            public string Text { get; set; }
            [Required]
            public int Count { get; set; }
        }

        public class WordsContext : DbContext
        {
            public WordsContext(string connString) 
                : base(connString)
            {
            }
            public WordsContext()                
            {
            }

            public DbSet<Word> Words { get; set; }
        }


    }
}
