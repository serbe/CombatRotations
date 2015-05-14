﻿using System;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Paladin Retribution SC", "Serb", WoWClass.Paladin, Specialization.PaladinRetribution, 5, 25)]

	public class SerbPaladinRetributionSC : SerbPaladin
	{
		public SerbPaladinRetributionSC ()
		{
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/blessing_of_kings,if=!aura.str_agi_int.up
			//	actions.precombat+=/blessing_of_might,if=!aura.mastery.up
			//	actions.precombat+=/seal_of_truth,if=active_enemies<2
			//	actions.precombat+=/seal_of_righteousness,if=active_enemies>=2
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_strength


			return false;
		}

		public override void Combat ()
		{
			//	actions=rebuke
			if (Interrupt ())
				return;
			//	actions+=/potion,name=draenic_strength,if=(buff.bloodlust.react|buff.avenging_wrath.up|target.time_to_die<=40)
			//	actions+=/auto_attack
			//	actions+=/speed_of_light,if=movement.distance>5
			//	actions+=/judgment,if=talent.empowered_seals.enabled&time<2
			//	actions+=/execution_sentence
			//	actions+=/lights_hammer
			if (LightsHammer ())
				return;
			//	actions+=/use_item,name=vial_of_convulsive_shadows,if=buff.avenging_wrath.up
			//	actions+=/holy_avenger,sync=seraphim,if=talent.seraphim.enabled
			//	actions+=/holy_avenger,if=holy_power<=2&!talent.seraphim.enabled
			//	actions+=/avenging_wrath,sync=seraphim,if=talent.seraphim.enabled
			//	actions+=/avenging_wrath,if=!talent.seraphim.enabled
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/seraphim
			Seraphim ();
			//	actions+=/wait,sec=cooldown.seraphim.remains,if=talent.seraphim.enabled&cooldown.seraphim.remains>0&cooldown.seraphim.remains<gcd.max&holy_power>=5
			//	actions+=/call_action_list,name=cleave,if=active_enemies>=3
			if (ActiveEnemies (8) >= 3)
				Cleave ();
			//	actions+=/call_action_list,name=single
			Single ();
		}

		void Single ()
		{
			//	actions.single=divine_storm,if=buff.divine_crusader.react&(holy_power=5|buff.holy_avenger.up&holy_power>=3)&buff.final_verdict.up
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&(holy_power=5|buff.holy_avenger.up&holy_power>=3)&active_enemies=2&!talent.final_verdict.enabled
			//	actions.single+=/divine_storm,if=(holy_power=5|buff.holy_avenger.up&holy_power>=3)&active_enemies=2&buff.final_verdict.up
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&(holy_power=5|buff.holy_avenger.up&holy_power>=3)&(talent.seraphim.enabled&cooldown.seraphim.remains<gcd*4)
			//	actions.single+=/templars_verdict,if=(holy_power=5|buff.holy_avenger.up&holy_power>=3)&(buff.avenging_wrath.down|target.health.pct>35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)
			//	actions.single+=/templars_verdict,if=buff.divine_purpose.react&buff.divine_purpose.remains<3
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.divine_crusader.remains<3&!talent.final_verdict.enabled
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.divine_crusader.remains<3&buff.final_verdict.up
			//	actions.single+=/final_verdict,if=holy_power=5|buff.holy_avenger.up&holy_power>=3
			//	actions.single+=/final_verdict,if=buff.divine_purpose.react&buff.divine_purpose.remains<3
			//	actions.single+=/hammer_of_wrath
			//	actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.truth&buff.maraads_truth.remains<cooldown.judgment.duration
			//	actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<cooldown.judgment.duration
			//	actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&cooldown.avenging_wrath.remains<cooldown.judgment.duration
			//	actions.single+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
			//	actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.down
			//	actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&cooldown.avenging_wrath.remains<cooldown.judgment.duration&buff.liadrins_righteousness.remains>cooldown.judgment.duration
			//	actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.maraads_truth.remains>cooldown.judgment.duration&buff.liadrins_righteousness.down&!buff.avenging_wrath.up&!buff.bloodlust.up
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			//	actions.single+=/divine_storm,if=active_enemies=2&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			//	actions.single+=/final_verdict,if=buff.avenging_wrath.up|target.health.pct<35
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&active_enemies=2&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			//	actions.single+=/templars_verdict,if=holy_power=5&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*3)
			//	actions.single+=/templars_verdict,if=holy_power=4&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)
			//	actions.single+=/templars_verdict,if=holy_power=3&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
			//	actions.single+=/crusader_strike,if=holy_power<5&talent.seraphim.enabled
			//	actions.single+=/crusader_strike,if=holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down)
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			//	actions.single+=/judgment,cycle_targets=1,if=last_judgment_target!=target&glyph.double_jeopardy.enabled&holy_power<5
			//	actions.single+=/exorcism,if=glyph.mass_exorcism.enabled&active_enemies>=2&holy_power<5&!glyph.double_jeopardy.enabled&!set_bonus.tier17_4pc=1
			//	actions.single+=/judgment,if=holy_power<5&talent.seraphim.enabled
			//	actions.single+=/judgment,if=holy_power<=3|(holy_power=4&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down)
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
			//	actions.single+=/divine_storm,if=active_enemies=2&holy_power>=4&buff.final_verdict.up
			//	actions.single+=/final_verdict,if=buff.divine_purpose.react
			//	actions.single+=/final_verdict,if=holy_power>=4
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&active_enemies=2&holy_power>=4&!talent.final_verdict.enabled
			//	actions.single+=/templars_verdict,if=buff.divine_purpose.react
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
			//	actions.single+=/templars_verdict,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
			//	actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.remains<cooldown.judgment.duration
			//	actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.liadrins_righteousness.remains<cooldown.judgment.duration&!buff.bloodlust.up
			//	actions.single+=/exorcism,if=holy_power<5&talent.seraphim.enabled
			//	actions.single+=/exorcism,if=holy_power<=3|(holy_power=4&(cooldown.judgment.remains>=gcd*2&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down))
			//	actions.single+=/divine_storm,if=active_enemies=2&holy_power>=3&buff.final_verdict.up
			//	actions.single+=/final_verdict,if=holy_power>=3
			//	actions.single+=/templars_verdict,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)
			//	actions.single+=/holy_prism
		}

		void Cleave ()
		{
			//	actions.cleave=final_verdict,if=buff.final_verdict.down&holy_power=5
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&buff.final_verdict.up
			//	actions.cleave+=/divine_storm,if=holy_power=5&buff.final_verdict.up
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&!talent.final_verdict.enabled
			//	actions.cleave+=/divine_storm,if=holy_power=5&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)&!talent.final_verdict.enabled
			//	actions.cleave+=/hammer_of_wrath
			//	actions.cleave+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<cooldown.judgment.duration
			//	actions.cleave+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			//	actions.cleave+=/divine_storm,if=buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			//	actions.cleave+=/final_verdict,if=buff.final_verdict.down&(buff.avenging_wrath.up|target.health.pct<35)
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			//	actions.cleave+=/divine_storm,if=holy_power=5&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*3)&!talent.final_verdict.enabled
			//	actions.cleave+=/divine_storm,if=holy_power=4&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)&!talent.final_verdict.enabled
			//	actions.cleave+=/divine_storm,if=holy_power=3&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
			//	actions.cleave+=/hammer_of_the_righteous,if=active_enemies>=4&holy_power<5&talent.seraphim.enabled
			//	actions.cleave+=/hammer_of_the_righteous,if=active_enemies>=4&(holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down))
			//	actions.cleave+=/crusader_strike,if=holy_power<5&talent.seraphim.enabled
			//	actions.cleave+=/crusader_strike,if=holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down)
			//	actions.cleave+=/exorcism,if=glyph.mass_exorcism.enabled&holy_power<5&!set_bonus.tier17_4pc=1
			//	actions.cleave+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&holy_power<5
			//	actions.cleave+=/judgment,if=holy_power<5&talent.seraphim.enabled
			//	actions.cleave+=/judgment,if=holy_power<=3|(holy_power=4&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down)
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
			//	actions.cleave+=/divine_storm,if=buff.divine_purpose.react&buff.final_verdict.up
			//	actions.cleave+=/divine_storm,if=holy_power>=4&buff.final_verdict.up
			//	actions.cleave+=/final_verdict,if=buff.divine_purpose.react&buff.final_verdict.down
			//	actions.cleave+=/final_verdict,if=holy_power>=4&buff.final_verdict.down
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
			//	actions.cleave+=/divine_storm,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
			//	actions.cleave+=/exorcism,if=holy_power<5&talent.seraphim.enabled
			//	actions.cleave+=/exorcism,if=holy_power<=3|(holy_power=4&(cooldown.judgment.remains>=gcd*2&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down))
			//	actions.cleave+=/divine_storm,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)&!talent.final_verdict.enabled
			//	actions.cleave+=/divine_storm,if=holy_power>=3&buff.final_verdict.up
			//	actions.cleave+=/final_verdict,if=holy_power>=3&buff.final_verdict.down
			//	actions.cleave+=/holy_prism,target=self
			if (HolyPrism (Me))
				return;
		}
	}
}
