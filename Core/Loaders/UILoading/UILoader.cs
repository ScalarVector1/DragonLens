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
		public static List<UserInterface> UserInterfaces = new();
		public static List<UserInterface> SortedUserInterfaces = new();

		/// <summary>
		/// The collection of all automatically loaded SmartUIStates.
		/// </summary>
		public static List<SmartUIState> UIStates = new();

		public static SmartUIState GetTopmostHoveredState()
		{
			if (SortedUserInterfaces is null)
				return null;

			Point mouse = Main.MouseScreen.ToPoint();

			foreach (UserInterface ui in SortedUserInterfaces)
			{
				if (ui?.CurrentState is not SmartUIState state || !state.Visible || !state.ParticipatesInHoverOwnership)
					continue;

				if (state.OwnsMouse(mouse))
					return state;
			}

			return null;
		}

		public static bool CanShowTooltip(UIElement element)
		{
			if (element is null)
			{
				//Main.NewText("Error: Null element!");
				return false;
			}

			SmartUIState owner = GetOwningState(element);
			SmartUIState topmost = GetTopmostHoveredState();
			bool result = owner is not null && ReferenceEquals(owner, topmost);

			string elementName = element.GetType().Name;
			string ownerName = owner?.GetType().Name ?? "null";
			string topmostName = topmost?.GetType().Name ?? "null";

#if DEBUG
			if (element.IsMouseHovering)
			{
				ModContent.GetInstance<DragonLens>().Logger.Debug(($"CanShowTooltip: element={elementName}, owner={ownerName}, topmost={topmostName}, result={result}"));
				//Main.NewText(($"CanShowTooltip: element={elementName}, owner={ownerName}, topmost={topmostName}, result={result}"));
			}
#endif

			return result;
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
			if (state is null || UIStates is null || UserInterfaces is null)
				return;

			int index = UIStates.IndexOf(state);

			if (index < 0 || index >= UserInterfaces.Count)
				return;

			SmartUIState uiState = UIStates[index];
			UserInterface userInterface = UserInterfaces[index];

			UIStates.RemoveAt(index);
			UserInterfaces.RemoveAt(index);

			UIStates.Add(uiState);
			UserInterfaces.Add(userInterface);
		}

		private static void RebuildSortedUserInterfaces(List<GameInterfaceLayer> layers)
		{
			List<Tuple<UserInterface, int, int>> orderedInterfaces = [];

			for (int k = 0; k < UserInterfaces.Count; k++)
			{
				UserInterface inter = UserInterfaces[k];

				if (inter?.CurrentState is not SmartUIState state)
					continue;

				int insertionIndex = state.InsertionIndex(layers);
				orderedInterfaces.Add(new Tuple<UserInterface, int, int>(inter, insertionIndex, k));
			}

			orderedInterfaces.Sort((a, b) =>
			{
				int indexCompare = b.Item2.CompareTo(a.Item2);

				if (indexCompare != 0)
					return indexCompare;

				return b.Item3.CompareTo(a.Item3);
			});

			SortedUserInterfaces = orderedInterfaces.Select(n => n.Item1).ToList();
		}

		/// <summary>
		/// Uses reflection to scan through and find all types extending SmartUIState that arent abstract, and loads an instance of them.
		/// </summary>
		public override void Load()
		{
			if (Main.dedServ)
				return;

			UserInterfaces = new List<UserInterface>();
			UIStates = new List<SmartUIState>();

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
				}
			}
		}

		public override void Unload()
		{
			UIStates.ForEach(n => n.Unload());
			UserInterfaces = null;
			UIStates = null;
		}

		/// <summary>
		/// Helper method for creating and inserting a LegacyGameInterfaceLayer automatically
		/// </summary>
		/// <param name="layers">The vanilla layers</param>
		/// <param name="state">the UIState to bind to the layer</param>
		/// <param name="index">Where this layer should be inserted</param>
		/// <param name="visible">The logic dictating the visibility of this layer</param>
		/// <param name="scale">The scale settings this layer should scale with</param>
		public static void AddLayer(List<GameInterfaceLayer> layers, UserInterface ui, int index, Func<bool> visible, InterfaceScaleType scale)
		{
			string name = ui?.CurrentState?.ToString() ?? "Unknown";
			layers.Insert(index, new LegacyGameInterfaceLayer("DragonLens: " + name,
				delegate
				{
					if (visible())
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
			Point mouse = Main.MouseScreen.ToPoint();

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
		}

		/// <summary>
		/// Gets the autoloaded SmartUIState instance for a given SmartUIState subclass
		/// </summary>
		/// <typeparam name="T">The SmartUIState subclass to get the instance of</typeparam>
		/// <returns>The autoloaded instance of the desired SmartUIState</returns>
		public static T GetUIState<T>() where T : SmartUIState
		{
			return UIStates.FirstOrDefault(n => n is T) as T;
		}

		/// <summary>
		/// Forcibly reloads a SmartUIState and it's associated UserInterface
		/// </summary>
		/// <typeparam name="T">The SmartUIState subclass to reload</typeparam>
		public static void ReloadState<T>() where T : SmartUIState
		{
			int index = UIStates.IndexOf(GetUIState<T>());
			UIStates[index] = (T)Activator.CreateInstance(typeof(T), null);
			UserInterfaces[index] = new UserInterface();
			UserInterfaces[index].SetState(UIStates[index]);
		}

		/// <summary>
		/// Handles the insertion of the automatically generated UIs
		/// </summary>
		/// <param name="layers"></param>
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			for (int k = 0; k < UserInterfaces.Count; k++)
			{
				UserInterface inter = UserInterfaces[k];

				if (inter?.CurrentState is not SmartUIState state)
					continue;

				int index = state.InsertionIndex(layers);
				AddLayer(layers, inter, index, () =>
				{
					if (Main.dedServ || Main.netMode == NetmodeID.SinglePlayer)
						return state.Visible;

					return state.Visible && PermissionHandler.CanUseTools(Main.LocalPlayer);
				}, state.Scale);
			}

			RebuildSortedUserInterfaces(layers);
		}
	}
}