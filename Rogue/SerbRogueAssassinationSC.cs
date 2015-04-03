using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Combat Rogue SC", "Serb", WoWClass.Rogue, Specialization.RogueAssassination, 5, 25)]

	public class SerbRogueAssassinationSC : SerbRogue
	{

		[JsonProperty ("MainHand Poison"), JsonConverter (typeof(StringEnumConverter))]
		public PoisonMaindHand MH = PoisonMaindHand.InstantPoison;
		[JsonProperty ("OffHand Poison"), JsonConverter (typeof(StringEnumConverter))]
		public PoisonOffHand OH = PoisonOffHand.CripplingPoison;


		public SerbRogueAssassinationSC ()
		{
			RangedAttack = HasSpell ("Shuriken Toss") ? "Shuriken Toss" : "Throw";
		}

		public override bool OutOfCombat ()
		{
			//actions.precombat=flask,type=greater_draenic_agility_flask
			//actions.precombat+=/food,type=sleeper_sushi
			//actions.precombat+=/apply_poison,lethal=deadly
			if (MainHandPoison (MH))
				return true;
			if (OffHandPoison (OH))
				return true;
			//# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//actions.precombat+=/snapshot_stats
			//actions.precombat+=/potion,name=draenic_agility
			//actions.precombat+=/stealth
			//actions.precombat+=/marked_for_death
			//actions.precombat+=/slice_and_dice,if=talent.marked_for_death.enabled

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
			if (Health < 0.45)
			if (Healthstone ())
				return;

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


//			actions=potion,name=draenic_agility,if=buff.bloodlust.react|target.time_to_die<40|debuff.vendetta.up
//			actions+=/kick
//			actions+=/preparation,if=!buff.vanish.up&cooldown.vanish.remains>60
			if (!Me.HasAura ("Vanish") && Cooldown ("Vanish") > 60) {
				if (Preparation ())
					return;
			}
//			actions+=/use_item,slot=trinket2,if=active_enemies>1|(debuff.vendetta.up&active_enemies=1)
//			actions+=/blood_fury
			if (BloodFury ())
				return;
//			actions+=/berserking
			if (Berserking ())
				return;
//			actions+=/arcane_torrent,if=energy<60
			if (Energy < 60) {
				if (ArcaneTorrent ())
					return;
			}
//			actions+=/vanish,if=time>10&!buff.stealth.up
			if (Time > 10 && !Me.HasAura ("Stealth")) {
				if (Vanish ())
					return;
			}
//			actions+=/rupture,if=combo_points=5&ticks_remain<3
			if (ComboPoints == 5 && Target.AuraTimeRemaining ("Rupture", true) < 3) {
				if (Rupture ())
					return;
			}
//			actions+=/rupture,cycle_targets=1,if=active_enemies>1&!ticking&combo_points=5
			if (EnemyInRange (6) > 1 && ComboPoints == 5) {
				CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && !x.HasAura ("Rupture")).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rupture ())
						return;
				}
			}

			if (ComboPoints == 5 && !InRaid && !InInstance) {
				if (KidneyShot ())
					return;
			}

//			actions+=/mutilate,if=buff.stealth.up
			if (Me.HasAura ("Stealth")) {
				if (Mutilate ())
					return;
			}
//			actions+=/slice_and_dice,if=buff.slice_and_dice.remains<5
			if (Me.AuraTimeRemaining ("Slice and Dice") < 5) {
				if (SliceandDice ())
					return;
			}
//			actions+=/marked_for_death,if=combo_points=0
			if (ComboPoints == 0) {
				if (MarkedforDeath ())
					return;
			}
//			actions+=/crimson_tempest,if=combo_points>4&active_enemies>=4&remains<8
			if (ComboPoints >= 4 && EnemyInRange (10) >= 4 && Target.AuraTimeRemaining ("Crimson Tempest", true) < 8) {
				if (CrimsonTempest ())
					return;
			}
//			actions+=/fan_of_knives,if=(combo_points<5|(talent.anticipation.enabled&anticipation_charges<4))&active_enemies>=4
			if (((!HasSpell ("Anticipation") && ComboPoints < 5) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 4)) && EnemyInRange (10) >= 4) {
				if (FanofKnives ())
					return;
			}
//			actions+=/rupture,if=(remains<2|(combo_points=5&remains<=(duration*0.3)))&active_enemies=1
			if ((Target.AuraTimeRemaining ("Rupture", true) < 2 || (ComboPoints == 5 && Target.AuraTimeRemaining ("Rupture", true) <= (24 * 0.3))) && EnemyInRange (6) == 1) {
				if (Rupture ())
					return;
			}
