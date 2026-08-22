using DragonLens.Content.Filters;
using DragonLens.Content.Filters.PlayerManagerFilters;
using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Multiplayer.Drawers;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

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
		public override int MinButtonSize => 10;
		public override int BrowserWidth => 520;

		public PlayerManagerBrowser()
		{
			buttonSize = 48;
			listMode = true;
		}

		public override void PostInitialize()
		{
			base.PostInitialize();

			RemoveChild(sizeSlider);
			RemoveChild(listButton);
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<PlayerManagerItem>();
			List<Player> players = [.. Main.ActivePlayers];

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
					var left = (PlayerManagerItem)a;
					var right = (PlayerManagerItem)b;

					int teamCompare = left.player.team.CompareTo(right.player.team);
					if (teamCompare != 0)
						return teamCompare;

					return string.Compare(left.Identifier, right.Identifier, StringComparison.OrdinalIgnoreCase);
				}));

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
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.NoTeam", n => n is PlayerManagerItem pb && pb.player.team != 0, new Rectangle(0, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Red", n => n is PlayerManagerItem pb && pb.player.team != 1, new Rectangle(18, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Green", n => n is PlayerManagerItem pb && pb.player.team != 2, new Rectangle(36, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Blue", n => n is PlayerManagerItem pb && pb.player.team != 3, new Rectangle(54, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Yellow", n => n is PlayerManagerItem pb && pb.player.team != 4, new Rectangle(72, 0, 16, 16), new Point(20, 20)));
			filters.AddFilter(new TeamFilter(teamIcons, "Tools.PlayerManager.Filters.Pink", n => n is PlayerManagerItem pb && pb.player.team != 5, new Rectangle(90, 0, 16, 16), new Point(20, 20)));
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
		private readonly UICharacter character;

		private PlayerManagerSystem PlayerManager => ModContent.GetInstance<PlayerManagerSystem>();
		private PlayerManagerBrowser PlayerBrowser => (PlayerManagerBrowser)parent;

		protected override int ListHeight => 74;
		protected override int ListWidth => (int)Parent.GetDimensions().Width - 12;

		public override string Identifier => player.active ? player.name : $"Player {player.whoAmI}";
		public override string Key => $"Player:{player.whoAmI}";

		public PlayerManagerItem(Player player, PlayerManagerBrowser parent) : base(parent)
		{
			this.player = player;
			CreateButtons();
		}

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
			AddActionButton("Admin", new("Admin", Assets.GUI.AdminIcon, ToggleAdmin, () => PermissionHandler.LooksLikeAdmin(player)));
		}

		private void AddActionButton(string key, PlayerManagerActionButton button)
		{
			actionButtons[key] = button;
			Append(button);
		}

		public override void Recalculate()
		{
			Height.Set(60, 0);

			int width = (int)GetDimensions().Width;
			int height = (int)GetDimensions().Height;
			int buttonSize = Math.Min(height - 8, 40);
			int buttonTop = 26;
			int right = width - 8;

			foreach ((string key, PlayerManagerActionButton button) in actionButtons)
			{
				right -= buttonSize;
				button.Left.Set(right, 0f);
				button.Top.Set(buttonTop, 0f);
				button.Width.Set(buttonSize, 0f);
				button.Height.Set(buttonSize, 0f);
				right -= 4;
			}

			base.Recalculate();
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			var bounds = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, bounds, ThemeHandler.BackgroundColor);

			var playerBox = new Rectangle(bounds.X + 4, bounds.Y + 4, 66, 66);
			Texture2D bgTex = PlayerBackgroundDrawer.GetBackground(player).Value;

			spriteBatch.Draw(bgTex, playerBox, new Rectangle(bgTex.Width / 2 - playerBox.Width / 2, bgTex.Height / 2 - playerBox.Height / 2, playerBox.Width, playerBox.Height), Color.White);
			PlayerBackgroundDrawer.DrawPlayerFull(spriteBatch, playerBox, player);

			playerBox.Inflate(4, 4);
			GUIHelper.DrawOutline(spriteBatch, playerBox, ThemeHandler.ButtonColor);

			Utils.DrawBorderString(spriteBatch, player.name, bounds.TopLeft() + new Vector2(80, 4), Main.teamColor[player.team]);

			Utils.DrawBorderString(spriteBatch, $"{player.statLife}/{player.statLifeMax}", bounds.TopLeft() + new Vector2(80, 30), new Color(255, 200, 200), 0.8f);
			Utils.DrawBorderString(spriteBatch, $"{(int)(player.position.X / 16)}, {(int)(player.position.Y / 16)}", bounds.TopLeft() + new Vector2(80, 48), Color.LightGray, 0.8f);

			if (IsMouseHovering && CanShowTooltip)
			{
				Main.LocalPlayer.mouseInterface = true;

				Tooltip.SetName(player.name);
				Tooltip.SetTooltip(LocalizationHelper.GetToolText("PlayerManager.PlayerTooltip", player.statLife, player.position));
			}
		}

		public void FreezePlayer()
		{
			PlayerManagerNetHandler.SendFrozenPlayer(player.whoAmI);
		}

		public void TeleportToMe()
		{
			PlayerManagerNetHandler.SendTeleportToMe(player.whoAmI);
		}

		public void TeleportToPlayer()
		{
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
			PlayerManagerNetHandler.SendKick(player.whoAmI);
		}

		public void Stalk()
		{
			if (PlayerManager.stalkedPlayer == player)
				PlayerManager.stalkedPlayer = null;
			else
				PlayerManager.stalkedPlayer = player;
		}

		public void OpenInventory()
		{
			InventoryManagerWindow inventory = UILoader.GetUIState<InventoryManagerWindow>();

			if (inventory.visible && inventory.player == player)
			{
				inventory.visible = false;
				return;
			}

			inventory.player = player;
			inventory.SetInventory();
			inventory.basePos = ((PlayerManagerBrowser)parent).basePos + new Vector2(760f, 0f);
			inventory.visible = true;
			UILoader.BringToFront(inventory);
			inventory.RecalculateEverything();
		}
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
			var dims = GetDimensions().ToRectangle();
			if (dims.Width <= 0 || dims.Height <= 0)
				return;

			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);
			spriteBatch.Draw(icon.Value, dims.TopLeft() + Vector2.One * 4f, Color.White);

			if (isActive())
				GUIHelper.DrawOutline(spriteBatch, dims, ThemeHandler.ButtonColor.InvertColor());

			if (IsMouseHovering && CanShowTooltip)
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