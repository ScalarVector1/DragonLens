using DragonLens.Content.Filters;
using DragonLens.Content.Filters.PlayerManagerFilters;
using DragonLens.Content.Filters.PlayerManagerFilters.Toggles;
using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Developer;
using DragonLens.Content.Tools.Gameplay;
using DragonLens.Content.Tools.Multiplayer.Drawers;
using DragonLens.Content.Tools.Multiplayer.Trackers;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using static AssGen.Assets;
using Biomes = Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes; // keep this, it's important for filter setup

namespace DragonLens.Content.Tools.Multiplayer
{
	internal sealed class PlayerManager : BrowserTool<PlayerManagerBrowser>
	{
		public override string IconKey => "PlayerManager";

		public override void OnActivate()
		{
			base.OnActivate();

			PlayerManagerBrowser state = UILoader.GetUIState<PlayerManagerBrowser>();

			if (state.visible)
			{
#if DEBUG
				state.Refresh(); // debug, rebuild entire state
#endif
				state.RefreshEntries();
			}
		}
	}

	internal sealed class PlayerManagerBrowser : Browser
	{
		public override string Name => LocalizationHelper.GetText("Tools.PlayerManager.DisplayName");
		public override string IconTexture => "PlayerManager";
		public override Vector2 DefaultPosition => new(0.1f, 0.3f);
		public override string HelpLink => "";
		public override int MinButtonSize => 40;
		public override int BrowserWidth => 520;
		//private ReloadButton reloadButton; // Keep this in case we re-add reload button.
		public PlayerManagerSettings Settings { get; } = new();

		public PlayerManagerBrowser()
		{
			buttonSize = 48;
			listMode = true;
		}

