using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Subtlety Rogue SC", "Serb", WoWClass.Rogue, Specialization.RogueSubtlety, 5, 25)]

	public class SerbRogueSubtletySc : SerbRogue
	{

		[JsonProperty ("MainHand Poison"), JsonConverter (typeof(StringEnumConverter))]
		public PoisonMaindHand Mh = PoisonMaindHand.InstantPoison;
		[JsonProperty ("OffHand Poison"), JsonConverter (typeof(StringEnumConverter))]
		public PoisonOffHand Oh = PoisonOffHand.CripplingPoison;

		public double Sleep;
		public string Spell;

		public SerbRogueSubtletySc ()
		{
			PullSpells = new [] {
				"Garrote",
				"Ambush",
				"Backstab",
			};
			RangedAttack = HasSpell ("Shuriken Toss") ? "Shuriken Toss" : "Throw";
		}

		public override bool OutOfCombat ()
		{
			// actions.precombat=flask,type=greater_draenic_agility_flask
			// actions.precombat+=/food,type=salty_squid_roll
			// actions.precombat+=/apply_poison,lethal=deadly
			if (MainHandPoison (Mh))
				return true;
			if (OffHandPoison (Oh))
				return true;
			// # Snapshot raid buffed stats before combat begins and pre-potting is done.
			// actions.precombat+=/snapshot_stats
			// actions.precombat+=/potion,name=draenic_agility
			// actions.precombat+=/stealth
			// actions.precombat+=/marked_for_death
			// actions.precombat+=/premeditation,if=!talent.marked_for_death.enabled
			// actions.precombat+=/slice_and_dice
			// actions.precombat+=/premeditation
			// # Proxy Honor Among Thieves action. Generates Combo Points at a mean rate of 2.2 seconds. Comment out to disable (and use the real Honor Among Thieves).
			// actions.precombat+=/honor_among_thieves,cooldown=2.2,cooldown_stddev=0.1


			// if (HasAura("Stealth") && Energy >= 35 && (InArena || InBG)) {
			// 	foreach(UnitObject p in API.CollectUnits(10)) {
			// 		if (p.IsEnemy && !p.IsDead && !p.InCombat && p.IsPlayer && !p.HasAura("Sap") && (p.HasAura("Stealth") || p.HasAura("Prowl") || p.HasAura("Shadowmeld") || p.HasAura("Camouflage") || p.HasAura("Invisibility"))) {
			// 			if (Cast("Sap")) return true;
			// 		}
			// 	}
			// }

			// Heal
			if ((!InRaid && Health (Me) < 0.8) || (Health (Me) < 0.3)) {
				if (Recuperate ())
					return true;
			}
			if (Me.Auras.Any (x => x.IsDebuff && x.DebuffType.Contains ("magic")))
				CloakofShadows ();

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (UseBurstOfSpeed && Me.IsMoving)
				BurstofSpeed ();

			if (InCombat) {
				InCombat = false;
			}

			Sleep = 0;

			return false;
		}

		public override bool AfterCombat ()
		{
			if (InArena) {
				if (Stealth ())
					return true;
			}
			return false;
		}

		public override void Combat ()
		{
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			if (Health (Me) < 0.9) {
				if (Heal ())
					return;
			}

			if (Me.CanNotParticipateInCombat ())
				Freedom ();

			// actions=potion,name=draenic_agility,if=buff.bloodlust.react|target.time_to_die<40|(buff.shadow_reflection.up|(!talent.shadow_reflection.enabled&buff.shadow_dance.up))&(trinket.stat.agi.react|trinket.stat.multistrike.react|buff.archmages_greater_incandescence_agi.react)|((buff.shadow_reflection.up|(!talent.shadow_reflection.enabled&buff.shadow_dance.up))&target.time_to_die<136)
			// actions+=/kick
			if (!Me.HasAura ("Stealth")) {
				Interrupt ();
				UnEnrage ();
			}
			// actions+=/use_item,slot=trinket2,if=buff.shadow_dance.up
			// actions+=/shadow_reflection,if=buff.shadow_dance.up
			if (Me.HasAura ("Shadow Dance"))
				ShadowReflection ();
			// actions+=/blood_fury,if=buff.shadow_dance.up
			if (Me.HasAura ("Shadow Dance"))
				BloodFury ();
			// actions+=/berserking,if=buff.shadow_dance.up
			if (Me.HasAura ("Shadow Dance"))
				Berserking ();
			// actions+=/arcane_torrent,if=energy<60&buff.shadow_dance.up
			if (Energy < 60 && Me.HasAura ("Shadow Dance"))
				ArcaneTorrent ();
			// actions+=/premeditation,if=combo_points<4
			if (ComboPoints < 4)
				Premeditation ();

			// CastPreventDouble("Pick Pocket", () => HasSpell("Pick Pocket") && UsePickPocket && !Target.IsPlayer && !IsBoss(Target) && (Me.HasAura("Stealth") || Me.HasAura("Subterfuge") || Me.HasAura("Shadow Dance")));

			if (InRaid && InInstance)
				TricksoftheTrade ();

			if (HasGlobalCooldown () && Gcd)
				return;

			// Analysis disable once CompareOfFloatsByEqualityOperator

			if (Energy < Sleep)
				return;
			Sleep = 0;
			if (CastSpell (Spell))
				return;

			if (Cc ())
				return;

			// actions+=/pool_resource,for_next=1
			if (HasSpell ("Garrote") && !IsPlayer () && TimeToRegen (Cost (45)) < 1 && Time < 1 && (Me.HasAura ("Stealth") || Me.AuraTimeRemaining ("Subterfuge") >= 1 || Me.AuraTimeRemaining ("Vanish") >= 1 || Me.AuraTimeRemaining ("Shadow Dance") >= 1)) {
				Sleep = Cost (45);
				Spell = "Garrote";
				return;
			}
			// actions+=/garrote,if=time<1
//			if (Time < 1 && (Me.HasAura ("Stealth") || Me.HasAura ("Subterfuge") || Me.HasAura ("Vanish") || Me.HasAura ("Shadow Dance"))) {
//				if (Garrote ())
//					return;
//			}
			// actions+=/wait,sec=buff.subterfuge.remains-0.1,if=buff.subterfuge.remains>0.5&buff.subterfuge.remains<1.6&time>6
			if (HasSpell ("Subterfuge") && Me.AuraTimeRemaining ("Subterfuge") > 0.5 && Me.AuraTimeRemaining ("Subterfuge") < 1.6 && Time > 6) {
				Sleep = Energy + (Me.AuraTimeRemaining ("Subterfuge") - 0.1) * EnergyRegen;
				Spell = "Ambush";
				return;
			}
			// actions+=/pool_resource,for_next=1,extra_amount=50
			if (Usable ("Shadow Dance", TimeToRegen (50)) && !Me.HasAura ("Stealth") && Me.AuraTimeRemaining ("Vanish") < TimeToRegen (50) && (Target.AuraTimeRemaining ("Find Weakness", true) < TimeToRegen (50) || (Me.AuraTimeRemaining ("Bloodlust") >= TimeToRegen (50) || (Target.AuraTimeRemaining ("Hemorrhage", true) >= TimeToRegen (50) || Target.AuraTimeRemaining ("Garrote", true) >= TimeToRegen (50) || Target.AuraTimeRemaining ("Rupture", true) >= TimeToRegen (50))))) {
				Sleep = 50;
				Spell = "Shadow Dance";
				return;
			}
			// actions+=/shadow_dance,if=energy>=50&buff.stealth.down&buff.vanish.down&debuff.find_weakness.down|(buff.bloodlust.up&(dot.hemorrhage.ticking|dot.garrote.ticking|dot.rupture.ticking))
//			if (Energy >= 50 && !Me.HasAura ("Stealth") && !Me.HasAura ("Vanish") && !Target.HasAura ("Find Weakness", true) || (Me.HasAura ("Bloodlust") || (Target.HasAura ("Hemorrhage", true) || Target.HasAura ("Garrote", true) || Target.HasAura ("Rupture", true)))) {
//				if (ShadowDance ())
//					return;
//			}
			// actions+=/pool_resource,for_next=1,extra_amount=50
			if (Usable ("Shadowmeld", TimeToRegen (50)) && (IsPlayer () || InBg || InArena || InInstance || InRaid) && HasSpell ("Shadow Focus") && ((!HasSpell ("Anticipation") && ComboPoints < 4) || (HasSpell ("Anticipation") && ComboPoints < 3)) && !Me.HasAura ("Stealth") && Me.AuraTimeRemaining ("Shadow Dance") < TimeToRegen (45) && Me.AuraTimeRemaining ("Master of Subtlety") < TimeToRegen (45) && Target.AuraTimeRemaining ("Find Weakness", true) < TimeToRegen (45)) {
				Sleep = 50;
				Spell = "Shadowmeld";
				return;
			}
			// actions+=/shadowmeld,if=talent.shadow_focus.enabled&energy>=45&energy<=75&combo_points<4-talent.anticipation.enabled&buff.stealth.down&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.down
			// actions+=/pool_resource,for_next=1,extra_amount=50
			if (Usable ("Vanish", TimeToRegen (50)) && (IsPlayer () || InBg || InArena || InInstance || InRaid) && HasSpell ("Shadow Focus") && ((!HasSpell ("Anticipation") && ComboPoints < 4) || (HasSpell ("Anticipation") && ComboPoints < 3)) && Me.AuraTimeRemaining ("Shadow Dance") < TimeToRegen (50) && Me.AuraTimeRemaining ("Master of Subtlety") < TimeToRegen (50) && Target.AuraTimeRemaining ("Find Weakness", true) < TimeToRegen (50)) {
				Sleep = 50;
				Spell = "Vanish";
				return;
			}
			// actions+=/vanish,if=talent.shadow_focus.enabled&energy>=45&energy<=75&combo_points<4-talent.anticipation.enabled&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.down
			// actions+=/pool_resource,for_next=1,extra_amount=90
			if (Usable ("Shadowmeld", TimeToRegen (90)) && HasSpell ("Subterfuge") && ((!HasSpell ("Anticipation") && ComboPoints < 4) || (HasSpell ("Anticipation") && ComboPoints < 3)) && Me.AuraTimeRemaining ("Shadow Dance") < TimeToRegen (90) && Me.AuraTimeRemaining ("Master of Subtlety") < TimeToRegen (90) && Target.AuraTimeRemaining ("Find Weakness", true) < TimeToRegen (90)) {
				Sleep = 90;
				Spell = "Shadowmeld";
				return;
			}
			// actions+=/shadowmeld,if=talent.subterfuge.enabled&energy>=90&combo_points<4-talent.anticipation.enabled&buff.stealth.down&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.down
			// actions+=/pool_resource,for_next=1,extra_amount=90
			if (Usable ("Vanish", TimeToRegen (90)) && (IsPlayer () || InBg || InArena || InInstance || InRaid) && HasSpell ("Subterfuge") && ((!HasSpell ("Anticipation") && ComboPoints < 4) || (HasSpell ("Anticipation") && ComboPoints < 3)) && Me.AuraTimeRemaining ("Shadow Dance") < TimeToRegen (90) && Me.AuraTimeRemaining ("Master of Subtlety") < TimeToRegen (90) && Target.AuraTimeRemaining ("Find Weakness", true) < TimeToRegen (90)) {
				Sleep = 90;
				Spell = "Vanish";
				return;
			}
			// actions+=/vanish,if=talent.subterfuge.enabled&energy>=90&combo_points<4-talent.anticipation.enabled&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.down
			// actions+=/marked_for_death,if=combo_points=0
			if (ComboPoints == 0)
				MarkedforDeath ();

			if (!Me.HasAura ("Slice and Dice") && ActiveEnemies (10) > 1) {
				if (SliceandDice ())
					return;
			}

			// actions+=/run_action_list,name=finisher,if=combo_points=5&(buff.vanish.down|!talent.shadow_focus.enabled)
			if (ComboPoints == 5 && (!Me.HasAura ("Vanish") || !HasSpell ("Shadow Focus"))) {
				if (ActionFinishers ())
					return;
			}
			// actions+=/run_action_list,name=generator,if=combo_points<4|(combo_points=4&cooldown.honor_among_thieves.remains>1&energy>95-25*talent.anticipation.enabled-energy.regen)|(talent.anticipation.enabled&anticipation_charges<3&debuff.find_weakness.down)
			if ((!HasSpell ("Honor Among Thieves") && ComboPoints < 5) || ComboPoints < 4 || (ComboPoints == 4 && Cooldown ("Honor Among Thieves") > 1 && (!HasSpell ("Anticipation") && Energy > 95 - EnergyRegen) || (HasSpell ("Anticipation") && Energy > 70 - EnergyRegen)) || (HasSpell ("Anticipation") && Anticipation < 3 && !Target.HasAura ("Find Weakness", true))) {
				if (ActionGenerators ())
					return;
			}
			// actions+=/run_action_list,name=pool
			if (ActionPool ())
				return;

			// run to enemy
			if (Target.CombatRange > 10 && Me.IsMoving && Run) {
				if (RunToEnemy ())
					return;
			}

			if (Me.IsMoving && Target.CombatRange > 6) {
				if (BurstofSpeed ())
					return;
			}

			Cast (RangedAttack, () => Energy >= 40 && !Me.IsMoving && !HasAura ("Stealth") && Target.IsInLoS && Target.CombatRange > 10 && Target.CombatRange <= 30 && UseRangedAttack);
		}

		public bool ActionGenerators ()
		{
			// actions.generator=run_action_list,name=pool,if=buff.master_of_subtlety.down&buff.shadow_dance.down&debuff.find_weakness.down&(energy+set_bonus.tier17_2pc*50+cooldown.shadow_dance.remains*energy.regen<=energy.max|energy+15+cooldown.vanish.remains*energy.regen<=energy.max)
			if (!Me.HasAura ("Master of Subtlety") && !Me.HasAura ("Shadow Dance") && !Target.HasAura ("Find Weakness", true) && (((HasSpell (165482) && Energy + 50 + Cooldown ("Shadow Dance") * EnergyRegen <= EnergyMax) || (!HasSpell (165482) && Energy + Cooldown ("Shadow Dance") * EnergyRegen <= EnergyMax)) || Energy + 15 + Cooldown ("Vanish") * EnergyRegen <= EnergyMax)) {
				if (ActionPool ())
					return true;
			}
			// actions.generator+=/pool_resource,for_next=1
			if (TimeToRegen (AmbushCost) > 0 && (Me.HasAura ("Stealth") || Me.AuraTimeRemaining ("Vanish") > TimeToRegen (AmbushCost) || Me.AuraTimeRemaining ("Shadow Dance") > TimeToRegen (AmbushCost) || Me.AuraTimeRemaining ("Subterfuge") > TimeToRegen (AmbushCost))) {
				Sleep = AmbushCost;
				Spell = "Ambush";
				return true;
			}
			// actions.generator+=/ambush
			// # If simulating AoE, it is recommended to use Anticipation as the level 90 talent.
			// actions.generator+=/fan_of_knives,if=active_enemies>1
			if (ActiveEnemies (10) > 1 && Aoe && !IncapacitatedInRange (10)) {
				if (FanofKnives ())
					return true;
			}
			// actions.generator+=/backstab,if=debuff.find_weakness.up|buff.archmages_greater_incandescence_agi.up|trinket.stat.any.up
			if (Target.HasAura ("Find Weakness", true) || Target.HasAura ("Archmage's Greater Incandescence")) {
				if (Backstab ())
					return true;
			}
			// actions.generator+=/hemorrhage,if=(remains<duration*0.3&target.time_to_die>=remains+duration+8&debuff.find_weakness.down)|!ticking|position_front
			if ((Target.HasAura ("Hemorrhage", true) && Target.AuraTimeRemaining ("Hemorrhage", true) < 7.2 && !Target.HasAura ("Find Weakness", true)) || !Target.HasAura ("Hemorrhage", true) || !Me.IsNotInFront (Target)) {
				if (Hemorrhage ())
					return true;
			}
			// actions.generator+=/shuriken_toss,if=energy<65&energy.regen<16
			if (Energy < 65 && EnergyRegen < 16) {
				if (ShurikenToss ())
					return true;
			}
			// actions.generator+=/backstab
			if (Backstab ())
				return true;
			// actions.generator+=/run_action_list,name=pool
			if (ActionPool ())
				return true;

			return false;
		}

		public bool ActionFinishers ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (((!InRaid && !InInstance) || (Target.Target == Me && !InRaid && !IsBoss (Target))) && Target.CanParticipateInCombat && !Target.HasAura ("Find Weakness", true)) {
				if (KidneyShot ())
					return true;
			}

			// actions.finisher=rupture,cycle_targets=1,if=(!ticking|remains<duration*0.3|(buff.shadow_reflection.remains>8&dot.rupture.remains<12))&target.time_to_die>=8
			if (Multitarget && Aoe) {
				Unit = targets.Where (x => x.IsInCombatRangeAndLoS && !IsNotForDamage (x) && (!x.HasAura ("Rupture", true) || (x.AuraTimeRemaining ("Rupture", true) < 7.2 || (Me.AuraTimeRemaining ("Shadow Reflection") > 8 && x.AuraTimeRemaining ("Rupture", true) < 12)))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (Rupture (Unit))
						return true;
				}
			} else if (!Target.HasAura ("Rupture", true) || (Target.AuraTimeRemaining ("Rupture", true) < 7.2 || (Me.AuraTimeRemaining ("Shadow Reflection") > 8 && Target.AuraTimeRemaining ("Rupture", true) < 12))) {
				if (Rupture (Target))
					return true;
			}
			// actions.finisher+=/slice_and_dice,if=((buff.slice_and_dice.remains<10.8&debuff.find_weakness.down)|buff.slice_and_dice.remains<6)&buff.slice_and_dice.remains<target.time_to_die
			if ((Me.AuraTimeRemaining ("Slice and Dice") < 10.8 && !Target.HasAura ("Find Weakness", true)) || Me.AuraTimeRemaining ("Slice and Dice") < 6) {
				if (SliceandDice ())
					return true;
			}
			// actions.finisher+=/death_from_above
			if (!IncapacitatedInRange (8)) {
				if (DeathfromAbove ())
					return true;
			}
			// actions.finisher+=/crimson_tempest,if=(active_enemies>=2&debuff.find_weakness.down)|active_enemies>=3&(cooldown.death_from_above.remains>0|!talent.death_from_above.enabled)
			if (Aoe && ((ActiveEnemies (10) >= 2 && !Target.HasAura ("Find Weakness")) || ActiveEnemies (10) >= 3 && (Cooldown ("Death from Above") > 0 || !HasSpell ("Death from Above"))) && !IncapacitatedInRange (10)) {
				if (CrimsonTempest ())
					return true;
			}
			// actions.finisher+=/eviscerate,if=(energy.time_to_max<=cooldown.death_from_above.remains+action.death_from_above.execute_time)|!talent.death_from_above.enabled
			if ((TimeToRegen (EnergyMax) <= Cooldown ("Death from Above") + 1) || !HasSpell ("Death from Above")) {
				if (Eviscerate ())
					return true;
			}
			// actions.finisher+=/run_action_list,name=pool
			if (ActionPool ())
				return true;

			return false;
		}

		public bool ActionPool ()
		{
			// actions.pool=preparation,if=!buff.vanish.up&cooldown.vanish.remains>60
			if (!Me.HasAura ("Vanish") && Cooldown ("Vanish") > 60)
				return (Preparation ());
			return false;
		}

		public bool RunToEnemy ()
		{
			if (Cast ("Shadowstep", () => HasSpell ("Shadowstep") && !HasAura ("Sprint") && !HasAura ("Burst of Speed")))
				return true;
			return CastSelf ("Sprint", () => !HasAura ("Sprint") && !HasAura ("Burst of Speed"));
		}
	}
}
