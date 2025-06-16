﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.UI;

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
		public static void AddLayer(List<GameInterfaceLayer> layers, UIState state, int index, bool visible, InterfaceScaleType scale)
		{
			string name = state == null ? "Unknown" : state.ToString();
			layers.Insert(index, new LegacyGameInterfaceLayer("DragonLens: " + name,
				delegate
				{
					if (visible)
						state.Draw(Main.spriteBatch);

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

			foreach (UserInterface eachState in SortedUserInterfaces)
			{
				if (eachState?.CurrentState != null && ((SmartUIState)eachState.CurrentState).Visible)
				{
					eachState.Update(gameTime);

					if (eachState.LeftMouse.WasDown && eachState.LeftMouse.LastDown is not null && eachState.LeftMouse.LastDown is not UIState)
						Main.mouseLeft = false;

					if (eachState.RightMouse.WasDown && eachState.RightMouse.LastDown is not null && eachState.RightMouse.LastDown is not UIState)
						Main.mouseRight = false;
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
			List<Tuple<UserInterface, int>> orderedInterfaces = new();

			for (int k = 0; k < UserInterfaces.Count; k++)
			{
				UserInterface inter = UserInterfaces[k];

				if (inter.CurrentState is not SmartUIState)
					continue;

				SmartUIState state = inter.CurrentState as SmartUIState;

				int index = state.InsertionIndex(layers);
				AddLayer(layers, state, index, state.Visible, state.Scale);
				orderedInterfaces.Add(new Tuple<UserInterface, int>(inter, index));
			}

			orderedInterfaces.Sort((a, b) => a.Item2.CompareTo(b.Item2) * -1);
			SortedUserInterfaces = orderedInterfaces.Select(a => a.Item1).ToList();
		}
	}
}