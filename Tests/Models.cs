using System;
using System.Collections.Generic;

using NUnit.Framework;


namespace Orm.Tests
{
	[TestFixture]
	public class ModelTest
	{
		[TestCase]
		public void InitTest()
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();
			dict["brand"] = "Volkswagen";
			dict["gas"] = 50.0;
			Car herbie = new Car(dict);

			Assert.AreEqual(herbie["brand"], "Volkswagen");
			Assert.AreEqual(herbie["gas"], 50.0);
		}
	}
}