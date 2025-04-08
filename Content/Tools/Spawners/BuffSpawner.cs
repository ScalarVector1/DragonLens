using DragonLens.Content.Filters;
using DragonLens.Content.Filters.BuffFilters;
using DragonLens.Content.GUI;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class BuffSpawner : BrowserTool<BuffBrowser>
	{
		public override string IconKey => "BuffSpawner";

		public override void SendPacket(BinaryWriter writer)
		{
			writer.WriteVector2(Main.MouseWorld);
			writer.Write(BuffBrowser.selected);
			writer.Write(BuffBrowser.duration);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			Vector2 pos = reader.ReadVector2();
			int type = reader.ReadInt32();
			int duration = reader.ReadInt32();

			foreach (NPC npc in Main.npc)
			{
				Rectangle clickbox = npc.Hitbox;
				clickbox.Inflate(32, 32);

				if (clickbox.Contains(pos.ToPoint()))
				{
					npc.AddBuff(type, duration);
					break;
				}
			}

			if (Main.netMode == NetmodeID.Server && sender >= 0)
			{
				BuffBrowser.selected = type;
				BuffBrowser.duration = duration;
				Main.mouseX = (int)pos.X;
				Main.mouseY = (int)pos.Y;
				NetSend(-1, sender);
			}
		}

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.BuffSpawner.{key}", args);
		}
	}

	internal class BuffBrowser : Browser
	{
		public static int selected = -1;

		public static int duration = 180;

		IntEditor durationEditor;

		public override string Name => BuffSpawner.GetText("DisplayName");

		public override string IconTexture => "BuffSpawner";

		public override Vector2 DefaultPosition => new(0.1f, 0.4f);

		public override void PostInitialize()
		{
			durationEditor = new IntEditor(BuffSpawner.GetText("DurationEditor"), n => duration = n, 180);
			Append(durationEditor);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			base.AdjustPositions(newPos);

			durationEditor.Left.Set(newPos.X - 160, 0);
			durationEditor.Top.Set(newPos.Y, 0);
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<BuffButton>();
			// `0` doesn't correspond to anything - not even BuffID.None (which
			// doesn't exist). ObsidianSkin is the first buff in the game (`1`).
			for (int k = 1; k < BuffLoader.BuffCount; k++)
			{
				buttons.Add(new BuffButton(k, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.BuffSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter(Assets.Filters.Vanilla, "Tools.BuffSpawner.Filters.Vanilla", n => !(n is BuffButton && (n as BuffButton).type <= BuffID.Count)) { isModFilter = true });

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModBuff>().Count() > 0))
			{
				filters.AddFilter(new BuffModFilter(mod));
			}

			filters.AddSeperator("Tools.BuffSpawner.FilterCategories.Buff");
			filters.AddFilter(new Filter(Assets.Filters.Friendly, "Tools.BuffSpawner.Filters.Buff", n => !(n is BuffButton && !Main.debuff[(n as BuffButton).type])));
			filters.AddFilter(new Filter(Assets.Filters.Hostile, "Tools.BuffSpawner.Filters.Debuff", n => !(n is BuffButton && Main.debuff[(n as BuffButton).type])));
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("ID", (a, b) => (a as BuffButton).type - (b as BuffButton).type));
			SortModes.Add(new("Alphabetical", (a, b) => a.Identifier.CompareTo(b.Identifier)));

			SortFunction = SortModes.First().Function;
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			base.DraggableUdpate(gameTime);

			if (selected != -1)
				Main.LocalPlayer.mouseInterface = true;

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll($"DragonLens: {Name}");
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			base.SafeClick(evt);

			if (selected != -1)
			{
				foreach (NPC npc in Main.npc)
				{
					Rectangle clickbox = npc.Hitbox;
					clickbox.Inflate(32, 32);

					PlayerInput.SetZoom_World();

					if (clickbox.Contains(Main.MouseWorld.ToPoint()))
					{
						npc.AddBuff(selected, duration);
						Main.NewText(BuffSpawner.GetText("Applied", Lang.GetBuffName(selected), npc.FullName));
						break;
					}

					PlayerInput.SetZoom_UI();
				}

				ToolHandler.NetSend<BuffSpawner>();
			}
		}

		public override void SafeRightClick(UIMouseEvent evt)
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

				if (!BoundingBox.Contains(Main.MouseScreen.ToPoint()) && !filters.IsMouseHovering && !durationEditor.IsMouseHovering)
				{

					PlayerInput.SetZoom_World();

					spriteBatch.End();
					spriteBatch.Begin(default, default, default, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);

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

					spriteBatch.End();
					spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

					PlayerInput.SetZoom_UI();
				}

				spriteBatch.Draw(tex, Main.MouseScreen + Vector2.One * 8, new Rectangle(0, 0, tex.Width, tex.Height), Color.White * alpha, 0, default, 1, 0, 0);
			}

			// Set name here to receive game language selection changes in real time
			// This is a bit of a hack, but it works
			durationEditor.name = BuffSpawner.GetText("DurationEditor");

			base.Draw(spriteBatch);
		}
	}

	internal class BuffButton : BrowserButton
	{
		public int type;

		public override string Identifier => Lang.GetBuffName(type);
		public override string Key => (ModContent.GetModBuff(type)?.Mod?.Name ?? "Terraria") + ":" + (ModContent.GetModBuff(type)?.Name ?? BuffID.Search.GetName(type));

		public BuffButton(int type, Browser browser) : base(browser)
		{
			this.type = type;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Texture2D tex = Terraria.GameContent.TextureAssets.Buff[type]?.Value;

			if (tex is null)
				return;

			float scale = iconBox.Width / 32f;
			if (tex.Width > 32 || tex.Height > 32)
				scale *= 32f / Math.Max(tex.Width, tex.Height);

			spriteBatch.Draw(tex, iconBox.Center(), new Rectangle(0, 0, tex.Width, tex.Height), Color.White, 0, new Vector2(tex.Width, tex.Height) / 2, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Lang.GetBuffName(type));
				Tooltip.SetTooltip(Main.GetBuffTooltip(Main.LocalPlayer, type));
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			BuffBrowser.selected = type;
			Main.NewText(BuffSpawner.GetText("Selected", Lang.GetBuffName(type)));
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			Main.LocalPlayer.AddBuff(type, BuffBrowser.duration);
			Main.NewText(BuffSpawner.GetText("Applied", Lang.GetBuffName(type), Main.LocalPlayer.name));
		}
	}
}