using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class StyledScrollbar : Terraria.ModLoader.UI.Elements.FixedUIScrollbar
	{
		public static MethodInfo handleMethod = typeof(UIScrollbar).GetMethod("GetHandleRectangle", BindingFlags.NonPublic | BindingFlags.Instance);

		public StyledScrollbar(UserInterface userInterface) : base(userInterface) { }

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (CanScroll)
			{
				var back = GetDimensions().ToRectangle();
				back.Inflate(2, 2);

				GUIHelper.DrawBox(spriteBatch, back, ThemeHandler.BackgroundColor);

				var handle = (Rectangle)handleMethod.Invoke(this, null);
				handle.Width = (int)(GetDimensions().Width - 4);
				handle.Offset(2, 0);

				GUIHelper.DrawBox(spriteBatch, handle, ThemeHandler.ButtonColor);
			}
		}
	}
}