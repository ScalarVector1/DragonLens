using DragonLens.Content.Tools.Multiplayer;
using DragonLens.Helpers;
using ReLogic.Content;
using System;

namespace DragonLens.Content.Filters.PlayerManagerFilters.Toggles
{
	internal sealed class StatOptionFilter : Filter
	{
		private readonly PlayerManagerBrowser browser;
		private readonly string key;

		public StatOptionFilter(PlayerManagerBrowser browser, string key, Asset<Texture2D> texture)
			: base(texture, LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}"), _ => false)
		{
			this.browser = browser;
			this.key = key;
		}
		public override string Description => LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}.Toggle");
		public bool Enabled => browser.Settings.IsStatVisible(key);

		public void Toggle()
		{
			bool enabled = browser.Settings.IsStatVisible(key);
			int availableHeight = browser.listMode ? Math.Max(48, browser.buttonSize) : browser.buttonSize;
			browser.Settings.TryToggleStat(key, !enabled, browser.listMode, availableHeight);
		}
	}
}