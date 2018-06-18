using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using Dapper;
using IdentityServer4.Test;

namespace identity
{
	public class QueryFilter
	{
		public QueryFilter(object param, string clause)
		{
			Param = param;
			Clause = clause;
		}

		public readonly object Param;
		public readonly string Clause;
	}

	public class UserRepository
	{
		private readonly string _connectionString;
		private readonly Func<SqlConnection, IUnitOfWork> _createUnitOfWork;

		/// <summary>
		/// TODO: Remove connection string, & use connection factory
		/// </summary>
		/// <param name="connectionString"></param>
		/// <param name="createUnitOfWork"></param>
		public UserRepository(string connectionString, Func<SqlConnection, IUnitOfWork> createUnitOfWork)
		{
			_connectionString = connectionString;
			_createUnitOfWork = createUnitOfWork;
		}
		/// <summary>
		/// Creates a new sql connection per query. Closing the connection after finishing processing the query
		/// does not actually close the physical connection but just returns it to the connection pool, managed
		/// by dotnet. Let dotnet handle connection pooling:
		/// https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-connection-pooling
		/// </summary>
		/// <returns></returns>
		private SqlConnection GetConnection()
		{
			var connection = new SqlConnection(_connectionString);
			connection.Open();
			return connection;
		}

		public IEnumerable<User> GetAllUsers()
		{
			return GetUsers();
		}

		public bool CredentialsValid(string username, string password)
		{
			var user = GetByUsername(username);
			return user != null && user.Password.Equals(password);
		}

		public User GetByUsername(string username)
		{
			var queryParam = new {username};
			var queryFilter = new QueryFilter(queryParam, "WHERE Users.Username = @Username");
			return Enumerable.FirstOrDefault(GetUsers(queryFilter));
		}

		public User GetBySubjectId(string subjectId)
		{
			var queryParam = new {subjectId};
			var queryFilter = new QueryFilter(queryParam, "WHERE Users.SubjectId = @SubjectId");
			return Enumerable.FirstOrDefault(GetUsers(queryFilter));
		}

		private IEnumerable<User> GetUsers(QueryFilter queryFilter = null)
		{
			var usersByUsername = new Dictionary<string, User>();
			using (var connection = GetConnection())
			{
				const string sql = "SELECT * FROM Users " +
				                   "LEFT JOIN UserClaims ON Users.SubjectId = UserClaims.SubjectId " +
				                   "LEFT JOIN IdentityProviders ON Users.SubjectId = IdentityProviders.SubjectId";

				SqlMapper.Query<User, UserClaim, IdentityProvider, User>(connection, queryFilter != null ? $"{sql}  {queryFilter.Clause}" : sql,
					(user, claim, provider) =>
					{
						if (!usersByUsername.TryGetValue(user.Username, out var u))
							usersByUsername.Add(user.Username, u = user);

						u.AddClaim(new Claim(claim.Type, claim.Value));
						u.AddProvider(provider);
						return u;
					},
					queryFilter?.Param);

				return usersByUsername.Values.ToList();
			}
		}

		public IEnumerable<Claim> GetClaims(string subjectId)
		{
			using (var connection = GetConnection())
			{
				return SqlMapper.Query<UserClaim>(connection, "SELECT * FROM UserClaims WHERE SubjectId = @SubjectId",
						new {subjectId})
					.Select(userClaim => new Claim(userClaim.Type, userClaim.Value));
			}
		}

		public void AddUser(User user)
		{
			using (IUnitOfWork unitOfWork = _createUnitOfWork(GetConnection()).Begin())
			{
				unitOfWork.Commit((connection, transaction) =>
				{
					connection.Execute(
						"INSERT INTO Users (SubjectId, TenantId, Username, Password, IsActive)"
						+ " VALUES(@SubjectId, @TenantId, @Username, @Password, @IsActive)",
						new
						{
							user.SubjectId,
							user.TenantId,
							user.Username,
							user.Password,
							user.IsActive
						},
						transaction);

					foreach (var claim in user.Claims)
					{
						connection.Execute(
							"INSERT INTO UserClaims VALUES(@Id, @SubjectId, @Type, @Value)",
							new
							{
								Id = Guid.NewGuid().ToString(),
								user.SubjectId,
								claim.Type,
								claim.Value
							},
							transaction);
					}

					foreach (var provider in user.IdentityProviders)
					{
						connection.Execute(
							"INSERT INTO IdentityProviders VALUES(@Id, @SubjectId, @Name, @ProviderSubjectId)",
							new
							{
								Id = Guid.NewGuid().ToString(),
								user.SubjectId,
								provider.Name,
								provider.ProviderSubjectId
							},
							transaction);
					}
				});
			}
		}

		public bool IsUserActive(string subjectId)
		{
			var user = GetBySubjectId(subjectId);

			return user != null && user.IsActive;
		}

		public void AddInitialUsers(IEnumerable<TestUser> testUsers, Tenant tenant)
		{
			if (Enumerable.Any(GetAllUsers())) return;
			foreach (var testUser in testUsers)
			{
				AddUser(new User
				{
					SubjectId = testUser.SubjectId,
					TenantId = tenant.Id.ToString(),
					Username = testUser.Username,
					Password = testUser.Password,
					IsActive = testUser.IsActive,
					Claims = testUser.Claims.ToList()
				});
			}
		}

		public User GetUser(string userId, string providerName)
		{
			var queryParam = new { ProviderSubjectId = userId, Name = providerName };
			var queryFilter = new QueryFilter(queryParam,
				"WHERE IdentityProviders.ProviderSubjectId = @ProviderSubjectId AND IdentityProviders.Name = @Name");
			return Enumerable.FirstOrDefault(GetUsers(queryFilter));
		}
	}
}