using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Gameplay;
using DragonLens.Content.Tools.Visualization;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		public UIList playerList = new();
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

			playerList = new();
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

			foreach(Player player in Main.ActivePlayers)
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
			Height.Set(80, 0);

			var admin = new PlayerManagerButton("Give Admin", 100);
			admin.Left.Set(10, 0);
			admin.Top.Set(40, 0);
			admin.OnLeftClick += (a, b) => ToggleAdmin();
			Append(admin);

			var kick = new PlayerManagerButton("Kick", 80);
			kick.Left.Set(120, 0);
			kick.Top.Set(40, 0);
			kick.OnLeftClick += (a, b) => Kick();
			Append(kick);

			var stalk = new PlayerManagerButton("View", 80);
			stalk.Left.Set(210, 0);
			stalk.Top.Set(40, 0);
			stalk.OnLeftClick += (a, b) => Stalk();
			Append(stalk);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Main.PlayerRenderer.DrawPlayerHead(Main.Camera, player, dims.TopLeft() + Vector2.One * 20);
			Utils.DrawBorderString(spriteBatch, player.name, dims.TopLeft() + new Vector2(46, 12), PermissionHandler.CanUseTools(player) ? Color.Cyan : Color.White);

			base.Draw(spriteBatch);
		}

		public void ToggleAdmin()
		{
			// Theoretically redundant but this is probably the most important button to protect
			if (Core.Systems.PermissionHandler.CanUseTools(Main.LocalPlayer))
			{
				if (Core.Systems.PermissionHandler.CanUseTools(player))
					Core.Systems.PermissionHandler.RemoveAdmin(player);
				else
					Core.Systems.PermissionHandler.AddAdmin(player);
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
	}

	internal class PlayerManagerButton : SmartUIElement
	{
		public string text;

		public PlayerManagerButton(string text, int width)
		{
			this.text = text;
			Width.Set(width, 0);
			Height.Set(30, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Utils.DrawBorderString(spriteBatch, text, dims.Center() + Vector2.UnitY * 4, Color.White, 0.8f, 0.5f, 0.5f);
		}
	}
}
