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

		public override string IconKey => "InfiniteReach";

		public override string DisplayName => "Omnipotent building";

		public override string Description => "You have infinite reach and can place blocks super quickly";

		public override void OnActivate()
		{
			active = !active;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				Color color = Color.White;
				color.A = 0;
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class InfiniteReachPlayer : ModPlayer
	{
		public override void UpdateEquips()
		{
			if (InfiniteReach.active)
			{
				Player.tileRangeX = int.MaxValue / 32 - 20;
				Player.tileRangeY = int.MaxValue / 32 - 20;
			}
		}

		public override bool CanUseItem(Item item)
		{
			return base.CanUseItem(item);
		}

		public override float UseTimeMultiplier(Item item)
		{
			if (InfiniteReach.active && (item.createTile != -1 || item.createWall != -1 || item.pick > 0 || item.axe > 0 || item.hammer > 0))
				return 0.1f;

			return 1;
		}

		public override float UseAnimationMultiplier(Item item)
		{
			if (InfiniteReach.active && (item.createTile != -1 || item.createWall != -1 || item.pick > 0 || item.axe > 0 || item.hammer > 0))
				return 0.1f;

			return 1;
		}
	}
}
