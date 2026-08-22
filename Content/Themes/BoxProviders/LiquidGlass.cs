using DragonLens.Core.Systems.ThemeSystem;
using ReLogic.Peripherals.RGB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonLens.Content.Themes.BoxProviders
{
	internal class LiquidGlass : ThemeBoxProvider
	{
		public override string NameKey => "Glass";

		public static Asset<Effect> effect = DragonLens.instance.Assets.Request<Effect>("Effects/LiquidGlassShader");

		public override void DrawBox(SpriteBatch spriteBatch, Rectangle target, Color color)
		{
			Texture2D tex = Assets.Themes.BoxProviders.LiquidGlass.NormalBox.Value;
			int cornerSize = 6;

			if (color == default)
				color = new Color(49, 84, 141) * 0.9f;

			var sourceCenter = new Rectangle(cornerSize, cornerSize, tex.Width - cornerSize * 2, tex.Height - cornerSize * 2);

			Rectangle inner = target;
			inner.Inflate(-cornerSize, -cornerSize);

			if (effect != null && effect.Value != null)
			{
				Effect shader = effect.Value;

				shader.Parameters["u_power"].SetValue(0.01f);
				shader.Parameters["u_color"].SetValue(color.ToVector4());
				shader.Parameters["back_t"].SetValue(Main.screenTarget);
				shader.Parameters["u_mouse"].SetValue(Main.MouseScreen * (Main.UIScale));
				shader.Parameters["u_scale"].SetValue(Matrix.CreateScale(1f / Main.UIScale));
				shader.Parameters["u_resolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

				shader.CurrentTechnique.Passes[0].Apply();

				RasterizerState r = new();
				r.ScissorTestEnable = true;

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, r, shader, Main.UIScaleMatrix);
			}

			spriteBatch.Draw(tex, inner, sourceCenter, color);

			spriteBatch.Draw(tex, new Rectangle(target.X + cornerSize, target.Y, target.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, 0, tex.Width - cornerSize * 2, tex.Height - cornerSize * 2), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X, target.Y + cornerSize, cornerSize, target.Height - cornerSize * 2), new Rectangle(0, cornerSize, cornerSize, tex.Height - cornerSize * 2), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X + cornerSize, target.Y + target.Height - cornerSize, target.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, tex.Height - cornerSize, cornerSize, tex.Height - cornerSize * 2), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X + target.Width - cornerSize, target.Y + cornerSize, cornerSize, target.Height - cornerSize * 2), new Rectangle(tex.Width - cornerSize, cornerSize, tex.Width - cornerSize * 2, cornerSize), color, 0, Vector2.Zero, 0, 0);

			spriteBatch.Draw(tex, new Rectangle(target.X, target.Y, cornerSize, cornerSize), new Rectangle(0, 0, cornerSize, cornerSize), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X + target.Width - cornerSize, target.Y, cornerSize, cornerSize), new Rectangle(tex.Width - cornerSize, 0, cornerSize, cornerSize), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X + target.Width - cornerSize, target.Y + target.Height - cornerSize, cornerSize, cornerSize), new Rectangle(tex.Width - cornerSize, tex.Height - cornerSize, cornerSize, cornerSize), color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X, target.Y + target.Height - cornerSize, cornerSize, cornerSize), new Rectangle(0, tex.Height - cornerSize, cornerSize, cornerSize), color, 0, Vector2.Zero, 0, 0);

			RasterizerState r2 = new();
			r2.ScissorTestEnable = true;

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, r2, default, Main.UIScaleMatrix);
		}

		public override void DrawBoxFancy(SpriteBatch spriteBatch, Rectangle target, Color color)
		{
			DrawBox(spriteBatch, target, color);
		}

		public override void DrawBoxSmall(SpriteBatch spriteBatch, Rectangle target, Color color)
		{
			DrawBox(spriteBatch, target, color);
		}

		public override void DrawOutline(SpriteBatch spriteBatch, Rectangle target, Color color)
		{
			Texture2D tex = Assets.Themes.BoxProviders.LiquidGlass.NormalBox.Value;

			if (color == default)
				color = new Color(49, 84, 141) * 0.9f;

			var sourceCorner = new Rectangle(0, 0, 6, 6);
			var sourceEdge = new Rectangle(6, 0, 4, 6);
			var sourceCenter = new Rectangle(6, 6, 4, 4);

			spriteBatch.Draw(tex, new Rectangle(target.X + 6, target.Y, target.Width - 12, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X, target.Y - 6 + target.Height, target.Height - 12, 6), sourceEdge, color, -(float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X - 6 + target.Width, target.Y + target.Height, target.Width - 12, 6), sourceEdge, color, (float)Math.PI, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 6, target.Height - 12, 6), sourceEdge, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);

			spriteBatch.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI, Vector2.Zero, 0, 0);
			spriteBatch.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI * 1.5f, Vector2.Zero, 0, 0);
		}
	}
}
