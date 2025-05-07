using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quivi.Domain.Entities;

namespace Quivi.Domain.Repositories.EntityFramework.Extensions
{
    public static class EntityTypeBuilderExtensions
    {
        public static void HasDeletedIndex<T>(this EntityTypeBuilder<T> entityTypeBuilder) where T : class, IDeletableEntity
        {
            entityTypeBuilder.HasIndex(e => e.DeletedDate)
                            .HasDatabaseName($"IX_{nameof(T)}_{nameof(IDeletableEntity.DeletedDate)}_NotDeleted")
                            .HasFilter($"[{nameof(IDeletableEntity.DeletedDate)}] IS NULL");
        }
    }
}