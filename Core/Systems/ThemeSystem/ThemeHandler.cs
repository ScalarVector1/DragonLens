using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ThemeSystem
{
	internal class ThemeHandler : ModSystem
	{
		private readonly static Dictionary<string, ThemeBoxProvider> allBoxProviders = new();
		private readonly static Dictionary<string, ThemeIconProvider> allIconProviders = new();

		private readonly static Dictionary<Type, ThemeBoxProvider> allBoxProvidersByType = new();
		private readonly static Dictionary<Type, ThemeIconProvider> allIconProvidersByType = new();

		public static ThemeBoxProvider currentBoxProvider;
		public static ThemeColorProvider currentColorProvider;
		public static ThemeIconProvider currentIconProvider;

		/// <summary>
		/// The color that buttons should be drawn in.
		/// </summary>
		public static Color ButtonColor => currentColorProvider.buttonColor;

		/// <summary>
		/// The color that background boxes should be drawn in.
		/// </summary>
		public static Color BackgroundColor => currentColorProvider.backgroundColor;

		/// <summary>
		/// Sets the current box provider based on a string key. The key should be the name of the ThemeBoxProvider's type.
		/// </summary>
		/// <param name="key">The type name of the ThemeBoxProvider to set</param>
		private static void SetBoxProvider(string key)
		{
			currentBoxProvider = allBoxProviders[key];
		}

		/// <summary>
		/// Sets the current box provider based on a type.
		/// </summary>
		/// <typeparam name="T">The type of the box provider to set</typeparam>
		public static void SetBoxProvider<T>() where T : ThemeBoxProvider
		{
			currentBoxProvider = allBoxProvidersByType[typeof(T)];
		}

		/// <summary>
		/// Sets the current icon provider based on a string key. The key should be the name of the ThemeIconProvider's type.
		/// </summary>
		/// <param name="key">The type name of the ThemeIconProvider to set</param>
		private static void SetIconProvider(string key)
		{
			currentIconProvider = allIconProviders[key];
		}

		/// <summary>
		/// Sets the current icon provider based on a type.
		/// </summary>
		/// <typeparam name="T">The type of the icon provider to set</typeparam>
		public static void SetIconProvider<T>() where T : ThemeIconProvider
		{
			currentIconProvider = allIconProvidersByType[typeof(T)];
		}

		public static ThemeBoxProvider GetBoxProvider<T>() where T : ThemeBoxProvider
		{
			return allBoxProvidersByType[typeof(T)];
		}

		public static ThemeIconProvider GetIconProvider<T>() where T : ThemeIconProvider
		{
			return allIconProvidersByType[typeof(T)];
		}

		public override void Load()
		{
			foreach (Type t in GetType().Assembly.GetTypes())
			{
				if (!t.IsAbstract && t.IsSubclassOf(typeof(ThemeBoxProvider)))
				{
					allBoxProviders.Add(t.FullName, (ThemeBoxProvider)Activator.CreateInstance(t));
					allBoxProvidersByType.Add(t, (ThemeBoxProvider)Activator.CreateInstance(t));
				}

				if (!t.IsAbstract && t.IsSubclassOf(typeof(ThemeIconProvider)))
				{
					allIconProviders.Add(t.FullName, (ThemeIconProvider)Activator.CreateInstance(t));
					allIconProvidersByType.Add(t, (ThemeIconProvider)Activator.CreateInstance(t));
				}
			}
		}

		/// <summary>
		/// Shortcut to get the current icon provider's icon for a given key.
		/// </summary>
		/// <param name="key">The key of the icon to get</param>
		/// <returns>a Texture2D for the icon</returns>
		public static Texture2D GetIcon(string key)
		{
			return currentIconProvider.GetIcon(key);
		}

		public static void SaveData(TagCompound tag)
		{
			var themeTag = new TagCompound
			{
				["BoxTheme"] = currentBoxProvider.GetType().FullName,
				["IconTheme"] = currentIconProvider.GetType().FullName
			};

			tag["Theme"] = themeTag;
		}

		public static void LoadData(TagCompound tag)
		{
			if (tag.TryGet("Theme", out TagCompound themeTag))
			{
				SetBoxProvider(themeTag.GetString("BoxTheme"));
				SetIconProvider(themeTag.GetString("IconTheme"));
			}
		}
	}
}
