using DragonSpark.Extensions;
using System;
using System.IO;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	/*public class DirectoryInfoFactory : FactoryBase<string, DirectoryInfo>
	{
		readonly string baseDirectory;

		public DirectoryInfoFactory( string baseDirectory )
		{
			this.baseDirectory = baseDirectory ?? ;
		}

		protected override DirectoryInfo CreateFrom( Type resultType, string parameter )
		{
			// var directory = Directory.CreateDirectory( Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Reports" ) );
			
		}
	}*/

	[MarkupExtensionReturnType( typeof(DirectoryInfo) )]
	public class DirectoryInfoExtension : MarkupExtension
	{
		readonly string path;

		public DirectoryInfoExtension( string path )
		{
			this.path = path;
		}

		public override object ProvideValue( IServiceProvider serviceProvider )
		{
			var item = Path.IsPathRooted( path ) ? path : Path.GetFullPath( path );
			var result = Directory.CreateDirectory( item );
			return result;
		}
	}
}