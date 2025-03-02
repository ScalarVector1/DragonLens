using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;

namespace DragonLens.Content.Tools.Visualization
{
	internal class FreeCamera : Tool
	{
		public static bool active = false;

		public static Vector2 freeCameraPos;

		public override string IconKey => "FreeCamera";

		public override void OnActivate()
		{
			active = !active;

			if (active)
			{
				freeCameraPos = Main.screenPosition;

				LockCamera.active = false;
			}
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = Assets.Misc.GlowAlpha.Value;
				var color = new Color(255, 255, 255)
				{
					A = 0
				};
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class FreeCameraSystem : ModSystem
	{
		public override void PostUpdateInput()
		{
			if (FreeCamera.active)
			{
				float speed = 10;

				if (Main.keyState.PressingShift())
					speed = 40;
				else if (Main.keyState.PressingControl())
					speed = 2;

				if (Main.LocalPlayer.controlUp)
					FreeCamera.freeCameraPos.Y -= speed;
				if (Main.LocalPlayer.controlDown)
					FreeCamera.freeCameraPos.Y += speed;
				if (Main.LocalPlayer.controlLeft)
					FreeCamera.freeCameraPos.X -= speed;
				if (Main.LocalPlayer.controlRight)
					FreeCamera.freeCameraPos.X += speed;
			}
		}

		public override void ModifyScreenPosition()
		{
			if (FreeCamera.active)
				Main.screenPosition = FreeCamera.freeCameraPos;
		}
	}

	internal class FreeCameraPlayer : ModPlayer
	{
		public override void PreUpdateMovement()
		{
			if (FreeCamera.active)
			{
				Player.velocity.X *= 0;
			}
		}
	}
}