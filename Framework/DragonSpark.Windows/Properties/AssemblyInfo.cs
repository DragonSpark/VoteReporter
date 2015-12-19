﻿using DragonSpark;
using DragonSpark.Setup;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using DragonSpark.Setup.Registration;

// using AmbientAttribute = hello::System.Windows.Markup.AmbientAttribute;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "DragonSpark.Windows" )]
[assembly: AssemblyDescription( "" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "" )]
[assembly: AssemblyProduct( "DragonSpark.Windows" )]
[assembly: AssemblyCopyright( "Copyright © 2015" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "4e4cf3dc-1c0e-4a37-9871-71124aef7cb9" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( "1.0.0.0" )]
[assembly: AssemblyFileVersion( "1.0.0.0" )]
[assembly: XmlnsPrefix( "http://framework.dragonspark.us", "ds" )]
[assembly: XmlnsDefinition( "http://framework.dragonspark.us", "DragonSpark.Windows.Markup" )]
[assembly: XmlnsDefinition( "http://framework.dragonspark.us", "DragonSpark.Windows.Setup" )]
[assembly: Registration( Priority.BelowLower, Namespaces = "DragonSpark.Windows.Modularity" )]
// [assembly: Include( typeof( Setup ) )]

// [assembly: TypeForwardedTo( typeof(AmbientAttribute) )]