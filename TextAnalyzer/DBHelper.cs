using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalyzer
{
    public class DBHelper
    {
        public class Word
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            [Required, Column(TypeName = "NVarChar"), MinLength(3), MaxLength(15), 
            Index(IsUnique = true)]
            public string Text { get; set; }
            [Required]
            public int Count { get; set; }
        }

        public class WordsContext : DbContext
        {
            public DbSet<Word> Words { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            }
        }


    }
}
