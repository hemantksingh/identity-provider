using System;
using FluentMigrator;

namespace identity_provider_sql_migrations
{
	[Migration(20180618011400)]
	public class AddTenants : Migration
	{
		public override void Up()
		{
			Create.Table("Tenants")
				.WithColumn("TenantId").AsString(64).NotNullable().PrimaryKey()
				.WithColumn("TenantName").AsString(64).NotNullable().Unique()
				.WithColumn("OrganizationName").AsString(100).NotNullable()
				.WithColumn("License").AsString(255).Nullable()
				.WithColumn("IsActive").AsBoolean().NotNullable();

			Alter.Table("Users").AddColumn("TenantId").AsString(64);

			Create.ForeignKey("FK_Users_Tenants_TenantId")
				.FromTable("Users").ForeignColumn("TenantId")
				.ToTable("Tenants").PrimaryColumn("TenantId");
		}

		public override void Down()
		{
			Delete.ForeignKey("FK_Users_Tenants_TenantId");
			Delete.Column("TenantId").FromTable("Users");
			Delete.Table("Tenants");
		}
	}
}