		public override void PostInitialize()
		{
			base.PostInitialize();

			// Don't add reload button, its kinda redundant... Keep this comment though.
			//reloadButton = new(this);
			//Append(reloadButton);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			base.AdjustPositions(newPos);

			// Keep this in case we re-add reload button.
			//if (reloadButton is not null)
			//{
			//	reloadButton.Left.Set(newPos.X - 50f, 0f);
			//	reloadButton.Top.Set(newPos.Y, 0f);
			//	reloadButton.Width.Set(42f, 0f);
			//	reloadButton.Height.Set(42f, 0f);
			//}
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<PlayerManagerItem>();
			List<Player> players = [.. Main.ActivePlayers, .. ModContent.GetInstance<PlayerManagerSystem>().fakePlayers];

			foreach (Player player in players)
			{
				if (player is null || !player.active)
					continue;

				buttons.Add(new PlayerManagerItem(player, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("Alphabetical",
				(a, b) => string.Compare(a.Identifier, b.Identifier, StringComparison.OrdinalIgnoreCase)));

			SortModes.Add(new("Teams",
				(a, b) =>
				{
					PlayerManagerItem left = (PlayerManagerItem)a;
					PlayerManagerItem right = (PlayerManagerItem)b;

					int teamCompare = left.player.team.CompareTo(right.player.team);
					if (teamCompare != 0)
						return teamCompare;

					return string.Compare(left.Identifier, right.Identifier, StringComparison.OrdinalIgnoreCase);
				}));

			//SortModes.Add(new("Life",
			//	(a, b) =>
			//	{
			//		PlayerManagerItem left = (PlayerManagerItem)a;
			//		PlayerManagerItem right = (PlayerManagerItem)b;

			//		int lifeCompare = right.player.statLife.CompareTo(left.player.statLife);
			//		if (lifeCompare != 0)
			//			return lifeCompare;

			//		return string.Compare(left.Identifier, right.Identifier, StringComparison.OrdinalIgnoreCase);
			//	}));

			//SortModes.Add(new("SessionTime",
			//	(a, b) =>
			//	{
			//		PlayerManagerItem left = (PlayerManagerItem)a;
			//		PlayerManagerItem right = (PlayerManagerItem)b;

			//		long leftTicks = SessionTracker.GetSessionDurationTicks(left.player.whoAmI);
			//		long rightTicks = SessionTracker.GetSessionDurationTicks(right.player.whoAmI);

			//		int sessionCompare = rightTicks.CompareTo(leftTicks);
			//		if (sessionCompare != 0)
			//			return sessionCompare;

			//		return string.Compare(left.Identifier, right.Identifier, StringComparison.OrdinalIgnoreCase);
			//	}));

			SortFunction = SortModes[0].Function;
		}

		public override void SetupFilters(FilterPanel filters)
		{
			Asset<Texture2D> teamIcons = TextureAssets.Pvp[1];

			// Filters
			filters.AddSeperator("Tools.PlayerManager.FilterCategories.Filter");
			filters.AddFilter(new Filter(Assets.GUI.AdminIcon, "Tools.PlayerManager.Filters.Admin", n => n is PlayerManagerItem pb && !PermissionHandler.LooksLikeAdmin(pb.player)));
			filters.AddFilter(new Filter(Assets.GUI.Frozen, "Tools.PlayerManager.Filters.Frozen", n => n is PlayerManagerItem pb && !ModContent.GetInstance<PlayerManagerSystem>().frozenPlayers.Contains(pb.player.whoAmI)));
			filters.AddFilter(new Filter(Assets.Filters.Dead, "Tools.PlayerManager.Filters.Dead", n => n is PlayerManagerItem pb && !pb.player.dead));
			filters.AddFilter(new Filter(Assets.Filters.HealthLow, "Tools.PlayerManager.Filters.LowHealth", n => n is PlayerManagerItem pb && (pb.player.dead || pb.player.statLife > pb.player.statLifeMax2 * 0.5f)));
			filters.AddFilter(new Filter(Assets.Filters.HealthFull, "Tools.PlayerManager.Filters.FullHealth", n => n is PlayerManagerItem pb && (pb.player.dead || pb.player.statLife < pb.player.statLifeMax2)));

			// Team filters
			filters.AddSeperator("Tools.PlayerManager.FilterCategories.Team");
			//filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.NoTeam",n => n is PlayerManagerItem pb && pb.player.team != 0,new Rectangle(0, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Red", n => n is PlayerManagerItem pb && pb.player.team != 1, new Rectangle(18, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Green", n => n is PlayerManagerItem pb && pb.player.team != 2, new Rectangle(36, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Blue", n => n is PlayerManagerItem pb && pb.player.team != 3, new Rectangle(54, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Yellow", n => n is PlayerManagerItem pb && pb.player.team != 4, new Rectangle(72, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Pink", n => n is PlayerManagerItem pb && pb.player.team != 5, new Rectangle(90, 0, 16, 16), new Point(20, 20)));

			// Biome filters
			filters.AddSeperator("Tools.PlayerManager.FilterCategories.Biome");
			SpawnConditionBestiaryInfoElement[] biomesToFilter =
			[
				Biomes.Surface,
				Biomes.Underground,
				Biomes.Caverns,
				Biomes.Sky,
				Biomes.TheUnderworld,
				Biomes.Graveyard,
				Biomes.Granite,
				Biomes.Marble,
				Biomes.UndergroundMushroom,
				Biomes.SpiderNest,
				Biomes.Snow,
				Biomes.UndergroundSnow,
				Biomes.Desert,
				Biomes.UndergroundDesert,
				Biomes.Ocean,
				Biomes.Jungle,
				Biomes.UndergroundJungle,
				Biomes.Meteor,
				Biomes.TheCorruption,
				Biomes.UndergroundCorruption,
				Biomes.CorruptIce,
				Biomes.CorruptDesert,
				Biomes.CorruptUndergroundDesert,
				Biomes.TheCrimson,
				Biomes.UndergroundCrimson,
				Biomes.CrimsonIce,
				Biomes.CrimsonDesert,
				Biomes.CrimsonUndergroundDesert,
				Biomes.TheHallow,
				Biomes.UndergroundHallow,
				Biomes.HallowIce,
				Biomes.HallowDesert,
				Biomes.HallowUndergroundDesert,
				Biomes.SurfaceMushroom,
				Biomes.TheTemple,
				Biomes.TheDungeon,
				Biomes.NebulaPillar,
				Biomes.SolarPillar,
				Biomes.VortexPillar,
				Biomes.StardustPillar
			];

			foreach (SpawnConditionBestiaryInfoElement biome in biomesToFilter)
				filters.AddFilter(new BiomeFilter(biome));

			// Special case for shimmer
			filters.AddFilter(new BiomeFilter(BiomeHelper.ShimmerBiome));

			// Button toggles
			filters.AddSeperator(LocalizationHelper.GetToolText("PlayerManager.FilterCategories.ButtonOptions"));
			filters.AddFilter(new ButtonOptionFilter(this, "Admin", Assets.GUI.AdminIcon));
			filters.AddFilter(new ButtonOptionFilter(this, "Kick", Assets.GUI.KickIcon));
			filters.AddFilter(new ButtonOptionFilter(this, "View", Assets.GUI.StalkIcon));
			filters.AddFilter(new ButtonOptionFilter(this, "Inventory", Assets.GUI.InventoryIcon));
			filters.AddFilter(new ButtonOptionFilter(this, "BringHere", Assets.GUI.BringHere));
			filters.AddFilter(new ButtonOptionFilter(this, "GoTo", Assets.GUI.GoTo));
			filters.AddFilter(new ButtonOptionFilter(this, "Frozen", Assets.GUI.Frozen));

			// Stat toggles
			filters.AddSeperator(LocalizationHelper.GetToolText("PlayerManager.FilterCategories.StatOptions"));
			filters.AddFilter(new StatOptionFilter(this, "Life", Assets.Stats.Heart));
			filters.AddFilter(new StatOptionFilter(this, "Mana", Assets.Stats.Mana));
			filters.AddFilter(new StatOptionFilter(this, "Defense", Assets.Stats.Defense));
			filters.AddFilter(new StatOptionFilter(this, "HeldItem", Assets.Stats.HeldItem));
			filters.AddFilter(new StatOptionFilter(this, "BiomeName", Assets.Filters.Vanilla));
			filters.AddFilter(new StatOptionFilter(this, "Position", Assets.Stats.Position));
			filters.AddFilter(new StatOptionFilter(this, "Team", Assets.Stats.TeamWhite));
			filters.AddFilter(new StatOptionFilter(this, "MovementSpeed", Assets.Stats.Stopwatch));
			filters.AddFilter(new StatOptionFilter(this, "Distance", Assets.Stats.Distance));
			filters.AddFilter(new StatOptionFilter(this, "SessionTime", Assets.Stats.Time));
			filters.AddFilter(new StatOptionFilter(this, "Ping", Assets.Stats.Ping));
			filters.AddFilter(new StatOptionFilter(this, "InventoryItemCount", Assets.Stats.InventoryCount));
			filters.AddFilter(new StatOptionFilter(this, "CoinCount", Assets.Stats.Coin));
			filters.AddFilter(new StatOptionFilter(this, "AmmoCount", Assets.Stats.Ammo));
			filters.AddFilter(new StatOptionFilter(this, "MinionCount", Assets.Stats.MinionCount));
			filters.AddFilter(new StatOptionFilter(this, "NearbyEnemies", Assets.Stats.NearbyEnemies));
			filters.AddFilter(new StatOptionFilter(this, "LastEnemyHit", Assets.Stats.PvE));
			filters.AddFilter(new StatOptionFilter(this, "LastPlayerHit", Assets.Stats.PvP));
			filters.AddFilter(new StatOptionFilter(this, "DeathCount", Assets.Filters.Dead));
			filters.AddFilter(new StatOptionFilter(this, "BossDamage", Assets.Stats.BossDamage));

			// Background toggles
			filters.AddSeperator(LocalizationHelper.GetToolText("PlayerManager.FilterCategories.BackgroundOptions"));
			filters.AddFilter(new BackgroundOptionFilter(this, "TeamColorBackground", Assets.Stats.WhiteBackground));
			filters.AddFilter(new BackgroundOptionFilter(this, "BiomeBackground", Assets.Stats.BiomeBackground));

			// Player options
			filters.AddSeperator(LocalizationHelper.GetToolText("PlayerManager.FilterCategories.PlayerOptions"));
			filters.AddFilter(new PlayerOptionFilter(this, "PlayerHead", Assets.Stats.PlayerHead));
			filters.AddFilter(new PlayerOptionFilter(this, "PlayerFull", Assets.Stats.PlayerFull));
		}

		public void RefreshEntries() 
		{ 
			options.Clear(); 
			PopulateGrid(options); 
			SortGrid(); 
			Recalculate(); 
		}
	}

	internal class PlayerManagerItem : BrowserButton
	{
		public Player player;

		private readonly Dictionary<string, PlayerManagerActionButton> actionButtons = [];

		private bool IsFake => player.whoAmI < 0;
		private PlayerManagerSystem PlayerManager => ModContent.GetInstance<PlayerManagerSystem>();
		private PlayerManagerBrowser PlayerBrowser => (PlayerManagerBrowser)parent;
		public PlayerManagerItem(Player player, PlayerManagerBrowser parent) : base(parent)
		{
			this.player = player;
			CreateButtons();
		}

		protected override int ListHeight => Math.Max(48, GridSize);
		protected override int ListWidth => (int)Parent.GetDimensions().Width - 12;

		public override string Identifier => player.active ? player.name : $"Player {player.whoAmI}";
		public override string Key => $"Player:{player.whoAmI}";

		private void CreateButtons()
		{
			AddActionButton("Frozen", new("Freeze", Assets.GUI.Frozen, FreezePlayer, () => PlayerManager.frozenPlayers.Contains(player.whoAmI)));
			AddActionButton("GoTo", new("GoTo", Assets.GUI.GoTo, TeleportToPlayer));
			AddActionButton("BringHere", new("BringHere", Assets.GUI.BringHere, TeleportToMe));
			AddActionButton("Inventory", new("Inventory", Assets.GUI.InventoryIcon, OpenInventory, () =>
			{
				InventoryManagerWindow window = UILoader.GetUIState<InventoryManagerWindow>();
				return window is not null && window.Visible && window.player == player;
			}));
			AddActionButton("View", new("View", Assets.GUI.StalkIcon, Stalk, () => PlayerManager.stalkedPlayer == player));
			AddActionButton("Kick", new("Kick", Assets.GUI.KickIcon, Kick));
			AddActionButton("Admin", new("Admin", Assets.GUI.AdminIcon, ToggleAdmin, () => !IsFake && PermissionHandler.LooksLikeAdmin(player)));
		}

		private void AddActionButton(string key, PlayerManagerActionButton button)
		{
			actionButtons[key] = button;
			Append(button);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!parent.listMode)
				Stalk();
		}

		public override void SafeRightMouseDown(UIMouseEvent evt)
		{
			if (!parent.listMode)
				FreezePlayer();
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			UpdateActionButtonLayout();
		}

		public override void Recalculate()
		{
			base.Recalculate();
			UpdateActionButtonLayout(recalculateButtons: true);
		}

		private void UpdateActionButtonLayout(bool recalculateButtons = false)
		{
			if (filtered || !parent.listMode)
			{
				HideAllButtons(recalculateButtons);
				return;
			}

			int width = (int)GetDimensions().Width;
			int height = (int)GetDimensions().Height;
			int buttonSize = Math.Min(height - 8, 40);
			int buttonTop = (height - buttonSize) / 2;
			int right = width - 8;

			foreach ((string key, PlayerManagerActionButton button) in actionButtons)
			{
				if (!PlayerBrowser.Settings.IsButtonVisible(key))
				{
					HideButton(button);
					if (recalculateButtons)
						button.Recalculate();

					continue;
				}

				right -= buttonSize;
				button.Left.Set(right, 0f);
				button.Top.Set(buttonTop, 0f);
				button.Width.Set(buttonSize, 0f);
				button.Height.Set(buttonSize, 0f);
				right -= 4;

				if (recalculateButtons)
					button.Recalculate();
			}
		}

		private void HideAllButtons(bool recalculateButtons = false)
		{
			foreach (PlayerManagerActionButton button in actionButtons.Values)
			{
				HideButton(button);

				if (recalculateButtons)
					button.Recalculate();
			}
		}

		private static void HideButton(PlayerManagerActionButton button)
		{
			button.Left.Set(0f, 0f);
			button.Top.Set(0f, 0f);
			button.Width.Set(0f, 0f);
			button.Height.Set(0f, 0f);
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			var bounds = GetDimensions().ToRectangle();

			// Draw empty box first in order to draw custom UI on top.
			if (parent.listMode)
				GUIHelper.DrawBox(spriteBatch, bounds, ThemeHandler.BackgroundColor);

			// Draw background.
			if (PlayerBrowser.Settings.IsBackgroundMode("BiomeBackground"))
				PlayerBackgroundDrawer.DrawMapFullscreenBackground(spriteBatch, bounds, player, parent.listMode);
			else if (PlayerBrowser.Settings.IsBackgroundMode("TeamColorBackground"))
				PlayerBackgroundDrawer.DrawTeamColorBackground(spriteBatch, bounds, player);

			// Draw player full.
			if (PlayerBrowser.Settings.IsPlayerMode("PlayerFull"))
				PlayerBackgroundDrawer.DrawPlayerFull(spriteBatch, bounds, player, parent.listMode);

			if (parent.listMode)
				DrawListMode(spriteBatch);
			else
				DrawGridMode(spriteBatch, iconBox);

			// Draw tooltip
			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;

				Tooltip.SetName(player.name);
				Tooltip.SetTooltip(PlayerStatDrawer.BuildTooltip(PlayerBrowser.Settings, player, bounds, true));
			}
		}

