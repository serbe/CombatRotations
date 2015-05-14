using System;
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

			if (Health (Me) <= 0.8) {
				if (Heal ())
					return true;
			}

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (InCombat)
				InCombat = false;

			return false;
		}

		public override void Combat ()
		{
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}


			//	actions=rebuke
			if (Interrupt ())
				return;
			//	actions+=/potion,name=draenic_strength,if=(buff.bloodlust.react|buff.avenging_wrath.up|target.time_to_die<=40)
			//	actions+=/auto_attack
			//	actions+=/speed_of_light,if=movement.distance>5
			if (Target.CombatRange > 5) {
				if (SpeedofLight ())
					return;
			}
			//	actions+=/judgment,if=talent.empowered_seals.enabled&time<2
			if (HasGlyph (54922) && Time < 2) {
				if (Judgment ())
					return;
			}
			//	actions+=/execution_sentence
			if (Danger () && ExecutionSentence ())
				return;
			//	actions+=/lights_hammer
			if (Danger () && LightsHammer ())
				return;
			//	actions+=/use_item,name=vial_of_convulsive_shadows,if=buff.avenging_wrath.up
			//	actions+=/holy_avenger,sync=seraphim,if=talent.seraphim.enabled
			if (Me.HasAura ("Seraphim")) {
				if (HolyAvenger ())
					return;
			}
			//	actions+=/holy_avenger,if=holy_power<=2&!talent.seraphim.enabled
			if (HolyPower <= 2 && !HasSpell ("Seraphim")) {
				if (HolyAvenger ())
					return;
			}
			//	actions+=/avenging_wrath,sync=seraphim,if=talent.seraphim.enabled
			if (Me.HasAura ("Seraphim") && HasSpell ("Seraphim")) {
				if (AvengingWrath ())
					return;
			}
			//	actions+=/avenging_wrath,if=!talent.seraphim.enabled
			if (!HasSpell ("Seraphim")) {
				if (AvengingWrath ())
					return;
			}
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/seraphim
			Seraphim ();
			//	actions+=/wait,sec=cooldown.seraphim.remains,if=talent.seraphim.enabled&cooldown.seraphim.remains>0&cooldown.seraphim.remains<gcd.max&holy_power>=5
			if (HasSpell ("Seraphim") && Cooldown ("Seraphim") > 0 && Cooldown ("Seraphim") < 1.5 && HolyPower >= 5)
				return;
			//	actions+=/call_action_list,name=cleave,if=active_enemies>=3
			if (ActiveEnemies (8) >= 3)
				Cleave ();
			//	actions+=/call_action_list,name=single
			Single ();
		}

		void Single ()
		{
			//	actions.single=divine_storm,if=buff.divine_crusader.react&(holy_power=5|buff.holy_avenger.up&holy_power>=3)&buff.final_verdict.up
			if (Me.HasAura ("Divine Crusader") && (HolyPower == 5 || Me.HasAura ("Holy Avenger") && HolyPower >= 3) && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&(holy_power=5|buff.holy_avenger.up&holy_power>=3)&active_enemies=2&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && (HolyPower == 5 || Me.HasAura ("Holy Avenger") && HolyPower >= 3) && ActiveEnemies (8) == 2 && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/divine_storm,if=(holy_power=5|buff.holy_avenger.up&holy_power>=3)&active_enemies=2&buff.final_verdict.up
			if ((HolyPower == 5 || Me.HasAura ("Holy Avenger") && HolyPower >= 3) && ActiveEnemies (8) == 2 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&(holy_power=5|buff.holy_avenger.up&holy_power>=3)&(talent.seraphim.enabled&cooldown.seraphim.remains<gcd*4)
			if (Me.HasAura ("Divine Crusader") && (HolyPower == 5 || Me.HasAura ("Holy Avenger") && HolyPower >= 3) && (HasSpell ("Seraphim") && Me.AuraTimeRemaining ("Seraphim") < 6)) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/templars_verdict,if=(holy_power=5|buff.holy_avenger.up&holy_power>=3)&(buff.avenging_wrath.down|target.health.pct>35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)
			if ((HolyPower == 5 || Me.HasAura ("Holy Avenger") && HolyPower >= 3) && (!Me.HasAura ("Avenging Wrath") || Health () > 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 6)) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/templars_verdict,if=buff.divine_purpose.react&buff.divine_purpose.remains<3
			if (Me.HasAura ("Divine Purpose") && Me.AuraTimeRemaining ("Divine Purpose") < 3) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.divine_crusader.remains<3&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && Me.AuraTimeRemaining ("Divine Crusader") < 3 && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
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


			// actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.divine_crusader.remains<3&buff.final_verdict.up
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && Me.AuraTimeRemaining ("Divine Crusader") < 3 && Me.HasAura ("Final Verdict")))
				return;
			// actions.single+=/final_verdict,if=holy_power=5|buff.holy_avenger.up&holy_power>=3
			if (Cast ("Final Verdict", () => HolyPower == 5 || Me.HasAura ("Holy Avenger") && HolyPower >= 3))
				return;
			// actions.single+=/final_verdict,if=buff.divine_purpose.react&buff.divine_purpose.remains<3
			if (Cast ("Final Verdict", () => Me.HasAura ("Divine Purpose") && Me.AuraTimeRemaining ("Divine Purpose") < 3))
				return;
			// actions.single+=/hammer_of_wrath
			if (Cast ("Hammer of Wrath", () => Health () <= 0.35 || Me.HasAura ("Avenging Wrath")))
				return;
			// actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.truth&buff.maraads_truth.remains<cooldown.judgment.duration
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Truth") && Me.AuraTimeRemaining ("Maraad's Truth") < 6 - Cooldown ("Judgment")))
				return;
			// actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<cooldown.judgment.duration
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Righteousness") && Me.AuraTimeRemaining ("Liadrin's Righteousness") < 6 - Cooldown ("Judgment")))
				return;
			// actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&cooldown.avenging_wrath.remains<cooldown.judgment.duration
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Righteousness") && Cooldown ("Avenging Wrath") < 6 - Cooldown ("Judgment")))
				return;
			// actions.single+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
			if (Cast ("Exorcism", () => Me.HasAura ("Blazing Contempt") && HolyPower <= 2 && !Me.HasAura ("Holy Avenger")))
				return;
			// actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.down
			if (Cast ("Seal of Truth", () => HasSpell ("Empowered Seals") && !Me.HasAura ("Maraad's Truth")))
				return;
			// actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&cooldown.avenging_wrath.remains<cooldown.judgment.duration&buff.liadrins_righteousness.remains>cooldown.judgment.duration
			if (Cast ("Seal of Truth", () => HasSpell ("Empowered Seals") && Cooldown ("Avenging Wrath") < Cooldown ("Judgment") && Me.AuraTimeRemaining ("Liadrin Righteousness") > 6 - Cooldown ("Judgment")))
				return;
			// actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.maraads_truth.remains>cooldown.judgment.duration&buff.liadrins_righteousness.down&!buff.avenging_wrath.up&!buff.bloodlust.up
			if (Cast ("Seal of Righteousness", () => HasSpell ("Empowered Seals") && Me.AuraTimeRemaining ("Maraad Truth") > 6 - Cooldown ("Judgment") && !Me.HasAura ("Liadrin Righteousness") && !Me.HasAura ("Avenging Wrath") && !Me.HasAura ("Bloodlust")))
				return;
			// actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)))
				return;
			// actions.single+=/divine_storm,if=active_enemies=2&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (Cast ("Divine Storm", () => ActiveEnemies (8) == 2 && Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)))
				return;
			// actions.single+=/final_verdict,if=buff.avenging_wrath.up|target.health.pct<35
			if (Cast ("Final Verdict", () => Me.HasAura ("Avenging Wrath") || Health () <= 0.35))
				return;
			// actions.single+=/divine_storm,if=buff.divine_crusader.react&active_enemies=2&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && ActiveEnemies (8) == 2 && (HasAura ("Avenging Wrath") || Health () <= 0.35)) && !HasSpell ("Final Verdict"))
				return;
			// actions.single+=/templars_verdict,if=holy_power=5&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*3)
			if (Cast ("Templar's Verdict", () => HolyPower == 5 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 4.5)))
				return;
			// actions.single+=/templars_verdict,if=holy_power=4&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)
			if (Cast ("Templar's Verdict", () => HolyPower == 4 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 6)))
				return;
			// actions.single+=/templars_verdict,if=holy_power=3&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
			if (Cast ("Templar's Verdict", () => HolyPower == 3 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 7.5)))
				return;
			// actions.single+=/crusader_strike,if=holy_power<5&talent.seraphim.enabled
			if (Cast ("Crusader Strike", () => HolyPower < 5 && HasSpell ("Seraphim")))
				return;
			// actions.single+=/crusader_strike,if=holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down)
			if (Cast ("Crusader Strike", () => HolyPower <= 3 || (HolyPower == 4 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))))
				return;
			// actions.single+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && (HasAura ("Avenging Wrath") || Health () <= 0.35) && !HasSpell ("Final Verdict")))
				return;
			// actions.single+=/judgment,cycle_targets=1,if=last_judgment_target!=target&glyph.double_jeopardy.enabled&holy_power<5
			if (Cooldown ("Judgment") == 0) {
				CycleTarget = targets.DefaultIfEmpty (null).FirstOrDefault ();
				if (Cast ("Judgment", CycleTarget, () => CycleTarget != null && HasGlyph (54922) && HolyPower < 5))
					return;
			}
			// actions.single+=/exorcism,if=glyph.mass_exorcism.enabled&active_enemies>=2&holy_power<5&!glyph.double_jeopardy.enabled&!set_bonus.tier17_4pc=1
			if (Cast ("Exorcism", () => HasGlyph (122028) && ActiveEnemies (8) >= 2 && HolyPower < 5 && !HasGlyph (54922) && !HasSpell (165439)))
				return;
			// actions.single+=/judgment,if=holy_power<5&talent.seraphim.enabled
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HolyPower < 5 && HasSpell ("Seraphim")))
				return;
			// actions.single+=/judgment,if=holy_power<=3|(holy_power=4&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down)
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HolyPower <= 3 || (HolyPower == 4 && Cooldown ("Crusader Strike") >= 3 && Health () > 0.35 && !Me.HasAura ("Avenging Wrath"))))
				return;
			// actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict")))
				return;
			// actions.single+=/divine_storm,if=active_enemies=2&holy_power>=4&buff.final_verdict.up
			if (Cast ("Divine Storm", () => ActiveEnemies (8) == 2 && HolyPower >= 4 && Me.HasAura ("Final Verdict")))
				return;
			// actions.single+=/final_verdict,if=buff.divine_purpose.react
			if (Cast ("Final Verdict", () => Me.HasAura ("Divine Purpose")))
				return;
			// actions.single+=/final_verdict,if=holy_power>=4
			if (Cast ("Final Verdict", () => HolyPower >= 4))
				return;
			// actions.single+=/divine_storm,if=buff.divine_crusader.react&active_enemies=2&holy_power>=4&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && ActiveEnemies (8) == 2 && HolyPower >= 4 && !HasSpell ("Final Verdict")))
				return;
			// actions.single+=/templars_verdict,if=buff.divine_purpose.react
			if (Cast ("Templar's Verdict", () => Me.HasAura ("Divine Purpose")))
				return;
			// actions.single+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && !HasSpell ("Final Verdict")))
				return;
			// actions.single+=/templars_verdict,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
			if (Cast ("Templar's Verdict", () => HolyPower >= 4 && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > 7.5)))
				return;
			// actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.remains<cooldown.judgment.duration
			if (Cast ("Seal of Truth", () => HasSpell ("Empowered Seals") && Me.AuraTimeRemaining ("Maraad's Truth") < 6 - Cooldown ("Judgment")))
				return;
			// actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.liadrins_righteousness.remains<cooldown.judgment.duration&!buff.bloodlust.up
			if (Cast ("Seal of Righteousness", () => HasSpell ("Empowered Seals") && Me.AuraTimeRemaining ("Liadrin's Righteousness") < 6 - Cooldown ("Judgment") && !Me.HasAura ("Bloodlust")))
				return;
			// actions.single+=/exorcism,if=holy_power<5&talent.seraphim.enabled
			if (Cast ("Exorcism", () => HolyPower < 5 && HasSpell ("Seraphim")))
				return;
			// actions.single+=/exorcism,if=holy_power<=3|(holy_power=4&(cooldown.judgment.remains>=gcd*2&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down))
			if (Cast ("Exorcism", () => HolyPower <= 3 || (HolyPower == 4 && (Cooldown ("Judgment") >= 3 && Cooldown ("Crusader Strike") >= 3 && Health () > 0.35 && !Me.HasAura ("Avenging Wrath")))))
				return;
			// actions.single+=/divine_storm,if=active_enemies=2&holy_power>=3&buff.final_verdict.up
			if (Cast ("Divine Storm", () => ActiveEnemies (8) == 2 && HolyPower >= 3 && Me.HasAura ("Final Verdict")))
				return;
			// actions.single+=/final_verdict,if=holy_power>=3
			if (Cast ("Final Verdict", () => HolyPower >= 3))
				return;
			// actions.single+=/templars_verdict,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)
			if (Cast ("Templar's Verdict", () => HolyPower >= 3 && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > 9)))
				return;
			// actions.single+=/holy_prism
			if (Cast ("Holy Prism"))
				return;


		}

		void Cleave ()
		{
			//	actions.cleave=final_verdict,if=buff.final_verdict.down&holy_power=5
			if (!Me.HasAura ("Final Verdict") && HolyPower == 5) {
				if (FinalVerdict ())
					return;
			}
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




			// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&buff.final_verdict.up
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && HolyPower == 5 && Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=holy_power=5&buff.final_verdict.up
			if (Cast ("Divine Storm", () => HolyPower == 5 && Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && HolyPower == 5 && !HasSpell ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=holy_power=5&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => HolyPower == 5 && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 6) && !HasSpell ("Final Verdict")))
				return;
			// actions.cleave+=/hammer_of_wrath
			if (Cast ("Hammer of Wrath", () => Health () <= 0.35 || Me.HasAura ("Avenging Wrath")))
				return;
			// actions.cleave+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<cooldown.judgment.duration
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Righteousness") && Me.AuraTimeRemaining ("Liadrin's Righteousness") < 6 - Cooldown ("Judgment")))
				return;
			// actions.cleave+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
			if (Cast ("Exorcism", () => Me.HasAura ("Blazing Contempt") && HolyPower <= 2 && !Me.HasAura ("Holy Avenger")))
				return;
			// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)))
				return;
			// actions.cleave+=/divine_storm,if=buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (Cast ("Divine Storm", () => Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)))
				return;
			// actions.cleave+=/final_verdict,if=buff.final_verdict.down&(buff.avenging_wrath.up|target.health.pct<35)
			if (Cast ("Final Verdict", () => !Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)))
				return;
			// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && (HasAura ("Avenging Wrath") || Health () <= 0.35) && !HasSpell ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=holy_power=5&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*3)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => HolyPower == 5 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 4.5)))
				return;
			// actions.cleave+=/divine_storm,if=holy_power=4&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => HolyPower == 4 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 6)))
				return;
			// actions.cleave+=/divine_storm,if=holy_power=3&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => HolyPower == 3 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 7.5)))
				return;
			// actions.cleave+=/hammer_of_the_righteous,if=active_enemies>=4&holy_power<5&talent.seraphim.enabled
			if (Cast ("Hammer of the Righteous", () => ActiveEnemies (8) >= 4 && HolyPower < 5 && HasSpell ("Seraphim")))
				return;
			// actions.cleave+=/hammer_of_the_righteous,,if=active_enemies>=4&(holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down))
			if (Cast ("Hammer of the Righteous", () => ActiveEnemies (8) >= 4 && (HolyPower <= 3 || (HolyPower == 4 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath")))))
				return;
			// actions.cleave+=/crusader_strike,if=holy_power<5&talent.seraphim.enabled
			if (Cast ("Crusader Strike", () => HolyPower < 5 && HasSpell ("Seraphim")))
				return;
			// actions.cleave+=/crusader_strike,if=holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down)
			if (Cast ("Crusader Strike", () => HolyPower <= 3 || (HolyPower == 4 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))))
				return;
			// actions.cleave+=/exorcism,if=glyph.mass_exorcism.enabled&holy_power<5&!set_bonus.tier17_4pc=1
			if (Cast ("Exorcism", () => HasGlyph (122028) && HolyPower < 5 && !HasSpell (165439)))
				return;
			// actions.cleave+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&holy_power<5
			if (Cooldown ("Judgment") == 0) {
				CycleTarget = targets.DefaultIfEmpty (null).FirstOrDefault ();
				if (Cast ("Judgment", CycleTarget, () => CycleTarget != null && HasGlyph (54922) && HolyPower < 5))
					return;
			}
			// actions.cleave+=/judgment,if=holy_power<5&talent.seraphim.enabled
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HolyPower < 5 && HasSpell ("Seraphim")))
				return;
			// actions.cleave+=/judgment,if=holy_power<=3|(holy_power=4&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down)
			if (Cast ("Judgment", () => Cooldown ("Judgment") == 0 && HolyPower <= 3 || (HolyPower == 4 && Cooldown ("Crusader Strike") >= 3 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))))
				return;
			// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=buff.divine_purpose.react&buff.final_verdict.up
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Purpose") && Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=holy_power>=4&buff.final_verdict.up
			if (Cast ("Divine Storm", () => HolyPower >= 4 && Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/final_verdict,if=buff.divine_purpose.react&buff.final_verdict.down
			if (Cast ("Final Verdict", () => Me.HasAura ("Divine Purpose") && !Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/final_verdict,if=holy_power>=4&buff.final_verdict.down
			if (Cast ("Final Verdict", () => HolyPower >= 4 && !Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => Me.HasAura ("Divine Crusader") && !HasSpell ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => HolyPower >= 4 && (!HasSpell ("Seraphim") || AuraTimeRemaining ("Seraphim") > 7.5) && !HasSpell ("Final Verdict")))
				return;
			// actions.cleave+=/exorcism,if=holy_power<5&talent.seraphim.enabled
			if (Cast ("Exorcism", () => HolyPower < 5 && HasSpell ("Seraphim")))
				return;
			// actions.cleave+=/exorcism,if=holy_power<=3|(holy_power=4&(cooldown.judgment.remains>=gcd*2&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down))
			if (Cast ("Exorcism", () => HolyPower <= 3 || (HolyPower == 4 && Cooldown ("Judgment") >= 3 && Cooldown ("Crusader Strike") >= 3 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))))
				return;
			// actions.cleave+=/divine_storm,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)&!talent.final_verdict.enabled
			if (Cast ("Divine Storm", () => HolyPower >= 3 && (!HasSpell ("Seraphim") || AuraTimeRemaining ("Seraphim") > 9) && !HasSpell ("Final Verdict")))
				return;
			// actions.cleave+=/divine_storm,if=holy_power>=3&buff.final_verdict.up
			if (Cast ("Divine Storm", () => HolyPower >= 3 && Me.HasAura ("Final Verdict")))
				return;
			// actions.cleave+=/final_verdict,if=holy_power>=3&buff.final_verdict.down
			if (Cast ("Final Verdict", () => HolyPower >= 3 && !Me.HasAura ("Final Verdict")))
				return;

		}
	}
}
