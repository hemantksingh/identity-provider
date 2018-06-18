using FluentMigrator;

namespace identity_provider_sql_migrations
{
	[Migration(20180127001104)]
	public class AddUserClaims: Migration
    {
	    public override void Up()
	    {
		    Alter.Table("UserClaims").AlterColumn("Type").AsString(100);
		    Alter.Table("UserClaims").AlterColumn("Value").AsString(255);
		    Alter.Table("Users").AlterColumn("Username").AsString(100).NotNullable().Unique();
		}

	    public override void Down()
	    {
		    Alter.Table("UserClaims").AlterColumn("Type").AsString(250);
			Alter.Table("UserClaims").AlterColumn("Value").AsString(250);
		    Alter.Table("Users").AlterColumn("Username").AsString(100).NotNullable();
	    }
    }
}
