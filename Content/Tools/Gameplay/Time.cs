using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Time : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/Time";

		public override string Name => "Time tool";

		public override string Description => "Adjust or pause the day/night cycle";

		public override void OnActivate()
		{
			TimeWindow state = UILoader.GetUIState<TimeWindow>();
			state.visible = !state.visible;
		}
	}

	internal class TimeWindow : DraggableUIState
	{
		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 32);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 400;
			height = 200;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, 400, 200), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ModContent.Request<Texture2D>("DragonLens/Assets/Tools/Time").Value;
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, "Set time", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}
}