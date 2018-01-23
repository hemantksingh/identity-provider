using System;
using FluentMigrator;

namespace identity_provider_sql_migrations
{
	[Migration(1)]
	public class CreateUsers : Migration
    {
	    public override void Up()
	    {
		    Create.Table("Users")
			    .WithColumn("SubjectId").AsString(64).NotNullable().PrimaryKey()
			    .WithColumn("Username").AsString(100).NotNullable()
			    .WithColumn("Password").AsString(100)
			    .WithColumn("IsActive").AsBoolean().NotNullable();

		    Create.Table("UserClaims")
			    .WithColumn("Id").AsString(64).NotNullable().PrimaryKey()
			    .WithColumn("SubjectId").AsString(64).NotNullable()
			    .WithColumn("Type").AsString(250).NotNullable()
			    .WithColumn("Value").AsString(250).NotNullable();

		    Create.Table("UserLogins")
			    .WithColumn("Id").AsString(64).NotNullable().PrimaryKey()
			    .WithColumn("SubjectId").AsString(64).NotNullable()
			    .WithColumn("LoginProvider").AsString(250).NotNullable()
			    .WithColumn("ProviderKey").AsString(250).NotNullable();

			Create.ForeignKey("fk_Users_UserClaims_SubjectId")
			    .FromTable("UserCLaims").ForeignColumn("SubjectId")
			    .ToTable("Users").PrimaryColumn("SubjectId");

		    Create.ForeignKey("fk_Users_UserLogins_SubjectId")
			    .FromTable("UserLogins").ForeignColumn("SubjectId")
			    .ToTable("Users").PrimaryColumn("SubjectId");
		}

	    public override void Down()
	    {
			Delete.Table("Users");
			Delete.Table("UserClaims");
			Delete.Table("UserLogins");
		}
    }
}
