using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DragonLens.Core.Systems.ThemeSystem
{
	public abstract class ThemeIconProvider
	{
		private readonly Dictionary<string, Texture2D> icons;

		private static readonly List<string> defaultKeysInner = new()
		{
			"AccessoryTray",
			"BuffDespawner",
			"BuffSpawner",
			"Customize",
			"Difficulty",
			"DogMode",
			"DustDespawner",
			"DustSpawner",
			"EntityEditor",
			"FastForward",
			"FreeCamera",
			"GodMode",
			"GoreDespawner",
			"HideMap",
			"Hitboxes",
			"InfiniteReach",
			"ItemDespawner",
			"ItemEditor",
			"ItemSpawner",
			"Lighting",
			"LockCamera",
			"Magnet",
			"MapTeleport",
			"Noclip",
			"NPCDespawner",
			"NPCSpawner",
			"Paint",
			"PlayerEditor",
			"PlayerManager",
			"ProjectileDespawner",
			"ProjectileSpawner",
			"RevealMap",
			"SoundSpawner",
			"SpawnTool",
			"SystemEditor",
			"TileSpawner",
			"Time",
			"VoidMagnet",
			"Weather"
		};

		/// <summary>
		/// A list of the default keys that need to be filled out by an IconProvider, provided for convenience for iteration.
		/// </summary>
		public ReadOnlyCollection<string> defaultKeys = new(defaultKeysInner);

		/// <summary>
		/// The name of this icon provider
		/// </summary>
		public abstract string NameKey { get; }

		public string Name => Helpers.LocalizationHelper.GetText($"{NameKey}Icons.Name");

		public string Description => Helpers.LocalizationHelper.GetText($"{NameKey}Icons.Description");

		public ThemeIconProvider()
		{
			icons = new Dictionary<string, Texture2D>();
			PopulateIcons(icons);
		}

		public abstract void PopulateIcons(Dictionary<string, Texture2D> icons);

		public Texture2D GetIcon(string key)
		{
			if (icons.ContainsKey(key))
				return icons[key];
			else
				return Assets.GUI.NoBox.Value;
		}
	}
}