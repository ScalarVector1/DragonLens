using DragonLens.Content.Tools.Multiplayer.Trackers;
using DragonLens.Core.Systems;
using DragonLens.Helpers;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using static DragonLens.Content.Tools.Multiplayer.BiomeHelper;

namespace DragonLens.Content.Tools.Multiplayer.Drawers
{
	/// <summary>
	/// Here's a guide for adding a new stat:
	/// 1. Add a new entry to the stats list in <see cref="Stats"/>
	/// 2. Add a new stat filter to <see cref="PlayerManagerBrowser.SetupFilters(GUI.FilterPanel)"/>
	/// 3. Add a new stat entry to <see cref="PlayerManagerSettings"/>
	/// (Optional) If the stat needs tracking, write a new system in <see cref="Trackers"/>, e.g <see cref="SessionTracker"/>
	/// </summary>
	internal static class PlayerStatDrawer
	{
		// The order in which stats are drawn. Try to match with SetupFilter in PlayerManagerBrowser for consistency.
		private static readonly PlayerManagerStat[] Stats =
		[
			new("Life", DrawLifeStat, player => $"{player.statLife}/{player.statLifeMax2}"),
			new("Mana", DrawManaStat, player => $"{player.statMana}/{player.statManaMax2}"),
			new("Defense", DrawDefenseStat, player => $"{player.statDefense} defense"),
			new("HeldItem", DrawHeldItemStat, GetHeldItemText),
			new("BiomeName", DrawBiomeStat, GetBiomeText),
			new("Position", DrawPositionStat, player => $"({(int)Math.Round(player.Center.X / 16f)}, {(int)Math.Round(player.Center.Y / 16f)})"),
			new("Team", DrawTeamStat, GetTeamText),
			new("MovementSpeed", DrawMovementSpeedStat, GetMovementSpeed),
			new("Distance", DrawDistanceStat, player => $"{Math.Round(Vector2.Distance(Main.LocalPlayer.Center, player.Center) / 16f)} tiles"),
			new("SessionTime", DrawSessionTimeStat, player => SessionTracker.GetSessionDuration(player.whoAmI)),
			new("Ping", DrawPingStat, player => $"{GetPlayerPingMs(player)} ms"),
			new("InventoryItemCount", DrawInventoryItemCountStat, player => $"{CountInventoryItems(player)} items"),
			new("CoinCount", DrawCoinCountStat, player => FormatTotalCoins(CountTotalCoins(player), out _)),
			new("AmmoCount", DrawAmmoCountStat, player => $"{CountAmmo(player)} ammo"),
			new("MinionCount", DrawMinionCountStat, player => $"{CountPlayerMinions(player)}/{player.maxMinions} minions"),
			new("NearbyEnemies", DrawNearbyEnemiesStat, player => $"{CountNearbyEnemies(player, 1200f)} nearby"),
			new("LastEnemyHit", DrawLastEnemyHitStat, GetLastEnemyHitText),
			new("LastPlayerHit", DrawLastPlayerHitStat, GetLastPlayerHitText),
			new("DeathCount", DrawDeathCountStat, player => $"{player.numberOfDeathsPVP} / {player.numberOfDeathsPVE}"),
			new("BossDamage", DrawBossDamageStat, GetBossDamageText),
		];

