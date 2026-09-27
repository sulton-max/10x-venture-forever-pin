using System.Text.Json;
using ForeverPin.Domain.Codes.Rules.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Serialization.Factories;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Json;
using ForeverPin.Persistence.Constants;

namespace ForeverPin.Persistence.Configurations;

/// <summary>Configures the codes table mapping.</summary>
public class CodeEntityConfiguration : IEntityTypeConfiguration<CodeEntity>
{
    public void Configure(EntityTypeBuilder<CodeEntity> builder)
    {
        builder.ToTable(CodeEntity.TableName);

        builder.HasKey(e => e.Id);

        // A guest write cannot cross an ownership transfer after its owner-scoped read.
        builder.Property(e => e.UserId).IsConcurrencyToken();

        builder.HasIndex(e => new { e.UserId, e.CreatedAt }).HasDatabaseName("ix_codes_user_id_created_at");

        // Null slugs remain distinct, so static codes can share this unique index.
        builder
            .HasIndex(e => e.Slug)
            .IsUnique();

        builder
            .Property(e => e.StyleJson)
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .IsRequired();

        var rulesProperty = builder
            .Property(e => e.Rules)
            .HasColumnName("rules")
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .HasJsonConversion(CodeJsonOptionsFactory.CreateRules())
            .IsRequired();

        // Replace only the stored read boundary; HasConversion would clear the SDK's deep comparer.
        rulesProperty.Metadata.SetValueConverter(new ValueConverter<List<CodeRuleValueObject>, string>(
                rules => JsonSerializer.Serialize(rules, CodeJsonOptionsFactory.CreateRules()),
                json => string.IsNullOrWhiteSpace(json) ? new List<CodeRuleValueObject>()
                    : JsonSerializer.Deserialize<List<CodeRuleValueObject>>(json, CodeJsonOptionsFactory.CreateRules()) ?? new List<CodeRuleValueObject>()));
    }
}
