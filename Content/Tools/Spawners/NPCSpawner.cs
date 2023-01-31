using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class NPCSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/NPCSpawner";

		public override string Name => "NPC spawner";

		public override string Description => "Spawn NPCs, from villagers to skeletons to bosses";

		public override void OnActivate()
		{
			NPCBrowser state = UILoader.GetUIState<NPCBrowser>();
			state.visible = !state.visible;

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

		public override string Name => "NPC spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/NPCSpawner";

		public override void PopulateGrid(UIGrid grid)
		{
			for (int k = 0; k < NPCLoader.NPCCount; k++)
			{
				var npc = new NPC();
				npc.SetDefaults(k);

				grid.Add(new NPCButton(npc));
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			if (selected != null)
			{
				NPC.NewNPC(null, (int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, selected.type);
				Main.isMouseLeftConsumedByUI = true;
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (selected != null)
				selected = null;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (selected != null)
			{
				selected.position = Main.MouseScreen + Vector2.One * 16;
				Main.instance.DrawNPCDirect(spriteBatch, selected, false, Vector2.Zero);
			}

			base.Draw(spriteBatch);
		}
	}

	internal class NPCButton : BrowserButton
	{
		public NPC npc;

		public UnlockableNPCEntryIcon icon;

		public override string Identifier => npc.FullName;

		public NPCButton(NPC npc)
		{
			this.npc = npc;
			icon = new(npc.type);

			OverrideSamplerState = Main.DefaultSamplerState;
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
			var info = new BestiaryUICollectionInfo
			{
				UnlockState = BestiaryEntryUnlockState.CanShowPortraitOnly_1
			};

			var settings = new EntryIconDrawSettings
			{
				iconbox = GetDimensions().ToRectangle(),
				IsPortrait = true
			};

			icon?.Draw(info, spriteBatch, settings);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.cursorItemIconEnabled = true;
				Main.LocalPlayer.cursorItemIconText = npc.FullName;
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			NPCBrowser.selected = (NPC)npc.Clone();
			Main.NewText($"{npc.FullName} selected, click anywhere in the world to spawn. Right click to deselect.");
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