		internal static void DrawStats(SpriteBatch sb, PlayerManagerSettings settings, Player player, Rectangle rect, bool listMode)
		{
			int availableHeight = rect.Height;

			// Row height lerp
			float rowHeightLerp = Utils.GetLerpValue(40f, 108f, availableHeight, true);
			rowHeightLerp = MathHelper.SmoothStep(0f, 1f, rowHeightLerp);
			int rowHeight = (int)MathHelper.Lerp(18f, 24f, rowHeightLerp);

			// Positioning
			const int iconSize = 20;
			const int textOffsetX = 24;
			rect.X += 8;
			rect.Y += 8;

			bool drawPlayerHead = settings.IsPlayerMode("PlayerHead");
			int columns = listMode ? settings.GetListStatColumnCount() : 1;
			int playerFullOffsetX = settings.IsPlayerMode("PlayerFull") && listMode ? 60 : 0;
			int totalRowWidth = Math.Min(rect.Width - 12, 110);
			const int columnGap = 6;
			const int columnWidth = 110;

			// Move all stats to the right if showing full player in list mode.
			if (settings.IsPlayerMode("PlayerFull") && listMode)
				rect.X += 60;

			// Determine max stats shown
			int maxStats = GetMaxStats(availableHeight, listMode, columns);

			int rowsPerColumn = listMode
				? availableHeight >= 48 + 46 ? 4 :
				  availableHeight >= 48 + 18 ? 3 :
				  availableHeight >= 48 ? 2 :
				  0
				: 0;

			int topRowOffset = drawPlayerHead && listMode ? 1 : 0;
			int usableRowsPerColumn = listMode ? Math.Max(1, rowsPerColumn - topRowOffset) : 0;

			// Move stats down by one row if showing player name in grid mode.
			const int minHeightForPlayerName = 102;
			bool reserveTopRowForPlayerName = settings.IsPlayerMode("PlayerFull") && !listMode && rect.Height >= minHeightForPlayerName;
			if (reserveTopRowForPlayerName)
			{
				rect.Y += rowHeight;
				maxStats = Math.Max(0, maxStats - 1);
			}

			int drawnStats = 0;

			// Draw player stat manually
			if (drawPlayerHead && drawnStats < maxStats)
			{
				int rowWidth = listMode ? columnWidth : Math.Min(rect.Width - 12, 110);
				Rectangle row = new(rect.X, rect.Y + drawnStats * rowHeight, rowWidth, rowHeight);
				Rectangle iconBox = new(row.X, row.Y, iconSize, iconSize);
				Rectangle textBox = new(row.X + textOffsetX, row.Y, Math.Max(0, row.Width - textOffsetX - 4), iconSize);

				DrawPlayerStat(sb, player, row, iconBox, textBox, listMode);
				drawnStats++;
			}

			foreach (PlayerManagerStat stat in Stats)
			{
				if (!settings.IsStatVisible(stat.Key))
					continue;

				if (drawnStats >= maxStats)
					break;

				Rectangle row;
				if (listMode)
				{
					int statIndex = drawnStats - (drawPlayerHead ? 1 : 0);
					int cellIndex = drawPlayerHead ? statIndex + 1 : statIndex;

					int columnIndex = rowsPerColumn > 0 ? cellIndex / rowsPerColumn : 0;
					int rowIndex = rowsPerColumn > 0 ? cellIndex % rowsPerColumn : 0;

					if (columnIndex >= columns)
						break;

					row = new(rect.X + columnIndex * (columnWidth + columnGap), rect.Y + rowIndex * rowHeight, columnWidth, rowHeight);
				}
				else
				{
					row = new(rect.X, rect.Y + drawnStats * rowHeight, totalRowWidth, rowHeight);
				}

				Rectangle iconBox = new(row.X, row.Y, iconSize, iconSize);
				Rectangle textBox = new(row.X + textOffsetX, row.Y, Math.Max(0, row.Width - textOffsetX - 4), iconSize);

#if DEBUG
				// // Debug draw "stat boxes", very useful!
				//sb.Draw(TextureAssets.MagicPixel.Value, iconBox, Color.OrangeRed * 0.2f);
				//sb.Draw(TextureAssets.MagicPixel.Value, textBox, Color.DarkRed * 0.2f);
#endif

				stat.Draw(sb, player, row, iconBox, textBox, listMode);
				drawnStats++;
			}
		}

		#region Stat drawing

		private static void DrawPlayerStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			const float textScale = 1f;
			const float extraXOffset = 10f;

			Color nameColor = PermissionHandler.LooksLikeAdmin(player) ? new Color(100, 235, 235) : Color.White;
			if (player.team > 0) 
				nameColor = Main.teamColor[player.team];

			if (player.dead)
			{
				sb.Draw(Assets.Filters.Dead.Value, iconBox, Color.White);
			}
			else
			{
				Player headPlayer = player.whoAmI < 0 ? Main.LocalPlayer : player;
				Color borderColor = player.team > 0 ? Main.teamColor[player.team] : Color.White;
				Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, headPlayer, iconBox.TopLeft() + new Vector2(10f, 10f), 1f, 0.8f, borderColor);
			}

