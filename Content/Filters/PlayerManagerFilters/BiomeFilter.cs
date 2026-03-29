using DragonLens.Content.Tools.Multiplayer;
using DragonLens.Helpers;
using System;
using Terraria.GameContent.Bestiary;
using Terraria.Localization;

namespace DragonLens.Content.Filters.PlayerManagerFilters
{
	internal sealed class BiomeFilter : Filter
	{
		private readonly SpawnConditionBestiaryInfoElement biome;
		private readonly Func<Player, bool> matchesPlayer;
		private readonly string customName;
		private readonly Asset<Texture2D> customTexture;

		public BiomeFilter(SpawnConditionBestiaryInfoElement biome)
			: base(null, "", n => n is not PlayerManagerItem item || !BiomeHelper.MatchesBiome(item.player, biome))
		{
			this.biome = biome;
		}

		public BiomeFilter(string name, Asset<Texture2D> texture, Func<Player, bool> matchesPlayer)
			: base(null, "", n => n is not PlayerManagerItem item || !matchesPlayer(item.player))
		{
			customName = name;
			customTexture = texture;
			this.matchesPlayer = matchesPlayer;
		}

		public override string Name
		{
			get
			{
				if (customName != null)
					return customName;

				if (biome == BiomeHelper.ShimmerBiome)
					return "Aether";

				return biome != null ? Language.GetTextValue(biome.GetDisplayNameKey()) : "Unknown";
			}
		}
		public override string Description => "";

		public override void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			if (customTexture != null)
			{
				Texture2D tex = customTexture.Value;
				int widest = Math.Max(tex.Width, tex.Height);
				spriteBatch.Draw(tex, target.Center.ToVector2(), null, Color.White, 0f, tex.Size() * 0.5f, target.Width / (float)widest, SpriteEffects.None, 0f);
				return;
			}

			if (biome == null || !BiomeHelper.TryGetBestiaryIconDrawData(biome, out Asset<Texture2D> texture, out Rectangle source))
				return;

			float scale = Math.Min((float)target.Width / source.Width, (float)target.Height / source.Height);
			spriteBatch.Draw(texture.Value, target.Center.ToVector2(), source, Color.White, 0f, source.Size() * 0.5f, scale, SpriteEffects.None, 0f);
		}
	}
}