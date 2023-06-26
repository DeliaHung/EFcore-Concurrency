using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFcoreConcurrency
{
    public class EFstock
    {
        public int Id { get; set; }

        public string ProductId { get; set; }

        //[ConcurrencyCheck]
        public int Quantity { get; set; }

        //[Timestamp]
        public byte[] Version { get; set; }
    }

    public class EFstockConfiguration : IEntityTypeConfiguration<EFstock>
    {
        public void Configure(EntityTypeBuilder<EFstock> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseIdentityColumn(1, 1);

            //fluent API配置
            //builder.Property(t => t.Quantity).IsConcurrencyToken();
            builder.Property(x => x.Version).IsRowVersion();
        }
    }
}
