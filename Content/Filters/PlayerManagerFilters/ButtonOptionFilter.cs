using DragonLens.Content.Tools.Multiplayer;
using DragonLens.Helpers;
using ReLogic.Content;

namespace DragonLens.Content.Filters.PlayerManagerFilters.Toggles
{
	internal sealed class ButtonOptionFilter : Filter
	{
		private readonly PlayerManagerBrowser browser;
		private readonly string key;

		public ButtonOptionFilter(PlayerManagerBrowser browser, string key, Asset<Texture2D> texture)
			: base(texture, LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}"), _ => false)
		{
			this.browser = browser;
			this.key = key;
		}
		public override string Name => base.Name + " Button";
		public override string Description => LocalizationHelper.GetToolText($"PlayerManager.Filters.{key}.Toggle");

		public bool Enabled => browser.Settings.IsButtonVisible(key);

		public void Toggle()
		{
			browser.Settings.SetButtonVisible(key, !browser.Settings.IsButtonVisible(key));
		}
	}
}