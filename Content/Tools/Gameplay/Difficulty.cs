using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
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

		public override bool HasRightClick => true;

		public override void OnActivate()
		{
			if (!Main.expertMode)
			{
				Main.GameMode = GameModeID.Expert;
				Main.NewText(LocalizationHelper.GetToolText("Difficulty.GameInExpertMode"), new Color(255, 150, 0));
			}
			else if (!Main.masterMode)
			{
				Main.GameMode = GameModeID.Master;
				Main.NewText(LocalizationHelper.GetToolText("Difficulty.GameInMasterMode"), new Color(255, 0, 0));
			}
			else
			{
				Main.GameMode = GameModeID.Normal;
				Main.NewText(LocalizationHelper.GetToolText("Difficulty.GameInNormalMode"), new Color(180, 180, 255));
			}

			NetSend();
		}

		public override void OnRightClick()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				Main.NewText(LocalizationHelper.GetToolText("Difficulty.JourneyToggleDisabled"), Color.Red);
				return;
			}

			if (Main.LocalPlayer.difficulty != 3)
			{
				oldPlayerDifficulty = Main.LocalPlayer.difficulty;
				Main.LocalPlayer.difficulty = 3;
				Main.NewText(LocalizationHelper.GetToolText("Difficulty.JourneyEnabled"), Main.creativeModeColor);
			}
			else
			{
				if (oldPlayerDifficulty == -1 || oldPlayerDifficulty == 3)
					oldPlayerDifficulty = 0;

				Main.LocalPlayer.difficulty = (byte)oldPlayerDifficulty;
				Main.NewText(LocalizationHelper.GetToolText("Difficulty.JourneyDisabled"), Color.LightGray);
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