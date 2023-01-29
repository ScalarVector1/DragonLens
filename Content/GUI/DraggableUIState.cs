using DragonLens.Core.Loaders.UILoading;
using Microsoft.Xna.Framework;
using Terraria;

namespace DragonLens.Content.GUI
{
	public abstract class DraggableUIState : SmartUIState
	{
		public Vector2 basePos;

		public bool dragging;
		public Vector2 dragOff;

		public abstract Rectangle DragBox { get; }

		public virtual void SafeUpdate(GameTime gameTime) { }

		public virtual void AdjustPositions(Vector2 newPos) { }

		public sealed override void Update(GameTime gameTime)
		{
			if (DragBox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft)
			{
				if (dragOff == Vector2.Zero)
					dragOff = Main.MouseScreen - basePos;

				basePos = Main.MouseScreen - dragOff;
			}
			else
			{
				dragOff = Vector2.Zero;
			}

			AdjustPositions(basePos);
			Recalculate();

			SafeUpdate(gameTime);
		}
	}
}
