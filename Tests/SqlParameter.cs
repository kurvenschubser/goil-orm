using System;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data;

using NUnit.Framework;


[TestFixture]
public class SqlParameterTest
{
	[SetUp]
	public void Init()
	{

	}

	[TearDown]
	public void Dispose()
	{

	}

	[TestCase]
	public void DbTypesAreAutoSet()
	{
		SqlParameter p = new SqlParameter("foo", new DateTime());
		Assert.AreEqual(p.DbType, DbType.DateTime);
		p.Value = 1.0;
		Assert.AreEqual(p.DbType, DbType.Double);
	}
}