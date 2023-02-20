using DragonLens.Configs;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Visualization
{
	internal class FreeCamera : Tool
	{
		public static bool active = false;

		public static Vector2 freeCameraPos;

		public override string Texture => "DragonLens/Assets/Tools/FreeCamera";

		public override string DisplayName => "Free camera";

		public override string Description => "You can move the camera around freely with your movement keys. Hold SHIFT to speed up and CTRL to slow down while moving. Mutually exclusive with lock camera.";

		public override void OnActivate()
		{
			active = !active;

			if (active)
			{
				freeCameraPos = Main.screenPosition;

				LockCamera.active = false;
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
}
