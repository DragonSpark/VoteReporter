﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime
{
	public class MethodFormatter : IFormattable
	{
		readonly MethodBase method;

		public MethodFormatter( MethodBase method )
		{
			this.method = method;
		}

		public string ToString( [Optional]string format, [Optional]IFormatProvider formatProvider ) => $"{method.DeclaringType.Name}.{method.Name}";
	}
}