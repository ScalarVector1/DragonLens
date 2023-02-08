using DragonLens.Core.Loaders.UILoading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace DragonLens.Content.GUI
{
	public abstract class DraggableUIState : SmartUIState
	{
		private UIImageButton closeButton;

		public Vector2 basePos;

		public bool dragging;
		public Vector2 dragOff;
		public bool visible;

		public int width;
		public int height;

		public abstract Rectangle DragBox { get; }

		public Rectangle BoundingBox => new((int)basePos.X, (int)basePos.Y, width, height);

		public override bool Visible => visible;

		public virtual void AdjustPositions(Vector2 newPos) { }

		public virtual void SafeOnInitialize() { }

		public virtual void SafeUpdate(GameTime gameTime)
		{
			base.Update(gameTime);
		}

		public sealed override void OnInitialize()
		{
			closeButton = new UIImageButton(ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Remove"));
			closeButton.Width.Set(16, 0);
			closeButton.Height.Set(16, 0);
			closeButton.OnClick += (a, b) => visible = false;
			Append(closeButton);

			SafeOnInitialize();
		}

		public sealed override void Update(GameTime gameTime)
		{
			if (!Main.mouseLeft && dragging)
				dragging = false;

			if (DragBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft || dragging)
			{
				dragging = true;

				if (dragOff == Vector2.Zero)
					dragOff = Main.MouseScreen - basePos;

				basePos = Main.MouseScreen - dragOff;
			}
			else
			{
				dragOff = Vector2.Zero;
			}

			closeButton.Left.Set(basePos.X + width - 24, 0);
			closeButton.Top.Set(basePos.Y + 8, 0);

			AdjustPositions(basePos);
			Recalculate();

			SafeUpdate(gameTime);
		}
	}
}
