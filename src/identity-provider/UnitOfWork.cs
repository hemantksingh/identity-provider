﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace identity
{
	public interface IUnitOfWork : IDisposable
	{
		IUnitOfWork Begin();
		void Commit(Action<SqlConnection, SqlTransaction> action);
		void Commit(IList<Action<SqlConnection, SqlTransaction>> actions);
		void Rollback();
	}

	public class UnitOfWork : IUnitOfWork
	{
		private readonly SqlConnection _connection;
		private SqlTransaction _transaction;

		public UnitOfWork(SqlConnection connection)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			if (_connection.State != ConnectionState.Open)
				_connection.Open();
		}

		public IUnitOfWork Begin()
		{
			_transaction = _connection.BeginTransaction();
			return this;
		}

		public void Commit(Action<SqlConnection, SqlTransaction> action)
		{
			if (_transaction == null)
				throw new InvalidOperationException(
					$"{nameof(UnitOfWork)} needs to {nameof(Begin).ToLower()} before {nameof(Commit).ToLower()}");

			action(_connection, _transaction);
			Commit(_transaction);
		}

		public void Commit(IList<Action<SqlConnection, SqlTransaction>> actions)
		{
			if (_transaction == null)
				throw new InvalidOperationException(
					$"{nameof(UnitOfWork)} needs to {nameof(Begin).ToLower()} before {nameof(Commit).ToLower()}");

			foreach (var action in actions)
				action(_connection, _transaction);
			Commit(_transaction);
		}

		private void Commit(SqlTransaction transaction)
		{
			try
			{
				transaction.Commit();
			}
			catch (Exception exception)
			{
				try
				{
					Rollback();
				}
				catch (Exception innerException)
				{
					throw new Exception($"Failed to {nameof(Rollback).ToLower()} transaction", innerException);
				}
				throw new Exception($"Failed to {nameof(Commit).ToLower()} transaction", exception);
			}
		}

		public void Rollback()
		{
			_transaction?.Rollback();
		}

		/// <summary>
		/// Disposing the unit of work closes the sql connection, making the connection available to the connection pool
		/// for it to be used again.
		/// </summary>
		public void Dispose()
		{
			_transaction?.Dispose();
			if (_connection.State != ConnectionState.Closed)
				_connection.Close();
			_connection.Dispose();
		}
	}
}