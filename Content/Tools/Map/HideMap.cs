using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Map
{
	internal class HideMap : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/HideMap";

		public override string DisplayName => "Hide map";

		public override string Description => "Resets the world map";

		public override void OnActivate()
		{
			for (int i = 0; i < Main.maxTilesX; i++)
			{
				for (int j = 0; j < Main.maxTilesY; j++)
				{
					if (WorldGen.InWorld(i, j))
						Main.Map.Update(i, j, 0);
				}
			}

			Main.refreshMap = true;
		}
	}
}
