using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class ProjectileSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ProjectileSpawner";

		public override string Name => "Projectile spawner";

		public override string Description => "Spawn projectiles, with options for setting velocity and other parameters";

		public override void OnActivate()
		{
			ProjectileBrowser state = UILoader.GetUIState<ProjectileBrowser>();
			state.visible = !state.visible;

			if (!state.initialized)
			{
				UILoader.GetUIState<ProjectileBrowser>().Refresh();
				state.initialized = true;
			}
		}
	}

	internal class ProjectileBrowser : Browser
	{
		public static Projectile selected;

		public Vector2 velocity;

		public override string Name => "Projectile spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/ProjectileSpawner";

		public override void PopulateGrid(UIGrid grid)
		{
			for (int k = 0; k < ProjectileLoader.ProjectileCount; k++)
			{
				var proj = new Projectile();
				proj.SetDefaults(k);

				grid.Add(new ProjectileButton(proj));
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			if (selected != null)
			{
				Projectile.NewProjectile(null, Main.MouseWorld, velocity, selected.type, selected.damage, selected.knockBack, Main.myPlayer);
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
				Main.instance.LoadProjectile(selected.type);
				Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[selected.type].Value;

				float scale = 1;
				if (tex.Width > 32 || tex.Height > 32)
					scale = 32f / Math.Max(tex.Width, tex.Height);

				spriteBatch.Draw(tex, Main.MouseScreen + Vector2.One * 8 + tex.Size(), new Rectangle(0, 0, tex.Width, tex.Height), Color.White * 0.5f, 0, new Vector2(tex.Width, tex.Height) / 2, scale, 0, 0);
			}

			base.Draw(spriteBatch);
		}
	}

	internal class ProjectileButton : BrowserButton
	{
		public Projectile proj;

		public override string Identifier => proj.Name;

		public ProjectileButton(Projectile proj)
		{
			this.proj = proj;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			Main.instance.LoadProjectile(proj.type);
			Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;

			float scale = 1;
			if (tex.Width > 32 || tex.Height > 32)
				scale = 32f / Math.Max(tex.Width, tex.Height);

			spriteBatch.Draw(tex, GetDimensions().Center(), new Rectangle(0, 0, tex.Width, tex.Height), Color.White, 0, new Vector2(tex.Width, tex.Height) / 2, scale, 0, 0);

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.LocalPlayer.cursorItemIconEnabled = true;
				Main.LocalPlayer.cursorItemIconText = proj.Name;
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			ProjectileBrowser.selected = proj;
			Main.NewText($"{proj.Name} selected, click anywhere in the world to spawn. Right click to deselect.");
		}

		public override void RightClick(UIMouseEvent evt)
		{

		}

		public override int CompareTo(object obj)
		{
			return proj.type - (obj as ProjectileButton).proj.type;
		}
	}
}
