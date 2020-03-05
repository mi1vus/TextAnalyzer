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
            [DatabaseGenerated(DatabaseGeneratedOption.Identity),
            //Key,
            //Index("IX_IdUni", IsClustered = false, IsUnique = true)
            ]
            public int Num { get; set; }
            [Required, Column(Order = 1, TypeName = "NVarChar"), MinLength(3), MaxLength(15),
            Key(),
            //Index("IX_CountTextClust", IsClustered = true, Order = 2),
            //Index("IX_TextUni", IsClustered = false, IsUnique = true)
            ]
            public string Text { get; set; }
            [Required,
            //Key(),
            //Column(Order = 1),
            //Index("IX_CountTextClust", IsClustered = true, Order = 1)
            ]
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
