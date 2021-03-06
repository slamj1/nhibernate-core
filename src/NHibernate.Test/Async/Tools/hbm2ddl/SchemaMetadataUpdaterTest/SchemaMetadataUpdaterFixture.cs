﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Util;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class SchemaMetadataUpdaterFixtureAsync
	{
		[Test]
		public async Task CanRetrieveReservedWordsAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync(CancellationToken.None));
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				var reserved = metaData.GetReservedWords();
				Assert.That(reserved, Is.Not.Empty);
				Assert.That(reserved, Has.Member("SELECT").IgnoreCase);
				Assert.That(reserved, Has.Member("FROM").IgnoreCase);
			}
			finally
			{
				connectionHelper.Release();
			}
		}

		[Test]
		public async Task UpdateReservedWordsInDialectAsync()
		{
			var reservedDb = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync(CancellationToken.None));
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				foreach (var rw in metaData.GetReservedWords())
				{
					reservedDb.Add(rw.ToLowerInvariant());
				}
			}
			finally
			{
				connectionHelper.Release();
			}

			var sf = (ISessionFactoryImplementor) configuration.BuildSessionFactory();
			await (SchemaMetadataUpdater.UpdateAsync(sf, CancellationToken.None));
			var match = reservedDb.Intersect(sf.Dialect.Keywords, StringComparer.OrdinalIgnoreCase);

			// tests that nothing in the first metaData.GetReservedWords() is left out of the second metaData.GetReservedWords() call.
			// i.e. always passes.
			Assert.That(match, Is.EquivalentTo(reservedDb));
		}

		[Test]
		public async Task EnsureReservedWordsHardCodedInDialectAsync()
		{
			var reservedDb = new HashSet<string>();
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync(CancellationToken.None));
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				foreach (var rw in metaData.GetReservedWords())
				{
					if (rw.Contains(" ")) continue;
					reservedDb.Add(rw.ToLowerInvariant());
				}
			}
			finally
			{
				connectionHelper.Release();
			}

			var sf = (ISessionFactoryImplementor)configuration.BuildSessionFactory();

			// use the dialect as configured, with no update
			var match = reservedDb.Intersect(sf.Dialect.Keywords).ToList();

			// tests that nothing in metaData.GetReservedWords() is left out of the Dialect.Keywords (without a refresh).
			var differences = reservedDb.Except(match).ToList();
			if (differences.Count > 0)
			{
				Console.WriteLine("Update Dialect {0} with RegisterKeyword:", sf.Dialect.GetType().Name);
				foreach (var keyword in differences.OrderBy(x => x))
				{
					Console.WriteLine("  RegisterKeyword(\"{0}\");", keyword);
				}
			}

			if (sf.ConnectionProvider.Driver is OdbcDriver)
			{
				Assert.Inconclusive("ODBC has excess keywords reserved");
			}

			Assert.That(match, Is.EquivalentTo(reservedDb));
		}

		[Test, Explicit]
		public async Task CheckForExcessReservedWordsHardCodedInDialectAsync()
		{
			var reservedDb = new HashSet<string>();
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync(CancellationToken.None));
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				foreach (var rw in metaData.GetReservedWords())
				{
					reservedDb.Add(rw.ToLowerInvariant());
				}
			}
			finally
			{
				connectionHelper.Release();
			}

			var sf = (ISessionFactoryImplementor)configuration.BuildSessionFactory();

			// use the dialect as configured, with no update
			// tests that nothing in Dialect.Keyword is not in metaData.GetReservedWords()
			var differences = sf.Dialect.Keywords.Except(reservedDb).Except(AnsiSqlKeywords.Sql2003).ToList();
			if (differences.Count > 0)
			{
				Console.WriteLine("Excess RegisterKeyword in Dialect {0}:", sf.Dialect.GetType().Name);
				foreach (var keyword in differences.OrderBy(x => x))
				{
					Console.WriteLine("  RegisterKeyword(\"{0}\");", keyword);
				}
			}

			// Don't fail incase the driver returns nothing.
			// This is an info-only test.
		}

		[Test]
		public async Task ExplicitAutoQuoteAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml",
																GetType().Assembly);

			var dialect = Dialect.Dialect.GetDialect(configuration.GetDerivedProperties());
			dialect.Keywords.Add("Abracadabra");

			await (SchemaMetadataUpdater.UpdateAsync(configuration, dialect, CancellationToken.None));
			SchemaMetadataUpdater.QuoteTableAndColumns(configuration, dialect);

			var cm = configuration.GetClassMapping(typeof(Order));
			Assert.That(cm.Table.IsQuoted);
			var culs = new List<Column>(cm.Table.ColumnIterator);
			Assert.That(GetColumnByName(culs, "From").IsQuoted);
			Assert.That(GetColumnByName(culs, "And").IsQuoted);
			Assert.That(GetColumnByName(culs, "Select").IsQuoted);
			Assert.That(GetColumnByName(culs, "Abracadabra").IsQuoted);
			Assert.That(!GetColumnByName(culs, "Name").IsQuoted);
		}

		[Test]
		public async Task AutoQuoteTableAndColumnsAtStratupIncludeKeyWordsImportAsync()
		{
			var reservedDb = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var dialect = Dialect.Dialect.GetDialect(configuration.Properties);
			var connectionHelper = new ManagedProviderConnectionHelper(configuration.Properties);
			await (connectionHelper.PrepareAsync(CancellationToken.None));
			try
			{
				var metaData = dialect.GetDataBaseSchema(connectionHelper.Connection);
				foreach (var rw in metaData.GetReservedWords())
				{
					reservedDb.Add(rw.ToLowerInvariant());
				}
			}
			finally
			{
				connectionHelper.Release();
			}

			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml",
																GetType().Assembly);
			var sf = (ISessionFactoryImplementor)configuration.BuildSessionFactory();
			var match = reservedDb.Intersect(sf.Dialect.Keywords, StringComparer.OrdinalIgnoreCase);
			Assert.That(match, Is.EquivalentTo(reservedDb).IgnoreCase);
		}

		private static Column GetColumnByName(IEnumerable<Column> columns, string colName)
		{
			return columns.FirstOrDefault(column => column.Name.Equals(colName));
		}

		[Test]
		public async Task CanWorkWithAutoQuoteTableAndColumnsAtStratupAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
			configuration.SetProperty(Environment.Hbm2ddlAuto, "create-drop");
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml",
																GetType().Assembly);
			var sf = configuration.BuildSessionFactory();
			using (ISession s = sf.OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.SaveAsync(new Order {From = "from", Column = "column", And = "order"}));
				await (t.CommitAsync());
			}

			using (ISession s = sf.OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.DeleteAsync("from Order"));
				await (t.CommitAsync());
			}

			TestCase.DropSchema(false, new SchemaExport(configuration), (ISessionFactoryImplementor)sf);
		}

		[Test]
		public async Task WhenConfiguredOnlyExplicitAutoQuoteAsync()
		{
			var configuration = TestConfigurationHelper.GetDefaultConfiguration();
			var driverClass = ReflectHelper.ClassForName(configuration.GetProperty(Environment.ConnectionDriver));
			// Test uses the default dialect driver, which will not accept Odbc or OleDb connection strings.
			if (typeof(OdbcDriver).IsAssignableFrom(driverClass) || typeof(OleDbDriver).IsAssignableFrom(driverClass))
				Assert.Ignore("Test is not compatible with OleDb or ODBC driver connection strings");

			var configuredDialect = Dialect.Dialect.GetDialect();
			if(!configuredDialect.DefaultProperties.ContainsKey(Environment.ConnectionDriver))
			{
				Assert.Ignore(GetType() + " does not apply to " + configuredDialect);
			}
			configuration.Properties.Remove(Environment.ConnectionDriver);
			configuration.AddResource("NHibernate.Test.Tools.hbm2ddl.SchemaMetadataUpdaterTest.HeavyEntity.hbm.xml",
																GetType().Assembly);

			var dialect = Dialect.Dialect.GetDialect(configuration.GetDerivedProperties());
			dialect.Keywords.Add("Abracadabra");

			await (SchemaMetadataUpdater.UpdateAsync(configuration, dialect, CancellationToken.None));
			SchemaMetadataUpdater.QuoteTableAndColumns(configuration, dialect);

			var cm = configuration.GetClassMapping(typeof(Order));
			Assert.That(cm.Table.IsQuoted);
			var culs = new List<Column>(cm.Table.ColumnIterator);
			Assert.That(GetColumnByName(culs, "From").IsQuoted);
			Assert.That(GetColumnByName(culs, "And").IsQuoted);
			Assert.That(GetColumnByName(culs, "Select").IsQuoted);
			Assert.That(GetColumnByName(culs, "Abracadabra").IsQuoted);
			Assert.That(!GetColumnByName(culs, "Name").IsQuoted);
		}
	}
}
