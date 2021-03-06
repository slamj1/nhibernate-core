﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1818
{
	using System.Threading.Tasks;
	[TestFixture]
	public class Fixture1818Async : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();

			using (var session = OpenSession())
			{
				session.Save(new DomainClass { Id = 1 });
				session.Flush();
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();
			using (var session = OpenSession())
			{
				session.Delete("from System.Object");
				session.Flush();
			}
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect as PostgreSQL82Dialect != null;
		}


		[Test]
		[Description("Test HQL query on a property mapped with a formula.")]
		public async Task ComputedPropertyShouldRetrieveDataCorrectlyAsync()
		{
			using (var session = OpenSession())
			{
				var obj = await (session.CreateQuery("from DomainClass dc where dc.AlwaysTrue").UniqueResultAsync<DomainClass>());
				Assert.IsNotNull(obj);
			}
		}
	}
}