using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot.Druid
{
	[Rotation ("Serb Feral Druid SC", "Serb", WoWClass.Druid, Specialization.DruidFeral, 5, 25)]

	public class SerbDruidFeralSc : SerbDruid
	{

		[JsonProperty ("Party Healing")]
		public bool PartyHealing = true;
		[JsonProperty ("Use moonfire")]
		public bool UseMoonfire;


		public double Sleep;
		public string Skill;

		public 	SerbDruidFeralSc ()
		{
			GroupBuffs = new[] {
				"Mark of the Wild"
			};
			PullSpells = new[] {
				"Rake",
				"Shred",
				"Faerie Swarm",
				"Faerie Fire",
				"Moonfire"
			};
		}


		public override bool OutOfCombat ()
		{
			//actions.precombat=flask,type=greater_draenic_agility_flask
			//actions.precombat+=/food,type=pickled_eel
			//actions.precombat+=/mark_of_the_wild,if=!aura.str_agi_int.up
			if (MarkoftheWild ())
				return true;
			//actions.precombat+=/healing_touch,if=talent.bloodtalons.enabled
			//actions.precombat+=/cat_form
			//actions.precombat+=/prowl
			//# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//actions.precombat+=/snapshot_stats
			//actions.precombat+=/potion,name=draenic_agility


			// Heal
			if (Health <= 0.75 && !Me.HasAura ("Rejuvenation")) {
				if (Rejuvenation ())
					return true;
			}
			if (Health <= 0.5 && !Me.IsMoving) {
				if (HealingTouch ())
					return true;
			}
			if (Me.Auras.Any (x => x.IsDebuff && "Curse,Poison".Contains (x.DebuffType))) {
				if (RemoveCorruption ())
					return true;
			}

			// if (CastSelf("Claws of Shirvallah", () => Me.MovementSpeed != 0 && !Me.IsSwimming && Me.DisplayId == Me.NativeDisplayId && Me.DistanceTo(API.GetNaviTarget()) > 20)) return true;

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (InCombat) {
				InCombat = false;
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

			var targets = Adds;
			targets.Add (Target);

			if (Health < 0.9) {
				if (Heal ())
					return;
			}

			if (PartyHealing && !IsSolo) {
				if (HealPartyMember ())
					return;
			}

			//Try and prevent Rogues and Priests from going invisible
			if (IsPlayer && !Me.HasAura ("Prowl"))
				NoInvisible ();

			// Стойка медведя когда хп меньше 30%
			// 	if (CastSelf("Bear Form", () => !HasAura("Bear Form") && Health < 0.30)) return;

			// // actions=cat_form
			// Interrupts
			if (Interrupt ())
				return;

			//	actions=cat_form
			if (!Me.HasAura ("Flight Form") && !Me.HasAura ("Bear Form")) {
				if (CatForm ())
					return;
			}

			if (HasGlobalCooldown () && Gcd)
				return;
			
			if (IsCatForm ()) {
				//	actions+=/wild_charge
				//	actions+=/displacer_beast,if=movement.distance>10
				//	actions+=/dash,if=movement.distance&buff.displacer_beast.down&buff.wild_charge_movement.down
				//	actions+=/rake,if=buff.prowl.up|buff.shadowmeld.up
				if (Me.HasAura ("Prowl") || Me.HasAura ("Shadowmeld")) {
					if (Rake ())
						return;
				}
				//	actions+=/auto_attack
				//	actions+=/skull_bash
				//	actions+=/force_of_nature,if=charges=3|trinket.proc.all.react|target.time_to_die<20
				if (SpellCharges ("Force of Nature") == 3 || TimeToDie (Target) < 20) {
					if (ForceofNature ())
						return;
				}
				//	actions+=/berserk,sync=tigers_fury,if=buff.king_of_the_jungle.up|!talent.incarnation.enabled
				if (Me.HasAura ("Tiger's Fury") && (!HasSpell ("Incarnation: King of the Jungle") || Me.HasAura ("Incarnation: King of the Jungle"))) {
					if (Berserk ())
						return;
				}
				//	actions+=/use_item,slot=trinket1,if=(prev.tigers_fury&(target.time_to_die>trinket.stat.any.cooldown|target.time_to_die<45))|prev.berserk|(buff.king_of_the_jungle.up&time<10)
				//	actions+=/potion,name=draenic_agility,if=(buff.berserk.remains>10&(target.time_to_die<180|(trinket.proc.all.react&target.health.pct<25)))|target.time_to_die<=40
				//	actions+=/blood_fury,sync=tigers_fury
				if (Me.HasAura ("Tiger's Fury")) {
					if (BloodFury ())
						return;
				}
				//	actions+=/berserking,sync=tigers_fury
				if (Me.HasAura ("Tiger's Fury")) {
					if (Berserking ())
						return; 
				}
				//	actions+=/arcane_torrent,sync=tigers_fury
				if (Me.HasAura ("Tiger's Fury")) {
					if (ArcaneTorrent ())
						return;
				}
				//	actions+=/tigers_fury,if=(!buff.omen_of_clarity.react&energy.max-energy>=60)|energy.max-energy>=80
				if ((!Me.HasAura ("Clearcasting") && EnergyMax - Energy >= 60) || EnergyMax - Energy >= 80)
					TigersFury ();
				//	actions+=/incarnation,if=cooldown.berserk.remains<10&energy.time_to_max>1
				if (Cooldown ("Berserk") < 10 && EnergyTimeToMax > 1) {
					if (IncarnationKingoftheJungle ())
						return;
				}
				//	actions+=/shadowmeld,if=dot.rake.remains<4.5&energy>=35&dot.rake.pmultiplier<2&(buff.bloodtalons.up|!talent.bloodtalons.enabled)&(!talent.incarnation.enabled|cooldown.incarnation.remains>15)&!buff.king_of_the_jungle.up
				if (Target.AuraTimeRemaining ("Rake", true) < 4.5 && Energy >= 35 && (Me.HasAura ("Bloodtalons") || !HasSpell ("Bloodtalons")) && (!HasSpell ("Incarnation: King of the Jungle") || Cooldown ("Incarnation: King of the Jungle") > 15) && !Me.HasAura ("Incarnation: King of the Jungle")) {
					if (Shadowmeld ())
						return;
				}
				//	# Keep Rip from falling off during execute range.
				//	actions+=/ferocious_bite,cycle_targets=1,if=dot.rip.ticking&dot.rip.remains<3&target.health.pct<25
				if (Usable ("Ferocious Bite") && ComboPoints > 0) {
					CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.HasAura ("Rip", true) && x.AuraTimeRemaining ("Rip", true) < 3 && x.HealthFraction < 0.25).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (FerociousBite (CycleTarget))
							return;
					}
				}
				//	actions+=/healing_touch,if=talent.bloodtalons.enabled&buff.predatory_swiftness.up&(combo_points>=4|buff.predatory_swiftness.remains<1.5)
				if (HasSpell ("Bloodtalons") && Me.HasAura ("Predatory Swiftness") && (ComboPoints >= 4 || Me.AuraTimeRemaining ("Predatory Swiftness") < 1.5)) {
					if (HealingTouch ())
						return;
				}
				//	actions+=/savage_roar,if=buff.savage_roar.down
				if (!Me.HasAura ("Savage Roar")) {
					if (SavageRoar ())
						return;
				}
				//	actions+=/pool_resource,for_next=1
				if (Usable ("Thrash") && Energy < 50 && !Me.HasAura ("Clearcasting") && Target.AuraTimeRemaining ("Thrash", true) + TimeToRegen (50) < 4.5 && EnemyInRange (8) >= 4) {
					Sleep = 50;
					Skill = "Thrash";
					return;
				}
				//	actions+=/thrash_cat,cycle_targets=1,if=remains<4.5&(active_enemies>=2&set_bonus.tier17_2pc|active_enemies>=4)
				if (Usable ("Thrash") && EnemyInRange (8) >= 4) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 8 && x.AuraTimeRemaining ("Thrash", true) < 4.5).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Thrash (CycleTarget))
							return;
					}
				}
				//	actions+=/call_action_list,name=finisher,if=combo_points=5
				if (ComboPoints == 5) {
					if (Finishers ())
						return;
				}
				//	actions+=/savage_roar,if=buff.savage_roar.remains<gcd
				if (Me.AuraTimeRemaining ("Savage Roar") < 1) {
					if (SavageRoar ())
						return;
				}
				//	actions+=/call_action_list,name=maintain,if=combo_points<5
				if (ComboPoints < 5) {
					if (Maintains ())
						return;
				}
				//	actions+=/pool_resource,for_next=1
				if (Usable ("Thrash") && Energy < 50 && !Me.HasAura ("Clearcasting") && Target.AuraTimeRemaining ("Thrash", true) + TimeToRegen (50) < 4.5 && EnemyInRange (8) >= 2) {
					Sleep = 50;
					Skill = "Thrash";
					return;
				}
				//	actions+=/thrash_cat,cycle_targets=1,if=remains<4.5&active_enemies>=2
				if (Usable ("Thrash") && EnemyInRange (8) >= 2) {
					CycleTarget = targets.Where (x => x.IsInLoS && x.CombatRange <= 8 && x.AuraTimeRemaining ("Thrash", true) < 4.5).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Thrash (CycleTarget))
							return;
					}
				}
				//	actions+=/call_action_list,name=generator,if=combo_points<5
				if (ComboPoints < 5) {
					if (Generators ())
						return;
				}

				if (Target.CombatRange > 8) {
					if (RunToTarget ())
						return;
				}
			}
		}

		public bool Finishers ()
		{
			var targets = Adds;
			targets.Add (Target);

			//	actions.finisher=ferocious_bite,cycle_targets=1,max_energy=1,if=target.health.pct<25&dot.rip.ticking
			if (Usable ("Ferocious Bite") && Energy >= 50) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.25 && u.HasAura ("Rip", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (FerociousBite (CycleTarget))
						return true;
				}
			}
			//	actions.finisher+=/rip,cycle_targets=1,if=remains<7.2&persistent_multiplier>dot.rip.pmultiplier&target.time_to_die-remains>18
			if (Usable ("Rip") && HasEnergy (30)) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rip", true) < 7.2 && TimeToDie (u) - u.AuraTimeRemaining ("Rip", true) > 18).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rip (CycleTarget))
						return true;
				}
			}
			//	actions.finisher+=/rip,cycle_targets=1,if=remains<7.2&persistent_multiplier=dot.rip.pmultiplier&(energy.time_to_max<=1|!talent.bloodtalons.enabled)&target.time_to_die-remains>18
			if (Usable ("Rip") && HasEnergy (30)) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rip", true) < 7.2 && (EnergyTimeToMax <= 1 || !HasSpell ("Bloodtalons")) && TimeToDie (u) - u.AuraTimeRemaining ("Rip", true) > 18).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rip (CycleTarget))
						return true;
				}
			}
			//	actions.finisher+=/rip,cycle_targets=1,if=remains<2&target.time_to_die-remains>18
			if (Usable ("Rip") && HasEnergy (30)) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rip", true) < 2 && TimeToDie (u) - u.AuraTimeRemaining ("Rip", true) > 18).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rip (CycleTarget))
						return true;
				}
			}
			//	actions.finisher+=/savage_roar,if=(energy.time_to_max<=1|buff.berserk.up|cooldown.tigers_fury.remains<3)&buff.savage_roar.remains<12.6
			if (Usable ("SavageRoar") && (EnergyTimeToMax <= 1 || Me.HasAura ("Berserk") || Cooldown ("Tiger's Fury") < 3) && Me.AuraTimeRemaining ("Savage Roar") < 12.6) {
				if (SavageRoar ())
					return true;
			}
			//	actions.finisher+=/ferocious_bite,max_energy=1,if=(energy.time_to_max<=1|buff.berserk.up|cooldown.tigers_fury.remains<3)
			if (Usable ("Ferocious Bite") && Energy >= 50 && (EnergyTimeToMax <= 1 || Me.HasAura ("Berserk") || Cooldown ("Tiger's Fury") < 3)) {
				if (FerociousBite ())
					return true;
			}

			return false;
		}

		public bool Maintains ()
		{
			var targets = Adds;
			targets.Add (Target);

			// actions.maintain=rake,cycle_targets=1,if=remains<3&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
			if (Usable ("Rake") && HasEnergy (35)) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rake", true) < 3 && ((TimeToDie (u) - u.AuraTimeRemaining ("Rake", true) > 3 && EnemyInRange (5) < 3) || TimeToDie (u) > 6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rake (CycleTarget))
						return true;
				}
			}
			// actions.maintain+=/rake,cycle_targets=1,if=remains<4.5&(persistent_multiplier>=dot.rake.pmultiplier|(talent.bloodtalons.enabled&(buff.bloodtalons.up|!buff.predatory_swiftness.up)))&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
			if (Usable ("Rake") && HasEnergy (35)) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rake", true) < 4.5 && (HasSpell ("Bloodtalons") && (Me.HasAura ("Bloodtalons") || !Me.HasAura ("Predatory Swiftness"))) && ((TimeToDie (u) - u.AuraTimeRemaining ("Rake", true) > 3 && EnemyInRange (5) < 3) || TimeToDie (u) > 6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rake (CycleTarget))
						return true;
				}
			}
			// actions.maintain+=/moonfire_cat,cycle_targets=1,if=remains<4.2&active_enemies<=5&target.time_to_die-remains>tick_time*5
			if (UseMoonfire && Usable ("Moonfire") && EnemyInRange (40) <= 5) {
				CycleTarget = targets.Where (u => u.IsInLoS && u.CombatRange <= 40 && u.AuraTimeRemaining ("Moonfire", true) < 4.2 && TimeToDie (u) > 20).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Moonfire (CycleTarget))
						return true;
				}
			}
			// actions.maintain+=/rake,cycle_targets=1,if=persistent_multiplier>dot.rake.pmultiplier&active_enemies=1&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)

			return false;
		}

		public bool Generators ()
		{
			//	actions.generator=swipe,if=active_enemies>=3
			if (EnemyInRange (8) >= 3) {
				if (Swipe ())
					return true;
			}
			//	actions.generator+=/shred,if=active_enemies<3
			if (EnemyInRange (8) < 3) {
				if (Shred ())
					return true;
			}

			return false;
		}
	}
}
