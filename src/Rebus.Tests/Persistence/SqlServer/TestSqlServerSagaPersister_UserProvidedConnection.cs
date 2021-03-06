﻿using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Rebus.Persistence.SqlServer;
using Shouldly;

namespace Rebus.Tests.Persistence.SqlServer
{
    [TestFixture, Category(TestCategories.MsSql)]
    public class TestSqlServerSagaPersister_UserProvidedConnection : SqlServerFixtureBase
    {
        SqlServerSagaPersister persister;
        const string SagaTableName = "testSagaTable";
        const string SagaIndexTableName = "testSagaIndexTable";

        SqlConnection currentConnection;
        SqlTransaction currentTransaction;

        protected override void DoSetUp()
        {
            // ensure the two tables are dropped
            try { ExecuteCommand("drop table " + SagaTableName); }
            catch { }
            try { ExecuteCommand("drop table " + SagaIndexTableName); }
            catch { }

            persister = new SqlServerSagaPersister(GetOrCreateConnection, SagaIndexTableName, SagaTableName);
            persister.EnsureTablesAreCreated();
        }

        protected override void DoTearDown()
        {
            if (currentConnection == null) return;

            currentConnection.Dispose();
            currentConnection = null;
        }

        SqlConnection GetOrCreateConnection()
        {
            if (currentConnection != null) return currentConnection;

            var newConnection = new SqlConnection(ConnectionStrings.SqlServer);
            newConnection.Open();
            currentConnection = newConnection;
            return newConnection;
        }

        void BeginTransaction()
        {
            if (currentTransaction != null)
            {
                throw new InvalidOperationException("Cannot begin new transaction when a transaction has already been started!");
            }
            currentTransaction = GetOrCreateConnection().BeginTransaction();
        }

        void CommitTransaction()
        {
            if (currentTransaction == null)
            {
                throw new InvalidOperationException("Cannot commit transaction when no transaction has been started!");
            }
            currentTransaction.Commit();
            currentTransaction = null;
        }

        [Test]
        public void WorksWithUserProvidedConnectionWithStartedTransaction()
        {
            // arrange
            var sagaId = Guid.NewGuid();
            var sagaData = new SomeSagaData { JustSomething = "hey!", Id = sagaId };

            // act
            BeginTransaction();

            // assert
            persister.Insert(sagaData, new string[0]);

            CommitTransaction();
        }

        [Test]
        public void WorksWithUserProvidedConnectionWithoutStartedTransaction()
        {
            // arrange
            var sagaId = Guid.NewGuid();
            var sagaData = new SomeSagaData { JustSomething = "hey!", Id = sagaId };

            // act

            // assert
            persister.Insert(sagaData, new string[0]);

        }

        class SomeSagaData : ISagaData
        {
            public Guid Id { get; set; }
            public int Revision { get; set; }
            public string JustSomething { get; set; }
        }

        [Test]
        public void CanCreateSagaTablesAutomatically()
        {
            // arrange

            // act
            persister.EnsureTablesAreCreated();

            // assert
            var existingTables = GetTableNames();
            existingTables.ShouldContain(SagaIndexTableName);
            existingTables.ShouldContain(SagaTableName);
        }
    }
}