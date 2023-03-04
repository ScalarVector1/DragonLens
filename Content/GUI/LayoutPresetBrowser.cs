using DragonLens.Content.Themes.BoxProviders;
using DragonLens.Content.Themes.IconProviders;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Despawners;
using DragonLens.Content.Tools.Editors;
using DragonLens.Content.Tools.Gameplay;
using DragonLens.Content.Tools.Map;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Content.Tools.Visualization;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolbarSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class LayoutPresetBrowser : Browser
	{
		public override string Name => "Layout Presets";

		public override void PopulateGrid(UIGrid grid)
		{
			//TODO: Move the definition of these elsewhere, and check if they already exist before regenerating them.

			//Simple layout, default for most players
			grid.Add(new LayoutPresetButton(this, "Simple", n =>
			{
				n.Add(
					new Toolbar(new Vector2(0.3f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemSpawner>()
					.AddTool<ProjectileSpawner>()
					.AddTool<NPCSpawner>()
					.AddTool<BuffSpawner>()
					);

				n.Add(
					new Toolbar(new Vector2(0.3f, 0.85f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemDespawner>()
					.AddTool<ProjectileDespawner>()
					.AddTool<NPCDespawner>()
					.AddTool<GoreDespawner>()
					);

				n.Add(
					new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Godmode>()
					.AddTool<InfiniteReach>()
					.AddTool<NoClip>()
					.AddTool<FastForward>()
					.AddTool<Time>()
					.AddTool<Weather>()
					.AddTool<CustomizeTool>()
					);

				n.Add(
					new Toolbar(new Vector2(0.7f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Floodlight>()
					.AddTool<FreeCamera>()
					.AddTool<LockCamera>()
					);

				n.Add(
					new Toolbar(new Vector2(1f, 0.5f), Orientation.Vertical, AutomaticHideOption.NoMapScreen)
					.AddTool<RevealMap>()
					.AddTool<HideMap>()
					.AddTool<MapTeleport>()
					.AddTool<CustomizeTool>()
					);
			},
			ThemeHandler.GetBoxProvider<SimpleBoxes>(),
			ThemeHandler.GetIconProvider<DefaultIcons>(),
			"A simplified layout, with the tools most commonly used by regular players or testers."));

			//advanced layout, default for mod devs
			grid.Add(new LayoutPresetButton(this, "Advanced", n =>
			{
				n.Add(
					new Toolbar(new Vector2(0f, 0.5f), Orientation.Vertical, AutomaticHideOption.Never)
					.AddTool<ItemSpawner>()
					.AddTool<ProjectileSpawner>()
					.AddTool<NPCSpawner>()
					.AddTool<BuffSpawner>()
					.AddTool<DustSpawner>()
					.AddTool<TileSpawner>()
					);

				n.Add(
					new Toolbar(new Vector2(1f, 0.5f), Orientation.Vertical, AutomaticHideOption.InventoryOpen)
					.AddTool<ItemDespawner>()
					.AddTool<ProjectileDespawner>()
					.AddTool<NPCDespawner>()
					.AddTool<GoreDespawner>()
					.AddTool<DustDespawner>()
					);

				n.Add(
					new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Godmode>()
					.AddTool<InfiniteReach>()
					.AddTool<NoClip>()
					.AddTool<FastForward>()
					.AddTool<Time>()
					.AddTool<Weather>()
					.AddTool<Difficulty>()
					.AddTool<SpawnTool>()
					.AddTool<ItemEditor>()
					.AddTool<PlayerEditorTool>()
					.AddTool<Paint>()
					.AddTool<CustomizeTool>()
					);

				n.Add(
					new Toolbar(new Vector2(0.7f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<Floodlight>()
					.AddTool<Hitboxes>()
					.AddTool<FreeCamera>()
					.AddTool<LockCamera>()
					);

				n.Add(
					new Toolbar(new Vector2(1f, 0.5f), Orientation.Vertical, AutomaticHideOption.NoMapScreen)
					.AddTool<RevealMap>()
					.AddTool<HideMap>()
					.AddTool<MapTeleport>()
					.AddTool<CustomizeTool>()
					);
			},
			ThemeHandler.GetBoxProvider<SimpleBoxes>(),
			ThemeHandler.GetIconProvider<DefaultIcons>(),
			"An advanced layout with every tool, for mod developers."));

			//Attempts to mock the HEROs mod UI as best as possible
			grid.Add(new LayoutPresetButton(this, "HEROS mod imitation", n =>
			{
				n.Add(
					new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<ItemSpawner>()
					.AddTool<InfiniteReach>()
					.AddTool<SpawnTool>()
					.AddTool<ItemDespawner>()
					.AddTool<Time>()
					.AddTool<Weather>()
					//TODO: Waypoints?
					.AddTool<NPCSpawner>()
					.AddTool<BuffSpawner>()
					.AddTool<Godmode>()
					.AddTool<ItemEditor>()
					.AddTool<CustomizeTool>()
					);

				n.Add(
					new Toolbar(new Vector2(0f, 0.9f), Orientation.Vertical, AutomaticHideOption.NoMapScreen)
					.AddTool<MapTeleport>()
					);
			},
			 ThemeHandler.GetBoxProvider<VanillaBoxes>(),
			 ThemeHandler.GetIconProvider<HEROsIcons>(),
			"A layout attempting to imitate HEROs mod"));

			//Attempts to mock the Cheatsheet mod UI as best as possible
			grid.Add(new LayoutPresetButton(this, "Cheatsheet imitation", n => n.Add(
						new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
						.AddTool<ItemSpawner>()
						.AddTool<NPCSpawner>()
						//TODO: Recipie browser?
						//TODO: Waypoints?
						.AddTool<ItemDespawner>()
						.AddTool<Paint>()
						.AddTool<PlayerEditorTool>()
						.AddTool<NPCDespawner>()
						.AddTool<SpawnTool>()
						.AddTool<Floodlight>()
						.AddTool<CustomizeTool>()
						),
						ThemeHandler.GetBoxProvider<VanillaBoxes>(),
						ThemeHandler.GetIconProvider<DefaultIcons>(),
						"A layout attempting to imitate Cheatsheet"));

			//A blank slate
			grid.Add(new LayoutPresetButton(this, "Empty", n => n.Add(
					new Toolbar(new Vector2(0.5f, 1f), Orientation.Horizontal, AutomaticHideOption.Never)
					.AddTool<CustomizeTool>()),
					ThemeHandler.GetBoxProvider<SimpleBoxes>(),
					ThemeHandler.GetIconProvider<DefaultIcons>(),
					"A blank layout for you to fill in however you like!"));
		}

		public override void PostInitialize()
		{
			listMode = true;
		}
	}

	internal class LayoutPresetButton : BrowserButton
	{
		private readonly string name;
		private readonly string tooltip;
		private readonly string presetPath;

		public override string Identifier => name;

		public ThemeBoxProvider boxes;
		public ThemeIconProvider icons;

		public LayoutPresetButton(Browser parent, string name, Action<List<Toolbar>> preset, ThemeBoxProvider boxes, ThemeIconProvider icons, string tooltip = "A toolbar layout preset") : base(parent)
		{
			this.name = name;
			presetPath = Path.Join(Main.SavePath, "DragonLensLayouts", name);

			this.boxes = boxes;
			this.icons = icons;

			ToolbarHandler.BuildPreset(name, preset, boxes, icons);
			this.tooltip = tooltip;
		}

		public override void Click(UIMouseEvent evt)
		{
			ToolbarHandler.LoadFromFile(presetPath);
			UILoader.GetUIState<ToolbarState>().Refresh();

			if (!FirstTimeSetupSystem.trueFirstTime)
			{
				UILoader.GetUIState<ToolbarState>().Customize();
			}
			else
			{
				parent.visible = false;
				Main.NewText($"{name} selected! Use the customize tool (wrench icon) to customize your layout or load a different preset.");

				FirstTimeSetupSystem.trueFirstTime = false;
			}

			Main.NewText($"Loaded layout: {name}");
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea)
		{
			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip(tooltip);
			}
		}
	}
}
