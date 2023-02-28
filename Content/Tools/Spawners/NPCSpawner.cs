using DragonLens.Configs;
using DragonLens.Content.Filters;
using DragonLens.Content.Filters.NPCFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using static Terraria.GameContent.Bestiary.Filters;

namespace DragonLens.Content.Tools.Spawners
{
	internal class NPCSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/NPCSpawner";

		public override string DisplayName => "NPC spawner";

		public override string Description => "Spawn NPCs, from villagers to skeletons to bosses";

		public override void OnActivate()
		{
			NPCBrowser state = UILoader.GetUIState<NPCBrowser>();
			state.visible = !state.visible;

			BrowserButton.drawDelayTimer = 2;

			if (!state.initialized)
			{
				UILoader.GetUIState<NPCBrowser>().Refresh();
				state.initialized = true;
			}
		}
	}

	internal class NPCBrowser : Browser
	{
		public static NPC selected;
		public static UnlockableNPCEntryIcon preview;

		public override string Name => "NPC spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/NPCSpawner";

		public override Vector2 DefaultPosition => new(0.4f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<NPCButton>();
			// `0` corresponds to NPCID.None - that is, no NPC.
			for (int k = 1; k < NPCLoader.NPCCount; k++)
			{
				var npc = new NPC();
				npc.SetDefaults_ForNetId(k, 1);

				buttons.Add(new NPCButton(npc, this));
			}

			grid.AddRange(buttons);//causes most of the delay
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Mod filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanilla", "Vanilla", "NPCs from the base game", n => !(n is NPCButton && (n as NPCButton).npc.ModNPC is null)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModNPC>().Count() > 0))
			{
				filters.AddFilter(new NPCModFilter(mod));
			}

			filters.AddSeperator("Type filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Boss", "Boss", "NPCs which are bosses", n => !(n is NPCButton && (n as NPCButton).npc.boss)));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Critter", "Critter", "NPCs which count as critters", n => !(n is NPCButton && (n as NPCButton).npc.CountsAsACritter)));

			filters.AddSeperator("Hostility filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Friendly", "Friendly", "NPCs which are friendly", n => !(n is NPCButton && (n as NPCButton).npc.friendly)));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Hostile", "Hostile", "Enemy NPCs", n => !(n is NPCButton && !(n as NPCButton).npc.friendly)));

			filters.AddSeperator("Bestiary filters");

			foreach (IBestiaryEntryFilter bestiary in Main.BestiaryDB.Filters.Where(n => n is ByInfoElement))
			{
				filters.AddFilter(new BestiaryFilter(bestiary));
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			if (selected != null)
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Click(UIMouseEvent evt)
		{
			base.Click(evt);

			if (selected != null)
				NPC.NewNPC(null, (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, selected.type);
		}

		public override void RightClick(UIMouseEvent evt)
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

		public NPCButton(NPC npc, Browser browser) : base(browser)
		{
			this.npc = npc;
			this.npc.IsABestiaryIconDummy = true;

			try
			{
				name = npc.TypeName;
			}
			catch
			{
				Main.NewText($"A NPCs ({npc.ModNPC.Name}) name threw an exception while getting it! Report to {npc.ModNPC.Mod.DisplayName} developers!");
				name = $"This NPCs name threw an exception while getting it! Report to {npc.ModNPC.Mod.DisplayName} developers!";
			}

			icon = new(npc.type);

			entry = Main.BestiaryDB.FindEntryByNPCID(npc.type);

			OverrideSamplerState = SamplerState.PointClamp;
			UseImmediateMode = true;
		}

		public override void Update(GameTime gameTime)
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

			base.Update(gameTime);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);
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

			Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
			spriteBatch.GraphicsDevice.ScissorRectangle = newClip;

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
					Main.NewText($"An NPC ({npc.ModNPC.Name}) throws an exception while drawing its bestiary entry! Report to {npc.ModNPC.Mod.DisplayName} developers!");
				}
			}

			spriteBatch.GraphicsDevice.ScissorRectangle = oldRect;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip($"Type: {npc.type}");
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			NPCBrowser.selected = (NPC)npc.Clone();
			NPCBrowser.preview = (UnlockableNPCEntryIcon)icon.CreateClone();
			Main.NewText($"{Identifier} selected, click anywhere in the world to spawn. Right click to deselect.");
		}

		public override void RightClick(UIMouseEvent evt)
		{

		}

		public override int CompareTo(object obj)
		{
			return npc.type - (obj as NPCButton).npc.type;
		}
	}
}
