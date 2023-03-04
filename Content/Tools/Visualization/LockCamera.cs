using DragonLens.Configs;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Visualization
{
	internal class LockCamera : Tool
	{
		public static bool active = false;

		public static Vector2 lockCameraPos;

		public override string IconKey => "LockCamera";

		public override string DisplayName => "Lock camera";

		public override string Description => "Lock the camera to it's current position. Mutually exclusive with free camera.";

		public override void OnActivate()
		{
			active = !active;

			if (active)
			{
				lockCameraPos = Main.screenPosition;

				FreeCamera.active = false;
			}
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			base.DrawIcon(spriteBatch, position);

			if (active)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle((int)position.X - 7, (int)position.Y - 7, 46, 46), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

				Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/Misc/GlowAlpha").Value;
				var color = new Color(255, 255, 255)
				{
					A = 0
				};
				var target = new Rectangle((int)position.X, (int)position.Y, 32, 32);

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
