using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class ProjectileSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ProjectileSpawner";

		public override string DisplayName => "Projectile spawner";

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

		public override Vector2 DefaultPosition => new(0.5f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<ProjectileButton>();
			for (int k = 0; k < ProjectileLoader.ProjectileCount; k++)
			{
				var proj = new Projectile();
				proj.SetDefaults(k);

				buttons.Add(new ProjectileButton(proj, this));
			}

			grid.AddRange(buttons);
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
				Projectile.NewProjectile(null, Main.MouseWorld, velocity, selected.type, selected.damage, selected.knockBack, Main.myPlayer);
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

		public ProjectileButton(Projectile proj, Browser browser) : base(browser)
		{
			this.proj = proj;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.instance.LoadProjectile(proj.type);
			Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;

			float scale = 1;
			if (tex.Width > 32 || tex.Height > 32)
				scale = 32f / Math.Max(tex.Width, tex.Height);

			spriteBatch.Draw(tex, iconBox.Center(), new Rectangle(0, 0, tex.Width, tex.Height), Color.White, 0, new Vector2(tex.Width, tex.Height) / 2, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(proj.Name);
				Tooltip.SetTooltip($"Type: {proj.type}");
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
