using DragonSpark.Activation;

namespace DragonSpark.Runtime
{
	public abstract class Program<T> : IProgram
	{
		void IProgram.Run( object arguments )
		{
			Run( (T)arguments );
		}

		protected abstract void Run( T arguments );
	}

	public sealed class StringCoercer : Coercer<string>
	{
		public new static StringCoercer Instance { get; } = new StringCoercer();
		StringCoercer() {}

		protected override string PerformCoercion( object parameter ) => parameter.ToString();
	}
}