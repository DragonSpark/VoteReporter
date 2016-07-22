using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DragonSpark
{
	public interface IPriorityAware
	{
		[DefaultValue( Priority.Normal )]
		Priority Priority { get; }
	}

	class PriorityAware : IPriorityAware
	{
		public static PriorityAware Default { get; } = new PriorityAware();

		public Priority Priority { get; } = Priority.Normal;
	}

	public class PriorityAwareLocator<T> : FactoryBase<T, IPriorityAware>
	{
		readonly Func<Type, IPriorityAware> get;
		public static PriorityAwareLocator<T> Instance { get; } = new PriorityAwareLocator<T>( AttributeSupport<PriorityAttribute>.Local.Get );
		PriorityAwareLocator( Func<Type, IPriorityAware> get )
		{
			this.get = get;
		}

		public override IPriorityAware Create( T parameter ) => parameter as IPriorityAware ?? get( parameter.GetType() ) ?? PriorityAware.Default;
	}

	public class PriorityComparer : IComparer<IPriorityAware>
	{
		public static PriorityComparer Instance { get; } = new PriorityComparer();

		public int Compare( IPriorityAware x, IPriorityAware y ) => Comparer<Priority>.Default.Compare( x.Priority, y.Priority );
	}

	public class PrioritizedCollection<T> : CollectionBase<T>
	{
		public PrioritizedCollection() {}
		public PrioritizedCollection( IEnumerable<T> items ) : base( items ) {}
		public PrioritizedCollection( ICollection<T> source ) : base( source ) {}

		protected override IEnumerable<T> Query => base.Query.Prioritize();
	}
		

	public class PriorityAwareCollection<T> : CollectionBase<T> where T : IPriorityAware
	{
		public PriorityAwareCollection() {}
		public PriorityAwareCollection( IEnumerable<T> items ) : base( items ) {}
		public PriorityAwareCollection( ICollection<T> source ) : base( source ) {}

		protected override IEnumerable<T> Query => base.Query.OrderBy( arg => arg, PriorityComparer.Instance );
	}
}