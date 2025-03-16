using DragonLens.Content.GUI;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameInput;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class FieldEditorMenu : SmartUIElement
	{
		public bool hide = false;

		public UIGrid modEditorList;
		public StyledScrollbar modEditorScroll;

		public TextField searchBar;

		public UserInterface userInterface;

		public FieldEditorMenu(UserInterface userInterface)
		{
			this.userInterface = userInterface;
			Width.Set(496, 0);
			Height.Set(590, 0);
		}

		public override void OnInitialize()
		{
			modEditorScroll = new(userInterface);
			modEditorScroll.Left.Set(480, 0);
			modEditorScroll.Top.Set(48, 0);
			modEditorScroll.Height.Set(540, 0);
			modEditorScroll.Width.Set(16, 0);
			Append(modEditorScroll);

			modEditorList = new();
			modEditorList.Width.Set(480, 0);
			modEditorList.Height.Set(540, 0);
			modEditorList.Top.Set(48, 0);
			modEditorList.ListPadding = 0;
			modEditorList.SetScrollbar(modEditorScroll);
			Append(modEditorList);

			searchBar = new();
			searchBar.Left.Set(312, 0);
			searchBar.Top.Set(10, 0);
			searchBar.Width.Set(180, 0);
			searchBar.Height.Set(32, 0);
			Append(searchBar);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (searchBar.updated)
			{
				foreach (UIElement element in modEditorList._items)
				{
					if (element is ObjectEditor container)
						container.Filter(searchBar.currentValue);
				}

				Recalculate();
				Recalculate();
			}
		}

		/// <summary>
		/// Places editors for each object passed into this element
		/// </summary>
		/// <param name="editing">The objects that editors should be generated for</param>
		public void SetEditing(object[] editing)
		{
			foreach (object obj in editing)
			{
				ObjectEditor container = new ObjectEditor(obj);
				if (container.modPlayerEditorList.Count > 1)
					modEditorList.Add(container);
			}
		}

		public void Clear()
		{
			modEditorList.Clear();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (hide)
				return;

			Vector2 pos = GetDimensions().Position();
			Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetGUIText("FieldEditor.ModdedFields"), pos + new Vector2(120, 30), Color.White, 1, 0f, 0.5f);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			spriteBatch.Draw(background, modEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);

			if (modEditorList.Count <= 0)
			{
				Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetGUIText("FieldEditor.NoFields"), pos + new Vector2(240, 310), Color.Gray, 1, 0.5f, 0.5f);
			}

			base.Draw(spriteBatch);
		}
	}
}