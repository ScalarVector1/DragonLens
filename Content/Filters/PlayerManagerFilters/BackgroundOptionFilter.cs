using DragonLens.Content.Tools.Multiplayer;
using DragonLens.Helpers;
using ReLogic.Content;

namespace DragonLens.Content.Filters.PlayerManagerFilters
{
	internal sealed class BackgroundOptionFilter : Filter
	{
		private readonly PlayerManagerBrowser browser;
		private readonly string key;

		public BackgroundOptionFilter(PlayerManagerBrowser browser, string key, Asset<Texture2D> texture)
			: base(texture, LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}"), _ => false)
		{
			this.browser = browser;
			this.key = key;
			isModFilter = true;
		}

		public override string Description => LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}.Toggle");
		public bool Enabled => browser.Settings.IsBackgroundMode(key);

		public void Toggle()
		{
			bool enabled = browser.Settings.IsBackgroundMode(key);
			browser.Settings.SetBackgroundMode(key, !enabled);
		}
	}
}