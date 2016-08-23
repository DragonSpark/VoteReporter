using DragonSpark.Extensions;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using PostSharp.Reflection;
using System;
using System.Linq;

namespace DragonSpark.Aspects
{
	public class OfTypeAttribute : LocationContractAttribute, ILocationValidationAspect<Type>
	{
		readonly Type[] types;

		public OfTypeAttribute( params Type[] types )
		{
			this.types = types;
		}

		protected override string GetErrorMessage()
		{
			var names = string.Join( " or ", types.Select( type => type.FullName ) );
			return /*ContractLocalizedTextProvider.Current.GetMessage( nameof(OfTypeAttribute) )*/ $"The specified type is not of type (or cannot be cast to) {names}";
		}

		public Exception ValidateValue( Type value, string locationName, LocationKind locationKind )
		{
			foreach ( var type in types )
			{
				if ( type.Adapt().IsAssignableFrom( value ) )
				{
					return null;
				}
			}

			return CreateException( value, locationName, locationKind, LocationValidationContext.SuccessPostcondition );
		}

		Exception CreateException( object value, string locationName, LocationKind locationKind, LocationValidationContext context )
		{
			var factory = context == LocationValidationContext.SuccessPostcondition ? (Func<object, string, LocationKind, Exception>)CreatePostconditionFailedException : CreateArgumentException;
			var result = factory( value, locationName, locationKind );
			return result;
		}
	}
}