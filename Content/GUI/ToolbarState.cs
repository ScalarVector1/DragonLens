using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class ToolbarState : SmartUIState
	{
		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			Recalculate();
		}

		/// <summary>
		/// Refresh the entire HUD with new toolbars, like when you would want to load a template
		/// </summary>
		public void Refresh()
		{
			RemoveAllChildren();

			foreach (Toolbar toolbar in ToolbarHandler.activeToolbars)
			{
				var element = new ToolbarElement(toolbar);
				element.Refresh();
				Append(element);
			}

			Recalculate();
		}

		public void Customize()
		{
			foreach (UIElement child in Children)
			{
				if (child is ToolbarElement)
					(child as ToolbarElement).Customize();
			}
		}

		public void FinishCustomize()
		{
			foreach (UIElement child in Children)
			{
				if (child is ToolbarElement)
					(child as ToolbarElement).FinishCustomize();
			}
		}
	}

	/// <summary>
	/// Handles refreshing the toolbar layout when appropriate
	/// </summary>
	internal class ToolbarStateHandler : ModSystem
	{
		Vector2 oldScreenSize;

		public override void Load()
		{
			On.Terraria.Main.CheckMonoliths += RefreshUI;
		}

		private void RefreshUI(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			orig();

			if (Main.screenWidth != oldScreenSize.X || Main.screenHeight != oldScreenSize.Y)
				UILoader.GetUIState<ToolbarState>().Refresh();

			oldScreenSize.X = Main.screenWidth;
			oldScreenSize.Y = Main.screenHeight;
		}
	}
}
