﻿using DragonSpark.Aspects.Build;
using System;
using System.Windows.Input;

namespace DragonSpark.Aspects.Validation
{
	sealed class CommandAutoValidationProfile : AutoValidationProfile
	{
		readonly static Type Type = typeof(ICommand);
		public static CommandAutoValidationProfile Default { get; } = new CommandAutoValidationProfile();
		CommandAutoValidationProfile() : base( Type, new MethodLocator( Type, nameof(ICommand.CanExecute) ), new MethodLocator( Type, nameof(ICommand.Execute) ), CommandAdapterFactory.Default.Get ) {}
	}
}