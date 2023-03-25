using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class EmergencyCustomizeMenu : SmartUIState
	{
		public static float scale = 0.6f;
		public static UIText button;

		public override bool Visible => Main.playerInventory;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			button = new("Customize toolbars", scale, false);
			button.Left.Set(-210, 1f);
			button.Top.Set(-90, 1f);
			button.Width.Set(200, 0);
			button.Height.Set(32, 0);
			button.TextOriginX = 0.5f;
			button.TextOriginY = 0.5f;

			button.OnClick += (a, b) =>
			{
				CustomizeTool.customizing = true;
				UILoader.GetUIState<ToolbarState>().Customize();
			};

			button.OnMouseOver += (a, b) => SoundEngine.PlaySound(SoundID.MenuTick);

			Append(button);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Recalculate();

			if (!button.IsMouseHovering && scale > 0.6f)
				scale -= 0.01f;

			if (button.IsMouseHovering && scale < 0.7f)
				scale += 0.01f;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Utils.DrawBorderStringBig(spriteBatch, "Customize tools", button.GetDimensions().Center(), new Color(240, 240, 240), scale, 0.5f, 0.5f);
		}
	}
}
