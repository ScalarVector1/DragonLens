using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using System.Diagnostics;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DragonLens
{
	public class DragonLens : Mod
	{
		public override void PostAddRecipes()
		{
			if (ModContent.GetInstance<ToolConfig>().preloadSpawners)
			{
				UILoader.GetUIState<ItemBrowser>().Refresh();
				UILoader.GetUIState<ItemBrowser>().initialized = true;

				UILoader.GetUIState<ProjectileBrowser>().Refresh();
				UILoader.GetUIState<ProjectileBrowser>().initialized = true;

				UILoader.GetUIState<NPCBrowser>().Refresh();
				UILoader.GetUIState<NPCBrowser>().initialized = true;

				UILoader.GetUIState<BuffBrowser>().Refresh();
				UILoader.GetUIState<BuffBrowser>().initialized = true;

				UILoader.GetUIState<TileBrowser>().Refresh();
				UILoader.GetUIState<TileBrowser>().initialized = true;

				UILoader.GetUIState<ToolBrowser>().Refresh();
				UILoader.GetUIState<ToolBrowser>().initialized = true;
			}

			if (ModContent.GetInstance<ToolConfig>().preloadAssets)
			{
				var itemThread = new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < ItemID.Count; k++)
					{
						Main.instance.LoadItem(k);
					}

					watch.Stop();
					Logger.Info($"Item assets finished loading in {watch.ElapsedMilliseconds} ms");
				});
				itemThread.Start();

				var projThread = new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < ProjectileID.Count; k++)
					{
						Main.instance.LoadProjectile(k);
					}

					watch.Stop();
					Logger.Info($"Projectile assets finished loading in {watch.ElapsedMilliseconds} ms");
				});
				projThread.Start();

				var npcThread = new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < NPCID.Count; k++)
					{
						Main.instance.LoadNPC(k);
					}

					watch.Stop();
					Logger.Info($"NPC assets finished loading in {watch.ElapsedMilliseconds} ms");
				});
				npcThread.Start();

				var tileThread = new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < TileID.Count; k++)
					{
						Main.instance.LoadTiles(k);
					}

					watch.Stop();
					Logger.Info($"Tile assets finished loading in {watch.ElapsedMilliseconds} ms");
				});
				tileThread.Start();
			}
		}
	}
}