		private void DrawGridMode(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			// If player disconnects... we need this check for now.
			if (!player.active)
				return;

			// Draw all player stats.
			PlayerStatDrawer.DrawStats(spriteBatch, PlayerBrowser.Settings, player, iconBox, false);

			// Draw white outline if we're spectating this player.
			if (PlayerManager.stalkedPlayer == player)
				GUIHelper.DrawOutline(spriteBatch, iconBox, ThemeHandler.ButtonColor.InvertColor());

			// Draw white outline if this player is frozen.
			if (PlayerManager.frozenPlayers.Contains(player.whoAmI))
				GUIHelper.DrawOutline(spriteBatch, iconBox, Color.White);

			// Draw tooltip for grid mode.
			//if (IsMouseHovering && CanShowTooltip)
			//{
			//	Main.LocalPlayer.mouseInterface = true;
			//	Tooltip.SetName(player.name);

			//	string leftClickText = PlayerManager.stalkedPlayer == player ? "Click to stop spectating" : "Click to spectate";
			//	string rightClickText = PlayerManager.frozenPlayers.Contains(player.whoAmI) ? "Right click to unfreeze player" : "Right click to freeze player";

			//	Tooltip.SetTooltip($"{leftClickText}\n{rightClickText}");
			//}
		}

		private void DrawListMode(SpriteBatch spriteBatch)
		{
			var bounds = GetDimensions().ToRectangle();

			// Draw all player stats.
			PlayerStatDrawer.DrawStats(spriteBatch, PlayerBrowser.Settings, player, bounds, true);
		}

