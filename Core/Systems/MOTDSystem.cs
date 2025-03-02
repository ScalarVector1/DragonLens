using DragonLens.Helpers;
using System;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems
{
	/// <summary>
	/// Displays a welcome message to the user
	/// </summary>
	internal class MOTDPlayer : ModPlayer
	{
		public Version seenMotd;

		public override void OnEnterWorld()
		{
			if (Mod.Version > seenMotd)
			{
				string MOTD = LocalizationHelper.GetText("MOTD", Mod.Version);
				Main.NewText(MOTD, new Color(200, 235, 255));
			}

			seenMotd = Mod.Version;
		}

		public override void SaveData(TagCompound tag)
		{
			if (seenMotd != null)
				tag["seenMotd"] = seenMotd.ToString();
		}

		public override void LoadData(TagCompound tag)
		{
			seenMotd = Version.Parse(tag.GetString("seenMotd") ?? "0.0.0");
		}
	}
}