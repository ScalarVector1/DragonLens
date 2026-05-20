using System;
using System.Collections.Generic;

internal sealed class PlayerManagerSettings
{
	#region Button toggle
	private readonly HashSet<string> hiddenButtons = [];
	public bool IsButtonVisible(string key) => !hiddenButtons.Contains(key);
	public void SetButtonVisible(string key, bool visible)
	{
		if (visible)
			hiddenButtons.Remove(key);
		else
			hiddenButtons.Add(key);
	}
	#endregion

	#region Stat toggle
	private const int TotalActionButtons = 7;

	internal static readonly string[] StatOrder =
	[
		"Life",
		"Mana",
		"Defense",
		"HeldItem",
		"BiomeName",
		"Position",
		"Team",
		"MovementSpeed",
		"Distance",
		"SessionTime",
		"Ping",
		"InventoryItemCount",
		"CoinCount",
		"AmmoCount",
		"MinionCount",
		"NearbyEnemies",
		"LastEnemyHit",
		"LastPlayerHit",
		"DeathCount",
		"BossDamage",
	];

	private readonly HashSet<string> visibleStats =
	[
		"Life",
		"Mana",
		"Defense"
	];

	private readonly List<string> statToggleOrder =
	[
		"Life",
		"Mana",
		"Defense"
	];

	public bool IsStatVisible(string key) => visibleStats.Contains(key);
	public int HiddenButtonCount => hiddenButtons.Count;
	public int VisibleButtonCount => Math.Max(0, TotalActionButtons - hiddenButtons.Count);
	public int GetListStatColumnCount() => 1 + hiddenButtons.Count / 2;

	public bool TryToggleStat(string key, bool visible, bool listMode, int availableHeight)
	{
		if (!visible)
		{
			visibleStats.Remove(key);
			statToggleOrder.Remove(key);
			return true;
		}

		if (IsStatVisible(key))
		{
			statToggleOrder.Remove(key);
			statToggleOrder.Add(key);
			return true;
		}

		HashSet<string> countedStats = [.. StatOrder];

		int maxVisibleStats = GetMaxVisibleStats(listMode, availableHeight);

		int visibleCount = 0;
		foreach (string stat in countedStats)
		{
			if (IsStatVisible(stat))
				visibleCount++;
		}

		if (countedStats.Contains(key) && visibleCount >= maxVisibleStats)
		{
			for (int i = 0; i < statToggleOrder.Count; i++)
			{
				string oldKey = statToggleOrder[i];
				if (!countedStats.Contains(oldKey))
					continue;

				visibleStats.Remove(oldKey);
				statToggleOrder.RemoveAt(i);
				break;
			}
		}

		visibleStats.Add(key);
		statToggleOrder.Remove(key);
		statToggleOrder.Add(key);
		return true;
	}

	public int GetMaxVisibleStats(bool listMode, int availableHeight)
	{
		if (listMode)
		{
			int rowsPerColumn =
				availableHeight >= 48 + 46 ? 4 :
				availableHeight >= 48 + 18 ? 3 :
				availableHeight >= 48 ? 2 :
				0;

			int totalSlots = rowsPerColumn * GetListStatColumnCount();
			if (IsPlayerMode("PlayerHead"))
				totalSlots = Math.Max(0, totalSlots - 1);

			return totalSlots;
		}

		int maxStats =
			availableHeight >= 48 + 46 ? 4 :
			availableHeight >= 48 + 18 ? 3 :
			availableHeight >= 48 ? 2 :
			availableHeight >= 40 ? 0 :
			0;

		bool reserveTopRowForPlayerName = IsPlayerMode("PlayerFull") && availableHeight >= 102;
		if (reserveTopRowForPlayerName)
			maxStats = Math.Max(0, maxStats - 1);

		return maxStats;
	}
	#endregion
	#region Background options (exclusive, only one can be shown at a time)
	private string selectedBackground = "BiomeBackground";

	public bool IsBackgroundMode(string key) => selectedBackground == key;
	public string SelectedBackground => selectedBackground;

	public void SetBackgroundMode(string key, bool enabled)
	{
		if (enabled)
			selectedBackground = key;
		else if (selectedBackground == key)
			selectedBackground = "None";
	}
	#endregion

	#region Player options (exclusive, only one can be shown at a time)
	private string selectedPlayerOption = "PlayerHead";

	public bool IsPlayerMode(string key) => selectedPlayerOption == key;

	public void SetPlayerMode(string key, bool enabled)
	{
		if (enabled)
			selectedPlayerOption = key;
		else if (selectedPlayerOption == key)
			selectedPlayerOption = "None";
	}
	#endregion
}