			string text = player.name;
			if (!listMode)
				text = FitStatText(text, textBox.Width, textScale);

			Utils.DrawBorderString(sb, text, textBox.TopLeft() + new Vector2(extraXOffset, 0f), nameColor, textScale);
		}

		private static void DrawSessionTimeStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			string text = SessionTracker.GetSessionDuration(player.whoAmI);
			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.Time.Value, 0.85f, text, Color.White);
		}

		private static void DrawLifeStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.Heart.Value, 0.85f, $"{player.statLife}/{player.statLifeMax2}", Color.IndianRed, 0.85f, 1);
		}

		private static void DrawManaStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.Mana.Value, 0.85f, $"{player.statMana}/{player.statManaMax2}", Color.LightSkyBlue, 0.85f, 1);
		}

		private static void DrawDistanceStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			string text = $"{Math.Round(Vector2.Distance(Main.LocalPlayer.Center, player.Center) / 16f)} tiles";
			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.Distance.Value, 0.85f, text, Color.White);
		}

		private static void DrawPositionStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			string text = $"({(int)Math.Round(player.Center.X / 16f)}, {(int)Math.Round(player.Center.Y / 16f)})";
			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.Position.Value, 0.85f, text, Color.White);
		}

		private static void DrawBiomeStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			const float textScale = 0.85f;

			PlayerBiomeVisual biome = BiomeHelper.GetBiomeVisual(player);
			string biomeName = Language.GetTextValue(biome.BestiaryBiome.GetDisplayNameKey());

			// Special-case for shimmer.
			if (biome.BestiaryBiome == BiomeHelper.ShimmerBiome)
				biomeName = "Aether";

			if (TryGetBestiaryIconDrawData(biome.BestiaryBiome, out Asset<Texture2D> iconTexture, out Rectangle iconSource))
			{
				DrawTextureStat(sb, iconBox, textBox, listMode, iconTexture.Value, iconSource, 0.85f, biomeName, Color.White, textScale);
			}
			else
			{
				// Fallback to the Vanilla filter asset if a bestiary icon cannot be found
				DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Filters.Vanilla.Value, 0.85f, biomeName, Color.White, textScale);
			}
		}

		private static void DrawTeamStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			const float textScale = 0.85f;

			string[] teamNames = ["No Team", "Red Team", "Green Team", "Blue Team", "Yellow Team", "Pink Team"];
			string teamText = player.team >= 0 && player.team < teamNames.Length ? teamNames[player.team] : "Unknown";
			Color teamColor = player.team > 0 ? Main.teamColor[player.team] : Color.White;

			DrawTextureStat(sb, iconBox, textBox, listMode, TextureAssets.Pvp[1].Value, new Rectangle(player.team * 18, 0, 16, 16), 0.85f, teamText, teamColor, textScale);
		}

		private static void DrawHeldItemStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			const float textScale = 0.85f;

			Item item = player.HeldItem;
			if (item == null || item.IsAir)
			{
				string text = listMode ? "-" : "None";
				DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.HeldItem.Value, 0.85f, text, Color.Gray, textScale);
				return;
			}

			DrawItemStat(sb, iconBox, textBox, listMode, item, item.Name, Color.White, textScale);
		}

		private static void DrawDefenseStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.Defense.Value, 0.85f, $"{player.statDefense} defense", Color.Silver);
		}

		private static void DrawMovementSpeedStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			string movementSpeed = GetMovementSpeed(player);
			DrawTextureStat(sb, iconBox, textBox, listMode, TextureAssets.Item[ItemID.Stopwatch].Value, 0.85f, movementSpeed, Color.White);
		}

		private static void DrawPingStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			int pingMs = GetPlayerPingMs(player);
			string pingText = $"{pingMs} ms";
			Color pingColor = pingMs <= 80 ? Color.LightGreen : pingMs <= 150 ? Color.Gold : Color.IndianRed;

			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.Ping.Value, 0.85f, pingText, pingColor);
		}

		private static void DrawInventoryItemCountStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			int itemCount = CountInventoryItems(player);
			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.InventoryCount.Value, 0.85f, $"{itemCount} items", Color.White);
		}

		private static void DrawCoinCountStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			long copper = CountTotalCoins(player);
			string text = FormatTotalCoins(copper, out int coinItem);
			Color textColor = GetCoinColor(coinItem);

			DrawTextureStat(sb, iconBox, textBox, listMode, TextureAssets.Item[coinItem].Value, 0.85f, text, textColor);
		}

		private static void DrawAmmoCountStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			int ammoCount = CountAmmo(player);
			Item ammoIconItem = GetMostCommonAmmoItem(player);

			if (ammoIconItem == null || ammoIconItem.IsAir)
			{
				DrawTextureStat(sb, iconBox, textBox, listMode, TextureAssets.Item[ItemID.MusketBall].Value, 0.85f, $"{ammoCount} ammo", Color.White);
				return;
			}

			DrawItemStat(sb, iconBox, textBox, listMode, ammoIconItem, $"{ammoCount} ammo", Color.White);
		}

		private static void DrawNearbyEnemiesStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			int enemyCount = CountNearbyEnemies(player, 1200f);
			NPC closest = GetClosestEnemy(player, 1200f);
			string text = $"{enemyCount} nearby";
			Color textColor = enemyCount == 0 ? Color.LightGreen : enemyCount < 4 ? Color.Gold : Color.IndianRed;

			if (closest != null && DrawNpcHead(sb, closest, iconBox))
			{
				if (!listMode)
					text = FitStatText(text, textBox.Width, 0.85f);

				Utils.DrawBorderString(sb, text, textBox.TopLeft(), textColor, 0.85f);
				return;
			}

			DrawTextureStat(sb, iconBox, textBox, listMode, TextureAssets.Item[ItemID.LifeformAnalyzer].Value, 0.85f, text, textColor);
		}
		private static void DrawLastEnemyHitStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			string text = string.IsNullOrEmpty(modPlayer.LastEnemyHitName) ? "None" : modPlayer.LastEnemyHitName;

			if (modPlayer.LastEnemyHitNpcType > 0)
			{
				NPC liveNpc = FindFirstNpcOfType(modPlayer.LastEnemyHitNpcType);

				if (liveNpc != null && DrawNpcHead(sb, liveNpc, iconBox))
				{
					if (!listMode)
						text = FitStatText(text, textBox.Width, 0.85f);

					Utils.DrawBorderString(sb, text, textBox.TopLeft(), Color.White, 0.85f);
					return;
				}
			}

			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.PvE.Value, 0.85f, text, Color.White);
		}
		private static void DrawLastPlayerHitStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			string text = string.IsNullOrEmpty(modPlayer.LastPlayerHitName) ? "None" : modPlayer.LastPlayerHitName;

			if (DrawPlayerHead(sb, modPlayer.LastPlayerHitWhoAmI, iconBox))
			{
				if (!listMode)
					text = FitStatText(text, textBox.Width, 0.85f);

				Utils.DrawBorderString(sb, text, textBox.TopLeft(), Color.White, 0.85f);
				return;
			}

			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.PvP.Value, 0.85f, text, Color.White);
		}
		private static void DrawMinionCountStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			string text = $"{CountPlayerMinions(player)}/{player.maxMinions} minions";

			if (modPlayer.LastMinionProjectileType > 0)
			{
				Texture2D texture = TextureAssets.Projectile[modPlayer.LastMinionProjectileType].Value;
				int frameCount = Main.projFrames[modPlayer.LastMinionProjectileType];
				Rectangle source = texture.Frame(1, Math.Max(1, frameCount), 0, 0);
				DrawTextureStat(sb, iconBox, textBox, listMode, texture, source, 0.85f, text, Color.White);
				return;
			}

			Projectile minion = GetAnyActiveMinion(player);
			if (minion != null)
			{
				Texture2D texture = TextureAssets.Projectile[minion.type].Value;
				Rectangle source = texture.Frame(1, Main.projFrames[minion.type], 0, minion.frame);
				DrawTextureStat(sb, iconBox, textBox, listMode, texture, source, 0.85f, text, Color.White);
				return;
			}

			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.MinionCount.Value, 0.85f, text, Color.White);
		}

		private static void DrawDeathCountStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			string text = $"{player.numberOfDeathsPVP} / {player.numberOfDeathsPVE}";

			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Filters.Dead.Value, 0.75f, text, Color.White);
		}

		private static void DrawBossDamageStat(SpriteBatch sb, Player player, Rectangle row, Rectangle iconBox, Rectangle textBox, bool listMode)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			string text = $"{FormatBossDamage(modPlayer.TotalBossDamage)} boss dmg";

			if (modPlayer.LastBossHitNpcType > 0)
			{
				NPC boss = FindFirstNpcOfType(modPlayer.LastBossHitNpcType);

				if (boss != null && DrawNpcHead(sb, boss, iconBox))
				{
					if (!listMode)
						text = FitStatText(text, textBox.Width, 0.85f);

					Utils.DrawBorderString(sb, text, textBox.TopLeft(), Color.Orange, 0.85f);
					return;
				}
			}

			DrawTextureStat(sb, iconBox, textBox, listMode, Assets.Stats.BossDamage.Value, 0.85f, text, Color.Orange);
		}

		#endregion

		#region Draw Helpers
		/// <summary>
		/// Used to draw stats in perfect icon and text boxes perfectly centered! :)
		/// </summary>
		internal sealed class PlayerManagerStat
		{
			public string Key;
			public Action<SpriteBatch, Player, Rectangle, Rectangle, Rectangle, bool> Draw;
			public Func<Player, string> GetTooltipText;

			public PlayerManagerStat(string key, Action<SpriteBatch, Player, Rectangle, Rectangle, Rectangle, bool> draw, Func<Player, string> getTooltipText)
			{
				Key = key;
				Draw = draw;
				GetTooltipText = getTooltipText;
			}
		}

		private static void DrawItemStat(SpriteBatch sb, Rectangle iconBox, Rectangle textBox, bool listMode, Item item, string text, Color textColor, float textScale = 0.85f, float iconScale = 1f, float itemSlotSize = 18f)
		{
			if (item == null || item.IsAir)
				return;

			Rectangle frame = Main.itemAnimations[item.type]?.GetFrame(TextureAssets.Item[item.type].Value) ?? TextureAssets.Item[item.type].Value.Frame();
			Color color = Color.White;

			Terraria.UI.ItemSlot.DrawItem_GetColorAndScale(item, iconScale, ref color, itemSlotSize, ref frame, out _, out _);
			Terraria.UI.ItemSlot.DrawItemIcon(item, 31, sb, iconBox.Center.ToVector2(), iconScale, itemSlotSize, Color.White);

			if (!listMode)
				text = FitStatText(text, textBox.Width, textScale);

			Utils.DrawBorderString(sb, text, textBox.TopLeft(), textColor, textScale);
		}

		private static void DrawTextureStat(SpriteBatch sb, Rectangle iconBox, Rectangle textBox, bool listMode, Texture2D texture, Rectangle source, float iconScale, string text, Color textColor, float textScale = 0.85f, int extraXOffset = 0)
		{
			Vector2 size = source.Size() * iconScale;
			Rectangle drawBox = new((int)(iconBox.Center.X - size.X * 0.5f + extraXOffset), (int)(iconBox.Center.Y - size.Y * 0.5f), (int)size.X, (int)size.Y);
			sb.Draw(texture, drawBox, source, Color.White);

			if (!listMode)
				text = FitStatText(text, textBox.Width, textScale);

			Utils.DrawBorderString(sb, text, textBox.TopLeft(), textColor, textScale);
		}

		private static void DrawTextureStat(SpriteBatch sb, Rectangle iconBox, Rectangle textBox, bool listMode, Texture2D texture, float iconScale, string text, Color textColor, float textScale = 0.85f, int extraXOffset = 0)
		{
			DrawTextureStat(sb, iconBox, textBox, listMode, texture, texture.Frame(), iconScale, text, textColor, textScale, extraXOffset);
		}

		private static bool DrawNpcHead(SpriteBatch sb, NPC npc, Rectangle iconBox)
		{
			if (npc == null || npc.type <= NPCID.None)
				return false;

			int headSlot = npc.GetBossHeadTextureIndex();

			if (headSlot >= 0 && headSlot < TextureAssets.NpcHeadBoss.Length && TextureAssets.NpcHeadBoss[headSlot]?.IsLoaded == true)
			{
				sb.Draw(TextureAssets.NpcHeadBoss[headSlot].Value, iconBox, Color.White);
				return true;
			}

			headSlot = NPC.TypeToDefaultHeadIndex(npc.type);

			if (headSlot >= 0 && headSlot < TextureAssets.NpcHead.Length && TextureAssets.NpcHead[headSlot]?.IsLoaded == true)
			{
				sb.Draw(TextureAssets.NpcHead[headSlot].Value, iconBox, Color.White);
				return true;
			}

			return false;
		}

		private static bool DrawPlayerHead(SpriteBatch sb, int whoAmI, Rectangle iconBox)
		{
			if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
				return false;

			Player target = Main.player[whoAmI];
			if (target == null || !target.active)
				return false;

			Color borderColor = target.team > 0 ? Main.teamColor[target.team] : Color.White;
			Main.MapPlayerRenderer.DrawPlayerHead(Main.Camera, target, iconBox.TopLeft() + new Vector2(10f, 10f), 1f, 0.8f, borderColor);
			return true;
		}

		#endregion

		#region Utility helpers
		internal static string BuildTooltip(PlayerManagerSettings settings, Player player, Rectangle rect, bool listMode)
		{
			bool drawPlayerHead = settings.IsPlayerMode("PlayerHead");
			int columns = listMode ? settings.GetListStatColumnCount() : 1;
			int maxStats = GetMaxStats(rect.Height, listMode, columns);

			const int minHeightForPlayerName = 102;
			bool reserveTopRowForPlayerName = settings.IsPlayerMode("PlayerFull") && !listMode && rect.Height >= minHeightForPlayerName;
			if (reserveTopRowForPlayerName)
				maxStats = Math.Max(0, maxStats - 1);

			int remainingStats = drawPlayerHead ? maxStats - 1 : maxStats;
			if (remainingStats <= 0)
				return "";

			List<string> lines = [];

			foreach (PlayerManagerStat stat in GetVisibleStats(settings, remainingStats))
			{
				string value = stat.GetTooltipText(player);
				if (string.IsNullOrWhiteSpace(value))
					continue;

				string label = LocalizationHelper.GetToolText($"PlayerManager.Filters.{stat.Key}.Name");
				lines.Add($"{label}: {value}");
			}

			return string.Join("\n", lines);
		}

		private static IEnumerable<PlayerManagerStat> GetVisibleStats(PlayerManagerSettings settings, int maxStats)
		{
			int drawn = 0;

			foreach (PlayerManagerStat stat in Stats)
			{
				if (!settings.IsStatVisible(stat.Key))
					continue;

				if (drawn >= maxStats)
					yield break;

				yield return stat;
				drawn++;
			}
		}

		private static int GetMaxStats(int availableHeight, bool listMode, int columns)
		{
			if (listMode)
			{
				int rowsPerColumn =
					availableHeight >= 48 + 46 ? 4 :
					availableHeight >= 48 + 18 ? 3 :
					availableHeight >= 48 ? 2 :
					0;

				return rowsPerColumn * columns;
			}

			return
				availableHeight >= 48 + 46 ? 4 :
				availableHeight >= 48 + 18 ? 3 :
				availableHeight >= 48 ? 2 :
				availableHeight >= 40 ? 0 :
				0;
		}
		
		private static string GetBiomeText(Player player)
		{
			PlayerBiomeVisual biome = BiomeHelper.GetBiomeVisual(player);
			string biomeName = Language.GetTextValue(biome.BestiaryBiome.GetDisplayNameKey());
			return biome.BestiaryBiome == BiomeHelper.ShimmerBiome ? "Aether" : biomeName;
		}

		private static string GetTeamText(Player player)
		{
			string[] teamNames = ["No Team", "Red Team", "Green Team", "Blue Team", "Yellow Team", "Pink Team"];
			return player.team >= 0 && player.team < teamNames.Length ? teamNames[player.team] : "Unknown";
		}

		private static string GetHeldItemText(Player player)
		{
			Item item = player.HeldItem;
			if (item == null || item.IsAir)
				return "-";

			return item.Name;
		}

		/// <summary>
		/// Used to reduce stat text content to fit inside grid view.
		/// </summary>
		public static string FitStatText(string text, float maxWidth, float scale)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			DynamicSpriteFont font = FontAssets.MouseText.Value;

			if (font.MeasureString(text).X * scale <= maxWidth)
				return text;

			const string suffix = "..";
			float suffixWidth = font.MeasureString(suffix).X * scale;
			int length = text.Length;

			while (length > 0)
			{
				string candidate = text[..length];
				float width = font.MeasureString(candidate).X * scale + suffixWidth;

				if (width <= maxWidth)
					return candidate + suffix;

				length--;
			}
			//Main.NewText(length);
			if (length <= 1)
				return "";

			return suffix;
		}

		private static string FormatBossDamage(long damage)
		{
			if (damage < 1000)
				return damage.ToString();

			if (damage % 1000 == 0)
				return $"{damage / 1000}k";

			return $"{Math.Round(damage / 1000f, 1)}k";
		}
		private static Item GetMostCommonAmmoItem(Player player)
		{
			Item bestItem = null;
			int bestStack = 0;

			for (int i = 54; i <= 57 && i < player.inventory.Length; i++)
			{
				Item item = player.inventory[i];
				if (item == null || item.IsAir || item.stack <= 0)
					continue;

				if (item.stack > bestStack)
				{
					bestStack = item.stack;
					bestItem = item;
				}
			}

			return bestItem;
		}
		private static Projectile GetAnyActiveMinion(Player player)
		{
			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (!projectile.active || projectile.owner != player.whoAmI || !projectile.minion)
					continue;

				return projectile;
			}

			return null;
		}
		private static int CountInventoryItems(Player player)
		{
			int count = 0;

			for (int i = 0; i < 50 && i < player.inventory.Length; i++)
			{
				Item item = player.inventory[i];
				if (item != null && !item.IsAir)
					count++;
			}

			return count;
		}

		private static long CountTotalCoins(Player player)
		{
			long copper = 0;

			for (int i = 50; i <= 53 && i < player.inventory.Length; i++)
			{
				Item item = player.inventory[i];
				if (item == null || item.IsAir || item.stack <= 0)
					continue;

				if (item.type == ItemID.CopperCoin)
					copper += item.stack;
				else if (item.type == ItemID.SilverCoin)
					copper += item.stack * 100L;
				else if (item.type == ItemID.GoldCoin)
					copper += item.stack * 10000L;
				else if (item.type == ItemID.PlatinumCoin)
					copper += item.stack * 1000000L;
			}

			return copper;
		}
		private static Color GetCoinColor(int coinItem)
		{
			return coinItem switch
			{
				ItemID.PlatinumCoin => new Color(220, 220, 255),
				ItemID.GoldCoin => new Color(255, 220, 80),
				ItemID.SilverCoin => new Color(210, 210, 210),
				_ => new Color(184, 115, 51),
			};
		}

		private static string FormatTotalCoins(long copper, out int coinItem)
		{
			string text = "";
			coinItem = ItemID.CopperCoin;

			if (copper >= 1000000)
			{
				coinItem = ItemID.PlatinumCoin;
				text = $"{copper / 1000000} plat";
			}
			else if (copper >= 10000)
			{
				coinItem = ItemID.GoldCoin;
				text = $"{copper / 10000} gold";
			}
			else if (copper >= 100)
			{
				coinItem = ItemID.SilverCoin;
				text = $"{copper / 100} silver";
			}
			else
				text = $"{copper} copper";

			return text;
		}

		private static int CountAmmo(Player player)
		{
			int count = 0;

			for (int i = 54; i <= 57 && i < player.inventory.Length; i++)
			{
				Item item = player.inventory[i];
				if (item != null && !item.IsAir)
					count += item.stack;
			}

			return count;
		}

		private static int CountNearbyEnemies(Player player, float maxDistance)
		{
			int count = 0;
			float maxDistanceSq = maxDistance * maxDistance;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
					continue;

				if (Vector2.DistanceSquared(player.Center, npc.Center) <= maxDistanceSq)
					count++;
			}

			return count;
		}
		private static string GetLastEnemyHitText(Player player)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			return string.IsNullOrEmpty(modPlayer.LastEnemyHitName) ? "None" : modPlayer.LastEnemyHitName;
		}

		private static string GetLastPlayerHitText(Player player)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			return string.IsNullOrEmpty(modPlayer.LastPlayerHitName) ? "None" : modPlayer.LastPlayerHitName;
		}

		private static string GetBossDamageText(Player player)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			return $"{FormatBossDamage(modPlayer.TotalBossDamage)} boss dmg";
		}

		private static int CountPlayerMinions(Player player)
		{
			int count = 0;

			foreach (Projectile projectile in Main.ActiveProjectiles)
			{
				if (!projectile.active || projectile.owner != player.whoAmI)
					continue;

				if (projectile.minion)
					count++;
			}

			return count;
		}
		private static string GetLastEnemyHitName(Player player)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			return modPlayer.LastEnemyHitName;
		}

		// Taken from Main.DrawInfoAccs(), if (info == InfoDisplay.Stopwatch)...
		private static string GetMovementSpeed(Player player) 
		{
			Vector2 vector = player.velocity + player.instantMovementAccumulatedThisFrame;
			if (Main.LocalPlayer.mount.Active && Main.player[Main.myPlayer].mount.IsConsideredASlimeMount && player.velocity.Y != 0f && !player.SlimeDontHyperJump)
			{
				vector.Y += Main.player[Main.myPlayer].velocity.Y;
			}
			int num15 = (int)(1f + vector.Length() * 6f);
			if (num15 > Main.player[Main.myPlayer].speedSlice.Length)
			{
				num15 = Main.player[Main.myPlayer].speedSlice.Length;
			}
			float num16 = 0f;
			for (int num17 = num15 - 1; num17 > 0; num17--)
			{
				Main.player[Main.myPlayer].speedSlice[num17] = Main.player[Main.myPlayer].speedSlice[num17 - 1];
			}
			Main.player[Main.myPlayer].speedSlice[0] = vector.Length();
			for (int m = 0; m < Main.player[Main.myPlayer].speedSlice.Length; m++)
			{
				if (m < num15)
				{
					num16 += Main.player[Main.myPlayer].speedSlice[m];
				}
				else
				{
					Main.player[Main.myPlayer].speedSlice[m] = num16 / (float)num15;
				}
			}
			num16 /= (float)num15;
			int num18 = 42240;
			int num19 = 216000;
			float num20 = num16 * (float)num19 / (float)num18;
			if (!Main.player[Main.myPlayer].merman && !Main.player[Main.myPlayer].ignoreWater)
			{
				if (Main.player[Main.myPlayer].honeyWet)
				{
					num20 /= 4f;
				}
				else if (Main.player[Main.myPlayer].wet)
				{
					num20 /= 2f;
				}
			}
			string text2 = Language.GetTextValue("GameUI.Speed", Math.Round(num20));
			return text2;
		}

		private static int GetPlayerPingMs(Player player)
		{
			return PingTracker.GetPing(player.whoAmI);
		}

		private static NPC FindFirstNpcOfType(int npcType)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.type == npcType)
					return npc;
			}

			return null;
		}
		private static NPC GetClosestEnemy(Player player, float range)
		{
			NPC best = null;
			float bestDistSq = range * range;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (!npc.CanBeChasedBy(player))
					continue;

				float distSq = Vector2.DistanceSquared(player.Center, npc.Center);
				if (distSq >= bestDistSq)
					continue;

				best = npc;
				bestDistSq = distSq;
			}

			return best;
		}
		#endregion
	}
}