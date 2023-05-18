using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
namespace DragonLens.Content.Tools.Visualization
{
	internal class LockCamera : Tool
	{
		public static bool active = false;

		public static Vector2 lockCameraPos;

		public override string IconKey => "LockCamera";

		public override void OnActivate()
		{
			active = !active;

			if (active)
			{
				lockCameraPos = Main.screenPosition;

				FreeCamera.active = false;
			}
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				var color = new Color(255, 255, 255)
				{
					A = 0
				};
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class LockCameraSystem : ModSystem
	{
		public override void ModifyScreenPosition()
		{
			if (LockCamera.active)
				Main.screenPosition = LockCamera.lockCameraPos;
		}
	}
}