//			actions+=/shadow_reflection,if=combo_points>4
			if (ComboPoints > 4) {
				if (ShadowReflection ())
					return;
			}
//			actions+=/vendetta,if=buff.shadow_reflection.up|!talent.shadow_reflection.enabled
			if (Me.HasAura ("Shadow Reflection") || !HasSpell ("Shadow Reflection")) {
				if (Vendetta ())
					return;
			}
//			actions+=/death_from_above,if=combo_points>4
			if (ComboPoints > 4) {
				if (DeathfromAbove ())
					return;
			}
//			actions+=/envenom,cycle_targets=1,if=(combo_points>4&(cooldown.death_from_above.remains>2|!talent.death_from_above.enabled))&active_enemies<4&!dot.deadly_poison_dot.ticking
			if (ComboPoints > 4 && (Cooldown ("Death from Above") > 2 || !HasSpell ("Death from Above")) && EnemyInRange (6) < 4) {
				CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && !x.HasAura ("Deadly Poison", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Envenom (CycleTarget))
						return;
				}
			}
//			actions+=/envenom,if=(combo_points>4&(cooldown.death_from_above.remains>2|!talent.death_from_above.enabled))&active_enemies<4&(buff.envenom.remains<=1.8|energy>55)
			if ((ComboPoints > 4 && (Cooldown ("Death from Above") > 2 || !HasSpell ("Death from Above"))) && EnemyInRange (6) < 4 && (Target.AuraTimeRemaining ("Envenom") <= 1.8 || Energy > 55)) {
				if (Envenom ())
					return;
			}
//			actions+=/fan_of_knives,cycle_targets=1,if=active_enemies>2&!dot.deadly_poison_dot.ticking&debuff.vendetta.down
			if (Energy >= 35 && EnemyInRange (10) > 2) {
				CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 10 && !x.HasAura ("Deadly Poison", true) && !x.HasAura ("Vendetta", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (FanofKnives ())
						return;
				}
			}
//			actions+=/dispatch,cycle_targets=1,if=(combo_points<5|(talent.anticipation.enabled&anticipation_charges<4))&active_enemies=2&!dot.deadly_poison_dot.ticking&debuff.vendetta.down
			if (((!HasSpell ("Anticipation") && ComboPoints < 5) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 4)) && EnemyInRange (6) == 2) {
				CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && !x.HasAura ("Deadly Poison", true) && !x.HasAura ("Vendetta", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Dispatch (CycleTarget))
						return;
				}
			}
//			actions+=/dispatch,if=(combo_points<5|(talent.anticipation.enabled&anticipation_charges<4))&active_enemies<4
			if (((!HasSpell ("Anticipation") && ComboPoints < 5) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 4)) && EnemyInRange (6) < 4) {
				if (Dispatch ())
					return;
			}
//			actions+=/mutilate,cycle_targets=1,if=target.health.pct>35&(combo_points<4|(talent.anticipation.enabled&anticipation_charges<3))&active_enemies=2&!dot.deadly_poison_dot.ticking&debuff.vendetta.down
			if (((!HasSpell ("Anticipation") && ComboPoints < 4) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 3)) && EnemyInRange (6) == 2) {
				CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.HealthFraction > 0.35 && !x.HasAura ("Deadly Poison", true) && !x.HasAura ("Vendetta", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Mutilate (CycleTarget))
						return;
				}
			}
//			actions+=/mutilate,if=target.health.pct>35&(combo_points<4|(talent.anticipation.enabled&anticipation_charges<3))&active_enemies<5
			if (TargetHealth > 0.35 && ((!HasSpell ("Anticipation") && ComboPoints < 4) || (HasSpell ("Anticipation") && SpellCharges ("Anticipation") < 3)) && EnemyInRange (6) < 5) {
				if (Mutilate ())
					return;
			}
//			actions+=/mutilate,cycle_targets=1,if=active_enemies=2&!dot.deadly_poison_dot.ticking&debuff.vendetta.down
			if (EnemyInRange (6) == 2) {
				CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && !x.HasAura ("Deadly Poison", true) && !x.HasAura ("Vendetta", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Mutilate (CycleTarget))
						return;
				}
			}
//			actions+=/mutilate,if=active_enemies<5
			if (EnemyInRange (6) < 5) {
				if (Mutilate ())
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
