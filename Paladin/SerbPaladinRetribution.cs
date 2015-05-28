
//  NEED UPDATE FROM CS

using System;
using ReBot.API;
using System.Linq;
using Newtonsoft.Json;

namespace ReBot
{
	[Rotation ("SC Paladin Retribution", "Serb", WoWClass.Paladin, Specialization.PaladinRetribution, 5, 25)]

	public class SerbPaladinRetributionSC : SerbPaladin
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public SerbPaladinRetributionSC ()
		{
			GroupBuffs = new[] { "Blessing of Kings" };
			PullSpells = new[] { "Judgment" };
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/blessing_of_kings,if=!aura.str_agi_int.up
			//	actions.precombat+=/blessing_of_might,if=!aura.mastery.up
			if (Buff (Me))
				return true;
			//	actions.precombat+=/seal_of_truth,if=active_enemies<2
			//	actions.precombat+=/seal_of_righteousness,if=active_enemies>=2
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_strength

//			if (Clean (Me))
//				return true;

			if (Heal ())
				return true;

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

			if (Me.CanNotParticipateInCombat ())
				Freedom ();
			
			if (Heal ())
				return;

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

			if (Gcd && HasGlobalCooldown ())
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
			if (Me.HasAura ("Divine Crusader") && Me.AuraTimeRemaining ("Divine Crusader") < 3 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/final_verdict,if=holy_power=5|buff.holy_avenger.up&holy_power>=3
			if (HolyPower == 5 || Me.HasAura ("Holy Avenger") && HolyPower >= 3) {
				if (FinalVerdict ())
					return;
			}
			//	actions.single+=/final_verdict,if=buff.divine_purpose.react&buff.divine_purpose.remains<3
			if (Me.HasAura ("Divine Purpose") && Me.AuraTimeRemaining ("Divine Purpose") < 3) {
				if (FinalVerdict ())
					return;
			}
			//	actions.single+=/hammer_of_wrath
			if (HammerofWrath ())
				return;
			//	actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.truth&buff.maraads_truth.remains<cooldown.judgment.duration
			if (HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Truth") && Me.AuraTimeRemaining ("Maraad's Truth") < 6) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<cooldown.judgment.duration
			if (HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Righteousness") && Me.AuraTimeRemaining ("Liadrin's Righteousness") < 6) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&cooldown.avenging_wrath.remains<cooldown.judgment.duration
			if (HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Righteousness") && Cooldown ("Avenging Wrath") < 6) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.single+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
			if (Me.HasAura ("Blazing Contempt") && HolyPower <= 2 && !Me.HasAura ("Holy Avenger")) {
				if (Exorcism ())
					return;
			}
			//	actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.down
			if (HasSpell ("Empowered Seals") && !Me.HasAura ("Maraad's Truth")) {
				if (SealofTruth ())
					return;
			}
			//	actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&cooldown.avenging_wrath.remains<cooldown.judgment.duration&buff.liadrins_righteousness.remains>cooldown.judgment.duration
			if (HasSpell ("Empowered Seals") && Cooldown ("Avenging Wrath") < 6 && Me.AuraTimeRemaining ("Liadrin Righteousness") > 6) {
				if (SealofTruth ())
					return;
			}
			//	actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.maraads_truth.remains>cooldown.judgment.duration&buff.liadrins_righteousness.down&!buff.avenging_wrath.up&!buff.bloodlust.up
			if (HasSpell ("Empowered Seals") && Me.AuraTimeRemaining ("Maraad Truth") > 6 && !Me.HasAura ("Liadrin Righteousness") && !Me.HasAura ("Avenging Wrath") && !Me.HasAura ("Bloodlust")) {
				if (SealofRighteousness ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/divine_storm,if=active_enemies=2&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (ActiveEnemies (8) == 2 && Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/final_verdict,if=buff.avenging_wrath.up|target.health.pct<35
			if (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) {
				if (FinalVerdict ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&active_enemies=2&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && ActiveEnemies (8) == 2 && (HasAura ("Avenging Wrath") || Health () <= 0.35) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/templars_verdict,if=holy_power=5&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*3)
			if (HolyPower == 5 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 4.5)) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/templars_verdict,if=holy_power=4&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)
			if (HolyPower == 4 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 6)) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/templars_verdict,if=holy_power=3&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
			if (HolyPower == 3 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 7.5)) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/crusader_strike,if=holy_power<5&talent.seraphim.enabled
			if (HolyPower < 5 && HasSpell ("Seraphim")) {
				if (CrusaderStrike ())
					return;
			}
			//	actions.single+=/crusader_strike,if=holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down)
			if (HolyPower <= 3 || (HolyPower == 4 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))) {
				if (CrusaderStrike ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && (HasAura ("Avenging Wrath") || Health () <= 0.35) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/judgment,cycle_targets=1,if=last_judgment_target!=target&glyph.double_jeopardy.enabled&holy_power<5
			if (HasGlyph (54922) && HolyPower < 5) {
				Unit = Enemy.Where (u => u != LastJudgmentTarget && Range (30, u)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Judgment (Unit)) {
					LastJudgmentTarget = Unit;
					return;
				}
			}
			//	actions.single+=/exorcism,if=glyph.mass_exorcism.enabled&active_enemies>=2&holy_power<5&!glyph.double_jeopardy.enabled&!set_bonus.tier17_4pc=1
			if (HasGlyph (122028) && ActiveEnemies (8) >= 2 && HolyPower < 5 && !HasGlyph (54922) && !HasSpell (165439)) {
				if (Exorcism ())
					return;
			}
			//	actions.single+=/judgment,if=holy_power<5&talent.seraphim.enabled
			if (HolyPower < 5 && HasSpell ("Seraphim")) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.single+=/judgment,if=holy_power<=3|(holy_power=4&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down)
			if (HolyPower <= 3 || (HolyPower == 4 && Cooldown ("Crusader Strike") >= 3 && Health () > 0.35 && !Me.HasAura ("Avenging Wrath"))) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
			if (Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/divine_storm,if=active_enemies=2&holy_power>=4&buff.final_verdict.up
			if (ActiveEnemies (8) == 2 && HolyPower >= 4 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/final_verdict,if=buff.divine_purpose.react
			if (Me.HasAura ("Divine Purpose")) {
				if (FinalVerdict ())
					return;
			}
			//	actions.single+=/final_verdict,if=holy_power>=4
			if (HolyPower >= 4) {
				if (FinalVerdict ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&active_enemies=2&holy_power>=4&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && ActiveEnemies (8) == 2 && HolyPower >= 4 && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/templars_verdict,if=buff.divine_purpose.react
			if (Me.HasAura ("Divine Purpose")) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/templars_verdict,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
			if (HolyPower >= 4 && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > 7.5)) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.remains<cooldown.judgment.duration
			if (HasSpell ("Empowered Seals") && Me.AuraTimeRemaining ("Maraad's Truth") < 6) {
				if (SealofTruth ())
					return;
			}
			//	actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.liadrins_righteousness.remains<cooldown.judgment.duration&!buff.bloodlust.up
			if (HasSpell ("Empowered Seals") && Me.AuraTimeRemaining ("Liadrin's Righteousness") < 6 && !Me.HasAura ("Bloodlust")) {
				if (SealofRighteousness ())
					return;
			}
			//	actions.single+=/exorcism,if=holy_power<5&talent.seraphim.enabled
			if (HolyPower < 5 && HasSpell ("Seraphim")) {
				if (Exorcism ())
					return;
			}
			//	actions.single+=/exorcism,if=holy_power<=3|(holy_power=4&(cooldown.judgment.remains>=gcd*2&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down))
			if (HolyPower <= 3 || (HolyPower == 4 && (Cooldown ("Judgment") >= 3 && Cooldown ("Crusader Strike") >= 3 && Health () > 0.35 && !Me.HasAura ("Avenging Wrath")))) {
				if (Exorcism ())
					return;
			}
			//	actions.single+=/divine_storm,if=active_enemies=2&holy_power>=3&buff.final_verdict.up
			if (ActiveEnemies (8) == 2 && HolyPower >= 3 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.single+=/final_verdict,if=holy_power>=3
			if (HolyPower >= 3) {
				if (FinalVerdict ())
					return;
			}
			//	actions.single+=/templars_verdict,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)
			if (HolyPower >= 3 && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > 9)) {
				if (TemplarsVerdict ())
					return;
			}
			//	actions.single+=/holy_prism
			if (HolyPrism ())
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
			if (Me.HasAura ("Divine Crusader") && HolyPower == 5 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power=5&buff.final_verdict.up
			if (HolyPower == 5 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && HolyPower == 5 && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power=5&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)&!talent.final_verdict.enabled
			if (HolyPower == 5 && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 6) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/hammer_of_wrath
			if (HammerofWrath ())
				return;
			//	actions.cleave+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<cooldown.judgment.duration
			if (HasSpell ("Empowered Seals") && IsInShapeshiftForm ("Seal of Righteousness") && Me.AuraTimeRemaining ("Liadrin's Righteousness") < 6) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.cleave+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
			if (Me.HasAura ("Blazing Contempt") && HolyPower <= 2 && !Me.HasAura ("Holy Avenger")) {
				if (Exorcism ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
			if (Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)) {
				if (DivineStorm ())
					return;
			}
			// actions.cleave+=/final_verdict,if=buff.final_verdict.down&(buff.avenging_wrath.up|target.health.pct<35)
			if (!Me.HasAura ("Final Verdict") && (HasAura ("Avenging Wrath") || Health () <= 0.35)) {
				if (FinalVerdict ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && (HasAura ("Avenging Wrath") || Health () <= 0.35) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power=5&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*3)&!talent.final_verdict.enabled
			if (HolyPower == 5 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 4.5) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power=4&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)&!talent.final_verdict.enabled
			if (HolyPower == 4 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 6) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power=3&(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
			if (HolyPower == 3 && (Me.HasAura ("Avenging Wrath") || Health () <= 0.35) && (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 7.5) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/hammer_of_the_righteous,if=active_enemies>=4&holy_power<5&talent.seraphim.enabled
			if (ActiveEnemies (8) >= 4 && HolyPower < 5 && HasSpell ("Seraphim")) {
				if (HammeroftheRighteous ())
					return;
			}
			//	actions.cleave+=/hammer_of_the_righteous,if=active_enemies>=4&(holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down))
			if (ActiveEnemies (8) >= 4 && (HolyPower <= 3 || (HolyPower == 4 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath")))) {
				if (HammeroftheRighteous ())
					return;
			}
			//	actions.cleave+=/crusader_strike,if=holy_power<5&talent.seraphim.enabled
			if (HolyPower < 5 && HasSpell ("Seraphim")) {
				if (CrusaderStrike ())
					return;
			}
			//	actions.cleave+=/crusader_strike,if=holy_power<=3|(holy_power=4&target.health.pct>=35&buff.avenging_wrath.down)
			if (HolyPower <= 3 || (HolyPower == 4 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))) {
				if (CrusaderStrike ())
					return;
			}
			//	actions.cleave+=/exorcism,if=glyph.mass_exorcism.enabled&holy_power<5&!set_bonus.tier17_4pc=1
			if (HasGlyph (122028) && HolyPower < 5 && !HasSpell (165439)) {
				if (Exorcism ())
					return;
			}
			//	actions.cleave+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&holy_power<5
			if (HasGlyph (54922) && HolyPower < 5) {
				Unit = Enemy.Where (u => Range (30, u)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Judgment (Unit)) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.cleave+=/judgment,if=holy_power<5&talent.seraphim.enabled
			if (HolyPower < 5 && HasSpell ("Seraphim")) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.cleave+=/judgment,if=holy_power<=3|(holy_power=4&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down)
			if (HolyPower <= 3 || (HolyPower == 4 && Cooldown ("Crusader Strike") >= 3 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))) {
				if (Judgment ()) {
					LastJudgmentTarget = Target;
					return;
				}
			}
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
			if (Me.HasAura ("Divine Crusader") && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=buff.divine_purpose.react&buff.final_verdict.up
			if (Me.HasAura ("Divine Purpose") && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power>=4&buff.final_verdict.up
			if (HolyPower >= 4 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/final_verdict,if=buff.divine_purpose.react&buff.final_verdict.down
			if (Me.HasAura ("Divine Purpose") && !Me.HasAura ("Final Verdict")) {
				if (FinalVerdict ())
					return;
			}
			//	actions.cleave+=/final_verdict,if=holy_power>=4&buff.final_verdict.down
			if (HolyPower >= 4 && !Me.HasAura ("Final Verdict")) {
				if (FinalVerdict ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
			if (Me.HasAura ("Divine Crusader") && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
			if (HolyPower >= 4 && (!HasSpell ("Seraphim") || AuraTimeRemaining ("Seraphim") > 7.5) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/exorcism,if=holy_power<5&talent.seraphim.enabled
			if (HolyPower < 5 && HasSpell ("Seraphim")) {
				if (Exorcism ())
					return;
			}
			//	actions.cleave+=/exorcism,if=holy_power<=3|(holy_power=4&(cooldown.judgment.remains>=gcd*2&cooldown.crusader_strike.remains>=gcd*2&target.health.pct>35&buff.avenging_wrath.down))
			if (HolyPower <= 3 || (HolyPower == 4 && Cooldown ("Judgment") >= 3 && Cooldown ("Crusader Strike") >= 3 && Health () >= 0.35 && !Me.HasAura ("Avenging Wrath"))) {
				if (Exorcism ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)&!talent.final_verdict.enabled
			if (HolyPower >= 3 && (!HasSpell ("Seraphim") || AuraTimeRemaining ("Seraphim") > 9) && !HasSpell ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/divine_storm,if=holy_power>=3&buff.final_verdict.up
			if (HolyPower >= 3 && Me.HasAura ("Final Verdict")) {
				if (DivineStorm ())
					return;
			}
			//	actions.cleave+=/final_verdict,if=holy_power>=3&buff.final_verdict.down
			if (HolyPower >= 3 && !Me.HasAura ("Final Verdict")) {
				if (FinalVerdict ())
					return;
			}
			//	actions.cleave+=/holy_prism,target=self
			if (HolyPrism (Me))
				return;
		}
	}
}
