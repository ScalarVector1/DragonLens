using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace DragonLens.Content.Tools.Multiplayer.Trackers
{
	internal sealed class NPCHitTracker : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (player == null || !player.active)
				return;

			UpdateTrackedNpcHit(player, npc, damageDone);
		}

		public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
				return;

			Player player = Main.player[projectile.owner];
			if (player == null || !player.active)
				return;

			UpdateTrackedNpcHit(player, npc, damageDone);

			if (projectile.minion)
				player.GetModPlayer<NPCHitTrackerPlayer>().LastMinionProjectileType = projectile.type;
		}

		private static void UpdateTrackedNpcHit(Player player, NPC npc, int damageDone)
		{
			NPCHitTrackerPlayer modPlayer = player.GetModPlayer<NPCHitTrackerPlayer>();
			modPlayer.LastEnemyHitName = npc.FullName;
			modPlayer.LastEnemyHitNpcType = npc.type;

			if (npc.boss)
			{
				modPlayer.TotalBossDamage += damageDone;
				modPlayer.LastBossHitNpcType = npc.type;
			}
		}
	}

	internal sealed class NPCHitTrackerPlayer : ModPlayer
	{
		public int PvpDeaths;
		public int PveDeaths;
		public long TotalBossDamage;

		public string LastEnemyHitName = "";
		public int LastEnemyHitNpcType = -1;

		public string LastPlayerHitName = "";
		public int LastPlayerHitWhoAmI = -1;

		public int LastBossHitNpcType = -1;
		public int LastMinionProjectileType = -1;

		public override void Initialize()
		{
			LastEnemyHitName = "";
			LastEnemyHitNpcType = -1;
			LastPlayerHitName = "";
			LastPlayerHitWhoAmI = -1;
			LastBossHitNpcType = -1;
			LastMinionProjectileType = -1;
			PvpDeaths = 0;
			PveDeaths = 0;
			TotalBossDamage = 0;
		}
	}
}
