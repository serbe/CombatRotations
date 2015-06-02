using System;
using ReBot.API;
using System.Linq;

namespace ReBot
{
	[Rotation ("Serb Warlock Affliction SC", "Serb", WoWClass.Warlock, Specialization.WarlockAffliction, 40)]

	public class SerbWarlockAffliction : SerbWarlock
	{
		public SerbWarlockAffliction ()
		{
			GroupBuffs = new[] {
				"Dark Intent",
				(CurrentBotName == "PvP" ? "Create Soulwell" : null)
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/dark_intent,if=!aura.spell_power_multiplier.up
			//	actions.precombat+=/summon_pet,if=!talent.demonic_servitude.enabled&(!talent.grimoire_of_sacrifice.enabled|buff.grimoire_of_sacrifice.down)
			//	actions.precombat+=/summon_doomguard,if=talent.demonic_servitude.enabled&active_enemies<9
			//	actions.precombat+=/summon_infernal,if=talent.demonic_servitude.enabled&active_enemies>=9
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/grimoire_of_sacrifice,if=talent.grimoire_of_sacrifice.enabled&!talent.demonic_servitude.enabled
			//	actions.precombat+=/potion,name=draenic_intellect


			return false;
		}

		public override void Combat ()
		{
			//	actions=summon_doomguard,if=!talent.demonic_servitude.enabled&active_enemies<9
			//	actions+=/summon_infernal,if=!talent.demonic_servitude.enabled&active_enemies>=9
			//	actions+=/service_pet,if=talent.grimoire_of_service.enabled&(target.time_to_die>120|target.time_to_die<=25|(buff.dark_soul.remains&target.health.pct<20))
			//	actions+=/potion,name=draenic_intellect,if=target.time_to_die<=25|buff.dark_soul.remains>10|(glyph.dark_soul.enabled&buff.dark_soul.remains)
			//	actions+=/berserking
			Berserking (Target, 40);
			//	actions+=/blood_fury
			BloodFury (Target, 40);
			//	actions+=/arcane_torrent
			ArcaneTorrent (Target, 40);
			//	actions+=/mannoroths_fury
			MannorothsFury ();
			//	actions+=/dark_soul,if=!talent.archimondes_darkness.enabled|(talent.archimondes_darkness.enabled&(charges=2|target.time_to_die<40|((trinket.proc.any.react|trinket.stacking_proc.any.react)&(!talent.grimoire_of_service.enabled|!talent.demonic_servitude.enabled|pet.service_doomguard.active|recharge_time<=cooldown.service_pet.remains))))
			//	actions+=/kiljaedens_cunning,if=(talent.cataclysm.enabled&!cooldown.cataclysm.remains)
			//	actions+=/kiljaedens_cunning,moving=1,if=!talent.cataclysm.enabled
			//	actions+=/cataclysm
			if (Cataclysm ())
				return;
			//	actions+=/agony,if=remains<=gcd
			if (Target.AuraTimeRemaining ("Agony", true) < 1.5) {
				if (Agony ())
					return;
			}
			//	actions+=/corruption,if=remains<=gcd
			if (Target.AuraTimeRemaining ("Corruption", true) < 1.5) {
				if (Corruption ())
					return;
			}
			//	actions+=/unstable_affliction,if=remains<=cast_time
			//	actions+=/soulburn,cycle_targets=1,if=!talent.soulburn_haunt.enabled&active_enemies>2&dot.corruption.remains<=dot.corruption.duration*0.3
			//	actions+=/seed_of_corruption,cycle_targets=1,if=!talent.soulburn_haunt.enabled&active_enemies>2&!dot.seed_of_corruption.remains&buff.soulburn.remains
			//	actions+=/haunt,if=shard_react&!talent.soulburn_haunt.enabled&!in_flight_to_target&(dot.haunt.remains<duration*0.3+cast_time+travel_time|soul_shard=4)&(trinket.proc.any.react|trinket.stacking_proc.any.react>6|buff.dark_soul.up|soul_shard>2|soul_shard*14<=target.time_to_die)&(buff.dark_soul.down|set_bonus.tier18_4pc=0)
			//	actions+=/soulburn,if=shard_react&talent.soulburn_haunt.enabled&buff.soulburn.down&(buff.haunting_spirits.remains-action.haunt.cast_time<=buff.haunting_spirits.duration*0.3)
			//	actions+=/haunt,if=shard_react&talent.soulburn_haunt.enabled&!in_flight_to_target&((buff.soulburn.up&((buff.haunting_spirits.remains-cast_time<=buff.haunting_spirits.duration*0.3&dot.haunt.remains-cast_time<=dot.haunt.duration*0.3)|buff.haunting_spirits.down)))
			//	actions+=/haunt,if=shard_react&talent.soulburn_haunt.enabled&!in_flight_to_target&buff.haunting_spirits.remains>=buff.haunting_spirits.duration*0.5&(dot.haunt.remains<duration*0.3+cast_time+travel_time|soul_shard=4)&(trinket.proc.any.react|trinket.stacking_proc.any.react>6|buff.dark_soul.up|soul_shard>2|soul_shard*14<=target.time_to_die)&(buff.dark_soul.down|set_bonus.tier18_4pc=0)
			//	actions+=/haunt,if=shard_react&!in_flight_to_target&buff.dark_soul.remains>cast_time+travel_time&!dot.haunt.ticking&set_bonus.tier18_4pc=1
			//	actions+=/agony,cycle_targets=1,if=target.time_to_die>16&remains<=(duration*0.3)&((talent.cataclysm.enabled&remains<=(cooldown.cataclysm.remains+action.cataclysm.cast_time))|!talent.cataclysm.enabled)
			//	actions+=/unstable_affliction,cycle_targets=1,if=target.time_to_die>10&remains-cast_time<=(duration*0.3)
			//	actions+=/seed_of_corruption,cycle_targets=1,if=!talent.soulburn_haunt.enabled&active_enemies>3&!dot.seed_of_corruption.ticking
			//	actions+=/corruption,cycle_targets=1,if=target.time_to_die>12&remains<=(duration*0.3)
			//	actions+=/seed_of_corruption,cycle_targets=1,if=active_enemies>3&!dot.seed_of_corruption.ticking
			//	actions+=/life_tap,if=mana.pct<40&buff.dark_soul.down
			//	actions+=/drain_soul,interrupt=1,chain=1
			//	actions+=/agony,cycle_targets=1,moving=1,if=mana.pct>50
			if (Usable ("Agony") && Me.IsMoving && Mana (Me) > 0.5) {
				Unit = Enemy.Where (u => Range (40, u)).OrderBy (u => u.AuraTimeRemaining ("Agony", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Agony (Unit))
					return;
			}
			//	actions+=/life_tap
			if (LifeTap ())
				return;

		}
	}
}
	