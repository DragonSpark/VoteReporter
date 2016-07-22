using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;

namespace DragonSpark.Windows.Markup
{
	[MarkupExtensionReturnType( typeof(int) )]
	public class NextExtension : MarkupExtension
	{
		[Locate, Required]
		public IIncrementer Incrementer { [return: Required]get; set; }
		
		public override object ProvideValue( IServiceProvider serviceProvider )
		{
			var context = DetermineContext( serviceProvider );
			var result = Incrementer.Next( context );
			return result;
		}

		protected virtual object DetermineContext( IServiceProvider serviceProvider )
		{
			var xamlSchemaContext = serviceProvider.Get<IXamlSchemaContextProvider>().SchemaContext;
			var types = new[] { typeof(DragonSpark.Runtime.DeclarativeCollection) }; // TODO: More general/generic implementation.
			var context = serviceProvider.Get<IAmbientProvider>().GetFirstAmbientValue( types.Select( xamlSchemaContext.GetXamlType ).ToArray() );
			return context;
		}
	}
}