using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Despawners
{
	internal class ItemDespawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ItemDespawner";

		public override string DisplayName => "Clear items";

		public override string Description => "Removes all items on the ground";

		public override void OnActivate()
		{
			foreach (Item item in Main.item)
			{
				item.active = false;
			}
		}
	}
}
