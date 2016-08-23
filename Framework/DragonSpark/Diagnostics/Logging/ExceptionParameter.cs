using System;

namespace DragonSpark.Diagnostics.Logging
{
	public struct ExceptionParameter<T>
	{
		public ExceptionParameter( Exception exception, T argument )
		{
			Exception = exception;
			Argument = argument;
		}

		public Exception Exception { get; }
		public T Argument { get; }
	}
}