using DragonSpark.Activation;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public class CacheValueFactory : FactoryBase<MethodInterceptionArgs, object>
	{
		readonly HashSet<int> codes = new HashSet<int>();

		readonly IDictionary<int, object> items = new Dictionary<int, object>();

		object Get( int code, MethodInterceptionArgs args )
		{
			var check = Add( code, args ) || ( args.Method as MethodInfo )?.ReturnType != typeof(void);
			var result = check ? items[code] : null;
			return result;
		}

		bool Add( int code, MethodInterceptionArgs args )
		{
			lock ( codes )
			{
				var result = !codes.Contains( code );
				if ( result )
				{
					codes.Add( code );
					items.Add( code, args.GetReturnValue() );
				}
				return result;
			}
		}

		protected override object CreateItem( MethodInterceptionArgs parameter )
		{
			var code = KeyFactory.Instance.Create( parameter.Arguments );
			var result = Get( code, parameter ) ?? parameter.ReturnValue;
			return result;
		}
	}

	[PSerializable, ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		CacheValueFactory Factory { get; set; }

		void Initialize() => Factory = new CacheValueFactory();

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Factory != null && ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				args.ReturnValue = Factory.Create( args );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => MemberwiseClone();

		void IInstanceScopedAspect.RuntimeInitializeInstance() => Initialize();
	}
}