		#region Actions for buttons
		public void FreezePlayer()
		{
			if (IsFake)
				return;

			PlayerManagerNetHandler.SendFrozenPlayer(player.whoAmI);
		}

		public void TeleportToMe()
		{
			if (IsFake)
				return;

			PlayerManagerNetHandler.SendTeleportToMe(player.whoAmI);
		}

		public void TeleportToPlayer()
		{
			if (IsFake)
				return;

			Player localPlayer = Main.LocalPlayer;

			if (localPlayer == null || !player.active)
				return;

			if (!PermissionHandler.CanUseTools(localPlayer))
				return;

			Vector2 telePos = player.Center - new Vector2(localPlayer.width, localPlayer.height) * 0.5f;

			if (Main.netMode == NetmodeID.SinglePlayer)
				localPlayer.Teleport(telePos, TeleportationStyleID.RodOfDiscord);
			else if (Main.netMode == NetmodeID.MultiplayerClient)
				NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 2, Main.LocalPlayer.whoAmI, telePos.X, telePos.Y, TeleportationStyleID.PotionOfReturn);
		}

		public void ToggleAdmin()
		{
			if (IsFake)
				return;

			if (!PermissionHandler.CanUseTools(Main.LocalPlayer))
				return;

			if (player == Main.LocalPlayer)
			{
				Main.NewText(LocalizationHelper.GetToolText("PlayerManager.TryRemoveYourself"), Color.Red);
				return;
			}

			if (PermissionHandler.LooksLikeAdmin(player))
			{
				PermissionHandler.RemoveAdmin(player);
				Main.NewText(LocalizationHelper.GetToolText("PlayerManager.RemoveAdmin", player.name), Color.Yellow);
			}
			else
			{
				PermissionHandler.AddAdmin(player);
				Main.NewText(LocalizationHelper.GetToolText("PlayerManager.AddAdmin", player.name), Color.Yellow);
			}
		}

