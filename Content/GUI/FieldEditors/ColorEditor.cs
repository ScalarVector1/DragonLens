using DragonLens.Configs;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class ColorEditor : FieldEditor<Color>
	{
		public Slider rSlider;
		public Slider gSlider;
		public Slider bSlider;
		public Slider aSlider;

		public ColorEditor(string name, Action<Color> onValueChanged, Color initialValue) : base(130, name, onValueChanged, initialValue)
		{
			rSlider = new Slider("DragonLens/Assets/GUI/RedScale", n =>
			{
				onValueChanged(new Color(n, gSlider.progress, bSlider.progress, aSlider.progress));
				value = new Color(n, gSlider.progress, bSlider.progress, aSlider.progress);
			});
			rSlider.Left.Set(10, 0);
			rSlider.Top.Set(32, 0);
			rSlider.progress = initialValue.R / 255f;
			Append(rSlider);

			gSlider = new Slider("DragonLens/Assets/GUI/GreenScale", n =>
			{
				onValueChanged(new Color(rSlider.progress, n, bSlider.progress, aSlider.progress));
				value = new Color(rSlider.progress, n, bSlider.progress, aSlider.progress);
			});
			gSlider.Left.Set(10, 0);
			gSlider.Top.Set(56, 0);
			gSlider.progress = initialValue.G / 255f;
			Append(gSlider);

			bSlider = new Slider("DragonLens/Assets/GUI/BlueScale", n =>
			{
				onValueChanged(new Color(rSlider.progress, gSlider.progress, n, aSlider.progress));
				value = new Color(rSlider.progress, gSlider.progress, n, aSlider.progress);
			});
			bSlider.Left.Set(10, 0);
			bSlider.Top.Set(80, 0);
			bSlider.progress = initialValue.B / 255f;
			Append(bSlider);

			aSlider = new Slider("DragonLens/Assets/GUI/AlphaScale", n =>
			{
				onValueChanged(new Color(rSlider.progress, gSlider.progress, bSlider.progress, n));
				value = new Color(rSlider.progress, gSlider.progress, bSlider.progress, n);
			});
			aSlider.Left.Set(10, 0);
			aSlider.Top.Set(104, 0);
			aSlider.progress = initialValue.A / 255f;
			Append(aSlider);
		}

		public override void SafeDraw(SpriteBatch sprite)
		{
			base.SafeDraw(sprite);

			Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var target = GetDimensions().ToRectangle();
			target.Width = 16;
			target.Height = 16;
			target.Offset(124, 8);
			sprite.Draw(tex, target, value);
		}
	}

	internal class Slider : UIElement
	{
		public bool dragging;
		public float progress;

		public string texture;
		public Action<float> onChanged;

		public Slider(string texture, Action<float> onChanged)
		{
			Width.Set(130, 0);
			Height.Set(16, 0);

			this.texture = texture;
			this.onChanged = onChanged;
		}

		public override void Update(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);
				onChanged(progress);

				if (!Main.mouseLeft)
					dragging = false;
			}

			base.Update(gameTime);
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>(texture).Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 6, dims.Y - 8, 12, 24);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ModContent.GetInstance<GUIConfig>().buttonColor);
		}
	}
}
