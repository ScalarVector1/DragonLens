using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;

namespace DragonLens.Content.Tools.Gameplay
{
	public sealed class SpawnPoint : Tool
	{
		private static Player Player => Main.LocalPlayer;
	
		public override string IconKey => "SpawnPoint";

		public override void OnActivate()
		{
			Main.spawnTileX = (int)((Player.position.X - 8f + Player.width / 2f) / 16f);
			Main.spawnTileY = (int)((Player.position.Y + Player.height) / 16f);
		
			Main.NewText(LocalizationHelper.GetToolText("SpawnPoint.Message", Main.spawnTileX, Main.spawnTileY));
		}
	}
}