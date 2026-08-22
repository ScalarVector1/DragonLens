using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;

namespace DragonLens.Content.Tools.Visualization
{
	internal class LockCamera : Tool
	{
		public static bool active = false;

		public override bool IsHighlighted => active;

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