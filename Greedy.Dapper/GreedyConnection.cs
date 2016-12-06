﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Greedy.Dapper
{
    public interface IGreedyConnection : IDbConnection
    {
        void Rollback();

        void Commit();
    }

    public class GreedyConnection : IGreedyConnection
    {
        internal IDbConnection Connection { get; private set; }
        private IDbTransaction Transaction { get; set; }
        private int deepCount = 0;

        public GreedyConnection(IDbConnection connection)
        {
            this.Connection = connection;
        }

        public GreedyConnection(string connectionString, Func<string, IDbConnection> factory)
        {
            this.Connection = factory(connectionString);
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            Interlocked.Increment(ref deepCount);
            if (Transaction == null)
            {
                bool wasClosed = State == ConnectionState.Closed;
                if (wasClosed) Open();
                Transaction = Connection.BeginTransaction(il);
                if (wasClosed) Close();
            }
            return Transaction;
        }

        public IDbTransaction BeginTransaction()
        {
            Interlocked.Increment(ref deepCount);
            if (Transaction == null)
            {
                bool wasClosed = State == ConnectionState.Closed;
                if (wasClosed) Open();
                Transaction = Connection.BeginTransaction();
                if (wasClosed) Close();
            }
            return Transaction;
        }

        public void ChangeDatabase(string databaseName)
        {
            Connection.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// Close the connection
        /// </summary>
        public void Close()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                Connection.Close();
            }
        }

        public string ConnectionString
        {
            get
            {
                return Connection.ConnectionString;
            }
            set
            {
                Connection.ConnectionString = value;
            }
        }

        public int ConnectionTimeout
        {
            get { return Connection.ConnectionTimeout; }
        }

        public IDbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        public string Database
        {
            get { return Connection.Database; }
        }

        /// <summary>
        /// Open the connection
        /// </summary>
        public void Open()
        {
            if (Connection.State != ConnectionState.Open)
            {
                Connection.Open();
            }
        }

        public void Rollback()
        {
            Interlocked.Decrement(ref deepCount);
            if (Transaction != null && deepCount <= 0)
            {
                lock (Transaction)
                {
                    bool wasClosed = State == ConnectionState.Closed;
                    if (wasClosed) Open();
                    Transaction.Rollback();
                    Transaction = null;
                    if (wasClosed) Close();
                }
            }
        }

        public void Commit()
        {
            Interlocked.Decrement(ref deepCount);
            if (Transaction != null && deepCount <= 0)
            {
                lock (Transaction)
                {
                    bool wasClosed = State == ConnectionState.Closed;
                    if (wasClosed) Open();
                    Transaction.Commit();
                    Transaction = null;
                    if (wasClosed) Close();
                }
            }
        }

        public ConnectionState State
        {
            get { return Connection.State; }
        }

        public void Dispose()
        {
            Rollback();
            Close();
            Connection.Dispose();
        }
    }
}
