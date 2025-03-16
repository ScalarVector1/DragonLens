using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace DragonLens.Content.Tools.Multiplayer
{
	internal class PlayerManager : Tool
	{
		public override string IconKey => "PlayerManager";

		public override void OnActivate()
		{
			PlayerManagerWindow state = UILoader.GetUIState<PlayerManagerWindow>();
			state.visible = !state.visible;

			if (state.visible)
				state.SetPlayers();
		}
	}

	/// <summary>
	/// Handles the stalking function of the player manager
	/// </summary>
	internal class PlayerManagerSystem : ModSystem
	{
		public Player stalkedPlayer;

		public override void ModifyScreenPosition()
		{
			if (stalkedPlayer != null)
			{
				Main.screenPosition = stalkedPlayer.Center - new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
			}
		}
	}

	/// <summary>
	/// Class to refresh player manager list when players log in or out
	/// </summary>
	internal class PlayerManagerUpdater : ModPlayer
	{
		public override void PlayerDisconnect()
		{
			if (Main.netMode != NetmodeID.Server)
				UILoader.GetUIState<PlayerManagerWindow>().SetPlayers();
		}

		public override void PlayerConnect()
		{
			if (Main.netMode != NetmodeID.Server)
				UILoader.GetUIState<PlayerManagerWindow>().SetPlayers();
		}
	}

	internal class PlayerManagerWindow : DraggableUIState
	{
		public UIList playerList = [];
		public StyledScrollbar playerScroll;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 32);

		public override Vector2 DefaultPosition => new(0.4f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 340;
			height = 600;

			playerScroll = new(UserInterface);
			playerScroll.Width.Set(32, 0);
			playerScroll.Height.Set(320, 0);
			Append(playerScroll);

			playerList = [];
			playerList.Width.Set(320, 0);
			playerList.Height.Set(500, 0);
			playerList.SetScrollbar(playerScroll);
			Append(playerList);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			playerList.Left.Set(newPos.X + 10, 0);
			playerList.Top.Set(newPos.Y + 64, 0);

			playerScroll.Left.Set(newPos.X + 330, 0);
			playerScroll.Top.Set(newPos.Y + 64, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var target = new Rectangle((int)basePos.X, (int)basePos.Y, 340, 600);

			GUIHelper.DrawBox(spriteBatch, target, ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 300, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D gridBack = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var gridBackTarget = playerList.GetDimensions().ToRectangle();
			gridBackTarget.Inflate(4, 4);
			spriteBatch.Draw(gridBack, gridBackTarget, Color.Black * 0.25f);

			Texture2D icon = ThemeHandler.GetIcon("PlayerManager");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetToolText("PlayerManager.DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			base.Draw(spriteBatch);
		}

		public void SetPlayers()
		{
			playerList.Clear();

			foreach (Player player in Main.ActivePlayers)
			{
				playerList.Add(new PlayerManagerEntry(player));
			}

			Recalculate();
			Recalculate();
		}
	}

	internal class PlayerManagerEntry : SmartUIElement
	{
		public Player player;

		public PlayerManagerEntry(Player player)
		{
			this.player = player;

			Width.Set(300, 0);
			Height.Set(90, 0);

			var admin = new PlayerManagerButton("Admin", Assets.GUI.AdminIcon, () => PermissionHandler.LooksLikeAdmin(player));
			admin.Left.Set(10, 0);
			admin.Top.Set(42, 0);
			admin.OnLeftClick += (a, b) => ToggleAdmin();
			Append(admin);

			var kick = new PlayerManagerButton("Kick", Assets.GUI.KickIcon, () => false);
			kick.Left.Set(58, 0);
			kick.Top.Set(42, 0);
			kick.OnLeftClick += (a, b) => Kick();
			Append(kick);

			var stalk = new PlayerManagerButton("View", Assets.GUI.StalkIcon, () => ModContent.GetInstance<PlayerManagerSystem>().stalkedPlayer == player);
			stalk.Left.Set(106, 0);
			stalk.Top.Set(42, 0);
			stalk.OnLeftClick += (a, b) => Stalk();
			Append(stalk);

			var inv = new PlayerManagerButton("Inventory", Assets.GUI.InventoryIcon, () => false);
			inv.Left.Set(154, 0);
			inv.Top.Set(42, 0);
			inv.OnLeftClick += (a, b) => OpenInventory();
			Append(inv);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle(dims.Left + 4, dims.Top + 4, 260, 34);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Main.PlayerRenderer.DrawPlayerHead(Main.Camera, player, dims.TopLeft() + Vector2.One * 20);
			Utils.DrawBorderString(spriteBatch, player.name, dims.TopLeft() + new Vector2(46, 12), PermissionHandler.LooksLikeAdmin(player) ? new Color(100, 235, 235) : Color.White);

			Texture2D teamIcon = Terraria.GameContent.TextureAssets.Pvp[1].Value;
			var source = new Rectangle(player.team * 18, 0, 18, 18);
			spriteBatch.Draw(teamIcon, dims.TopLeft() + new Vector2(272, 12), source, Color.White);

			Utils.DrawBorderString(spriteBatch, $"{Math.Round(player.Center.X / 16)}, {Math.Round(player.Center.Y / 16)}", dims.TopLeft() + new Vector2(200, 52), Color.Gray);

			if (new Rectangle(dims.X, dims.Y, dims.Width, 34).Contains(Main.MouseScreen.ToPoint()))
			{
				Tooltip.SetName(player.name);
				string life = $"{player.statLife}/{player.statLifeMax2}";
				string mana = $"{player.statMana}/{player.statManaMax2}";
				string position = $"{Math.Round(player.Center.X / 16)}, {Math.Round(player.Center.Y / 16)}";
				string velocity = $"{Math.Round(player.velocity.X)}, {Math.Round(player.velocity.Y)}";
				string alive = $"{!player.dead}";
				Tooltip.SetTooltip(LocalizationHelper.GetToolText("PlayerManager.Stats",
					life, mana, position, velocity, alive));
			}

			base.Draw(spriteBatch);
		}

		public void ToggleAdmin()
		{
			// Theoretically redundant but this is probably the most important button to protect
			if (PermissionHandler.CanUseTools(Main.LocalPlayer))
			{
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
		}

		public void Kick()
		{
			PlayerManagerNetHandler.SendKick(player.whoAmI);
		}

		public void Stalk()
		{
			if (ModContent.GetInstance<PlayerManagerSystem>().stalkedPlayer != player)
				ModContent.GetInstance<PlayerManagerSystem>().stalkedPlayer = player;
			else
				ModContent.GetInstance<PlayerManagerSystem>().stalkedPlayer = null;
		}

		public void OpenInventory()
		{
			UILoader.GetUIState<InventoryManagerWindow>().player = player;
			UILoader.GetUIState<InventoryManagerWindow>().SetInventory();
			UILoader.GetUIState<InventoryManagerWindow>().basePos = UILoader.GetUIState<PlayerManagerWindow>().basePos + new Vector2(350, 0);
			UILoader.GetUIState<InventoryManagerWindow>().visible = true;

			UILoader.GetUIState<InventoryManagerWindow>().RecalculateEverything();
		}
	}

	internal class PlayerManagerButton : SmartUIElement
	{
		public string localizationKey;
		public string name => LocalizationHelper.GetToolText($"PlayerManager.{localizationKey}.Name");
		public string tooltip => LocalizationHelper.GetToolText($"PlayerManager.{localizationKey}.Tooltip");
		public Asset<Texture2D> icon;

		public Func<bool> active;

		public PlayerManagerButton(string key, Asset<Texture2D> icon, Func<bool> active)
		{
			localizationKey = key;
			this.icon = icon;
			this.active = active;

			Width.Set(40, 0);
			Height.Set(40, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);
			spriteBatch.Draw(icon.Value, dims.TopLeft() + Vector2.One * 4, Color.White);

			if (active())
				GUIHelper.DrawOutline(spriteBatch, dims, ThemeHandler.ButtonColor.InvertColor());

			if (IsMouseHovering)
			{
				Tooltip.SetName(name);
				Tooltip.SetTooltip(tooltip);
			}
		}
	}
}