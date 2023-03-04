using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ThemeSystem
{
	public abstract class ThemeIconProvider
	{
		private readonly Dictionary<string, Texture2D> icons;

		private static readonly List<string> defaultKeysInner = new()
		{
			"BuffDespawner",
			"BuffSpawner",
			"Customize",
			"Difficulty",
			"DogMode",
			"DustDespawner",
			"DustSpawner",
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
			"MapTeleport",
			"Noclip",
			"NPCDespawner",
			"NPCSpawner",
			"Paint",
			"PlayerEditor",
			"ProjectileDespawner",
			"ProjectileSpawner",
			"RevealMap",
			"SpawnTool",
			"TileSpawner",
			"Time",
			"Weather"
		};

		/// <summary>
		/// A list of the default keys that need to be filled out by an IconProvider, provided for convenience for iteration.
		/// </summary>
		public ReadOnlyCollection<string> defaultKeys = new(defaultKeysInner);

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
				return ModContent.Request<Texture2D>("DragonLens/Assets/GUI/NoBox").Value;
		}
	}
}
