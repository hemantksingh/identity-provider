using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using Dapper;
using IdentityServer4.Test;

namespace identity_provider.Quickstart.Users
{
	public class UserRepository
	{
		private static SqlConnection GetConnection()
		{
			var connection = new SqlConnection(
				@"Server=localhost;Database=identity;Data Source=.;Initial Catalog=identity;Integrated Security=True");
			connection.Open();
			return connection;
		}

		public IEnumerable<User> GetAllUsers()
		{
			string sql = "SELECT * FROM Users; SELECT * FROM UserClaims";

			using (var connection = GetConnection())
			{
				SqlMapper.GridReader reader = connection.QueryMultiple(sql);

				IEnumerable<User> users = reader.Read<User>();
				IEnumerable<UserClaim> claims = reader.Read<UserClaim>();

				foreach (var user in users)
					user.UserClaims.AddRange(claims.Where(claim => claim.SubjectId == user.SubjectId));

				return users;
			}
		}

		public bool CredentialsValid(string username, string password)
		{
			var user = GetByUsername(username);
			return user != null && user.Password.Equals(password);
		}

		
		public User GetByUsername(string username)
		{
			var usersByUsername = new Dictionary<string, User>();
			using (var connection = GetConnection())
			{
				connection.Query<User, UserClaim, User>(
					"SELECT * FROM Users u " +
					"INNER JOIN UserClaims uc ON u.SubjectId = uc.SubjectId " +
					"WHERE u.Username = @Username",
					(user, claim) =>
					{
						if (!usersByUsername.TryGetValue(user.Username, out var u))
							usersByUsername.Add(user.Username, u = user);

						u.UserClaims.Add(claim);
						return u;
					},
					new {username});

				return usersByUsername.Values.FirstOrDefault();
			}
		}

		public IEnumerable<Claim> GetClaims(string subjectId)
		{
			using (var connection = GetConnection())
			{
				return connection.Query<UserClaim>(
					"SELECT * FROM UserClaims WHERE SubjectId = @SubjectId", 
					new {subjectId})
					.Select(userClaim => new Claim(userClaim.Type, userClaim.Value));
			}
		}

		public void AddUser(User user)
		{
			using (var connection = GetConnection())
			{
				using (var transaction = connection.BeginTransaction())
				{
					connection.Execute(
						"INSERT INTO Users (SubjectId, Username, Password, IsActive)"
						+ " VALUES(@SubjectId, @Username, @Password, @IsActive)",
						new
						{
							user.SubjectId,
							user.Username,
							user.Password,
							user.IsActive
						},
						transaction);

					foreach (var claim in user.UserClaims)
					{
						connection.Execute(
							"INSERT INTO UserClaims VALUES(@Id, @SubjectId, @Type, @Value)",
							claim,
							transaction);
					}

					transaction.Commit();
				}
			}
		}

		public bool IsUserActive(string subjectId)
		{
			return true;
		}

		public void AddInitialUsers(IEnumerable<TestUser> testUsers)
		{
			if (GetAllUsers().Any()) return;
			foreach (var testUser in testUsers)
			{
				AddUser(new User
				{
					SubjectId = testUser.SubjectId,
					Username = testUser.Username,
					Password = testUser.Password,
					IsActive = testUser.IsActive,
					UserClaims = testUser.Claims.Select(claim => new UserClaim
					{
						Id = Guid.NewGuid().ToString(),
						SubjectId = testUser.SubjectId,
						Type = claim.Type,
						Value = claim.Value
					}).ToList() 
				});
			}
		}
	}
}