﻿using DragonLens.Content.Filters;
using DragonLens.Content.Filters.NPCFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using static Terraria.GameContent.Bestiary.Filters;

namespace DragonLens.Content.Tools.Spawners
{
	internal class NPCSpawner : BrowserTool<NPCBrowser>
	{
		public override string IconKey => "NPCSpawner";

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(NPCBrowser.selected is null ? 0 : NPCBrowser.selected.netID);
			writer.WriteVector2(Main.MouseWorld);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			int type = reader.ReadInt32();
			Vector2 pos = reader.ReadVector2();

			NPC.NewNPC(null, (int)pos.X, (int)pos.Y, type);

			if (Main.netMode == NetmodeID.Server && sender >= 0)
			{
				if (NPCBrowser.selected != null)
					NPCBrowser.selected.netID = type;

				Main.mouseX = (int)pos.X;
				Main.mouseY = (int)pos.Y;
				NetSend(-1, sender);
			}
		}

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.NPCSpawner.{key}", args);
		}
	}

	internal class NPCBrowser : Browser
	{
		public static NPC selected;
		public static UnlockableNPCEntryIcon preview;

		public override string Name => NPCSpawner.GetText("DisplayName");

		public override string IconTexture => "NPCSpawner";

		public override Vector2 DefaultPosition => new(0.4f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<NPCButton>();

			// -65 is the lowest negative netID, may need to be changed, unfortunately I dont think this is saved as a constant anywhere?
			for (int k = -65; k < NPCLoader.NPCCount; k++)
			{
				// `0` corresponds to NPCID.None - that is, no NPC.
				if (k == 0)
					continue;

				var npc = new NPC();
				npc.SetDefaults_ForNetId(k, 1);
				npc.netID = k;

				buttons.Add(new NPCButton(npc, this));
			}

			grid.AddRange(buttons);//causes most of the delay
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.NPCSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter(Assets.Filters.Vanilla, "Tools.NPCSpawner.Filters.Vanilla", n => !(n is NPCButton && (n as NPCButton).npc.ModNPC is null)) { isModFilter = true });

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModNPC>().Count() > 0))
			{
				filters.AddFilter(new NPCModFilter(mod));
			}

			filters.AddSeperator("Tools.NPCSpawner.FilterCategories.Type");
			filters.AddFilter(new Filter(Assets.Filters.Boss, "Tools.NPCSpawner.Filters.Boss", n => !(n is NPCButton && (n as NPCButton).npc.boss)));
			filters.AddFilter(new Filter(Assets.Filters.Critter, "Tools.NPCSpawner.Filters.Critter", n => !(n is NPCButton && (n as NPCButton).npc.CountsAsACritter)));

			filters.AddSeperator("Tools.NPCSpawner.FilterCategories.Hostility");
			filters.AddFilter(new Filter(Assets.Filters.Friendly, "Tools.NPCSpawner.Filters.Friendly", n => !(n is NPCButton && (n as NPCButton).npc.friendly)));
			filters.AddFilter(new Filter(Assets.Filters.Hostile, "Tools.NPCSpawner.Filters.Hostile", n => !(n is NPCButton && !(n as NPCButton).npc.friendly)));

			filters.AddSeperator("Tools.NPCSpawner.FilterCategories.Bestiary");

			foreach (IBestiaryEntryFilter bestiary in Main.BestiaryDB.Filters.Where(n => n is ByInfoElement))
			{
				filters.AddFilter(new BestiaryFilter(bestiary));
			}
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("ID", (a, b) => (a as NPCButton).npc.type - (b as NPCButton).npc.type));
			SortModes.Add(new("Alphabetical", (a, b) => a.Identifier.CompareTo(b.Identifier)));
			SortModes.Add(new("Damage", (a, b) => -1 * ((a as NPCButton).npc.damage - (b as NPCButton).npc.damage)));
			SortModes.Add(new("Life", (a, b) => -1 * ((a as NPCButton).npc.life - (b as NPCButton).npc.life)));
			SortModes.Add(new("Value", (a, b) => -1 * ((int)(a as NPCButton).npc.value - (int)(b as NPCButton).npc.value)));

			SortFunction = SortModes.First().Function;
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			base.DraggableUdpate(gameTime);

			if (selected != null)
				Main.LocalPlayer.mouseInterface = true;

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll($"DragonLens: {Name}");
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			base.SafeClick(evt);

			if (selected != null && !BoundingBox.Contains(Main.MouseScreen.ToPoint()) && !filters.IsMouseHovering)
			{
				PlayerInput.SetZoom_World();
				NPC.NewNPC(null, (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, selected.netID);
				ToolHandler.NetSend<NPCSpawner>();
				PlayerInput.SetZoom_UI();
			}
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			if (selected != null)
			{
				selected = null;
				preview = null;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (selected != null && preview != null)
			{
				var iconBox = new Rectangle((int)Main.MouseScreen.X + 16, (int)Main.MouseScreen.Y + 16, 64, 64);

				var info = new BestiaryUICollectionInfo
				{
					UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1
				};

				var settings = new EntryIconDrawSettings
				{
					iconbox = iconBox,
					IsPortrait = true
				};

				Rectangle newClip = iconBox;
				newClip.Inflate(-4, -4);

				Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
				spriteBatch.GraphicsDevice.ScissorRectangle = newClip;
				preview.Draw(info, spriteBatch, settings);
				spriteBatch.GraphicsDevice.ScissorRectangle = oldRect;
			}

			base.Draw(spriteBatch);
		}
	}

	internal class NPCButton : BrowserButton
	{
		public NPC npc;
		public BestiaryEntry entry;
		public string name;

		public UnlockableNPCEntryIcon icon;

		public override string Identifier => name;
		public override string Key => (npc.ModNPC?.Mod?.Name ?? "Terraria") + ":" + (npc.ModNPC?.Name ?? NPCID.Search.GetName(npc.type) + $"({npc.netID})");

		public NPCButton(NPC npc, Browser browser) : base(browser)
		{
			this.npc = npc;
			this.npc.IsABestiaryIconDummy = true;

			try
			{
				name = npc.TypeName;

				if (string.IsNullOrEmpty(name)) // Fall back to internal name
					name = npc?.ModNPC?.Name ?? "Unknown";
			}
			catch
			{
				Main.NewText(NPCSpawner.GetText("NameExceptionMessage", npc.ModNPC.Name, npc.ModNPC.Mod.DisplayName));
				name = NPCSpawner.GetText("NameException", npc.ModNPC.Mod.DisplayName);
			}

			icon = new(npc.netID);

			entry = Main.BestiaryDB.FindEntryByNPCID(npc.netID);

			OverrideSamplerState = SamplerState.PointClamp;
			UseImmediateMode = true;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			var info = new BestiaryUICollectionInfo
			{
				UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1
			};

			var settings = new EntryIconDrawSettings
			{
				iconbox = GetDimensions().ToRectangle(),
				IsPortrait = true
			};

			icon?.Update(info, GetDimensions().ToRectangle(), settings);

			base.SafeUpdate(gameTime);
		}

		public override void DrawSelf(SpriteBatch spriteBatch)
		{
			int size = (int)MathHelper.Clamp(parent.buttonSize, 36, 108);
			var iconBox = GetDimensions().ToRectangle();

			if (parent.listMode)
				iconBox.Width = size;

			var info = new BestiaryUICollectionInfo
			{
				UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1
			};

			var settings = new EntryIconDrawSettings
			{
				iconbox = iconBox,
				IsPortrait = true
			};

			Rectangle newClip = iconBox;
			newClip.Inflate(-4, -4);

			int infAmount = (int)(newClip.Width * Main.UIScale) - newClip.Width;
			newClip.Inflate(infAmount / 2, infAmount / 2);

			var offset = (newClip.Center() * Main.UIScale - newClip.Center()).ToPoint();
			newClip.Offset(offset);

			Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			var finalRect = Rectangle.Intersect(newClip, oldRect);

			spriteBatch.GraphicsDevice.ScissorRectangle = finalRect;

			if (icon != null)
			{
				try
				{
					npc.IsABestiaryIconDummy = true;
					icon?.Draw(info, spriteBatch, settings);
				}
				catch
				{
					icon = null;
					Main.NewText(NPCSpawner.GetText("BestiaryEntryException", npc.ModNPC.Name, npc.ModNPC.Mod.DisplayName));
				}
			}

			spriteBatch.GraphicsDevice.ScissorRectangle = oldRect;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip(NPCSpawner.GetText("NPCType", npc.type, npc.netID));
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			NPCBrowser.selected = (NPC)npc.Clone();
			NPCBrowser.preview = (UnlockableNPCEntryIcon)icon.CreateClone();
			Main.NewText(NPCSpawner.GetText("Selected", Identifier));
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			// TODO: This lacks the packet syncing included when left-clicking
			//       to select an NPC and then spawning at the cursor in-world.
			//       Related: NPCBrowser.SafeClick
			//       We could replicate the logic or stub the values.  Shrug.
			NPC.NewNPC(null, (int)Main.LocalPlayer.Center.X, (int)Main.LocalPlayer.Center.Y, npc.netID);
		}
	}
}