using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ID;

namespace DragonLens.Core.Systems
{
	internal class AssetPreloadSystem : ModSystem
	{
		static void LoadAsset<T>(Asset<T> asset) where T : class
		{
			if (asset.State == AssetState.NotLoaded)
				Main.Assets.Request<Texture2D>(asset.Name, AssetRequestMode.AsyncLoad).Wait();
		}

		public override void PostAddRecipes()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

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
				var itemThread =
				new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < ItemID.Count; k++)
					{
						try
						{
							LoadAsset(TextureAssets.Item[k]);
						}
						catch
						{
							Mod.Logger.Warn($"Item asset {k} failed to load");
							continue;
						}
					}

					watch.Stop();
					Mod.Logger.Info($"Item assets finished loading in {watch.ElapsedMilliseconds} ms");
				})
				{
					IsBackground = true
				};
				itemThread.Start();

				var projThread =
				new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < ProjectileID.Count; k++)
					{
						try
						{
							LoadAsset(TextureAssets.Projectile[k]);
						}
						catch
						{
							Mod.Logger.Warn($"Projectile asset {k} failed to load");
							continue;
						}
					}

					watch.Stop();
					Mod.Logger.Info($"Projectile assets finished loading in {watch.ElapsedMilliseconds} ms");
				})
				{
					IsBackground = true
				};
				projThread.Start();

				var npcThread =
				new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < NPCID.Count; k++)
					{
						try
						{
							LoadAsset(TextureAssets.Npc[k]);
						}
						catch
						{
							Mod.Logger.Warn($"NPC asset {k} failed to load");
							continue;
						}
					}

					watch.Stop();
					Mod.Logger.Info($"NPC assets finished loading in {watch.ElapsedMilliseconds} ms");
				})
				{
					IsBackground = true
				};
				npcThread.Start();

				var tileThread =
				new Thread(() =>
				{
					Stopwatch watch = new();
					watch.Start();

					for (int k = 0; k < TileID.Count; k++)
					{
						try
						{
							LoadAsset(TextureAssets.Tile[k]);
						}
						catch
						{
							Mod.Logger.Warn($"Tile asset {k} failed to load");
							continue;
						}
					}

					watch.Stop();
					Mod.Logger.Info($"Tile assets finished loading in {watch.ElapsedMilliseconds} ms");
				})
				{
					IsBackground = true
				};
				tileThread.Start();
			}
		}
	}
}