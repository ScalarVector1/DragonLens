using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics.Renderers;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class StyledScrollbar : Terraria.ModLoader.UI.Elements.FixedUIScrollbar
	{
		public float oldValue;
		public int scrolledRecently;
		public static MethodInfo handleMethod = typeof(UIScrollbar).GetMethod("GetHandleRectangle", BindingFlags.NonPublic | BindingFlags.Instance);

		public StyledScrollbar(UserInterface userInterface) : base(userInterface) { }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			float value = GetValue();

			if (value != oldValue)
			{
				oldValue = value;
				scrolledRecently = 2;
			}

			if (scrolledRecently > 0)
			{
				Parent?.Recalculate();
				scrolledRecently--;
			}
		}

		public override void DrawSelf(SpriteBatch spriteBatch)
		{
			if (userInterface == null || !CanScroll)
				return;

			CalculatedStyle dimensions = GetDimensions();
			CalculatedStyle innerDimensions = GetInnerDimensions();
			if (_isDragging)
			{
				float num = UserInterface.ActiveInstance.MousePosition.Y - innerDimensions.Y - _dragYOffset;
				_viewPosition = MathHelper.Clamp(num / innerDimensions.Height * _maxViewSize, 0f, _maxViewSize - _viewSize);
			}

			Rectangle handleRectangle = GetHandleRectangle();
			Vector2 mousePosition = UserInterface.ActiveInstance.MousePosition;
			bool isHoveringOverHandle = _isHoveringOverHandle;
			_isHoveringOverHandle = base.IsMouseHovering && handleRectangle.Contains(new Point((int)mousePosition.X, (int)mousePosition.Y));
			if (!isHoveringOverHandle && _isHoveringOverHandle && Main.hasFocus)
			{
				SoundEngine.PlaySound(12);
			}

			Rectangle back = GetDimensions().ToRectangle();
			back.Inflate(2, 2);
			GUIHelper.DrawBox(spriteBatch, back, ThemeHandler.BackgroundColor);

			Rectangle handle = (Rectangle)handleMethod.Invoke(this, null);
			handle.Width = (int)GetDimensions().Width - 4;
			handle.Offset(2, 0);

			GUIHelper.DrawBox(spriteBatch, handle, ThemeHandler.ButtonColor);
		}
	}
}