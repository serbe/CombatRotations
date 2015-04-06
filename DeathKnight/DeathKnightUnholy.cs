﻿using System;
using ReBot.API;

namespace ReBot
{
	[Rotation("Serb Unholy DeathKnight SC", "Serb", WoWClass.DeathKnight, Specialization.DeathknightUnholy, 5, 25)]

	public class SerbDeathKnightUnholySC : DeathKnight
	{
		public SerbDeathKnightUnholySC ()
		{
		}

		public override bool OutOfCombat() {
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/horn_of_winter
			if (HornofWinter()) return true;
			//	actions.precombat+=/unholy_presence
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/army_of_the_dead
			//	actions.precombat+=/potion,name=draenic_strength
			//	actions.precombat+=/raise_dead
			if (RaiseDead()) return true;

			return false;
		}

		public override void Combat ()
		{
			//actions=auto_attack
			//actions+=/deaths_advance,if=movement.remains>2
			//actions+=/run_action_list,name=bos,if=talent.breath_of_sindragosa.enabled
			if (HasSpell("Breath of Sindragosa")) {
				if (BOS_action ())
					return;
			}
			//actions+=/antimagic_shell,damage=100000
			//actions+=/blood_fury
			if (BloodFury()) return;
			//actions+=/berserking
			if (Berserking()) return;
			//actions+=/arcane_torrent
			if (ArcaneTorrent()) return;
			//actions+=/use_item,slot=trinket2
			//actions+=/potion,name=draenic_strength,if=buff.dark_transformation.up&target.time_to_die<=60
			//actions+=/run_action_list,name=aoe,if=(!talent.necrotic_plague.enabled&active_enemies>=2)|active_enemies>=4
			if ((!HasSpell ("Necrotic Plague") && EnemyInRange (10) >= 2) || EnemyInRange (10) >= 4) {
				if (AOE_action ())
					return;
			}
			//actions+=/run_action_list,name=single_target,if=(!talent.necrotic_plague.enabled&active_enemies<2)|active_enemies<4
			if ((!HasSpell ("Necrotic Plague") && EnemyInRange (10) < 2) || EnemyInRange (10) < 4) {
				if (Single_action ())
					return;
			}
		}

		public bool BOS_action() {
			//actions.bos=antimagic_shell,damage=100000,if=(dot.breath_of_sindragosa.ticking&runic_power<25)|cooldown.breath_of_sindragosa.remains>40
			if (Me.HealthFraction <= 0.75 && ((Me.HasAura ("Breath of Sindragosa") && RunicPower < 25) || Cooldown ("Breath of Sindragosa") > 40)) {
				if (AntimagicShell ())
					return true;
			}
			//actions.bos+=/blood_fury,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (BloodFury ())
					return true;
			}
			//actions.bos+=/berserking
			if (Berserking ())
				return true;
			//actions.bos+=/use_item,slot=trinket2,if=dot.breath_of_sindragosa.ticking
			//actions.bos+=/potion,name=draenic_strength,if=dot.breath_of_sindragosa.ticking
			//actions.bos+=/run_action_list,name=bos_st
			if (BOS_st_action()) return true;

			return false;
		}

		public bool AOE_action() {
			//actions.aoe=unholy_blight
			//actions.aoe+=/call_action_list,name=spread,if=!dot.blood_plague.ticking|!dot.frost_fever.ticking|(!dot.necrotic_plague.ticking&talent.necrotic_plague.enabled)
			//actions.aoe+=/defile
			//actions.aoe+=/blood_boil,if=blood=2|(frost=2&death=2)
			//actions.aoe+=/summon_gargoyle
			//actions.aoe+=/dark_transformation
			//actions.aoe+=/blood_tap,if=level<=90&buff.shadow_infusion.stack=5
			//actions.aoe+=/defile
			//actions.aoe+=/death_and_decay,if=unholy=1
			//actions.aoe+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=45
			//actions.aoe+=/scourge_strike,if=unholy=2
			//actions.aoe+=/blood_tap,if=buff.blood_charge.stack>10
			//actions.aoe+=/death_coil,if=runic_power>90|buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
			//actions.aoe+=/blood_boil
			//actions.aoe+=/icy_touch
			//actions.aoe+=/scourge_strike,if=unholy=1
			//actions.aoe+=/death_coil
			//actions.aoe+=/blood_tap
			//actions.aoe+=/plague_leech
			//actions.aoe+=/empower_rune_weapon

			return false;
		}