		public void Kick()
		{
			if (IsFake)
				return;

			PlayerManagerNetHandler.SendKick(player.whoAmI);
		}

		public void Stalk()
		{
			if (IsFake)
				return;

			if (PlayerManager.stalkedPlayer == player)
				PlayerManager.stalkedPlayer = null;
			else
				PlayerManager.stalkedPlayer = player;
		}

		public void OpenInventory()
		{
			if (IsFake)
				return;

			InventoryManagerWindow inventory = UILoader.GetUIState<InventoryManagerWindow>();

			if (inventory.visible && inventory.player == player)
			{
				inventory.visible = false;
				return;
			}

			inventory.player = player;
			inventory.SetInventory();
			inventory.basePos = ((PlayerManagerBrowser)parent).basePos + new Vector2(350f, 0f);
			inventory.visible = true;
			inventory.RecalculateEverything();
		}
		#endregion
	}

	internal class PlayerManagerActionButton : SmartUIElement
	{
		private readonly string localizationKey;
		private readonly Asset<Texture2D> icon;
		private readonly Func<bool> isActive;
		private readonly Action onLeftClick;

		private string NameText => LocalizationHelper.GetToolText($"PlayerManager.{localizationKey}.Name");
		private string TooltipText => LocalizationHelper.GetToolText($"PlayerManager.{localizationKey}.Tooltip");

		public PlayerManagerActionButton(string key, Asset<Texture2D> icon, Action onLeftClick, Func<bool> isActive = null)
		{
			localizationKey = key;
			this.icon = icon;
			this.onLeftClick = onLeftClick;
			this.isActive = isActive ?? (() => false);

			Width.Set(40f, 0f);
			Height.Set(40f, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Rectangle dims = GetDimensions().ToRectangle();
			if (dims.Width <= 0 || dims.Height <= 0)
				return;

			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);
			spriteBatch.Draw(icon.Value, dims.TopLeft() + Vector2.One * 4f, Color.White);

			if (isActive())
			{
				// Draw yellow outline if the tool is active
				GUIHelper.DrawOutline(spriteBatch, dims, ThemeHandler.ButtonColor.InvertColor());

				// Draw glow in special cases.
				Texture2D glowTex = Assets.Misc.GlowAlpha.Value;
				var color = new Color(255, 220, 100, 0);
				spriteBatch.Draw(glowTex, new Rectangle(dims.X, dims.Y, 38, 38), color);
			}

			if (IsMouseHovering)
			{
				Tooltip.SetName(NameText);
				Tooltip.SetTooltip(TooltipText);
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			onLeftClick?.Invoke();
		}
	}
}
