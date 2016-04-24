using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Windows.Properties;
using System.IO;

namespace DragonSpark.Windows.Entity
{
	public class InstallDatabaseCommand : Command<object>
	{
		[Factory( typeof(AttachedDatabaseFileFactory) )]
		public FileInfo Database { get; set; }

		protected override void OnExecute( object parameter ) => Database.Exists.IsFalse( () =>
		{
			var items = EntityFiles.WithLog( Database ).TupleWith( new[] { Resources.Blank, Resources.Blank_log } );
			items.Each( tuple => 
			{
				using ( var stream = File.Create( tuple.Item1.FullName ) )
				{
					stream.Write( tuple.Item2, 0, tuple.Item2.Length );
				}
			} );
		} );
	}
}