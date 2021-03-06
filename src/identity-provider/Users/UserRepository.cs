﻿using System;
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
		private readonly Func<SqlConnection> _createConnection;
		private readonly Func<IUnitOfWork> _createUnitOfWork;

		public UserRepository(Func<SqlConnection> createConnection, Func<IUnitOfWork> createUnitOfWork)
		{
			_createConnection = createConnection;
			_createUnitOfWork = createUnitOfWork;
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
			return GetUsers(queryFilter).FirstOrDefault();
		}

		public User GetBySubjectId(string subjectId)
		{
			var queryParam = new {subjectId};
			var queryFilter = new QueryFilter(queryParam, "WHERE Users.SubjectId = @SubjectId");
			return GetUsers(queryFilter).FirstOrDefault();
		}

		private IEnumerable<User> GetUsers(QueryFilter queryFilter = null)
		{
			var usersByUsername = new Dictionary<string, User>();
			using (var connection = _createConnection())
			{
				const string sql = "SELECT * FROM Users " +
				                   "LEFT JOIN UserClaims ON Users.SubjectId = UserClaims.SubjectId " +
				                   "LEFT JOIN IdentityProviders ON Users.SubjectId = IdentityProviders.SubjectId";

				connection.Query<User, UserClaim, IdentityProvider, User>(queryFilter != null ? $"{sql}  {queryFilter.Clause}" : sql,
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
			using (var connection = _createConnection())
			{
				return connection.Query<UserClaim>("SELECT * FROM UserClaims WHERE SubjectId = @SubjectId",
						new {subjectId})
					.Select(userClaim => new Claim(userClaim.Type, userClaim.Value));
			}
		}

		public void AddUser(User user)
		{
			var actions = new List<Action<SqlConnection, SqlTransaction>>
			{
				(connection, transaction) => connection.Execute(
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
					transaction)
			};

			foreach (var claim in user.Claims)
			{
				actions.Add((connection, transaction) => connection.Execute(
					"INSERT INTO UserClaims VALUES(@Id, @SubjectId, @Type, @Value)",
					new
					{
						Id = Guid.NewGuid().ToString(),
						user.SubjectId,
						claim.Type,
						claim.Value
					},
					transaction));
			}

			foreach (var provider in user.IdentityProviders)
			{
				actions.Add((connection, transaction) => connection.Execute(
					"INSERT INTO IdentityProviders VALUES(@Id, @SubjectId, @Name, @ProviderSubjectId)",
					new
					{
						Id = Guid.NewGuid().ToString(),
						user.SubjectId,
						provider.Name,
						provider.ProviderSubjectId
					},
					transaction));
			}

			using (IUnitOfWork unitOfWork = _createUnitOfWork().Begin())
			{
				unitOfWork.Commit(actions);
			}
		}

		public bool IsUserActive(string subjectId)
		{
			var user = GetBySubjectId(subjectId);

			return user != null && user.IsActive;
		}

		public void AddInitialUsers(IEnumerable<TestUser> testUsers, Tenant tenant)
		{
			if (GetAllUsers().Any()) return;
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
			return GetUsers(queryFilter).FirstOrDefault();
		}
	}
}