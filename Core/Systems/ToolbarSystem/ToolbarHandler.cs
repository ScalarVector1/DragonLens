using DragonLens.Content.GUI;
using DragonLens.Content.Tools;
using DragonLens.Core.Loaders.UILoading;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ToolbarSystem
{
	internal class ToolbarHandler : ModSystem
	{
		public static readonly List<Toolbar> activeToolbars = new();

		public override void OnModLoad()
		{
			string currentPath = Path.Join(Main.SavePath, "DragonLensLayouts", "Current");

			if (File.Exists(currentPath))
			{
				LoadFromFile(currentPath);
			}
			else
			{
				string dir = Path.Join(Main.SavePath, "DragonLensLayouts");
				Directory.CreateDirectory(dir);

				FileStream stream = File.Create(currentPath);
				stream.Close();

				LoadFallback();
				ExportToFile(currentPath);
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
		/// Fallback loading for the default layout, if your current layout dosent exist or first time use.
		/// </summary>
		private static void LoadFallback()
		{
			activeToolbars.Add(
				new Toolbar(new Vector2(0, 0.5f), Orientation.Vertical, AutomaticHideOption.Never)
				.AddTool<TestTool>()
				.AddTool<TestTool>()
				.AddTool<TestTool>()
				.AddTool<TestTool>()
				);
		}
	}
}
