using System.ComponentModel.DataAnnotations;

namespace DragonSpark.Testing.TestObjects
{
	[MetadataType( typeof(RelayedDescriptor) )]
	class Relayed
	{
		[Attribute( PropertyName = "This is a relayed class attribute." )]
		class RelayedDescriptor
		{
			[Attribute( PropertyName = "This is a relayed property attribute." )]
			public string Property { get; set; }
		}

		public string Property { get; set; }
	}
}