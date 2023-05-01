using Terraria.Localization;

namespace DragonLens.Helpers
{
	internal static class LocalizationHelper
	{
		/// <summary>
		/// Gets a localized text value of the mod
		/// </summary>
		/// <param name="key">the localization key</param>
		/// <param name="args">optional args that should be passed</param>
		/// <returns>the text should be displayed</returns>
		public static string GetText(string key, params object[] args)
		{
			return Language.GetTextValue($"Mods.DragonLens.{key}", args);
		}
		
		public static string GetGUIText(string key, params object[] args)
		{
			return Language.GetTextValue($"Mods.DragonLens.GUI.{key}", args);
		}
		
		public static string GetToolText(string key, params object[] args)
		{
			return Language.GetTextValue($"Mods.DragonLens.Tools.{key}", args);
		}
		
		public static string GetCommonText(string key, params object[] args)
		{
			return Language.GetTextValue($"Mods.DragonLens.Common.{key}", args);
		}
	}
}