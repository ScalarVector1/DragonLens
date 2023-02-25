using DragonLens.Content.Filters;
using DragonLens.Content.Filters.BuffFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class BuffSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/BuffSpawner";

		public override string DisplayName => "Buff spawner";

		public override string Description => "Allows you to apply buffs to yourself or NPCs";

		public override void OnActivate()
		{
			BuffBrowser state = UILoader.GetUIState<BuffBrowser>();
			state.visible = !state.visible;

			if (!state.initialized)
			{
				UILoader.GetUIState<BuffBrowser>().Refresh();
				state.initialized = true;
			}
		}
	}

	internal class BuffBrowser : Browser
	{
		public static int selected = -1;

		public int duration = 180;

		public override string Name => "Buff spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/BuffSpawner";

		public override Vector2 DefaultPosition => new(0.1f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<BuffButton>();
			for (int k = 0; k < BuffLoader.BuffCount; k++)
			{
				buttons.Add(new BuffButton(k, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Mod filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanilla", "Vanilla", "Buffs from vanilla", n => !(n is BuffButton && (n as BuffButton).type <= BuffID.Count)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModBuff>().Count() > 0))
			{
				filters.AddFilter(new BuffModFilter(mod));
			}

			filters.AddSeperator("Buff type filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Friendly", "Buff", "Buffs with positive effects", n => !(n is BuffButton && !Main.debuff[(n as BuffButton).type])));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Hostile", "Debuff", "Buffs with negative effects", n => !(n is BuffButton && Main.debuff[(n as BuffButton).type])));
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			if (selected != -1)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);

			if (selected != -1)
			{
				foreach (NPC npc in Main.npc)
				{
					Rectangle clickbox = npc.Hitbox;
					clickbox.Inflate(32, 32);

					if (clickbox.Contains(Main.MouseWorld.ToPoint()))
					{
						npc.AddBuff(selected, duration);
						Main.NewText($"Applied {Lang.GetBuffName(selected)} to {npc.FullName}");
						break;
					}
				}
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (selected != -1)
				selected = -1;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (selected != -1)
			{
				Texture2D tex = Terraria.GameContent.TextureAssets.Buff[selected]?.Value;

				if (tex is null)
					return;

				float alpha = 0.5f;

				foreach (NPC npc in Main.npc)
				{
					Rectangle clickbox = npc.Hitbox;
					clickbox.Inflate(32, 32);

					if (clickbox.Contains(Main.MouseWorld.ToPoint()))
					{
						alpha = 1;
						Rectangle offset = clickbox;
						offset.Offset((-Main.screenPosition).ToPoint());
						Helpers.GUIHelper.DrawOutline(spriteBatch, offset, Color.Red);

						break;
					}
				}

				spriteBatch.Draw(tex, Main.MouseScreen + Vector2.One * 8, new Rectangle(0, 0, tex.Width, tex.Height), Color.White * alpha, 0, default, 1, 0, 0);
			}

			base.Draw(spriteBatch);
		}
	}

	internal class BuffButton : BrowserButton
	{
		public int type;

		public override string Identifier => Lang.GetBuffName(type);

		public BuffButton(int type, Browser browser) : base(browser)
		{
			this.type = type;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.Buff[type]?.Value;

			if (tex is null)
				return;

			float scale = 1;
			if (tex.Width > 32 || tex.Height > 32)
				scale = 32f / Math.Max(tex.Width, tex.Height);

			spriteBatch.Draw(tex, iconBox.Center(), new Rectangle(0, 0, tex.Width, tex.Height), Color.White, 0, new Vector2(tex.Width, tex.Height) / 2, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Lang.GetBuffName(type));
				Tooltip.SetTooltip(Main.GetBuffTooltip(Main.LocalPlayer, type));
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			BuffBrowser.selected = type;
			Main.NewText($"{Lang.GetBuffName(type)} selected, click an NPC to apply it to them. Right click to deselect. You can right click a buff in the browser to apply it to yourself instead.");
		}

		public override void RightClick(UIMouseEvent evt)
		{
			Main.LocalPlayer.AddBuff(type, 180);
			Main.NewText($"Applied {Lang.GetBuffName(type)} to {Main.LocalPlayer.name}");
		}

		public override int CompareTo(object obj)
		{
			return type - (obj as BuffButton).type;
		}
	}
}
