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
using Terraria.UI;

namespace DragonLens.Content.GUI
{
	internal class ObjectEditorLabel : SmartUIElement
	{
		readonly string message;

		public ObjectEditorLabel(string message)
		{
			this.message = message;
			Width.Set(480, 0);
			Height.Set(32, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (Parent.Parent.Parent.Height.Pixels <= 0)
				return;

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = GetDimensions().ToRectangle();
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, message, GetDimensions().ToRectangle().TopLeft() + Vector2.One * 4, Color.White, 0.8f);
		}
	}

	internal class ObjectEditor : SmartUIElement
	{
		public UIGrid modPlayerEditorList;
		public object obj;

		private float height = 32;

		private string nameOverride;

		public string smartLabel => obj is ModType mt ? mt.Mod.Name + ": " + mt.Name : obj.GetType().Name;
		public string Label => nameOverride == string.Empty ? smartLabel : nameOverride;

		public ObjectEditor(object obj, string nameOverride = "")
		{
			this.obj = obj;
			this.nameOverride = nameOverride;

			modPlayerEditorList = new();
			modPlayerEditorList.Add(new ObjectEditorLabel(Label));

			modPlayerEditorList.Width.Set(480, 0);

			if (obj != null)
			{
				//TODO: some sort of GetEditor generic or something so we dont have to do... this
				foreach (FieldInfo fieldInfo in obj.GetType().GetFields())
				{
					TryAddEditor<bool, BoolEditor>(fieldInfo, obj);
					TryAddEditor<int, IntEditor>(fieldInfo, obj);
					TryAddEditor<float, FloatEditor>(fieldInfo, obj);
					TryAddEditor<Vector2, Vector2Editor>(fieldInfo, obj);
					TryAddEditor<Color, ColorEditor>(fieldInfo, obj);
					TryAddEditor<string, StringEditor>(fieldInfo, obj);
					TryAddEditor<NPC, NPCEditor>(fieldInfo, obj);
					TryAddEditor<Projectile, ProjectileEditor>(fieldInfo, obj);
					TryAddEditor<Player, PlayerEditor>(fieldInfo, obj);
					TryAddEditor<Item, ItemEditor>(fieldInfo, obj);
				}

				foreach (PropertyInfo propInfo in obj.GetType().GetProperties().Where(n => n.SetMethod != null))
				{
					if (propInfo.Name == "Entity")
						continue;

					TryAddEditor<bool, BoolEditor>(propInfo, obj);
					TryAddEditor<int, IntEditor>(propInfo, obj);
					TryAddEditor<float, FloatEditor>(propInfo, obj);
					TryAddEditor<Vector2, Vector2Editor>(propInfo, obj);
					TryAddEditor<Color, ColorEditor>(propInfo, obj);
					TryAddEditor<string, StringEditor>(propInfo, obj);
					TryAddEditor<NPC, NPCEditor>(propInfo, obj);
					TryAddEditor<Projectile, ProjectileEditor>(propInfo, obj);
					TryAddEditor<Player, PlayerEditor>(propInfo, obj);
					TryAddEditor<Item, ItemEditor>(propInfo, obj);
				}
			}

			CalcHeight();

			Append(modPlayerEditorList);
		}

		private void CalcHeight()
		{
			height = 36;

			float tallest = 0;
			for (int k = 0; k < modPlayerEditorList.Count - 1; k++)
			{
				if (k > 0 && k % 3 == 0)
				{
					if (tallest != 0)
						height += tallest + modPlayerEditorList.ListPadding;
					
					tallest = 0;
				}

				var item = modPlayerEditorList._items[k + 1];
				if (item.Height.Pixels > tallest)
					tallest = item.Height.Pixels;
			}

			if (tallest != 0)
				height += tallest + modPlayerEditorList.ListPadding;

			Width.Set(480, 0);
			Height.Set(height, 0);
			MaxHeight.Set(height, 0);

			modPlayerEditorList.Height.Set(height, 0);
			modPlayerEditorList.MaxHeight.Set(height, 0);
		}

		private void TryAddEditor<T, E>(FieldInfo t, object mt) where E : FieldEditor<T>
		{
			if (t.FieldType == typeof(T))
			{
				try
				{
					string message = LocalizationHelper.GetToolText("PlayerEditor.AutogenMsg");

					var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(mt, n)), (T)t.GetValue(mt), () => (T)t.GetValue(mt), message });

					if (t.IsStatic)
						newEditor.isStatic = true;

					if (t.IsLiteral || t.IsInitOnly)
						newEditor.isLocked = true;
					
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
			if (t.PropertyType == typeof(T) && t.CanRead)
			{
				try
				{
					string message = LocalizationHelper.GetToolText("PlayerEditor.AutogenMsg");

					var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(mt, n)), (T)t.GetValue(mt), () => (T)t.GetValue(mt), message });

					if (t.GetGetMethod().IsStatic)
						newEditor.isStatic = true;

					if (!t.CanWrite)
						newEditor.isLocked = true;

					modPlayerEditorList.Add(newEditor);
				}
				catch
				{
					Console.WriteLine($"Error while attempting to add editor for field {t?.Name ?? "Unknown"}");
				}
			}
		}

		/// <summary>
		/// Filters out editors whos name does not match a search term
		/// </summary>
		/// <param name="filter">The search term</param>
		public void Filter(string filter)
		{
			bool anyShown = false;

			foreach(UIElement element in modPlayerEditorList._items)
			{
				if (element is FieldEditor editor)
				{
					if (editor.name.ToLower().Contains(filter.ToLower()))
					{
						editor.Height.Set(editor.height, 0);
						anyShown = true;
					}
					else
					{
						element.Height.Set(0, 0);
					}
				}
			}

			if (anyShown)
			{
				modPlayerEditorList.UpdateOrder();
				CalcHeight();
			}
			else
			{
				Height.Set(0, 0);
				modPlayerEditorList.Height.Set(0, 0);
			}
		}

		public override int CompareTo(object obj)
		{
			if (obj is ObjectEditor container)
				return Label.CompareTo(container.Label);

			return base.CompareTo(obj);
		}
	}
}
