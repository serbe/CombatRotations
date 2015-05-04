using ReBot.API;
using System;
using System.Threading;

namespace ReBot
{
	[Rotation ("Serb Shaman Elemental SC", "ReBot", WoWClass.Shaman, Specialization.ShamanElemental, 5, 25)]

	public class SerbShamanElementalSc : SerbShaman
	{
		public SerbShamanElementalSc ()
		{
			PullSpells = new[] {
				"Lightning Bolt"
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/lightning_shield,if=!buff.lightning_shield.up
			if (LightningShield ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_intellect

			return false;
		}

		public override void Combat ()
		{
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			//	actions=wind_shear
			Interrupt ();

			//	# Bloodlust casting behavior mirrors the simulator settings for proxy bloodlust. See options 'bloodlust_percent', and 'bloodlust_time'.
			//	actions+=/bloodlust,if=target.health.pct<25|time>0.500
			if (Health () < 0.25 || Timer > 0.5)
				Bloodlust ();
			//	# In-combat potion is preferentially linked to Ascendance, unless combat will end shortly
			//	actions+=/potion,name=draenic_intellect,if=buff.ascendance.up|target.time_to_die<=30
			//	actions+=/berserking,if=!buff.bloodlust.up&!buff.elemental_mastery.up&(set_bonus.tier15_4pc_caster=1|(buff.ascendance.cooldown_remains=0&(dot.flame_shock.remains>buff.ascendance.duration|level<87)))
			if (!Me.HasAura("Bloodlust") && !Me.HasAura("Elemental Mastery") &&
			//	actions+=/blood_fury,if=buff.bloodlust.up|buff.ascendance.up|((cooldown.ascendance.remains>10|level<87)&cooldown.fire_elemental_totem.remains>10)
			//	actions+=/arcane_torrent
			//	actions+=/elemental_mastery,if=action.lava_burst.cast_time>=1.2
			//	actions+=/ancestral_swiftness,if=!buff.ascendance.up
			//	actions+=/storm_elemental_totem
			//	actions+=/fire_elemental_totem,if=!active
			//	actions+=/ascendance,if=active_enemies>1|(dot.flame_shock.remains>buff.ascendance.duration&(target.time_to_die<20|buff.bloodlust.up|time>=60)&cooldown.lava_burst.remains>0)
			//	actions+=/liquid_magma,if=pet.searing_totem.remains>=15|pet.fire_elemental_totem.remains>=15
			//	# If one or two enemies, priority follows the 'single' action list.
			//	actions+=/call_action_list,name=single,if=active_enemies<3
			//	# On multiple enemies, the priority follows the 'aoe' action list.
			//	actions+=/call_action_list,name=aoe,if=active_enemies>2
		}
	}
}


//
//	# Executed every time the actor is available.
//

//
//	# Single target action priority list
//
//	actions.single=unleash_flame,moving=1
//	actions.single+=/spiritwalkers_grace,moving=1,if=buff.ascendance.up
//	actions.single+=/earth_shock,if=buff.lightning_shield.react=buff.lightning_shield.max_stack
//	actions.single+=/lava_burst,if=dot.flame_shock.remains>cast_time&(buff.ascendance.up|cooldown_react)
//	actions.single+=/earth_shock,if=(set_bonus.tier17_4pc&buff.lightning_shield.react>=12&!buff.lava_surge.up)|(!set_bonus.tier17_4pc&buff.lightning_shield.react>15)
//	actions.single+=/flame_shock,if=dot.flame_shock.remains<=9
//	actions.single+=/elemental_blast
//	# After the initial Ascendance, use Flame Shock pre-emptively just before Ascendance to guarantee Flame Shock staying up for the full duration of the Ascendance buff
//	actions.single+=/flame_shock,if=time>60&remains<=buff.ascendance.duration&cooldown.ascendance.remains+buff.ascendance.duration<duration
//	# Keep Searing Totem up, unless Fire Elemental Totem is coming off cooldown in the next 20 seconds
//	actions.single+=/searing_totem,if=(!talent.liquid_magma.enabled&(!totem.fire.active|(pet.searing_totem.remains<=10&!pet.fire_elemental_totem.active&talent.unleashed_fury.enabled)))|(talent.liquid_magma.enabled&pet.searing_totem.remains<=20&!pet.fire_elemental_totem.active&!buff.liquid_magma.up)
//	actions.single+=/unleash_flame,if=talent.unleashed_fury.enabled&!buff.ascendance.up
//	actions.single+=/spiritwalkers_grace,moving=1,if=((talent.elemental_blast.enabled&cooldown.elemental_blast.remains=0)|(cooldown.lava_burst.remains=0&!buff.lava_surge.react))
//	actions.single+=/lightning_bolt
//
//	# Multi target action priority list
//
//	actions.aoe=earthquake,cycle_targets=1,if=!ticking&(buff.enhanced_chain_lightning.up|level<=90)&active_enemies>=2
//	actions.aoe+=/lava_beam
//	actions.aoe+=/earth_shock,if=buff.lightning_shield.react=buff.lightning_shield.max_stack
//	actions.aoe+=/thunderstorm,if=active_enemies>=10
//	actions.aoe+=/searing_totem,if=(!talent.liquid_magma.enabled&!totem.fire.active)|(talent.liquid_magma.enabled&pet.searing_totem.remains<=20&!pet.fire_elemental_totem.active&!buff.liquid_magma.up)
//	actions.aoe+=/chain_lightning,if=active_enemies>=2
//	actions.aoe+=/lightning_bolt
//