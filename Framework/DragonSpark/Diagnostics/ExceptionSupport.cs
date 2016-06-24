using DragonSpark.Extensions;
using System;

namespace DragonSpark.Diagnostics
{
	public static class ExceptionSupport
	{
		public static void Process( this IExceptionHandler target, Exception exception ) => target.Handle( exception ).With( a => a.RethrowRecommended.IsTrue( () => { throw a.Exception; } ) );
	}
}