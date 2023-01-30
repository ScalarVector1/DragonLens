using DragonLens.Core.Loaders.UILoading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class RemoveButton : UIElement
	{
		private readonly ToolButton parent;

		public RemoveButton(ToolButton parent)
		{
			this.parent = parent;

			Width.Set(16, 0);
			Height.Set(16, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			parent.parent.toolbar.toolList.Remove(parent.tool);
			UILoader.GetUIState<ToolbarState>().Refresh();
			UILoader.GetUIState<ToolbarState>().Customize();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Remove").Value;
			spriteBatch.Draw(tex, GetDimensions().Position(), Color.White);

			base.Draw(spriteBatch);
		}
	}
}
