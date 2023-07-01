using DragonLens.Core.Systems.ThemeSystem;
using ReLogic.Content;
using System.Collections.Generic;

namespace DragonLens.Content.Themes.IconProviders
{
	internal class HEROsIcons : ThemeIconProvider
	{
		public override string NameKey => "HEROs";

		public override void PopulateIcons(Dictionary<string, Texture2D> icons)
		{
			foreach (string key in defaultKeys)
			{
				icons.Add(key, ModContent.Request<Texture2D>($"DragonLens/Assets/Themes/IconProviders/HEROsIcons/{key}", AssetRequestMode.ImmediateLoad).Value);
			}
		}
	}
}