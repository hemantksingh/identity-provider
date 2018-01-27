using FluentMigrator;

namespace identity_provider_sql_migrations
{
	[Migration(2)]
	public class AlterUsers: Migration
    {
	    public override void Up()
	    {
		    Alter.Table("UserClaims").AlterColumn("Type").AsString(100);
		    Alter.Table("UserClaims").AlterColumn("Value").AsString(255);
		    Create.UniqueConstraint("uk_Users_Username")
				.OnTable("Users").Column("Username");
		}

	    public override void Down()
	    {
		    Alter.Table("UserClaims").AlterColumn("Type").AsString(250);
			Alter.Table("UserClaims").AlterColumn("Value").AsString(250);
			Delete.UniqueConstraint("uk_Users_Username").FromTable("Users");
	    }
    }
}
