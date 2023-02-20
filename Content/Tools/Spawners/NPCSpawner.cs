using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
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

		public override string DisplayName => "NPC spawner";

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

		public override Vector2 DefaultPosition => new(0.4f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<NPCButton>();
			for (int k = 0; k < NPCLoader.NPCCount; k++)
			{
				var npc = new NPC();
				npc.SetDefaults_ForNetId(k, 1);

				buttons.Add(new NPCButton(npc, this));
			}

			grid.AddRange(buttons);//causes most of the delay
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

		public NPCButton(NPC npc, Browser browser) : base(browser)
		{
			this.npc = npc;
			icon = new(npc.type);

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
			icon?.Draw(info, spriteBatch, settings);
			spriteBatch.GraphicsDevice.ScissorRectangle = oldRect;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			if (IsMouseHovering)
			{
				Tooltip.SetName(npc.TypeName);
				Tooltip.SetTooltip($"Type: {npc.type}");
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
