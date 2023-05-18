using DragonLens.Core.Systems.ThemeSystem;
using ReLogic.Content;
using System.Collections.Generic;
namespace DragonLens.Content.Themes.IconProviders
{
	internal class DefaultIcons : ThemeIconProvider
	{
		public override string Name => "DragonLens";

		public override string Description => "The default icons for DragonLens";

		public override void PopulateIcons(Dictionary<string, Texture2D> icons)
		{
			foreach (string key in defaultKeys)
			{
				icons.Add(key, ModContent.Request<Texture2D>($"DragonLens/Assets/Themes/IconProviders/DefaultIcons/{key}", AssetRequestMode.ImmediateLoad).Value);
			}
		}
	}
}