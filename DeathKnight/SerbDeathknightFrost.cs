﻿using System.Linq;
using ReBot.API;
using ReBot.DeathKnight;

namespace ReBot.DeathKnight
{
	[Rotation ("Serb DeathKnight Frost 2H SC", "Serb", WoWClass.DeathKnight, Specialization.DeathknightFrost, 5, 25)]

	public class DeathKnightFrost2HSC: SerbDeathKnight
	{

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=buttered_sturgeon
			//	actions.precombat+=/horn_of_winter
			if (HornofWinter ())
				return true;
			//	actions.precombat+=/frost_presence
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/army_of_the_dead
			//	actions.precombat+=/potion,name=draenic_strength
			//	actions.precombat+=/pillar_of_frost

			return false;
		}

		public override void Combat ()
		{
			//	actions=auto_attack
			//	actions+=/deaths_advance,if=movement.remains>2
			//	actions+=/antimagic_shell,damage=100000
			//	actions+=/pillar_of_frost
			if (PillarofFrost ())
				return;
			//	actions+=/potion,name=draenic_strength,if=target.time_to_die<=30|(target.time_to_die<=60&buff.pillar_of_frost.up)
			//	actions+=/empower_rune_weapon,if=target.time_to_die<=60&buff.potion.up
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/use_item,slot=trinket2
			//	actions+=/run_action_list,name=aoe,if=active_enemies>=4
			if (ActiveEnemies (10) >= 4) {
				if (Aoe ())
					return;
			}
			//	actions+=/run_action_list,name=single_target,if=active_enemies<4
			if (ActiveEnemies (10) < 4) {
				if (Single_target ())
					return;
			}

		}

		bool Aoe ()
		{
			//	actions.aoe=unholy_blight
			//	actions.aoe+=/blood_boil,if=dot.blood_plague.ticking&(!talent.unholy_blight.enabled|cooldown.unholy_blight.remains<49),line_cd=28
			//	actions.aoe+=/defile
			//	actions.aoe+=/breath_of_sindragosa,if=runic_power>75
			//	actions.aoe+=/run_action_list,name=bos_aoe,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (Bos_aoe ())
					return true;
			}
			//	actions.aoe+=/howling_blast
			//	actions.aoe+=/blood_tap,if=buff.blood_charge.stack>10
			//	actions.aoe+=/frost_strike,if=runic_power>88
			//	actions.aoe+=/death_and_decay,if=unholy=1
			//	actions.aoe+=/plague_strike,if=unholy=2
			//	actions.aoe+=/blood_tap
			//	actions.aoe+=/frost_strike,if=!talent.breath_of_sindragosa.enabled|cooldown.breath_of_sindragosa.remains>=10
			//	actions.aoe+=/plague_leech
			//	actions.aoe+=/plague_strike,if=unholy=1
			//	actions.aoe+=/empower_rune_weapon

