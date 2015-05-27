using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Repository.Test
{
    public class CarConfig : EntityTypeConfiguration<Car>
    {
        public CarConfig()
        {
            this.HasKey(r => r.ID);
            this.Property(r => r.ID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
            this.Property(r => r.CarName).HasMaxLength(255);
            // this.Property(r => r.CarId).HasColumnType("nvarchar36");
        }
    }
}
