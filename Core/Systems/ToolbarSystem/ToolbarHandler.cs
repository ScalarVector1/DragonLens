using DragonLens.Content.GUI;
using DragonLens.Content.Themes.BoxProviders;
using DragonLens.Content.Themes.IconProviders;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ToolbarSystem
{
	/// <summary>
	/// Manages all active toolbars and toolbar layouts.
	/// </summary>
	internal class ToolbarHandler : ModSystem
	{
		/// <summary>
		/// The collection of toolbars currently in use by the current layout
		/// </summary>
		public static readonly List<Toolbar> activeToolbars = new();

		public override void OnModLoad()
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			// Set up our directory
			string dir = Path.Join(Main.SavePath, "DragonLensLayouts");

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			// Check version file. If version do not match, re-generate presets!
			string versionPath = Path.Join(Main.SavePath, "DragonLensLayouts", "Version");

			if (File.Exists(versionPath))
			{
				string comp = File.ReadAllText(versionPath);
				if(comp != $"{Mod.Version}")
				{
					FirstTimeSetupSystem.SetupPresets();
					File.WriteAllText(versionPath, $"{Mod.Version}");
				}
			}
			else
			{
				FirstTimeSetupSystem.SetupPresets();
				File.WriteAllText(versionPath, $"{Mod.Version}");
			}

			// Attempt to load the current layout
			string currentPath = Path.Join(Main.SavePath, "DragonLensLayouts", "Current");

			if (File.Exists(currentPath))
			{
				try
				{
					LoadFromFile(currentPath);
				}
				catch
				{
					Main.NewText(LocalizationHelper.GetGUIText("Layout.Invalid"), Color.Red);
					LoadFallback();
				}
			}
			else
			{
				LoadFallback();
			}
		}

		/// <summary>
		/// Saves a layout to a TagCompound
		/// </summary>
		/// <param name="tag"></param>
		public static void SaveLayout(TagCompound tag)
		{
			List<TagCompound> tags = new();

			foreach (Toolbar bar in activeToolbars)
			{
				var newTag = new TagCompound();
				bar.SaveData(newTag);
				tags.Add(newTag);
			}

			tag["Toolbars"] = tags;
			ThemeHandler.SaveData(tag);
		}

		/// <summary>
		/// Loads a layout from a TagCompound
		/// </summary>
		/// <param name="tag"></param>
		public static void LoadLayout(TagCompound tag)
		{
			activeToolbars.Clear();

			var tags = (List<TagCompound>)tag.GetList<TagCompound>("Toolbars");

			foreach (TagCompound loadedTag in tags)
			{
				var newBar = new Toolbar();
				newBar.LoadData(loadedTag);
				activeToolbars.Add(newBar);
			}

			activeToolbars.RemoveAll(n => n is null);

			ThemeHandler.LoadData(tag);

			UILoader.GetUIState<ToolbarState>().Refresh();
		}

		/// <summary>
		/// Exports the current toolbar layout to a file, given a string path.
		/// </summary>
		/// <param name="path">The path to export the file to</param>
		public static void ExportToFile(string path)
		{
			var tag = new TagCompound();
			SaveLayout(tag);

			if (!File.Exists(path))
			{
				FileStream stream = File.Create(path);
				stream.Close();
			}

			TagIO.ToFile(tag, path);
		}

		/// <summary>
		/// Loads a toolbar layout from a file.
		/// </summary>
		/// <param name="path">The path to the file to load from</param>
		public static void LoadFromFile(string path)
		{
			TagCompound tag = TagIO.FromFile(path);

			if (tag != null)
				LoadLayout(tag);
			else
				Main.NewText("Failed to load toolbar! File is missing or corrupted.", Color.Red);
		}

		/// <summary>
		/// Builds a preset and exports it to the layouts folder
		/// </summary>
		/// <param name="name">The name of your preset</param>
		/// <param name="build">The action to build your preset. You should add to the toolbars parameter passed to build out the preset.</param>
		public static void BuildPreset(string name, Action<List<Toolbar>> build, ThemeBoxProvider boxes, ThemeIconProvider icons)
		{
			activeToolbars.Clear();
			build(activeToolbars);
			ThemeHandler.currentBoxProvider = boxes;
			ThemeHandler.currentIconProvider = icons;
			ThemeHandler.currentColorProvider = new ThemeColorProvider();
			ExportToFile(Path.Join(Main.SavePath, "DragonLensLayouts", name));
		}

		/// <summary>
		/// Fallback loading for the default layout, if your current layout dosent exist or first time use.
		/// </summary>
		private static void LoadFallback()
		{
			FirstTimeSetupSystem.trueFirstTime = true;

			FirstTimeSetupSystem.SetupPresets();

			ThemeHandler.SetBoxProvider<SimpleBoxes>();
			ThemeHandler.SetIconProvider<DefaultIcons>();

			UILoader.GetUIState<ToolbarState>().Refresh();
		}
	}
}