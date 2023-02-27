using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ToolbarSystem
{
	/// <summary>
	/// The direction in which a toolbar expands with new tools
	/// </summary>
	public enum Orientation
	{
		Horizontal,
		Vertical
	}

	/// <summary>
	/// When a toolbar should automatically remove itself from the UI
	/// </summary>
	public enum AutomaticHideOption
	{
		Never,
		InventoryOpen,
		InventoryClosed,
		NoMapScreen
	}

	/// <summary>
	/// The direction in which a toolbar should collapse itself
	/// </summary>
	public enum CollapseDirection
	{
		Left,
		Right,
		Up,
		Down,
		Floating
	}

	/// <summary>
	/// Holds data about a toolbar, including it's position, orientation, and what tools it contains.
	/// </summary>
	internal class Toolbar
	{
		/// <summary>
		/// If the toolbar should be collapsed to a collapse button only
		/// </summary>
		public bool collapsed;

		/// <summary>
		/// Where a toolbar should position itself on the UI
		/// </summary>
		public Vector2 relativePosition;

		/// <summary>
		/// The direction in which a toolbar expands with new tools
		/// </summary>
		public Orientation orientation;

		/// <summary>
		/// When a toolbar should automatically remove itself from the UI
		/// </summary>
		public AutomaticHideOption automaticHideOption;

		/// <summary>
		/// The tools that a toolbar contains buttons to use
		/// </summary>
		public List<Tool> toolList = new();

		/// <summary>
		/// If the toolbar should not draw at all, even a collapse button
		/// </summary>
		public bool Invisible => automaticHideOption switch
		{
			AutomaticHideOption.Never => false || Main.mapFullscreen,
			AutomaticHideOption.InventoryOpen => Main.playerInventory || Main.mapFullscreen,
			AutomaticHideOption.InventoryClosed => !Main.playerInventory || Main.mapFullscreen,
			AutomaticHideOption.NoMapScreen => !Main.mapFullscreen,
			_ => false,
		};

		/// <summary>
		/// The direction in which a toolbar should collapse itself
		/// </summary>
		public CollapseDirection CollapseDirection
		{
			get
			{
				if (relativePosition.X == 0 && orientation == Orientation.Vertical)
					return CollapseDirection.Left;

				if (relativePosition.X == 1 && orientation == Orientation.Vertical)
					return CollapseDirection.Right;

				if (relativePosition.Y == 0 && orientation == Orientation.Horizontal)
					return CollapseDirection.Up;

				if (relativePosition.Y == 1 && orientation == Orientation.Horizontal)
					return CollapseDirection.Down;

				return CollapseDirection.Floating;
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
			Tool tool = ModContent.GetInstance<T>();

			if (tool != null)
				toolList.Add(tool);

			return this;
		}

		/// <summary>
		/// Adds a tool specified by a string representation of a type to the toolbar. Intended to only be used by I/O code.
		/// </summary>
		/// <param name="typeName">The name of the type to load</param>
		public void AddTool(string typeName)
		{
			ModContent.SplitName(typeName, out string modName, out string type);

			if (!ModLoader.TryGetMod(modName, out Mod mod))
				return;

			Tool tool = mod.Find<Tool>(type);

			if (tool != null)
				toolList.Add(tool);
		}

		/// <summary>
		/// Adds a tool instance to this toolbar.
		/// </summary>
		/// <param name="tool">The tool to add.</param>
		public void AddTool(Tool tool)
		{
			toolList.Add(tool);
		}

		public void SaveData(TagCompound tag)
		{
			tag["position"] = relativePosition;
			tag["orientation"] = (int)orientation;
			tag["automaticHideOption"] = (int)automaticHideOption;

			List<string> toolData = new();
			toolList.ForEach(n => toolData.Add(n.FullName));

			tag["tools"] = toolData;
		}

		public void LoadData(TagCompound tag)
		{
			relativePosition = tag.Get<Vector2>("position");
			orientation = (Orientation)tag.GetInt("orientation");
			automaticHideOption = (AutomaticHideOption)tag.GetInt("automaticHideOption");

			var toolData = (List<string>)tag.GetList<string>("tools");
			toolData.ForEach(AddTool);
		}
	}
}