			return false;
		}

		bool Single_target ()
		{
			//	actions.single_target=plague_leech,if=disease.min_remains<1
			//	actions.single_target+=/soul_reaper,if=target.health.pct-3*(target.health.pct%target.time_to_die)<=35
			//	actions.single_target+=/blood_tap,if=(target.health.pct-3*(target.health.pct%target.time_to_die)<=35&cooldown.soul_reaper.remains=0)
			//	actions.single_target+=/defile
			//	actions.single_target+=/blood_tap,if=talent.defile.enabled&cooldown.defile.remains=0
			//	actions.single_target+=/howling_blast,if=buff.rime.react&disease.min_remains>5&buff.killing_machine.react
			//	actions.single_target+=/obliterate,if=buff.killing_machine.react
			//	actions.single_target+=/blood_tap,if=buff.killing_machine.react
			//	actions.single_target+=/howling_blast,if=!talent.necrotic_plague.enabled&!dot.frost_fever.ticking&buff.rime.react
			//	actions.single_target+=/outbreak,if=!disease.max_ticking
			//	actions.single_target+=/unholy_blight,if=!disease.min_ticking
			//	actions.single_target+=/breath_of_sindragosa,if=runic_power>75
			//	actions.single_target+=/run_action_list,name=bos_st,if=dot.breath_of_sindragosa.ticking
			if (Me.HasAura ("Breath of Sindragosa")) {
				if (Bos_st ())
					return true;
			}
			//	actions.single_target+=/obliterate,if=talent.breath_of_sindragosa.enabled&cooldown.breath_of_sindragosa.remains<7&runic_power<76
			//	actions.single_target+=/howling_blast,if=talent.breath_of_sindragosa.enabled&cooldown.breath_of_sindragosa.remains<3&runic_power<88
			//	actions.single_target+=/howling_blast,if=!talent.necrotic_plague.enabled&!dot.frost_fever.ticking
			//	actions.single_target+=/howling_blast,if=talent.necrotic_plague.enabled&!dot.necrotic_plague.ticking
			//	actions.single_target+=/plague_strike,if=!talent.necrotic_plague.enabled&!dot.blood_plague.ticking
			//	actions.single_target+=/blood_tap,if=buff.blood_charge.stack>10&runic_power>76
			//	actions.single_target+=/frost_strike,if=runic_power>76
			//	actions.single_target+=/howling_blast,if=buff.rime.react&disease.min_remains>5&(blood.frac>=1.8|unholy.frac>=1.8|frost.frac>=1.8)
			//	actions.single_target+=/obliterate,if=blood.frac>=1.8|unholy.frac>=1.8|frost.frac>=1.8
			//	actions.single_target+=/plague_leech,if=disease.min_remains<3&((blood.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&blood.frac<=0.95))
			//	actions.single_target+=/frost_strike,if=talent.runic_empowerment.enabled&(frost=0|unholy=0|blood=0)&(!buff.killing_machine.react|!obliterate.ready_in<=1)
			//	actions.single_target+=/frost_strike,if=talent.blood_tap.enabled&buff.blood_charge.stack<=10&(!buff.killing_machine.react|!obliterate.ready_in<=1)
			//	actions.single_target+=/howling_blast,if=buff.rime.react&disease.min_remains>5
			//	actions.single_target+=/obliterate,if=blood.frac>=1.5|unholy.frac>=1.6|frost.frac>=1.6|buff.bloodlust.up|cooldown.plague_leech.remains<=4
			//	actions.single_target+=/blood_tap,if=(buff.blood_charge.stack>10&runic_power>=20)|(blood.frac>=1.4|unholy.frac>=1.6|frost.frac>=1.6)
			//	actions.single_target+=/frost_strike,if=!buff.killing_machine.react
			//	actions.single_target+=/plague_leech,if=(blood.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&unholy.frac<=0.95)|(frost.frac<=0.95&blood.frac<=0.95)
			//	actions.single_target+=/empower_rune_weapon

			return false;
		}

		bool Bos_aoe ()
		{
			//	actions.bos_aoe=howling_blast
			//	actions.bos_aoe+=/blood_tap,if=buff.blood_charge.stack>10
			//	actions.bos_aoe+=/death_and_decay,if=unholy=1
			//	actions.bos_aoe+=/plague_strike,if=unholy=2
			//	actions.bos_aoe+=/blood_tap
			//	actions.bos_aoe+=/plague_leech
			//	actions.bos_aoe+=/plague_strike,if=unholy=1
			//	actions.bos_aoe+=/empower_rune_weapon
		
			return false;
		}

		bool Bos_st ()
		{
			//	actions.bos_st=obliterate,if=buff.killing_machine.react
			//	actions.bos_st+=/blood_tap,if=buff.killing_machine.react&buff.blood_charge.stack>=5
			//	actions.bos_st+=/plague_leech,if=buff.killing_machine.react
			//	actions.bos_st+=/blood_tap,if=buff.blood_charge.stack>=5
			//	actions.bos_st+=/plague_leech
			//	actions.bos_st+=/obliterate,if=runic_power<76
			//	actions.bos_st+=/howling_blast,if=((death=1&frost=0&unholy=0)|death=0&frost=1&unholy=0)&runic_power<88
		
			return false;
		}
	}
}
	
