using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class NoClip : Tool
	{
		public static bool active = false;

		public static Vector2 desiredPos;

		public override string IconKey => "Noclip";

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
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = Assets.Misc.GlowAlpha.Value;
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
			if (NoClip.active && PermissionHandler.CanUseTools(Player))
			{
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
}