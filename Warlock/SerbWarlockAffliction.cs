﻿using System;
using ReBot.API;

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
			//	actions+=/agony,if=remains<=gcd
			//	actions+=/corruption,if=remains<=gcd
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
			//	actions+=/life_tap
			if (LifeTap ())
				return;

		}
	}
}

//	warlock="Warlock_Affliction_T17N"
//	level=100
//	race=gnome
//	role=spell
//	position=back
//	talents=0000111
//	talent_override=grimoire_of_service,if=talent.demonic_servitude.enabled
//	talent_override=soulburn_haunt,if=enemies=2
//	talent_override=cataclysm,if=enemies>2
//	spec=affliction
//
//	# This default action priority list is automatically created based on your character.
//	# It is a attempt to provide you with a action list that is both simple and practicable,
//	# while resulting in a meaningful and good simulation. It may not result in the absolutely highest possible dps.
//	# Feel free to edit, adapt and improve it to your own needs.
//	# SimulationCraft is always looking for updates and improvements to the default action lists.
//
//	# Executed before combat begins. Accepts non-harmful actions only.
//

//
//	# Executed every time the actor is available.
//

//
//	head=shadow_councils_hood,id=115586
//	neck=gruuls_lip_ring,id=113872,enchant_id=5318
//	shoulders=hexweave_mantle,id=114809,bonus_id=118/559
//	back=runefrenzy_greatcloak,id=113937,enchant_id=5311
//	chest=shadow_councils_robes,id=115588
//	wrists=hexweave_bracers,id=114814,bonus_id=119/559
//	hands=shadow_councils_gloves,id=115585
//	waist=seeking_ember_girdle,id=113941
//	legs=shadow_councils_leggings,id=115587
//	feet=hexweave_slippers,id=114815,bonus_id=118/559
//	finger1=ukuroggs_corrupted_seal,id=113975,enchant_id=5325
//	finger2=timeless_solium_band_of_the_archmage,id=118296,enchant_id=5325
//	trinket1=darmacs_unstable_talisman,id=113948
//	trinket2=blackiron_micro_crucible,id=113984
//	main_hand=infernoflame_staff,id=113869,enchant_id=5337
//
