using DragonLens.Configs;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ToolbarSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class FirstTimeLayoutPresetMenu : SmartUIState
	{
		public override bool Visible => FirstTimeSetupSystem.trueFirstTime;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			var newButton = new FirstTimeLayoutButton("Simple", Path.Join(Main.SavePath, "DragonLensLayouts", "Simple"), "DragonLens/Assets/Misc/PreviewSimple", "A simplified layout, perfect for players or mod testers.");
			newButton.Left.Set(-220, 0.5f);
			newButton.Top.Set(-262, 0.5f);
			Append(newButton);

			newButton = new FirstTimeLayoutButton("Advanced", Path.Join(Main.SavePath, "DragonLensLayouts", "Advanced"), "DragonLens/Assets/Misc/PreviewAdv", "An advanced layout with every tool available. Perfect for mod developers or power users.");
			newButton.Left.Set(20, 0.5f);
			newButton.Top.Set(-262, 0.5f);
			Append(newButton);

			newButton = new FirstTimeLayoutButton("HEROs Mod imitation", Path.Join(Main.SavePath, "DragonLensLayouts", "HEROs mod imitation"), "DragonLens/Assets/Misc/PreviewHeros", "A layout attempting to mimic HEROs mod as closely as possible.");
			newButton.Left.Set(-220, 0.5f);
			newButton.Top.Set(20, 0.5f);
			Append(newButton);

			newButton = new FirstTimeLayoutButton("Cheatsheet imitation", Path.Join(Main.SavePath, "DragonLensLayouts", "Cheatsheet imitation"), "DragonLens/Assets/Misc/PreviewCheatsheet", "A layout attempting to mimic Cheatsheet as closely as possible.");
			newButton.Left.Set(20, 0.5f);
			newButton.Top.Set(20, 0.5f);
			Append(newButton);
		}

		public override void Update(GameTime gameTime)
		{
			Recalculate();
			base.Update(gameTime);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Utils.DrawBorderStringBig(spriteBatch, "Welcome to DragonLens!", GetDimensions().Center() + new Vector2(0, -380), Color.White, 1, 0.5f, 0);
			Utils.DrawBorderStringBig(spriteBatch, "Please select a layout to get started", GetDimensions().Center() + new Vector2(0, -320), Color.LightGray, 0.6f, 0.5f, 0);

			base.Draw(spriteBatch);
		}
	}

	internal class FirstTimeLayoutButton : UIElement
	{
		private readonly string name;
		private readonly string tooltip;
		private readonly string presetPath;
		private readonly string texPath;

		public FirstTimeLayoutButton(string name, string presetPath, string texPath, string tooltip = "A toolbar layout preset")
		{
			this.name = name;
			this.presetPath = presetPath;
			this.tooltip = tooltip;
			this.texPath = texPath;

			Width.Set(218, 0);
			Height.Set(244, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			ToolbarHandler.LoadFromFile(presetPath);
			UILoader.GetUIState<ToolbarState>().Refresh();

			Main.NewText($"{name} selected! Use the customize tool (wrench icon) to customize your layout or load a different preset.");
			FirstTimeSetupSystem.trueFirstTime = false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Utils.DrawBorderString(spriteBatch, name, GetDimensions().Position() + Vector2.One * 8, Color.White);

			Texture2D tex = ModContent.Request<Texture2D>(texPath).Value;
			spriteBatch.Draw(tex, GetDimensions().Position() + new Vector2(8, 36), null, Color.White, 0, Vector2.Zero, 0.5f, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(name);
				Tooltip.SetTooltip(tooltip);
			}
		}
	}
}
