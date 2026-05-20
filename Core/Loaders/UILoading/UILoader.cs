using DragonLens.Content.GUI;
using DragonLens.Core.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.UI;
using static ReLogic.Peripherals.RGB.Corsair.CorsairDeviceGroup;

namespace DragonLens.Core.Loaders.UILoading
{
	/// <summary>
	/// Automatically loads SmartUIStates ala IoC.
	/// </summary>
	class UILoader : ModSystem
	{
		/// <summary>
		/// The collection of automatically craetaed UserInterfaces for SmartUIStates.
		/// </summary>
		public static List<UserInterface> UserInterfaces = [];
		public static List<UserInterface> SortedUserInterfaces = [];

		/// <summary>
		/// The collection of all automatically loaded SmartUIStates.
		/// </summary>
		public static List<SmartUIState> UIStates = [];
		public static Dictionary<Type, SmartUIState> UIStatesDict = [];

		private static UserInterface focusNextFrame;

		/// <summary>
		/// Gets the topmost UIState which the mouse is currently hovering 
		/// </summary>
		/// <returns></returns>
		public static SmartUIState GetTopmostHoveredState()
		{
			if (SortedUserInterfaces is null)
				return null;

			var mouse = Main.MouseScreen.ToPoint();

			foreach (UserInterface ui in SortedUserInterfaces)
			{
				if (ui?.CurrentState is not SmartUIState state || !state.Visible || !state.ParticipatesInHoverOwnership)
					continue;

				if (state.OwnsMouse(mouse))
					return state;
			}

			return null;
		}

		/// <summary>
		/// Returns if the given element is on the topmost hovered state. Commonly checked for things like displaying tooltips
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static bool IsOnTopmostHoveredState(UIElement element)
		{
			return GetOwningState(element) == GetTopmostHoveredState();
		}

		private static SmartUIState GetOwningState(UIElement element)
		{
			UIElement current = element;

			while (current is not null)
			{
				if (current is SmartUIState state)
					return state;

				current = current.Parent;
			}

			return null;
		}

		public static void BringToFront(SmartUIState state)
		{
			// Since we trigger focus in the update loop, we cant change ordering untill the next frame, else we hit a collection modified during iteration
			focusNextFrame = state.UserInterface;
		}

		/// <summary>
		/// Uses reflection to scan through and find all types extending SmartUIState that arent abstract, and loads an instance of them.
		/// </summary>
		public override void Load()
		{
			if (Main.dedServ)
				return;

			UserInterfaces = [];
			UIStates = [];
			UIStatesDict = [];

			foreach (Type t in Mod.Code.GetTypes())
			{
				if (!t.IsAbstract && t.IsSubclassOf(typeof(SmartUIState)))
				{
					var state = (SmartUIState)Activator.CreateInstance(t, null);
					var userInterface = new UserInterface();
					userInterface.SetState(state);
					state.UserInterface = userInterface;

					UIStates?.Add(state);
					UserInterfaces?.Add(userInterface);
					SortedUserInterfaces?.Add(userInterface);
					UIStatesDict[state.GetType()] = state;
				}
			}
		}

		public override void Unload()
		{
			UIStates.ForEach(n => n.Unload());
			UserInterfaces = null;
			UIStates = null;
			UIStatesDict = null;
		}

		/// <summary>
		/// Helper method for creating and inserting a LegacyGameInterfaceLayer automatically
		/// </summary>
		/// <param name="layers">The vanilla layers</param>
		/// <param name="state">the UIState to bind to the layer</param>
		/// <param name="index">Where this layer should be inserted</param>
		/// <param name="visible">The logic dictating the visibility of this layer</param>
		/// <param name="scale">The scale settings this layer should scale with</param>
		public static void AddLayer(List<GameInterfaceLayer> layers, UserInterface ui, int index, bool visible, InterfaceScaleType scale)
		{
			string name = ui?.CurrentState?.ToString() ?? "Unknown";
			layers.Insert(index, new LegacyGameInterfaceLayer("DragonLens:" + name,
				delegate
				{
					if (visible)
						ui.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);

					return true;
				}, scale));
		}

		/// <summary>
		/// Handles updating the UI states correctly
		/// </summary>
		/// <param name="gameTime"></param>
		public override void UpdateUI(GameTime gameTime)
		{
			if (Main.ingameOptionsWindow || Main.InGameUI.IsVisible || SortedUserInterfaces is null)
				return;

			bool blockLowerLeftClick = false;
			bool blockLowerRightClick = false;
			var mouse = Main.MouseScreen.ToPoint();

			foreach (UserInterface eachState in SortedUserInterfaces)
			{
				if (eachState?.CurrentState is not SmartUIState s || !s.Visible)
					continue;

				if (Main.netMode != NetmodeID.SinglePlayer && !PermissionHandler.CanUseTools(Main.LocalPlayer))
					continue;

				bool suppressMouseForThisState = false;

				if (s is DraggableUIState draggable && draggable.BoundingBox.Contains(mouse))
				{
					suppressMouseForThisState = blockLowerLeftClick || blockLowerRightClick;

					if (!suppressMouseForThisState)
					{
						blockLowerLeftClick = true;
						blockLowerRightClick = true;
					}
				}

				bool oldMouseLeft = Main.mouseLeft;
				bool oldMouseRight = Main.mouseRight;

				if (suppressMouseForThisState)
				{
					Main.mouseLeft = false;
					Main.mouseRight = false;
				}

				eachState.Update(gameTime);

				if (eachState.LeftMouse.WasDown && eachState.LeftMouse.LastDown is not null && eachState.LeftMouse.LastDown is not UIState)
					Main.mouseLeft = false;

				if (eachState.RightMouse.WasDown && eachState.RightMouse.LastDown is not null && eachState.RightMouse.LastDown is not UIState)
					Main.mouseRight = false;

				if (suppressMouseForThisState)
				{
					Main.mouseLeft = oldMouseLeft;
					Main.mouseRight = oldMouseRight;
				}
			}

			if (focusNextFrame != null && SortedUserInterfaces.Contains(focusNextFrame))
			{
				SortedUserInterfaces.Remove(focusNextFrame);
				SortedUserInterfaces.Insert(0, focusNextFrame);
			}

			focusNextFrame = null;
		}

		/// <summary>
		/// Gets the autoloaded SmartUIState instance for a given SmartUIState subclass
		/// </summary>
		/// <typeparam name="T">The SmartUIState subclass to get the instance of</typeparam>
		/// <returns>The autoloaded instance of the desired SmartUIState</returns>
		public static T GetUIState<T>() where T : SmartUIState
		{
			return UIStatesDict.ContainsKey(typeof(T)) ? (T)UIStatesDict[typeof(T)] : null;
		}

		/// <summary>
		/// Handles the insertion of the automatically generated UIs
		/// </summary>
		/// <param name="layers"></param>
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			// We loop backwards so that layers are inserted appropriately according to the focus order when targeting the same vanilla index
			for (int k = SortedUserInterfaces.Count - 1; k >= 0; k--)
			{
				UserInterface inter = SortedUserInterfaces[k];

				if (inter?.CurrentState is SmartUIState state)
				{
					int index = state.InsertionIndex(layers);
					AddLayer(layers, inter, index, state.Visible, state.Scale);
				}
			}
		}
	}
}