using DragonLens.Core.Systems.ThemeSystem;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace DragonLens.Content.Themes.IconProviders
{
	internal class HEROsIcons : ThemeIconProvider
	{
		public override void PopulateIcons(Dictionary<string, Texture2D> icons)
		{
			foreach (string key in defaultKeys)
			{
				icons.Add(key, ModContent.Request<Texture2D>($"DragonLens/Assets/Themes/IconProviders/HEROsIcons/{key}", AssetRequestMode.ImmediateLoad).Value);
			}
		}
	}
}
