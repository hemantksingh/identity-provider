using System;
using System.Data;
using System.Data.SqlClient;

namespace identity_provider.Quickstart
{
	public interface IUnitOfWork : IDisposable
	{
		IUnitOfWork Begin();
		void Commit(Action<SqlConnection, SqlTransaction> doWork);
		void Rollback();
	}

	public class UnitOfWork : IUnitOfWork
	{
		private readonly SqlConnection _connection;
		private SqlTransaction _transaction;

		public UnitOfWork(SqlConnection connection)
		{
			_connection = connection ?? throw new ArgumentNullException(nameof(connection), "cannot be null");
			if (_connection.State != ConnectionState.Open)
				_connection.Open();
		}

		public IUnitOfWork Begin()
		{
			_transaction = _connection.BeginTransaction();
			return this;
		}

		public void Commit(Action<SqlConnection, SqlTransaction> doWork)
		{
			if (_transaction == null)
				throw new InvalidOperationException($"{nameof(UnitOfWork)} needs to {nameof(Begin)} before {nameof(Commit)}");
			doWork(_connection, _transaction);
			_transaction.Commit();
		}

		public void Rollback()
		{
			_transaction?.Rollback();
		}

		public void Dispose()
		{
			_transaction?.Dispose();
			if (_connection.State != ConnectionState.Closed)
				_connection.Close();
			_connection.Dispose();
		}
	}
}