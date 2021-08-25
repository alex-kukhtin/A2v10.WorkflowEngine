// Copyright © 2020-2021 Alex Kukhtin. All rights reserved.

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using A2v10.Workflow.Serialization;

namespace A2v10.Workflow.Tests.Serialization
{
	[TestClass]
	[TestCategory("Converters.Test")]
	public class TestConverters
	{
		private readonly JsonSerializerSettings settings = new()
		{
			Converters = new JsonConverter[]
			{
				new DoubleConverter()
			}
		};

		public class Test
		{
			public Double x;
			public Double y;
			public Double n;
		}


		[TestMethod]
		public void TestDoubleConverter()
		{
			var s = new { x = 5, y = 7.12, n = Double.NaN };
			var x = JsonConvert.SerializeObject(s, settings);
			Console.WriteLine(x);
			var z = JsonConvert.DeserializeObject<Test>(x);
			Assert.AreEqual("{\"x\":5,\"y\":7.12,\"n\":\"NaN\"}", x);
			Assert.AreEqual(s.x, z.x);
			Assert.AreEqual(s.y, z.y);
			Assert.AreEqual(s.n, z.n);
			Assert.AreEqual(z.n, Double.NaN);
		}
	}
}
