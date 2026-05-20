using DragonLens.Core.Loaders.UILoading;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria.ID;

namespace DragonLens.Content.Tools.Multiplayer
{
	/// <summary>
	/// Handles updating player states in-game.
	/// Communicates with its UI counterpart: <see cref="PlayerManager"/>
	/// </summary>
	internal class PlayerManagerSystem : ModSystem
	{
		public Player stalkedPlayer;
		public HashSet<int> frozenPlayers = [];
		public List<Player> fakePlayers = [];
		public override void PreUpdatePlayers()
		{
			foreach (Player player in Main.ActivePlayers)
			{
				if (!frozenPlayers.Contains(player.whoAmI))
					continue;

				player.AddBuff(BuffID.Frozen, 2);
				player.velocity = Vector2.Zero;
				player.position = player.oldPosition;
				player.AddImmuneTime(ImmunityCooldownID.General, 2);
			}
		}

		public override void ModifyScreenPosition()
		{
			if (stalkedPlayer != null)
				Main.screenPosition = stalkedPlayer.Center - new Vector2(Main.screenWidth, Main.screenHeight) / 2f;
		}

		// For debug purposes, add fake player entries
		public override void PostUpdateInput()
		{
			if (Main.dedServ)
				return;

			PlayerManagerBrowser window = UILoader.GetUIState<PlayerManagerBrowser>();
			if (!window.visible)
				return;

#if DEBUG
			if (Main.keyState.IsKeyDown(Keys.NumPad1) && !Main.oldKeyState.IsKeyDown(Keys.NumPad1))
			{
				fakePlayers.Add(CreateFakePlayer());
				window.RefreshEntries();
			}

			if (Main.keyState.IsKeyDown(Keys.NumPad2) && !Main.oldKeyState.IsKeyDown(Keys.NumPad2))
			{
				if (fakePlayers.Count > 0)
				{
					fakePlayers.RemoveAt(fakePlayers.Count - 1);
					window.RefreshEntries();
				}
			}
#endif
		}

		private static Player CreateFakePlayer()
		{
			string[] animals = ["Elephant", "Cobra", "Tiger", "Lion", "Panda", "Otter", "Falcon", "Shark", "Koala"];

			Player player = new();
			player.active = true;
			player.whoAmI = -1;
			player.team = Main.rand.Next(6);
			player.name = animals[Main.rand.Next(animals.Length)] + animals[Main.rand.Next(animals.Length)];
			player.statLife = player.statLifeMax2 = 100;
			player.statMana = player.statManaMax2 = 20;
			player.width = Main.LocalPlayer.width;
			player.height = Main.LocalPlayer.height;
			player.position = Main.LocalPlayer.position;

			return player;
		}
	}
}