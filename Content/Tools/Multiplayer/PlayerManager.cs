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
		public int player;
		public int index;
		public int inventory;
		public Item item;

		public override string IconKey => "PlayerManager";

		public override void OnActivate()
		{
			PlayerManagerWindow state = UILoader.GetUIState<PlayerManagerWindow>();
			state.visible = !state.visible;

			if (state.visible)
				state.SetPlayers();
		}

		public override void SendPacket(BinaryWriter writer)
		{
			if (item is null)
				return;

			writer.Write(player);
			writer.Write(index);
			writer.Write(inventory);
			ItemIO.Send(item, writer, true);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			int pIndex = reader.ReadInt32();
			Player player = Main.player[pIndex];

			int index = reader.ReadInt32();
			int invIndex = reader.ReadInt32();

			Item[] inventory = invIndex switch
			{
				0 => player.inventory,
				2 => player.bank.item,
				4 => player.bank2.item,
				6 => player.bank3.item,
				8 => player.bank4.item,
				_ => player.inventory
			};

			Item item = ItemIO.Receive(reader, true);
			inventory[index] = item.Clone();

			if (Main.netMode == NetmodeID.Server)
				SendItem(pIndex, index, invIndex, item, sender);
		}

		public void SendItem(int player, int index, int inventory, Item item, int ignore = -1)
		{
			if (item is null)
				return;

			this.player = player;
			this.index = index;
			this.inventory = inventory;
			this.item = item.Clone();

			NetSend(-1, ignore);
		}
	}

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

			Utils.DrawBorderStringBig(spriteBatch, "Player Manager", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			base.Draw(spriteBatch);
		}

		public void SetPlayers()
		{
			playerList.Clear();

			foreach (Player player in Main.ActivePlayers)
			{
				playerList.Add(new PlayerManagerEntry(player));
			}
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

			var admin = new PlayerManagerButton("Toggle Admin", "Gives or revokes admin permissions from this player. Make sure this is someone you can trust! Admins can use all features of DragonLens, including this one!", Assets.GUI.AdminIcon, () => PermissionHandler.CanUseTools(player));
			admin.Left.Set(10, 0);
			admin.Top.Set(42, 0);
			admin.OnLeftClick += (a, b) => ToggleAdmin();
			Append(admin);

			var kick = new PlayerManagerButton("Kick", "Forcibly disconnect this player from the game", Assets.GUI.KickIcon, () => false);
			kick.Left.Set(58, 0);
			kick.Top.Set(42, 0);
			kick.OnLeftClick += (a, b) => Kick();
			Append(kick);

			var stalk = new PlayerManagerButton("View", "Move the camera to this players position", Assets.GUI.StalkIcon, () => ModContent.GetInstance<PlayerManagerSystem>().stalkedPlayer == player);
			stalk.Left.Set(106, 0);
			stalk.Top.Set(42, 0);
			stalk.OnLeftClick += (a, b) => Stalk();
			Append(stalk);

			var inv = new PlayerManagerButton("Inventory", "View and edit this player's inventories", Assets.GUI.StalkIcon, () => false);
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
			Utils.DrawBorderString(spriteBatch, player.name, dims.TopLeft() + new Vector2(46, 12), PermissionHandler.CanUseTools(player) ? new Color(100, 235, 235) : Color.White);

			Texture2D teamIcon = Terraria.GameContent.TextureAssets.Pvp[1].Value;
			var source = new Rectangle(player.team * 18, 0, 18, 18);
			spriteBatch.Draw(teamIcon, dims.TopLeft() + new Vector2(272, 12), source, Color.White);

			Utils.DrawBorderString(spriteBatch, $"{Math.Round(player.Center.X / 16)}, {Math.Round(player.Center.Y / 16)}", dims.TopLeft() + new Vector2(200, 52), Color.Gray);

			base.Draw(spriteBatch);
		}

		public void ToggleAdmin()
		{
			// Theoretically redundant but this is probably the most important button to protect
			if (PermissionHandler.CanUseTools(Main.LocalPlayer))
			{
				if (player == Main.LocalPlayer)
				{
					Main.NewText("You cannot remove your own permissions!", Color.Red);
					return;
				}

				if (PermissionHandler.CanUseTools(player))
				{
					PermissionHandler.RemoveAdmin(player);
					Main.NewText($"{player.name} is no longer an admin.", Color.Yellow);
				}
				else
				{
					PermissionHandler.AddAdmin(player);
					Main.NewText($"{player.name} is now an admin.", Color.Yellow);
				}
			}
		}

		public void Kick()
		{
			NetMessage.SendData(2, -1, Main.myPlayer);
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
			UILoader.GetUIState<InventoryManagerWindow>().visible = true;
		}
	}

	internal class PlayerManagerButton : SmartUIElement
	{
		public string name;
		public string tooltip;
		public Asset<Texture2D> icon;

		public Func<bool> active;

		public PlayerManagerButton(string name, string tooltip, Asset<Texture2D> icon, Func<bool> active)
		{
			this.name = name;
			this.tooltip = tooltip;
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
