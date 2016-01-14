using PostSharp.Patterns.Contracts;

namespace DragonSpark.Runtime.Specifications
{
	public class WrappedSpecification<T> : SpecificationBase<T>
	{
		readonly ISpecification inner;

		public WrappedSpecification( [Required]ISpecification inner )
		{
			this.inner = inner;
		}

		protected override bool IsSatisfiedByParameter( T parameter ) => base.IsSatisfiedByParameter( parameter ) && inner.IsSatisfiedBy( parameter );
	}
}