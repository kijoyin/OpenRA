#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

namespace OpenRA
{
	public interface IUtilityCommand
	{
		/// <summary>
		/// The string used to invoke the command.
		/// </summary>
		string Name { get; }

		void Run(ModData modData, string[] args);
	}
}
