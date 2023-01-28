using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ToolbarSystem
{
	public enum Orientation
	{
		Horizontal,
		Vertical
	}

	public enum AutomaticHideOption
	{
		Never,
		InventoryOpen,
		InventoryClosed
	}

	/// <summary>
	/// Holds data about a toolbar, including it's position, orientation, and what tools it contains.
	/// </summary>
	internal class Toolbar
	{
		public bool hidden;

		public Vector2 relativePosition;
		public Orientation orientation;
		public AutomaticHideOption automaticHideOption;

		public List<Tool> tools;

		/// <summary>
		/// If the toolbar should appear collapsed, based on hidden state and automatic hide options
		/// </summary>
		public bool Visible
		{
			get
			{
				if (hidden)
				{
					return false;
				}
				else
				{
					return automaticHideOption switch
					{
						AutomaticHideOption.Never => false,
						AutomaticHideOption.InventoryOpen => Main.playerInventory,
						AutomaticHideOption.InventoryClosed => !Main.playerInventory,
						_ => false,
					};
				}
			}
		}

		public Toolbar() { }

		public Toolbar(Vector2 relativePosition, Orientation orientation, AutomaticHideOption automaticHideOption)
		{
			this.relativePosition = relativePosition;
			this.orientation = orientation;
			this.automaticHideOption = automaticHideOption;
		}

		/// <summary>
		/// Adds the tool of the given type to the toolbar, can be used in a builder-like pattern
		/// </summary>
		/// <typeparam name="T">The type of the singleton tool to add</typeparam>
		/// <returns>the toolbar instance that was added to</returns>
		public Toolbar AddTool<T>() where T : Tool
		{
			Tool tool = ToolHandler.GetTool<T>();

			if (tool != null)
				tools.Add(tool);

			return this;
		}

		/// <summary>
		/// Adds a tool specified by a string representation of a type to the toolbar. Intended to only be used by I/O code.
		/// </summary>
		/// <param name="typeName">The name of the type to load</param>
		public void AddTool(string typeName)
		{
			Tool tool = ToolHandler.GetTool(typeName);

			if (tool != null)
				tools.Add(tool);
		}

		public void SaveData(TagCompound tag)
		{
			tag["position"] = relativePosition;
			tag["orientation"] = (int)orientation;
			tag["automaticHideOption"] = (int)automaticHideOption;

			List<string> toolData = new();
			tools.ForEach(n => toolData.Add(n.GetType().Name));

			tag["tools"] = toolData;
		}

		public void LoadData(TagCompound tag)
		{
			relativePosition = tag.Get<Vector2>("position");
			orientation = (Orientation)tag.GetInt("orientation");
			automaticHideOption = (AutomaticHideOption)tag.GetInt("automaticHideOption");

			var toolData = (List<string>)tag.GetList<string>("tools");
			toolData.ForEach(n => AddTool(n));
		}
	}
}
