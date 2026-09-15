using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Rules;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Persistence.Constants;

namespace ForeverPin.Persistence.Configurations;

/// <summary>Configures the codes table mapping.</summary>
public class CodeEntityConfiguration : IEntityTypeConfiguration<CodeEntity>
{
    public void Configure(EntityTypeBuilder<CodeEntity> builder)
    {
        builder.ToTable(CodeEntity.TableName);

        builder.HasKey(e => e.Id);

        // Null slugs remain distinct, so static codes can share this unique index.
        builder
            .HasIndex(e => e.Slug)
            .IsUnique();

        builder
            .Property(e => e.StyleJson)
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .IsRequired();

        // Persist the polymorphic rule collection as one jsonb document.
        var rulesConverter = new ValueConverter<List<CodeRuleValueObject>, string>(
            rules => CodeRuleJson.Serialize(rules),
            json => CodeRuleJson.Deserialize(json));

        // Compare rule values and snapshot list membership for EF change tracking.
        var rulesComparer = new ValueComparer<List<CodeRuleValueObject>>(
            (left, right) => left!.SequenceEqual(right!),
            rules => rules.Aggregate(0, (hash, rule) => HashCode.Combine(hash, rule.GetHashCode())),
            rules => rules.ToList());

        builder
            .Property(e => e.Rules)
            .HasColumnName("rules")
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .HasConversion(rulesConverter, rulesComparer)
            .IsRequired();
    }
}
