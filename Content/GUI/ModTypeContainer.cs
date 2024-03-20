using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.UI.Elements;

namespace DragonLens.Content.GUI
{
	internal class ModTypeSeperator : SmartUIElement
	{
		readonly string message;

		public ModTypeSeperator(string message)
		{
			this.message = message;
			Width.Set(480, 0);
			Height.Set(32, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = GetDimensions().ToRectangle();
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, message, GetDimensions().ToRectangle().TopLeft() + Vector2.One * 4, Color.White, 0.8f);
		}
	}

	internal class ModTypeContainer : SmartUIElement
	{
		public UIGrid modPlayerEditorList;
		public ModType modType;

		private float height = 32;

		private string nameOverride;

		public string Label => nameOverride == string.Empty ? modType.Mod.DisplayName + ": " + modType.Name : nameOverride;

		public ModTypeContainer(ModType modType, string nameOverride = "")
		{
			this.modType = modType;
			this.nameOverride = nameOverride;

			modPlayerEditorList = new();
			modPlayerEditorList.Add(new ModTypeSeperator(Label));

			modPlayerEditorList.Width.Set(480, 0);

			Main.NewText(Label);

			if (modType != null)
			{
				//TODO: some sort of GetEditor generic or something so we dont have to do... this
				foreach (FieldInfo t in modType.GetType().GetFields())
				{
					TryAddEditor<bool, BoolEditor>(t, modType);
					TryAddEditor<int, IntEditor>(t, modType);
					TryAddEditor<float, FloatEditor>(t, modType);
					TryAddEditor<Vector2, Vector2Editor>(t, modType);
					TryAddEditor<Color, ColorEditor>(t, modType);
					TryAddEditor<string, StringEditor>(t, modType);
					TryAddEditor<NPC, NPCEditor>(t, modType);
					TryAddEditor<Projectile, ProjectileEditor>(t, modType);
					TryAddEditor<Player, PlayerEditor>(t, modType);
				}

				foreach (PropertyInfo t in modType.GetType().GetProperties().Where(n => n.SetMethod != null))
				{
					if (t.Name == "Entity")
						continue;

					TryAddEditor<bool, BoolEditor>(t, modType);
					TryAddEditor<int, IntEditor>(t, modType);
					TryAddEditor<float, FloatEditor>(t, modType);
					TryAddEditor<Vector2, Vector2Editor>(t, modType);
					TryAddEditor<Color, ColorEditor>(t, modType);
					TryAddEditor<string, StringEditor>(t, modType);
					TryAddEditor<NPC, NPCEditor>(t, modType);
					TryAddEditor<Projectile, ProjectileEditor>(t, modType);
					TryAddEditor<Player, PlayerEditor>(t, modType);
				}
			}

			float tallest = 0;
			for(int k = 0; k < modPlayerEditorList.Count - 1; k++)
			{
				if (k > 0 && k % 3 == 0)
				{
					height += tallest + modPlayerEditorList.ListPadding;
					tallest = 0;
				}

				var item = modPlayerEditorList._items[k + 1];
				if(item.Height.Pixels > tallest)
					tallest = item.Height.Pixels;
			}

			height += tallest + modPlayerEditorList.ListPadding;

			Width.Set(480, 0);
			Height.Set(height, 0);
			MaxHeight.Set(height, 0);

			modPlayerEditorList.Height.Set(height, 0);
			modPlayerEditorList.MaxHeight.Set(height, 0);

			Append(modPlayerEditorList);
		}

		private void TryAddEditor<T, E>(FieldInfo t, object mt) where E : FieldEditor<T>
		{
			if (t.FieldType == typeof(T))
			{
				try
				{
					string message = LocalizationHelper.GetToolText("PlayerEditor.AutogenMsg");

					var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(mt, n)), (T)t.GetValue(mt), () => (T)t.GetValue(mt), message });
					modPlayerEditorList.Add(newEditor);
				}
				catch
				{
					Console.WriteLine($"Error while attempting to add editor for field {t?.Name ?? "Unknown"}");
				}
			}
		}

		private void TryAddEditor<T, E>(PropertyInfo t, object mt) where E : FieldEditor<T>
		{
			if (t.PropertyType == typeof(T))
			{
				try
				{
					string message = LocalizationHelper.GetToolText("PlayerEditor.AutogenMsg");

					var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(mt, n)), (T)t.GetValue(mt), () => (T)t.GetValue(mt), message });
					modPlayerEditorList.Add(newEditor);
				}
				catch
				{
					Console.WriteLine($"Error while attempting to add editor for field {t?.Name ?? "Unknown"}");
				}
			}
		}

		public override int CompareTo(object obj)
		{
			if (obj is ModTypeContainer container)
				return Label.CompareTo(container.Label);

			return base.CompareTo(obj);
		}
	}
}
