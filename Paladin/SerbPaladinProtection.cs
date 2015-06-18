using System;
using ReBot.API;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReBot
{
	[Rotation ("SC Paladin Protection", "Serb", WoWClass.Paladin, Specialization.PaladinProtection, 5, 25)]

	public class SerbPaladinProtectionSC : SerbPaladin
	{
		public enum ConfigRotation
		{
			Normal,
			MaxDPS,
			MaxSurvival
		}

		[JsonProperty ("Rotation"), JsonConverter (typeof(StringEnumConverter))]
		public ConfigRotation SelectedRotation = ConfigRotation.Normal;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Check Seal")]
		public bool CheckSeal = true;

		public bool WaitCrusaderStrike;
		public bool WaitJudgment;

		DateTime CheckTimer;

		public double TimeCheck {
			get {
				TimeSpan cTime = DateTime.Now.Subtract (CheckTimer);
				return cTime.TotalSeconds;
			}
		}

		public SerbPaladinProtectionSC ()
		{
			GroupBuffs = new[] { "Blessing of Kings" };
			PullSpells = new[] { "Judgment" };
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_stamina_flask
			//	actions.precombat+=/flask,type=greater_draenic_strength_flask,if=role.attack|using_apl.max_dps
			//	actions.precombat+=/food,type=whiptail_fillet
			//	actions.precombat+=/food,type=pickled_eel,if=role.attack|using_apl.max_dps
			//	actions.precombat+=/blessing_of_kings,if=(!aura.str_agi_int.up)&(aura.mastery.up)
			//	actions.precombat+=/blessing_of_might,if=!aura.mastery.up
//			if (Buff (Me))
//				return true;
			//	actions.precombat+=/seal_of_insight
			//	actions.precombat+=/seal_of_righteousness,if=role.attack|using_apl.max_dps
			//	actions.precombat+=/sacred_shield
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_armor
			if (InCombat)
				InCombat = false;

			if (WaitCrusaderStrike)
				WaitCrusaderStrike = false;
			if (WaitJudgment)
				WaitJudgment = false;

			// Heal

			if (Health (Me) <= 0.8) {
				if (FlashofLight (Me))
					return true;
			}

			if (CombatRole == CombatRole.Tank && !Me.HasAura ("Righteous Fury")) {
				if (RighteousFury ())
					return true;
			}

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (CheckSeal && TimeCheck > 2) {
				API.Print ("Seal of Insight " + IsInShapeshiftForm ("Seal of Insight") + " " + Me.HasAura ("Seal of Insight"));
				API.Print ("Seal of Righteousness " + IsInShapeshiftForm ("Seal of Righteousness") + " " + Me.HasAura ("Seal of Righteousness"));
	
				CheckTimer = DateTime.Now;
			}

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

			if (WaitCrusaderStrike) {
				if (Cooldown ("Crusader Strike") != 0)
					return;
				WaitCrusaderStrike = false;
				CrusaderStrike ();
				return;
			}
			if (WaitJudgment) {
				if (Cooldown ("Judgment") != 0)
					return;
				WaitJudgment = false;
				if (Judgment ())
					LastJudgmentTarget = Target;
				return;
			}

			if (MeIsBusy)
				return;

			if (Interrupt ())
				return;

			if (Heal ())
				return;

			if (IsBoss () && Target.IsCasting && Target.CastingTime > 0.5 && Target.CastingTime < 1.5 && !Me.HasAura ("Shield of the Righteous") && HolyPower >= 3) {
				if (ShieldoftheRighteous ())
					return;
			}

			if (SelectedRotation == ConfigRotation.Normal) {
				if (NormalRotation ())
					return;
			}
			if (SelectedRotation == ConfigRotation.MaxDPS) {
				if (MaxDPS ())
					return;
			}
			if (SelectedRotation == ConfigRotation.MaxSurvival) {
				if (MaxSurvival ())
					return;
			}

			if (Health (Me) <= 0.5 && !Me.IsMoving) {
				if (FlashofLight (Me))
					return;
			}
		}

		public bool NormalRotation ()
		{
			//	actions=auto_attack
			//	actions+=/speed_of_light,if=movement.remains>1
			if (Range (40, Target, 20))
				SpeedofLight ();
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	# Off-GCD spells.

			//	actions+=/holy_avenger
			HolyAvenger ();
			//	actions+=/potion,name=draenic_armor,if=buff.shield_of_the_righteous.down&buff.seraphim.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down&buff.ardent_defender.down
			//	actions+=/seraphim
			Seraphim ();
			//	actions+=/divine_protection,if=time<5|!talent.seraphim.enabled|(buff.seraphim.down&cooldown.seraphim.remains>5&cooldown.seraphim.remains<9)
			if (Time < 5 || !HasSpell ("Seraphim") || (!Me.HasAura ("Seraphim") && Cooldown ("Seraphim") > 5 && Cooldown ("Seraphim") < 9)) {
				if (NeedDivineProtection)
					DivineProtection ();
			}
			//	actions+=/guardian_of_ancient_kings,if=time<5|(buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down)
			if (Time < 5 || (!Me.HasAura ("Holy Avenger") && !Me.HasAura ("Shield of the Righteous") && !Me.HasAura ("Divine Protection") && !Me.HasAura ("Ardent Defender"))) {
				if (Health (Me) <= 0.4)
					GuardianofAncientKings ();
			}
			//	actions+=/ardent_defender,if=time<5|(buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down)
			if (Time < 5 || (!Me.HasAura ("Holy Avenger") && !Me.HasAura ("Shield of the Righteous") && !Me.HasAura ("Divine Protection") && !Me.HasAura ("Guardian of Ancient Kings"))) {
				if (Health (Me) <= 0.2)
					ArdentDefender ();
			}
			//	actions+=/eternal_flame,if=buff.eternal_flame.remains<2&buff.bastion_of_glory.react>2&(holy_power>=3|buff.divine_purpose.react|buff.bastion_of_power.react)
			if (Me.AuraTimeRemaining ("Eternal Flame") < 2 && GetAuraStack ("Bastion of Glory", Me) > 2 && (HolyPower >= 3 || Me.HasAura ("Divine Purpose") || Me.HasAura ("Bastion of Power"))) {
				if (EternalFlame ())
					return true;
			}
			//	actions+=/eternal_flame,if=buff.bastion_of_power.react&buff.bastion_of_glory.react>=5
			if (Me.HasAura ("Bastion of Power") && GetAuraStack ("Bastion of Glory", Me) >= 5) {
				if (EternalFlame ())
					return true;
			}
			//	actions+=/shield_of_the_righteous,if=buff.divine_purpose.react
			if (Me.HasAura ("Divine Purpose"))
				ShieldoftheRighteous ();
			//	actions+=/shield_of_the_righteous,if=(holy_power>=5|incoming_damage_1500ms>=health.max*0.3)&(!talent.seraphim.enabled|cooldown.seraphim.remains>5)
			if ((HolyPower >= 5 || DamageTaken (1500) >= Me.MaxHealth * 0.3) && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > 5)) {
				if (ShieldoftheRighteous ())
					return true;
			}
			//	actions+=/shield_of_the_righteous,if=buff.holy_avenger.remains>time_to_hpg&(!talent.seraphim.enabled|cooldown.seraphim.remains>time_to_hpg)
			if (Me.AuraTimeRemaining ("Holy Avenger") > TimeToHpg && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > TimeToHpg)) {
				if (ShieldoftheRighteous ())
					return true;
			}
			//	# GCD-bound spells

			//	actions+=/seal_of_insight,if=talent.empowered_seals.enabled&!seal.insight&buff.uthers_insight.remains<cooldown.judgment.remains
			if (HasSpell ("Empowered Seals") && !IsInShapeshiftForm ("Seal of Insight") && Me.AuraTimeRemaining ("Uther's Insight") < Cooldown ("Judgment")) {
				if (SealofInsight ())
					return true;
			}
			//	actions+=/seal_of_righteousness,if=talent.empowered_seals.enabled&!seal.righteousness&buff.uthers_insight.remains>cooldown.judgment.remains&buff.liadrins_righteousness.down
			if (HasSpell ("Empowered Seals") && !IsInShapeshiftForm ("Seal of Righteousness") && Me.AuraTimeRemaining ("Uther's Insight") > Cooldown ("Judgment") && !Me.HasAura ("Liadrins Righteousness")) {
				if (SealofRighteousness ())
					return true;
			}
			//	actions+=/avengers_shield,if=buff.grand_crusader.react&active_enemies>1&!glyph.focused_shield.enabled
			if (Me.HasAura ("Grand Crusader") && ActiveEnemies (30) > 1 && !HasGlyph (54930)) {
				if (AvengersShield ())
					return true;
			}
			//	actions+=/hammer_of_the_righteous,if=active_enemies>=3
			if (ActiveEnemies (8) >= 3) {
				if (HammeroftheRighteous ())
					return true;
			}
			//	actions+=/crusader_strike
			if (CrusaderStrike ())
				return true;
			//	actions+=/wait,sec=cooldown.crusader_strike.remains,if=cooldown.crusader_strike.remains>0&cooldown.crusader_strike.remains<=0.35
			if (HasSpell ("Crusader Strike") && Cooldown ("Crusader Strike") > 0 && Cooldown ("Crusader Strike") <= 0.35) {
				WaitCrusaderStrike = true;
				return true;
			}
			//	actions+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&last_judgment_target!=target
			if (Usable ("Judgment") && HasGlyph (54922)) {
				var Unit = Enemy.Where (u => u != LastJudgmentTarget).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (Judgment (Unit))
						LastJudgmentTarget = Unit;
					return true;
				}
			}
			//	actions+=/judgment
			if (Judgment ()) {
				LastJudgmentTarget = Target;
				return true;
			}
			//	actions+=/wait,sec=cooldown.judgment.remains,if=cooldown.judgment.remains>0&cooldown.judgment.remains<=0.35
			if (HasSpell ("Judgment") && Cooldown ("Judgment") > 0 && Cooldown ("Judgment") <= 0.35) {
				WaitJudgment = true;
				return true;
			}
			//	actions+=/avengers_shield,if=active_enemies>1&!glyph.focused_shield.enabled
			if (ActiveEnemiesWithTarget (10) > 1 && !HasGlyph (54930)) {
				if (AvengersShield ())
					return true;
			}
			//	actions+=/holy_wrath,if=talent.sanctified_wrath.enabled
			if (HasSpell ("Sanctified Wrath")) {
				if (HolyWrath ())
					return true;
			}
			//	actions+=/avengers_shield,if=buff.grand_crusader.react
			if (Me.HasAura ("Grand Crusader")) {
				if (AvengersShield ())
					return true;
			}
			//	actions+=/sacred_shield,if=target.dot.sacred_shield.remains<2
			if (Target.AuraTimeRemaining ("Sacred Shield") < 2) {
				if (SacredShield (Me))
					return true;
			}
			//	actions+=/holy_wrath,if=glyph.final_wrath.enabled&target.health.pct<=20
			if (HasGlyph (54935) && Health () <= 0.2) {
				if (HolyWrath ())
					return true;
			}
			//	actions+=/avengers_shield
			if (AvengersShield ())
				return true;
			//	actions+=/lights_hammer,if=!talent.seraphim.enabled|buff.seraphim.remains>10|cooldown.seraphim.remains<6
			if (!HasSpell ("Seraphim") || Me.AuraTimeRemaining ("Seraphim") > 10 || Cooldown ("Seraphim") < 6) {
				if (LightsHammer ())
					return true;
			}
			//	actions+=/holy_prism,if=!talent.seraphim.enabled|buff.seraphim.up|cooldown.seraphim.remains>5|time<5
			if (!HasSpell ("Seraphim") || Me.HasAura ("Seraphim") || Cooldown ("Seraphim") > 5 || Time < 5) {
				if (HolyPrism ())
					return true;
			}
			//	actions+=/consecration,if=target.debuff.flying.down&active_enemies>=3
			if (!Target.IsFlying && ActiveEnemies (8) >= 3) {
				if (Consecration ())
					return true;
			}
			//	actions+=/execution_sentence,if=!talent.seraphim.enabled|buff.seraphim.up|time<12
			if (!HasSpell ("Seraphim") || Me.HasAura ("Seraphim") || Time < 12) {
				if (Health (Me) < 0.4 && ExecutionSentence (Me))
					return true;
			}
			//	actions+=/hammer_of_wrath
			if (HammerofWrath ())
				return true;
			//	actions+=/sacred_shield,if=target.dot.sacred_shield.remains<8
			if (Target.AuraTimeRemaining ("Sacred Shield") < 8) {
				if (SacredShield (Me))
					return true;
			}
			//	actions+=/consecration,if=target.debuff.flying.down
			if (!Target.IsFlying) {
				if (Consecration ())
					return true;
			}
			//	actions+=/holy_wrath
			if (HolyWrath ())
				return true;
			//	actions+=/seal_of_insight,if=talent.empowered_seals.enabled&!seal.insight&buff.uthers_insight.remains<=buff.liadrins_righteousness.remains
			if (HasSpell ("Empowered Seals") && !IsInShapeshiftForm ("Seal of Insight") && Me.AuraTimeRemaining ("Uther's Insight") <= Me.AuraTimeRemaining ("Liadrins Righteousness")) {
				if (SealofInsight ())
					return true;
			}
			//	actions+=/seal_of_righteousness,if=talent.empowered_seals.enabled&!seal.righteousness&buff.liadrins_righteousness.remains<=buff.uthers_insight.remains
			if (HasSpell ("Empowered Seals") && !IsInShapeshiftForm ("Seal of Righteousness") && Me.AuraTimeRemaining ("Uther's Insight") <= Me.AuraTimeRemaining ("Uther's Insight")) {
				if (SealofRighteousness ())
					return true;
			}
			//	actions+=/sacred_shield
			if (SacredShield (Me))
				return true;
			//	actions+=/flash_of_light,if=talent.selfless_healer.enabled&buff.selfless_healer.stack>=3
			if (HasSpell ("Selfless Healer") && GetAuraStack ("Selfless Healer", Me) >= 3) {
				if (Health (Me) < 0.8 && FlashofLight (Me))
					return true;
			}

			return false;
		}

		public bool MaxDPS ()
		{
			//	# This is a high-DPS (but low-survivability) configuration.
			//	# Invoke by adding "actions+=/run_action_list,name=max_dps" to the beginning of the default APL.
			//
			//	actions.max_dps=auto_attack
			//	actions.max_dps+=/speed_of_light,if=movement.remains>1
			if (Range (40, Target, 20))
				SpeedofLight ();
			//	actions.max_dps+=/blood_fury
			BloodFury ();
			//	actions.max_dps+=/berserking
			Berserking ();
			//	actions.max_dps+=/arcane_torrent
			ArcaneTorrent ();
			//	# Off-GCD spells.
			//	actions.max_dps+=/holy_avenger
			HolyAvenger ();
			//	actions.max_dps+=/potion,name=draenic_armor,if=buff.holy_avenger.up|(!talent.holy_avenger.enabled&(buff.seraphim.up|(!talent.seraphim.enabled&buff.bloodlust.react)))|target.time_to_die<=20
			//	actions.max_dps+=/seraphim
			Seraphim ();
			//	actions.max_dps+=/shield_of_the_righteous,if=buff.divine_purpose.react
			if (Me.HasAura ("Divine Purpose"))
				ShieldoftheRighteous ();
			//	actions.max_dps+=/shield_of_the_righteous,if=(holy_power>=5|talent.holy_avenger.enabled)&(!talent.seraphim.enabled|cooldown.seraphim.remains>5)
			if ((HolyPower >= 5 || HasSpell ("Holy Avenger")) && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > 5)) {
				if (ShieldoftheRighteous ())
					return true;
			}
			//	actions.max_dps+=/shield_of_the_righteous,if=buff.holy_avenger.remains>time_to_hpg&(!talent.seraphim.enabled|cooldown.seraphim.remains>time_to_hpg)
			if (Me.AuraTimeRemaining ("Holy Avenger") > TimeToHpg && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > TimeToHpg)) {
				if (ShieldoftheRighteous ())
					return true;
			}
			//	# GCD-bound spells

			//	actions.max_dps+=/avengers_shield,if=buff.grand_crusader.react&active_enemies>1&!glyph.focused_shield.enabled
			if (Me.HasAura ("Grand Crusader") && ActiveEnemies (30) > 1 && !HasGlyph (54930)) {
				if (AvengersShield ())
					return true;
			}
			//	actions.max_dps+=/holy_wrath,if=talent.sanctified_wrath.enabled&(buff.seraphim.react|(glyph.final_wrath.enabled&target.health.pct<=20))
			if (HasSpell ("Sanctified Wrath") && (Me.HasAura ("Seraphim") || (HasGlyph (54935) && Health () <= 0.2))) {
				if (HolyWrath ())
					return true;
			}
			//	actions.max_dps+=/hammer_of_the_righteous,if=active_enemies>=3
			if (ActiveEnemies (8) >= 3) {
				if (HammeroftheRighteous ())
					return true;
			}
			//	actions.max_dps+=/judgment,if=talent.empowered_seals.enabled&buff.liadrins_righteousness.down
			if (HasSpell ("Empowered Seals") && !Me.HasAura ("Liadrins Righteousness")) {
				if (Judgment ())
					return true;
			}
			//	actions.max_dps+=/crusader_strike
			if (CrusaderStrike ())
				return true;
			//	actions.max_dps+=/wait,sec=cooldown.crusader_strike.remains,if=cooldown.crusader_strike.remains>0&cooldown.crusader_strike.remains<=0.35
			if (HasSpell ("Crusader Strike") && Cooldown ("Crusader Strike") > 0 && Cooldown ("Crusader Strike") <= 0.35) {
				WaitCrusaderStrike = true;
				return true;
			}
			//	actions.max_dps+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&last_judgment_target!=target
			if (Usable ("Judgment") && HasGlyph (54922)) {
				var Unit = Enemy.Where (u => u != LastJudgmentTarget).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (Judgment (Unit))
						LastJudgmentTarget = Unit;
					return true;
				}
			}
			//	actions.max_dps+=/judgment
			if (Judgment ()) {
				LastJudgmentTarget = Target;
				return true;
			}
			//	actions.max_dps+=/wait,sec=cooldown.judgment.remains,if=cooldown.judgment.remains>0&cooldown.judgment.remains<=0.35
			if (HasSpell ("Judgment") && Cooldown ("Judgment") > 0 && Cooldown ("Judgment") <= 0.35) {
				WaitJudgment = true;
				return true;
			}
			//	actions.max_dps+=/avengers_shield,if=active_enemies>1&!glyph.focused_shield.enabled
			if (ActiveEnemiesWithTarget (10) > 1 && !HasGlyph (54930)) {
				if (AvengersShield ())
					return true;
			}
			//	actions.max_dps+=/holy_wrath,if=talent.sanctified_wrath.enabled
			if (HasSpell ("Sanctified Wrath")) {
				if (HolyWrath ())
					return true;
			}
			//	actions.max_dps+=/avengers_shield,if=buff.grand_crusader.react
			if (Me.HasAura ("Grand Crusader")) {
				if (AvengersShield ())
					return true;
			}
			//	actions.max_dps+=/execution_sentence,if=active_enemies<3
			if (ActiveEnemies (30) < 3) {
				if (ExecutionSentence ())
					return true;
			}
			//	actions.max_dps+=/holy_wrath,if=glyph.final_wrath.enabled&target.health.pct<=20
			if (HasGlyph (54935) && Health () <= 0.2) {
				if (HolyWrath ())
					return true;
			}
			//	actions.max_dps+=/avengers_shield
			if (AvengersShield ())
				return true;
			//	actions.max_dps+=/seal_of_righteousness,if=talent.empowered_seals.enabled&!seal.righteousness
			if (HasSpell ("Empowered Seals") && !IsInShapeshiftForm ("Seal of Righteousness")) {
				if (SealofRighteousness ())
					return true;
			}
			//	actions.max_dps+=/lights_hammer
			if (LightsHammer ())
				return true;
			//	actions.max_dps+=/holy_prism
			if (HolyPrism ())
				return true;
			//	actions.max_dps+=/consecration,if=target.debuff.flying.down&active_enemies>=3
			if (!Target.IsFlying && ActiveEnemies (8) >= 3) {
				if (Consecration ())
					return true;
			}
			//	actions.max_dps+=/execution_sentence
			if (ExecutionSentence ())
				return true;
			//	actions.max_dps+=/hammer_of_wrath
			if (HammerofWrath ())
				return true;
			//	actions.max_dps+=/consecration,if=target.debuff.flying.down
			if (!Target.IsFlying) {
				if (Consecration ())
					return true;
			}
			//	actions.max_dps+=/holy_wrath
			if (HolyWrath ())
				return true;
			//	actions.max_dps+=/sacred_shield
			if (SacredShield (Me))
				return true;
			//	actions.max_dps+=/flash_of_light,if=talent.selfless_healer.enabled&buff.selfless_healer.stack>=3
			if (HasSpell ("Selfless Healer") && GetAuraStack ("Selfless Healer", Me) >= 3) {
				if (Health (Me) < 0.8 && FlashofLight (Me))
					return true;
			}

			return false;
		}

		public bool MaxSurvival ()
		{
			//	# This is a high-survivability (but low-DPS) configuration.
			//	# Invoke by adding "actions+=/run_action_list,name=max_survival" to the beginning of the default APL.
			//
			//	actions.max_survival=auto_attack
			//	actions.max_survival+=/speed_of_light,if=movement.remains>1
			if (Range (40, Target, 20))
				SpeedofLight ();
			//	actions.max_survival+=/blood_fury
			BloodFury ();
			//	actions.max_survival+=/berserking
			Berserking ();
			//	actions.max_survival+=/arcane_torrent
			ArcaneTorrent ();
			//	# Off-GCD spells.

			//	actions.max_survival+=/holy_avenger
			HolyAvenger ();
			//	actions.max_survival+=/potion,name=draenic_armor,if=buff.shield_of_the_righteous.down&buff.seraphim.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down&buff.ardent_defender.down
			//	actions.max_survival+=/divine_protection,if=time<5|!talent.seraphim.enabled|(buff.seraphim.down&cooldown.seraphim.remains>5&cooldown.seraphim.remains<9)
			if (Time < 5 || !HasSpell ("Seraphim") || (!Me.HasAura ("Seraphim") && Cooldown ("Seraphim") > 5 && Cooldown ("Seraphim") < 9)) {
				if (NeedDivineProtection)
					DivineProtection ();
			}
			//	actions.max_survival+=/seraphim,if=buff.divine_protection.down&cooldown.divine_protection.remains>0
			if (!Me.HasAura ("Divine Protection") && Me.AuraTimeRemaining ("Divine Protection") > 0) {
				Seraphim ();
			}
			//	actions.max_survival+=/guardian_of_ancient_kings,if=buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down
			if (!Me.HasAura ("Holy Avenger") && !Me.HasAura ("Shield of the Righteous") && !Me.HasAura ("Divine Protection") && !Me.HasAura ("Ardent Defender")) {
				if (Health (Me) <= 0.4)
					GuardianofAncientKings ();
			}
			//	actions.max_survival+=/ardent_defender,if=buff.holy_avenger.down&buff.shield_of_the_righteous.down&buff.divine_protection.down&buff.guardian_of_ancient_kings.down
			if (!Me.HasAura ("Holy Avenger") && !Me.HasAura ("Shield of the Righteous") && !Me.HasAura ("Divine Protection") && !Me.HasAura ("Guardian of Ancient Kings")) {
				if (Health (Me) <= 0.2)
					ArdentDefender ();
			}
			//	actions.max_survival+=/eternal_flame,if=buff.eternal_flame.remains<2&buff.bastion_of_glory.react>2&(holy_power>=3|buff.divine_purpose.react|buff.bastion_of_power.react)
			if (Me.AuraTimeRemaining ("Eternal Flame") < 2 && GetAuraStack ("Bastion of Glory", Me) > 2 && (HolyPower >= 3 || Me.HasAura ("Divine Purpose") || Me.HasAura ("Bastion of Power"))) {
				if (EternalFlame ())
					return true;
			}
			//	actions.max_survival+=/eternal_flame,if=buff.bastion_of_power.react&buff.bastion_of_glory.react>=5
			if (Me.HasAura ("Bastion of Power") && GetAuraStack ("Bastion of Glory", Me) >= 5) {
				if (EternalFlame ())
					return true;
			}
			//	actions.max_survival+=/shield_of_the_righteous,if=buff.divine_purpose.react
			if (Me.HasAura ("Divine Purpose")) {
				ShieldoftheRighteous ();
			}
			//	actions.max_survival+=/shield_of_the_righteous,if=(holy_power>=5|incoming_damage_1500ms>=health.max*0.3)&(!talent.seraphim.enabled|cooldown.seraphim.remains>5)
			if ((HolyPower >= 5 || DamageTaken (1500) >= Me.MaxHealth * 0.3) && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > 5)) {
				ShieldoftheRighteous ();
			}
			//	actions.max_survival+=/shield_of_the_righteous,if=buff.holy_avenger.remains>time_to_hpg&(!talent.seraphim.enabled|cooldown.seraphim.remains>time_to_hpg)
			if (Me.AuraTimeRemaining ("Holy Avenger") > TimeToHpg && (!HasSpell ("Seraphim") || Cooldown ("Seraphim") > TimeToHpg)) {
				ShieldoftheRighteous ();
			}
			//	# GCD-bound spells
			if (HasGlobalCooldown () && Gcd)
				return true;
			//	actions.max_survival+=/hammer_of_the_righteous,if=active_enemies>=3
			if (ActiveEnemies (8) >= 3) {
				if (HammeroftheRighteous ())
					return true;
			}
			//	actions.max_survival+=/crusader_strike
			if (CrusaderStrike ())
				return true;
			//	actions.max_survival+=/wait,sec=cooldown.crusader_strike.remains,if=cooldown.crusader_strike.remains>0&cooldown.crusader_strike.remains<=0.35
			if (HasSpell ("Crusader Strike") && Cooldown ("Crusader Strike") > 0 && Cooldown ("Crusader Strike") <= 0.35) {
				WaitCrusaderStrike = true;
				return true;
			}
			//	actions.max_survival+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&last_judgment_target!=target
			if (Usable ("Judgment") && HasGlyph (54922)) {
				var Unit = Enemy.Where (u => u != LastJudgmentTarget).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Judgment (Unit)) {
					LastJudgmentTarget = Unit;
					return true;
				}
			}
			//	actions.max_survival+=/judgment
			if (Judgment ()) {
				LastJudgmentTarget = Target;
				return true;
			}
			//	actions.max_survival+=/wait,sec=cooldown.judgment.remains,if=cooldown.judgment.remains>0&cooldown.judgment.remains<=0.35
			if (HasSpell ("Judgment") && Cooldown ("Judgment") > 0 && Cooldown ("Judgment") <= 0.35) {
				WaitJudgment = true;
				return true;
			}
			//	actions.max_survival+=/avengers_shield,if=buff.grand_crusader.react&active_enemies>1
			if (Me.HasAura ("Grand Crusader") && ActiveEnemies (30) > 1) {
				if (AvengersShield ())
					return true;
			}
			//	actions.max_survival+=/holy_wrath,if=talent.sanctified_wrath.enabled
			if (HasSpell ("Sanctified Wrath")) {
				if (HolyWrath ())
					return true;
			}
			//	actions.max_survival+=/avengers_shield,if=buff.grand_crusader.react
			if (Me.HasAura ("Grand Crusader")) {
				if (AvengersShield ())
					return true;
			}
			//	actions.max_survival+=/sacred_shield,if=target.dot.sacred_shield.remains<2
			if (Target.AuraTimeRemaining ("Sacred Shield") < 2) {
				if (SacredShield (Me))
					return true;
			}
			//	actions.max_survival+=/avengers_shield
			if (AvengersShield ())
				return true;
			//	actions.max_survival+=/lights_hammer
			if (LightsHammer ())
				return true;
			//	actions.max_survival+=/holy_prism
			if (HolyPrism ())
				return true;
			//	actions.max_survival+=/consecration,if=target.debuff.flying.down&active_enemies>=3
			if (!Target.IsFlying && ActiveEnemies (8) >= 3) {
				if (Consecration ())
					return true;
			}
			//	actions.max_survival+=/execution_sentence
			if (Health (Me) < 0.4 && ExecutionSentence (Me))
				return true;
			//	actions.max_survival+=/flash_of_light,if=talent.selfless_healer.enabled&buff.selfless_healer.stack>=3
			if (HasSpell ("Selfless Healer") && GetAuraStack ("Selfless Healer", Me) >= 3) {
				if (Health (Me) < 0.8 && FlashofLight (Me))
					return true;
			}
			//	actions.max_survival+=/hammer_of_wrath
			if (HammerofWrath ())
				return true;
			//	actions.max_survival+=/sacred_shield,if=target.dot.sacred_shield.remains<8
			if (Target.AuraTimeRemaining ("Sacred Shield") < 8) {
				if (SacredShield (Me))
					return true;
			}
			//	actions.max_survival+=/holy_wrath,if=glyph.final_wrath.enabled&target.health.pct<=20
			if (HasGlyph (54935) && Health () <= 0.2) {
				if (HolyWrath ())
					return true;
			}
			//	actions.max_survival+=/consecration,if=target.debuff.flying.down&!ticking
			if (!Target.IsFlying && !Target.HasAura ("Consecration")) {
				if (Consecration ())
					return true;
			}
			//	actions.max_survival+=/holy_wrath
			if (HolyWrath ())
				return true;
			//	actions.max_survival+=/sacred_shield
			if (SacredShield (Me))
				return true;

			return false;
		}
	}
}
	