using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Spawners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;
using static Terraria.GameContent.Bestiary.Filters;

namespace DragonLens.Content.Filters.NPCFilters
{
	internal class BestiaryFilter : Filter
	{
		public static FieldInfo elementInfo = typeof(ByInfoElement).GetField("_element", BindingFlags.NonPublic | BindingFlags.Instance);
		public static FieldInfo frameInfo = typeof(FilterProviderInfoElement).GetField("_filterIconFrame", BindingFlags.NonPublic | BindingFlags.Instance);

		public IBestiaryEntryFilter bestiaryFilter;

		public BestiaryFilter(IBestiaryEntryFilter bestiaryFilter) : base("", Language.GetTextValue(bestiaryFilter.GetDisplayNameKey()), $"NPCs with the bestiary attribute {Language.GetTextValue(bestiaryFilter.GetDisplayNameKey())}", n => FilterByBestiary(n, bestiaryFilter))
		{
			this.bestiaryFilter = bestiaryFilter;
		}

		public static bool FilterByBestiary(BrowserButton button, IBestiaryEntryFilter bestiaryFilter)
		{
			if (button is NPCButton)
			{
				var nb = button as NPCButton;

				if (nb != null && bestiaryFilter.FitsFilter(nb.entry))
					return false;
			}

			return true;
		}

		public override void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			if (bestiaryFilter is ByInfoElement)
			{
				var provider = elementInfo.GetValue(bestiaryFilter) as FilterProviderInfoElement;

				if (provider is null)
					return;

				var frame = (Point)frameInfo.GetValue(provider);

				if (frame == default)
					return;

				ReLogic.Content.Asset<Texture2D> tex = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Tags_Shadow");
				Rectangle source = tex.Frame(16, 5, frame.X, frame.Y);

				spriteBatch.Draw(tex.Value, target, source, Color.White);
			}
		}
	}
}
