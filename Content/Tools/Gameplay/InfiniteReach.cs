using DragonLens.Configs;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class InfiniteReach : Tool
	{
		public static bool active = false;

		public override string Texture => "DragonLens/Assets/Tools/InfiniteReach";

		public override string Name => "Omnipotent building";

		public override string Description => "You have infinite reach and can place blocks super quickly";

		public override void OnActivate()
		{
			active = !active;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle((int)position.X - 7, (int)position.Y - 7, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = Color.White;
				color.A = 0;
				var target = new Rectangle((int)position.X, (int)position.Y, 32, 32);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class InfiniteReachPlayer : ModPlayer
	{
		public override void UpdateEquips()
		{
			if (InfiniteReach.active)
				Player.blockRange = int.MaxValue;
		}

		public override float UseAnimationMultiplier(Item item)
		{
			if (InfiniteReach.active && item.createTile != -1)
				return 0.1f;

			return 1;
		}
	}
}
