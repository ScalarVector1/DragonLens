using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal abstract class FieldEditor : SmartUIElement
	{
		/// <summary>
		/// The name that gets displated above the panel to the user
		/// </summary>
		public string name;

		/// <summary>
		/// The info sown when hovering over this panel
		/// </summary>
		public string description;

		/// <summary>
		/// Base height for this editor, used when heights are changed to filter
		/// </summary>
		public int height;

		/// <summary>
		/// Allows subclasses that want custom tooltips to hide this elements tooltip
		/// </summary>
		public bool hideTooltip;

		/// <summary>
		/// If this editor is currently being used to change a value, and thus shouldn't listen for update
		/// </summary>
		public virtual bool Editing => false;

		public override int CompareTo(object obj)
		{
			if (obj is FieldEditor editor)
			{
				if ((Height.Pixels > 0) != (editor.Height.Pixels > 0))
					return Height.Pixels.CompareTo(editor.Height.Pixels) * -1;

				return name.CompareTo(editor.name);
			}
				

			return base.CompareTo(obj);
		}
	}

	/// <summary>
	/// A UI element for changing the value of 'something'. 
	/// </summary>
	internal abstract class FieldEditor<T> : FieldEditor
	{
		/// <summary>
		/// The current value this editor believes the field its tied to to have. This wont update in real time so be careful
		/// </summary>
		public T value;

		/// <summary>
		/// The callback that should happen when this editor thinks the value its tracking has changed. You'll likely need to cast the object parameter to the correct type.
		/// </summary>
		public readonly Action<T> onValueChanged;

		/// <summary>
		/// This function, called every frame while the editor is not being used, is used to update the editor's value to the current value of the tracked value.
		/// </summary>
		public readonly Func<T> listenForUpdate;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="height">Height of the panel</param>
		/// <param name="name">The name that gets displated above the panel to the user</param>
		/// <param name="onValueChanged">The callback that should happen when this editor thinks the value its tracking has changed. You'll likely need to cast the object parameter to the correct type.</param>
		/// <param name="initialValue">A hint for what the initial value of the field tracked by this editor is</param>
		public FieldEditor(int height, string name, Action<T> onValueChanged, Func<T> listenForUpdate = null, T initialValue = default, string description = "")
		{
			Width.Set(150, 0);
			Height.Set(height, 0);
			this.height = height;
			this.name = name;
			this.onValueChanged = onValueChanged;
			this.listenForUpdate = listenForUpdate;
			value = initialValue;
			this.description = description;
		}

		/// <summary>
		/// Defines what should happen when a new value is recieved from the value update listener. Note that value has not yet been updated when this is called, so you can compare to the old value.
		/// </summary>
		/// <param name="newValue">The new value that was recieved</param>
		public virtual void OnRecieveNewValue(T newValue) { }

		public sealed override void SafeUpdate(GameTime gameTime)
		{
			if (!Editing && listenForUpdate != null)
			{
				T newValue = listenForUpdate();
				OnRecieveNewValue(newValue);
				value = newValue;
			}

			EditorUpdate(gameTime);
		}

		public virtual void EditorUpdate(GameTime gameTime) { }

		public sealed override void Draw(SpriteBatch spriteBatch)
		{
			if (Height.Pixels <= 0 && Height.Percent <= 0)
				return;

			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.BackgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = GetDimensions().ToRectangle();
			backTarget.Height = 24;
			backTarget.Offset(new Point(4, 4));
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, name, GetDimensions().Position() + new Vector2(8, 4), Color.White, 0.7f);
			Utils.DrawBorderString(spriteBatch, typeof(T).Name, GetDimensions().Position() + new Vector2(8, 18), Color.Gray, 0.65f);

			base.Draw(spriteBatch);

			SafeDraw(spriteBatch);

			if (IsMouseHovering && !hideTooltip)
			{
				Tooltip.SetName(name);
				Tooltip.SetTooltip(description);
				hideTooltip = false;
			}
		}

		public virtual void SafeDraw(SpriteBatch sprite) { }
	}
}