using DragonLens.Content.Tools.Multiplayer;
using DragonLens.Helpers;
using ReLogic.Content;

namespace DragonLens.Content.Filters.PlayerManagerFilters
{
	internal sealed class PlayerOptionFilter : Filter
	{
		private readonly PlayerManagerBrowser browser;
		private readonly string key;

		public PlayerOptionFilter(PlayerManagerBrowser browser, string key, Asset<Texture2D> texture)
			: base(texture, LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}"), _ => false)
		{
			this.browser = browser;
			this.key = key;
			isModFilter = true;
		}

		public override string Description => LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}.Toggle");
		public bool Enabled => browser.Settings.IsPlayerMode(key);

		public void Toggle()
		{
			bool enabled = browser.Settings.IsPlayerMode(key);
			browser.Settings.SetPlayerMode(key, !enabled);

			// Debug print
			//Main.NewText(key + ": " + !enabled);
		}
	}
}