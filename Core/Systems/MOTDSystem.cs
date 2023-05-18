using DragonLens.Helpers;

namespace DragonLens.Core.Systems
{
	/// <summary>
	/// Displays a welcome message to the user
	/// </summary>
	internal class MOTDPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			string MOTD = LocalizationHelper.GetText("MOTD", Mod.Version);

			Main.NewText(MOTD, new Color(200, 235, 255));
		}
	}
}