		public bool Single_action() {
			//actions.single_target=plague_leech,if=(cooldown.outbreak.remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
			//actions.single_target+=/plague_leech,if=((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))&disease.min_remains<3
			//actions.single_target+=/plague_leech,if=disease.min_remains<1
			//actions.single_target+=/outbreak,if=!disease.min_ticking
			//actions.single_target+=/unholy_blight,if=!talent.necrotic_plague.enabled&disease.min_remains<3
			//actions.single_target+=/unholy_blight,if=talent.necrotic_plague.enabled&dot.necrotic_plague.remains<1
			//actions.single_target+=/death_coil,if=runic_power>90
			//actions.single_target+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
			//actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			//actions.single_target+=/death_and_decay,if=(!talent.unholy_blight.enabled|!talent.necrotic_plague.enabled)&unholy=2
			//actions.single_target+=/defile,if=unholy=2
			//actions.single_target+=/plague_strike,if=!disease.min_ticking&unholy=2
			//actions.single_target+=/scourge_strike,if=unholy=2
			//actions.single_target+=/death_coil,if=runic_power>80
			//actions.single_target+=/festering_strike,if=talent.necrotic_plague.enabled&talent.unholy_blight.enabled&dot.necrotic_plague.remains<cooldown.unholy_blight.remains%2
			//actions.single_target+=/festering_strike,if=blood=2&frost=2&(((Frost-death)>0)|((Blood-death)>0))
			//actions.single_target+=/festering_strike,if=(blood=2|frost=2)&(((Frost-death)>0)&((Blood-death)>0))
			//actions.single_target+=/defile,if=blood=2|frost=2
			//actions.single_target+=/plague_strike,if=!disease.min_ticking&(blood=2|frost=2)
			//actions.single_target+=/scourge_strike,if=blood=2|frost=2
			//actions.single_target+=/festering_strike,if=((Blood-death)>1)
			//actions.single_target+=/blood_boil,if=((Blood-death)>1)
			//actions.single_target+=/festering_strike,if=((Frost-death)>1)
			//actions.single_target+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			//actions.single_target+=/summon_gargoyle
			//actions.single_target+=/death_and_decay,if=(!talent.unholy_blight.enabled|!talent.necrotic_plague.enabled)
			//actions.single_target+=/defile
			//actions.single_target+=/blood_tap,if=talent.defile.enabled&cooldown.defile.remains=0
			//actions.single_target+=/plague_strike,if=!disease.min_ticking
			//actions.single_target+=/dark_transformation
			//actions.single_target+=/blood_tap,if=buff.blood_charge.stack>10&(buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1))
			//actions.single_target+=/death_coil,if=buff.sudden_doom.react|(buff.dark_transformation.down&unholy<=1)
			//actions.single_target+=/scourge_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(Unholy>=2)
			//actions.single_target+=/blood_tap
			//actions.single_target+=/festering_strike,if=!((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)|(((Frost-death)>0)&((Blood-death)>0))
			//actions.single_target+=/death_coil
			//actions.single_target+=/plague_leech
			//actions.single_target+=/scourge_strike,if=cooldown.empower_rune_weapon.remains=0
			//actions.single_target+=/festering_strike,if=cooldown.empower_rune_weapon.remains=0
			//actions.single_target+=/blood_boil,if=cooldown.empower_rune_weapon.remains=0
			//actions.single_target+=/icy_touch,if=cooldown.empower_rune_weapon.remains=0
			//actions.single_target+=/empower_rune_weapon,if=blood<1&unholy<1&frost<1

			return false;
		}

		public bool BOS_st_action() {
			//actions.bos_st=plague_leech,if=((cooldown.outbreak.remains<1)|disease.min_remains<1)&((blood<1&frost<1)|(blood<1&unholy<1)|(frost<1&unholy<1))
			//actions.bos_st+=/soul_reaper,if=(target.health.pct-3*(target.health.pct%target.time_to_die))<=45
			//actions.bos_st+=/blood_tap,if=((target.health.pct-3*(target.health.pct%target.time_to_die))<=45)&cooldown.soul_reaper.remains=0
			//actions.bos_st+=/breath_of_sindragosa,if=runic_power>75
			//actions.bos_st+=/run_action_list,name=bos_active,if=dot.breath_of_sindragosa.ticking
			//actions.bos_st+=/summon_gargoyle
			//actions.bos_st+=/unholy_blight,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			//actions.bos_st+=/outbreak,cycle_targets=1,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			//actions.bos_st+=/plague_strike,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			//actions.bos_st+=/blood_boil,cycle_targets=1,if=!(dot.blood_plague.ticking|dot.frost_fever.ticking)
			//actions.bos_st+=/death_and_decay,if=active_enemies>1&unholy>1
			//actions.bos_st+=/festering_strike,if=blood>1&frost>1
			//actions.bos_st+=/scourge_strike,if=((unholy>1|death>1)&active_enemies<=3)|(unholy>1&active_enemies>=4)
			//actions.bos_st+=/death_and_decay,if=active_enemies>1
			//actions.bos_st+=/blood_boil,if=active_enemies>=4&(blood=2|(frost=2&death=2))
			//actions.bos_st+=/dark_transformation
			//actions.bos_st+=/blood_tap,if=buff.blood_charge.stack>10
			//actions.bos_st+=/blood_boil,if=active_enemies>=4
			//actions.bos_st+=/death_coil,if=(buff.sudden_doom.react|runic_power>80)&(buff.blood_charge.stack<=10)
			//actions.bos_st+=/scourge_strike,if=cooldown.breath_of_sindragosa.remains>6|runic_power<75
			//actions.bos_st+=/festering_strike,if=cooldown.breath_of_sindragosa.remains>6|runic_power<75
			//actions.bos_st+=/death_coil,if=cooldown.breath_of_sindragosa.remains>20
			//actions.bos_st+=/plague_leech

			return false;
		}
	}
}


//actions.bos_active=plague_strike,if=!disease.ticking
//actions.bos_active+=/blood_boil,cycle_targets=1,if=(active_enemies>=2&!(dot.blood_plague.ticking|dot.frost_fever.ticking))|active_enemies>=4&(runic_power<88&runic_power>30)
//actions.bos_active+=/scourge_strike,if=active_enemies<=3&(runic_power<88&runic_power>30)
//actions.bos_active+=/festering_strike,if=runic_power<77
//actions.bos_active+=/blood_boil,if=active_enemies>=4
//actions.bos_active+=/scourge_strike,if=active_enemies<=3
//actions.bos_active+=/blood_tap,if=buff.blood_charge.stack>=5
//actions.bos_active+=/arcane_torrent,if=runic_power<70
//actions.bos_active+=/plague_leech
//actions.bos_active+=/empower_rune_weapon,if=runic_power<60
//actions.bos_active+=/death_coil,if=buff.sudden_doom.react
//
//actions.spread=blood_boil,cycle_targets=1,if=!disease.min_ticking
//actions.spread+=/outbreak,if=!disease.min_ticking
//actions.spread+=/plague_strike,if=!disease.min_ticking