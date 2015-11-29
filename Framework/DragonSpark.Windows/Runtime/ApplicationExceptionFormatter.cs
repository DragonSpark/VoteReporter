using DragonSpark.TypeSystem;

namespace DragonSpark.Windows.Runtime
{
	public class ApplicationExceptionFormatter : DragonSpark.Diagnostics.ApplicationExceptionFormatter
	{
		public ApplicationExceptionFormatter( AssemblyInformation information ) : base( EnterpriseLibraryExceptionFormatter.Instance, information )
		{}
	}
}