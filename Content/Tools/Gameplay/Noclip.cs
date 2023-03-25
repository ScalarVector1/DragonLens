using DragonLens.Configs;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class NoClip : Tool
	{
		public static bool active = false;

		public static Vector2 desiredPos;

		public override string IconKey => "Noclip";

		public override string DisplayName => "Noclip";

		public override string Description => "You can move without any inhibition";

		public override void OnActivate()
		{
			active = !active;

			if (active)
				desiredPos = Main.LocalPlayer.Center;
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

	internal class NoClipPlayer : ModPlayer
	{
		public override void PostUpdate()
		{
			if (NoClip.active)
				Player.Center = NoClip.desiredPos;

			if (Player.controlLeft)
				NoClip.desiredPos.X -= 15;
			if (Player.controlRight)
				NoClip.desiredPos.X += 15;
			if (Player.controlUp)
				NoClip.desiredPos.Y -= 15;
			if (Player.controlDown)
				NoClip.desiredPos.Y += 15;
		}
	}
}