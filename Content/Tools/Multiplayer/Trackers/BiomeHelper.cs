using System.Reflection;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Biomes = Terraria.GameContent.Bestiary.BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes; // keep this, it's important for filter setup

namespace DragonLens.Content.Tools.Multiplayer
{
	internal static class BiomeHelper
	{
		private static readonly FieldInfo FilterIconFrameInfo = typeof(FilterProviderInfoElement).GetField("_filterIconFrame", BindingFlags.NonPublic | BindingFlags.Instance);
		internal static readonly SpawnConditionBestiaryInfoElement ShimmerBiome = new("Mods.DragonLens.Biomes.Shimmer", 0, "DragonLens/Assets/GUI/ShimmerBackground");
		internal readonly struct PlayerBiomeVisual
		{
			public readonly SpawnConditionBestiaryInfoElement BestiaryBiome;
			public readonly Color BackgroundColor;

			public PlayerBiomeVisual(SpawnConditionBestiaryInfoElement bestiaryBiome, Color backgroundColor)
			{
				BestiaryBiome = bestiaryBiome;
				BackgroundColor = backgroundColor;
			}
		}

		internal static PlayerBiomeVisual GetBiomeVisual(Player player)
		{
			if (player.ZoneShimmer)
				return new(ShimmerBiome, Color.White);

			int tileX = (int)(player.Center.X / 16f);
			int tileY = (int)(player.Center.Y / 16f);
			Color color = player.dead ? new Color(50, 50, 50, 255) : Color.White;

			if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
				return new(Biomes.Surface, color);

			Tile tile = Main.tile[tileX, tileY];
			if (tile == null)
				return new(Biomes.Surface, color);

			int wall = tile.WallType;
			bool ocean = player.ZoneOverworldHeight && (tileX < 380 || tileX > Main.maxTilesX - 380);
			bool underground = player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight;

			if (player.ZoneUnderworldHeight)
				return new(Biomes.TheUnderworld, color);

			if (player.ZoneDungeon)
				return new(Biomes.TheDungeon, color);

			if (wall == 87)
				return new(Biomes.SpiderNest, color);

			if (underground)
			{
				if (wall is 86 or 108)
					return new(Biomes.Granite, color);

				if (wall is 180 or 184)
					return new(Biomes.Marble, color);

				if (player.ZoneGlowshroom)
					return new(Biomes.UndergroundMushroom, color);

				if (player.ZoneCorrupt)
				{
					if (player.ZoneDesert)
						return new(Biomes.CorruptUndergroundDesert, color);

					if (player.ZoneSnow)
						return new(Biomes.CorruptIce, color);

					return new(Biomes.UndergroundCorruption, color);
				}

				if (player.ZoneCrimson)
				{
					if (player.ZoneDesert)
						return new(Biomes.CrimsonUndergroundDesert, color);

					if (player.ZoneSnow)
						return new(Biomes.CrimsonIce, color);

					return new(Biomes.UndergroundCrimson, color);
				}

				if (player.ZoneHallow)
				{
					if (player.ZoneDesert)
						return new(Biomes.HallowUndergroundDesert, color);

					if (player.ZoneSnow)
						return new(Biomes.HallowIce, color);

					return new(Biomes.UndergroundHallow, color);
				}

				if (player.ZoneSnow)
					return new(Biomes.UndergroundSnow, color);

				if (player.ZoneJungle)
					return new(Biomes.UndergroundJungle, color);

				if (player.ZoneDesert)
					return new(Biomes.UndergroundDesert, color);

				if (player.ZoneRockLayerHeight)
					return new(Biomes.Caverns, color);

				return new(Biomes.Underground, color);
			}
			if (player.ZoneGlowshroom)
				return new(Biomes.SurfaceMushroom, color);

			if (player.ZoneSkyHeight)
				return new(Biomes.Sky, color);

			if (player.ZoneCorrupt)
				return new(player.ZoneDesert ? Biomes.CorruptDesert : Biomes.TheCorruption, color);

			if (player.ZoneCrimson)
				return new(player.ZoneDesert ? Biomes.CrimsonDesert : Biomes.TheCrimson, color);

			if (player.ZoneHallow)
				return new(player.ZoneDesert ? Biomes.HallowDesert : Biomes.TheHallow, color);

			if (ocean)
				return new(Biomes.Ocean, color);

			if (player.ZoneSnow)
				return new(Biomes.Snow, color);

			if (player.ZoneJungle)
				return new(Biomes.Jungle, color);

			if (player.ZoneDesert)
				return new(Biomes.Desert, color);

			if (player.ZoneGraveyard)
				return new(Biomes.Graveyard, color);

			if (Main.bloodMoon)
				return new(Biomes.Surface, color * 2f);

			return new(Biomes.Surface, color);
		}
		internal static bool MatchesBiome(Player player, SpawnConditionBestiaryInfoElement biome)
		{
			return player != null && player.active && GetBiomeVisual(player).BestiaryBiome == biome;
		}

		internal static bool TryGetBestiaryIconDrawData(SpawnConditionBestiaryInfoElement biome, out Asset<Texture2D> texture, out Rectangle source)
		{
			// Special-case for shimmer.
			if (biome == ShimmerBiome)
			{
				texture = Assets.Filters.Shimmer;
				source = texture.Value.Frame();
				return true;
			}

			// A spritesheet of all the bestiary (biome) icons.
			texture = Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Icon_Tags_Shadow");

			source = default;

			if (biome == null)
				return false;

			object frame = FilterIconFrameInfo?.GetValue(biome);

			if (frame is Point point)
			{
				source = texture.Frame(16, 5, point.X, point.Y);
				return true;
			}

			if (frame is int index)
			{
				source = texture.Frame(16, 5, index % 16, index / 16);
				return true;
			}

			return false;
		}
	}
}
