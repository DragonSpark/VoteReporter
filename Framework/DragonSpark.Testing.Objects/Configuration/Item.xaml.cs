﻿using DragonSpark.Diagnostics;
using System.IO;

namespace DragonSpark.Testing.Objects.Configuration
{
	/// <summary>
	/// Interaction logic for Item.xaml
	/// </summary>
	public partial class Item
	{
		public Item()
		{
			Policies.Retry<IOException>( InitializeComponent );
		}
	}
}
