using DragonLens.Core.Loaders.UILoading;
using DragonLens.Helpers;
using Terraria.GameContent.UI.Elements;

namespace DragonLens.Content.GUI
{
	/// <summary>
	/// Defines a UIState for a simple window that can be dragged around the screen.
	/// </summary>
	public abstract class DraggableUIState : SmartUIState
	{
		private UIImageButton closeButton;
		private UIImageButton helpButton;

		/// <summary>
		/// The top-left of the main window
		/// </summary>
		public Vector2 basePos;

		public bool dragging;
		public Vector2 dragOff;
		public bool visible;

		public int width;
		public int height;

		/// <summary>
		/// The area where the user can click and drag to move the main window
		/// </summary>
		public abstract Rectangle DragBox { get; }

		/// <summary>
		/// The dimensions of the main window
		/// </summary>
		public Rectangle BoundingBox => new((int)basePos.X, (int)basePos.Y, width, height);

		public override bool Visible => visible;

		/// <summary>
		/// Where the main window will be placed initially
		/// </summary>
		public virtual Vector2 DefaultPosition => Vector2.Zero;

		/// <summary>
		/// The link that clicking the help button should bring you to. No help button will be created if this is left as empty string.
		/// </summary>
		public virtual string HelpLink => "";

		/// <summary>
		/// You should adjust the position of all child elements of your UIState here so they move when the window is being dragged.
		/// </summary>
		/// <param name="newPos">The new position of the base window</param>
		public virtual void AdjustPositions(Vector2 newPos) { }

		public virtual void SafeOnInitialize() { }

		public virtual void DraggableUdpate(GameTime gameTime) { }

		public sealed override void OnInitialize()
		{
			basePos = new Vector2(DefaultPosition.X * Main.screenWidth, DefaultPosition.Y * Main.screenHeight);

			closeButton = new UIImageButton(ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Remove"));
			closeButton.Width.Set(16, 0);
			closeButton.Height.Set(16, 0);
			closeButton.OnLeftClick += (a, b) => visible = false;
			Append(closeButton);

			if (HelpLink != "")
			{
				helpButton = new UIImageButton(ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Help"));
				helpButton.Width.Set(16, 0);
				helpButton.Height.Set(16, 0);
				helpButton.OnLeftClick += (a, b) => GUIHelper.OpenUrl(HelpLink);
				Append(helpButton);
			}

			SafeOnInitialize();

			base.OnInitialize();
		}

		public sealed override void SafeUpdate(GameTime gameTime)
		{
			Recalculate();

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

			if (closeButton.IsMouseHovering)
			{
				Tooltip.SetName(LocalizationHelper.GetGUIText("DraggableUIState.Close"));
				Tooltip.SetTooltip("");
			}

			helpButton?.Left.Set(basePos.X + width - 44, 0);
			helpButton?.Top.Set(basePos.Y + 8, 0);

			if (helpButton != null && helpButton.IsMouseHovering)
			{
				Tooltip.SetName(LocalizationHelper.GetGUIText("DraggableUIState.UserGuide.Name"));
				Tooltip.SetTooltip(LocalizationHelper.GetGUIText("DraggableUIState.UserGuide.Tooltip"));
			}

			AdjustPositions(basePos);
			Recalculate();

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				Main.LocalPlayer.mouseInterface = true;

			DraggableUdpate(gameTime);
		}
	}
}