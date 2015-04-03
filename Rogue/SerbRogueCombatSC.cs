using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Combat Rogue SC", "Serb", WoWClass.Rogue, Specialization.RogueCombat, 5, 25)]

	public class SerbRogueCombatSC : SerbRogue
	{

		[JsonProperty ("MainHand Poison"), JsonConverter (typeof(StringEnumConverter))]
		public PoisonMaindHand MH = PoisonMaindHand.InstantPoison;
		[JsonProperty ("OffHand Poison"), JsonConverter (typeof(StringEnumConverter))]
		public PoisonOffHand OH = PoisonOffHand.CripplingPoison;


		public SerbRogueCombatSC ()
		{
			RangedAttack = HasSpell ("Shuriken Toss") ? "Shuriken Toss" : "Throw";
		}

		public override bool OutOfCombat ()
		{
			// actions.precombat=flask,type=greater_draenic_agility_flask
			// actions.precombat+=/food,type=buttered_sturgeon
			// actions.precombat+=/apply_poison,lethal=deadly
			if (MainHandPoison (MH))
				return true;
			if (OffHandPoison (OH))
				return true;
			// # Snapshot raid buffed stats before combat begins and pre-potting is done.
			// actions.precombat+=/snapshot_stats
			// actions.precombat+=/potion,name=draenic_agility
			// actions.precombat+=/stealth
			// actions.precombat+=/marked_for_death
			// actions.precombat+=/slice_and_dice,if=talent.marked_for_death.enabled

			if (HasAura ("Blade Flurry")) {
				CancelAura ("Blade Flurry");
				return true;
			}

			if (Me.IsMoving && UseBurstOfSpeed) {
				if (BurstofSpeed ())
					return true;
			}

			// if ((InBG || InArena) && TimeToStartBattle < 5)
			// 	if (Stealth()) return true;

			// if (HasAura("Stealth") && Energy >= 35 && (InArena || InBG)) {
			// 	foreach(UnitObject p in API.CollectUnits(10)) {
			// 		if (p.IsEnemy && !p.IsDead && !p.InCombat && p.IsPlayer && !p.HasAura("Sap") && (p.HasAura("Stealth") || p.HasAura("Prowl") || p.HasAura("Shadowmeld") || p.HasAura("Camouflage") || p.HasAura("Invisibility"))) {
			// 			if (Cast("Sap")) return true;
			// 		}
			// 	}
			// }

			// Heal
			if ((!InRaid && Health < 0.8) || (Health < 0.3)) {
				if (Recuperate ())
					return true;
			}

			if (Me.Auras.Any (x => x.IsDebuff && x.DebuffType.Contains ("magic")))
				CloakofShadows ();

//			if (!InArena)
			if (CrystalOfInsanity ())
				return true;

			// if (!InArena)
			if (OraliusWhisperingCrystal ())
				return true;


			if (InCombat == true) {
				InCombat = false;
				return true;
			}

			return false;
		}

		public override void Combat ()
		{
			if (InCombat == false) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			var targets = Adds;
			targets.Add (Target);

			if (Health < 0.55 || (Health < 0.4 && Me.Auras.Any (x => x.IsDebuff && x.DebuffType.Contains ("magic"))))
				CloakofShadows ();
			if (Health < 0.65)
				CombatReadiness ();
			if (Health < 0.4)
				Evasion ();
			if (Health < 0.45) {
				if (Healthstone ())
					return;
			}

			if ((!InRaid && !InInstance && Health < 0.9) || (!InRaid && Health < 0.3)) {
				if (Recuperate ())
					return;
			}

			if (Me.CanNotParticipateInCombat ())
				Freedom ();

			if (!Me.IsMoving && Health < 0.5) {
				if (SmokeBomb ())
					return;
			}

			if (!Me.HasAura ("Stealth")) {
				Interrupt ();
				UnEnrage ();
			}

			if (ComboPoints < 4)
				Premeditation ();

			if (InRaid && InInstance)
				TricksoftheTrade ();

			if (HasGlobalCooldown () && GCD)
				return;

			if (HasAura ("Blade Flurry") && !InRaid && !InInstance && IncapacitatedInRange (8) && EnemyInRange (8) < 3)
				CancelAura ("Blade Flurry");

			if ((InArena || InBG) && Health < 0.7) {
				if (Feint ())
					return;
			}

			if (Usable ("Feint") && Health < 0.8 && (InRaid || InInstance)) {
				var UseFeint = targets.Where (x => IsBoss (x) && x.CombatRange <= 30 && x.IsCasting && x.RemainingCastTime > Me.AuraTimeRemaining ("Feint")).DefaultIfEmpty (null).FirstOrDefault ();
				if (UseFeint != null) {
					if (Feint ())
						return;
				}
			}

			if ((InBG && InArena) && IsPlayer && Target.CanParticipateInCombat) {
				if (CheapShot ())
					return;
			}

			if (!InRaid && Usable ("Gouge") && EnemyInRange (6) == 2 && Multitarget) {
				CycleTarget = Adds.Where (x => x.IsInCombatRangeAndLoS && x.CanParticipateInCombat && Target != x).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Gouge (CycleTarget))
						return;
				}
			}

			if (!InRaid && Usable ("Blind") && EnemyInRange (6) == 2 && Multitarget) {
				CycleTarget = Adds.Where (x => x.IsInLoS && x.CombatRange <= 15 && x.CanParticipateInCombat && Target != x).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Blind (CycleTarget))
						return;
				}
			}

			// actions=potion,name=draenic_agility,if=buff.bloodlust.react|target.time_to_die<40|(buff.adrenaline_rush.up&(trinket.proc.any.react|trinket.stacking_proc.any.react|buff.archmages_greater_incandescence_agi.react))
			// actions+=/kick
			// actions+=/preparation,if=!buff.vanish.up&cooldown.vanish.remains>30
			if (!Me.HasAura ("Vanish") && Cooldown ("Vanish") > 30) {
				if (Preparation ())
					return;
			}
			// actions+=/use_item,slot=trinket2
			// actions+=/blood_fury
			if (BloodFury ())
				return;
			// actions+=/berserking
			if (Berserking ())
				return;
			// actions+=/arcane_torrent,if=energy<60
			if (Energy < 60) {
				if (ArcaneTorrent ())
					return;
			}
			// actions+=/blade_flurry,if=(active_enemies>=2&!buff.blade_flurry.up)|(active_enemies<2&buff.blade_flurry.up)
			if (!Me.HasAura ("Blade Flurry") && EnemyInRange (8) >= 2) {
				if (!IncapacitatedInRange (8) && (!InArena || (InArena && EnemyInRange (8) > 2))) {
					if (BladeFlurry ())
						return;
				}
			}
			if (HasAura ("Blade Flurry") && EnemyInRange (8) < 2) {
				if (BladeFlurry ())
					return;
			}
			// actions+=/shadow_reflection,if=(cooldown.killing_spree.remains<10&combo_points>3)|buff.adrenaline_rush.up
			if ((HasSpell ("Killing Spree") && Cooldown ("Killing Spree") < 10 && ComboPoints > 3) || HasAura ("Adrenaline Rush")) {
				if (ShadowReflection ())
					return;
			}
			// actions+=/ambush
			if (Ambush ())
				return;
			// actions+=/vanish,if=time>10&(combo_points<3|(talent.anticipation.enabled&anticipation_charges<3)|(combo_points<4|(talent.anticipation.enabled&anticipation_charges<4)))&((talent.shadow_focus.enabled&buff.adrenaline_rush.down&energy<90&energy>=15)|(talent.subterfuge.enabled&energy>=90)|(!talent.shadow_focus.enabled&!talent.subterfuge.enabled&energy>=60))
			if (Time > 10 && ((!HasSpell ("Anticipation") && ComboPoints < 3) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 3) || ((!HasSpell ("Anticipation") && ComboPoints < 4) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 4))) && ((HasSpell ("Shadow Focus") && !HasSpell ("Adrenaline Rush") && Energy < 90 && Energy >= 15) || (HasSpell ("Subterfuge") && Energy >= 90) || (!HasSpell ("Shadow Focus") && !HasSpell ("Subterfuge") && Energy >= 60))) {
				if (Vanish ())
					return;
			}
			// actions+=/slice_and_dice,if=buff.slice_and_dice.remains<2|((target.time_to_die>45&combo_points=5&buff.slice_and_dice.remains<12)&buff.deep_insight.down)
			if (Me.AuraTimeRemaining ("Slice and Dice") < 2 || ((TimeToDie (Target) > 45 && ComboPoints == 5 && Me.AuraTimeRemaining ("Slice and Dice") < 12) && !HasAura ("Deep Insight"))) {
				if (SliceandDice ())
					return;
			}
			// actions+=/call_action_list,name=adrenaline_rush,if=cooldown.killing_spree.remains>10
			if (Cooldown ("Killing Spree") > 10 || (!HasSpell ("Killing Spree") && IsBoss (Target))) {
				if (ActionAdrenalineRush ())
					return;
			}
			// actions+=/call_action_list,name=killing_spree,if=(energy<40|(buff.bloodlust.up&time<10)|buff.bloodlust.remains>20)&buff.adrenaline_rush.down&(!talent.shadow_reflection.enabled|cooldown.shadow_reflection.remains>30|buff.shadow_reflection.remains>3)
			if ((Energy < 40 || (HasAura ("Bloodlust") && Time < 10) || Me.AuraTimeRemaining ("Bloodlust") > 20) && !HasAura ("Adrenaline Rush") && (!HasSpell ("Shadow Reflection") || Cooldown ("Shadow Reflection") > 30 || Me.AuraTimeRemaining ("Shadow Reflection") > 3)) {
				if (ActionKillingSpree ())
					return;
			}
			// actions+=/marked_for_death,if=combo_points<=1&dot.revealing_strike.ticking&(!talent.shadow_reflection.enabled|buff.shadow_reflection.up|cooldown.shadow_reflection.remains>30)
			if (ComboPoints <= 1 && Target.HasAura ("Revealing Strike", true) && (!HasSpell ("Shadow Reflection") || Me.HasAura ("Shadow Reflection") || Cooldown ("Shadow Reflection") > 30)) {
				if (MarkedforDeath ())
					return;
			}
			// actions+=/call_action_list,name=generator,if=combo_points<5|!dot.revealing_strike.ticking|(talent.anticipation.enabled&anticipation_charges<3&buff.deep_insight.down)
			if ((!HasSpell ("Anticipation") && ComboPoints < 5) || !Target.HasAura ("Revealing Strike", true) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 3 && !HasAura ("Deep Insight"))) {
				if (ActionGenerators ())
					return;
			}
			// actions+=/call_action_list,name=finisher,if=combo_points=5&dot.revealing_strike.ticking&(buff.deep_insight.up|!talent.anticipation.enabled|(talent.anticipation.enabled&anticipation_charges>=3))
			if (ComboPoints == 5 && Target.HasAura ("Revealing Strike", true) && (Me.HasAura ("Deep Insight") || !HasSpell ("Anticipation") || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") >= 3))) {
				if (ActionFinishers ())
					return;
			}

			// run to enemy
			if (Target.CombatRange > 10 && Me.IsMoving && Run) {
				if (ActionRun ())
					return;
			}

			if (Me.IsMoving && Target.CombatRange > 6) {
				if (BurstofSpeed ())
					return;
			}

			if (Cast (RangedAttack, () => Energy >= 40 && !Me.IsMoving && !HasAura ("Stealth") && Target.IsInLoS && Target.CombatRange > 10 && Target.CombatRange <= 30 && UseRangedAttack))
				return;
		}

		public bool ActionAdrenalineRush ()
		{
			// actions.adrenaline_rush=adrenaline_rush,if=time_to_die>=44
			if (TimeToDie (Target) >= 44) {
				if (AdrenalineRush ())
					return true;
			}
			// actions.adrenaline_rush+=/adrenaline_rush,if=time_to_die<44&(buff.archmages_greater_incandescence_agi.react|trinket.proc.any.react|trinket.stacking_proc.any.react)
			if (TimeToDie (Target) < 44 && (HasAura (177172))) {
				if (AdrenalineRush ())
					return true;
			}
			// actions.adrenaline_rush+=/adrenaline_rush,if=time_to_die<=buff.adrenaline_rush.duration*1.5
			if (TimeToDie (Target) <= 22.5) {
				if (AdrenalineRush ())
					return true;
			}
			return false;
		}

		public bool ActionKillingSpree ()
		{
			// actions.killing_spree=killing_spree,if=time_to_die>=44
			if (TimeToDie (Target) >= 44) {
				if (KillingSpree ())
					return true;
			}
			// actions.killing_spree+=/killing_spree,if=time_to_die<44&buff.archmages_greater_incandescence_agi.react&buff.archmages_greater_incandescence_agi.remains>=buff.killing_spree.duration
			if (TimeToDie (Target) < 44 && Me.HasAura (177172) && Me.AuraTimeRemaining (177172) >= (8 - 3)) {
				if (KillingSpree ())
					return true;
			}
			// actions.killing_spree+=/killing_spree,if=time_to_die<44&trinket.proc.any.react&trinket.proc.any.remains>=buff.killing_spree.duration
			// actions.killing_spree+=/killing_spree,if=time_to_die<44&trinket.stacking_proc.any.react&trinket.stacking_proc.any.remains>=buff.killing_spree.duration
			// actions.killing_spree+=/killing_spree,if=time_to_die<=buff.killing_spree.duration*1.5
			if (TimeToDie (Target) <= 4.5) {
				if (KillingSpree ())
					return true;
			}
			return false;
		}

		// # Combo point generators
		public bool ActionGenerators ()
		{
			// actions.generator=revealing_strike,if=(combo_points=4&dot.revealing_strike.remains<7.2&(target.time_to_die>dot.revealing_strike.remains+7.2)|(target.time_to_die<dot.revealing_strike.remains+7.2&ticks_remain<2))|!ticking
			if ((ComboPoints == 4 && Target.AuraTimeRemaining ("Revealing Strike", true) < 7.2 && (TimeToDie (Target) > Target.AuraTimeRemaining ("Revealing Strike", true) + 7.2)) || !Target.HasAura ("Revealing Strike", true)) {
				if (RevealingStrike ())
					return true;
			}
			// actions.generator+=/sinister_strike,if=dot.revealing_strike.ticking
			if (Target.HasAura ("Revealing Strike", true)) {
				if (SinisterStrike ())
					return true;
			}
			return false;
		}

		// # Combo point finishers
		public bool ActionFinishers ()
		{
			if (!InRaid && !InInstance) {
				if (KidneyShot ())
					return true;
			}

			// actions.finisher=death_from_above
			if (DeathfromAbove ())
				return true;

			if (!InArena && EnemyInRange (10) >= 5) {
				if (CrimsonTempest ())
					return true;
			}

			// actions.finisher+=/eviscerate,if=(!talent.death_from_above.enabled|cooldown.death_from_above.remains)
			if (!HasSpell ("Death from Above") || Cooldown ("Death from Above") > 0) {
				if (Eviscerate ())
					return true;
			}

			return false;
		}

		public bool ActionRun ()
		{
			if (!HasAura ("Sprint")) {
				if (Shadowstep ())
					return true;
			}

			if (!HasAura ("Burst of Speed")) {
				if (Sprint ())
					return true;
			}

			// if (API.HasItem(109829) && !HasAura("Burst of Speed"))
			// 	if (API.UseItem(109829)) return true;

			return false;
		}
	}
}
