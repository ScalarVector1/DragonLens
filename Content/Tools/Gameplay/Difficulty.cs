using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Difficulty : Tool
	{
		public static bool originallyJourney = false;

		public static int oldPlayerDifficulty = -1;

		public override string IconKey => "Difficulty";

		public override string DisplayName => "Difficulty switcher";

		public override string Description => "Cycle through the 3 main game difficulties NEWBLOCK Right click to toggle journey mode";

		public override bool HasRightClick => true;

		public override string RightClickName => "Toggle journey mode";

		public override void OnActivate()
		{
			if (!Main.expertMode)
			{
				Main.GameMode = GameModeID.Expert;
				Main.NewText("The game is now in expert mode.", new Color(255, 150, 0));
			}
			else if (!Main.masterMode)
			{
				Main.GameMode = GameModeID.Master;
				Main.NewText("The game is now in master mode.", new Color(255, 0, 0));
			}
			else
			{
				Main.GameMode = GameModeID.Normal;
				Main.NewText("The game is now in normal mode.", new Color(180, 180, 255));
			}

			NetSend();
		}

		public override void OnRightClick()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				Main.NewText("Journey toggle is disabled in multiplayer", Color.Red);
				return;
			}

			if (Main.LocalPlayer.difficulty != 3)
			{
				oldPlayerDifficulty = Main.LocalPlayer.difficulty;
				Main.LocalPlayer.difficulty = 3;
				Main.NewText("Journey mode enabled.", Main.creativeModeColor);
			}
			else
			{
				if (oldPlayerDifficulty == -1 || oldPlayerDifficulty == 3)
					oldPlayerDifficulty = 0;

				Main.LocalPlayer.difficulty = (byte)oldPlayerDifficulty;
				Main.NewText("Journey mode disabled.", Color.LightGray);
			}
		}

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(Main.GameMode);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			Main.GameMode = reader.ReadInt32();

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}
	}

	/// <summary>
	/// This prevents the player from mutating the journey state of their save
	/// </summary>
	internal class JourneySafetyPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Player.difficulty == 3)
				Difficulty.originallyJourney = true;
		}

		public override void PreSavePlayer()
		{
			if (Difficulty.originallyJourney && Main.LocalPlayer.difficulty != 3)
				Main.LocalPlayer.difficulty = 3;

			if (!Difficulty.originallyJourney && Main.LocalPlayer.difficulty == 3)
				Main.LocalPlayer.difficulty = (byte)Difficulty.oldPlayerDifficulty;
		}
	}
}