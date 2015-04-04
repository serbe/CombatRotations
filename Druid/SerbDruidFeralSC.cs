using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Feral Druid SC", "Serb", WoWClass.Druid, Specialization.DruidFeral, 5, 25)]

	public class SerbDruidFeralSC : SerbDruid
	{
		

		[JsonProperty ("PvP Healing")]
		public bool PvPHealing = true;
		[JsonProperty ("Use moonfire")]
		public bool UseMoonfire;
		[JsonProperty ("Cast Rejuvenation And Cenarion Ward")]
		public int HealingPercent = 80;

		public double Sleep;

		public bool hasCatForm ()
		{
			return (HasAura ("Cat Form") || HasAura ("Claws of Shirvallah"));
		}

		public bool CastCatForm ()
		{
			if (hasCatForm () == false) {
				// Cast("Claws of Shirvallah");
				CastSelf ("Cat Form");
				return true;
			}
			return false;
		}

		public 	SerbDruidFeralSC ()
		{
			GroupBuffs = new[] {
				"Mark of the Wild"
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

			if (InArena && InInstance) {
				CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && x.IsInLoS && x.CombatRange < 40 && x.HealthFraction <= 0.9 && !x.HasAura ("Rejuvenation", true) && !x.HasAura ("Cenarion Ward", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Rejuvenation (CycleTarget))
						return;
				}
				if (Me.HasAura ("Predatory Swiftness")) {
					CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && x.IsInLoS && x.CombatRange < 40 && x.HealthFraction <= 0.9 && x.HealthFraction < Health && !x.HasAura ("Cenarion Ward", true)).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (HealingTouch (CycleTarget))
							return;
					}
				}				
			}

			if (Health <= 0.9 && !Me.HasAura ("Rejuvenation", true) && !Me.HasAura ("Cenarion Ward", true)) {
				if (Rejuvenation ())
					return;
			}
			if (Health <= 0.8) {
				if (CenarionWard ())
					return;
			}
			if (Me.HasAura ("Predatory Swiftness") && Health < 0.8 && !Me.HasAura ("Cenarion Ward", true)) {
				if (HealingTouch ())
					return;
			}
			if (Health < 0.45) {
				if (Healthstone ())
					return;
			}

			//Try and prevent Rogues and Priests from going invisible
			if (IsPlayer && (Target.Class == WoWClass.Rogue || Target.Class == WoWClass.Priest) && !Target.HasAura ("Faerie Swarm")) {
				if (FaerieSwarm (Target))
					return;
			}

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
			if (Me.HasAura ("Claws of Shirvallah") || Me.HasAura ("Cat Form")) {
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
				if (Me.HasAura ("Tiger's Fury") && (Me.HasAura ("Incarnation: King of the Jungle") || !HasSpell ("Incarnation: King of the Jungle"))) {
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
				//	actions+=/arcane_torrent,sync=tigers_fury
				//	actions+=/tigers_fury,if=(!buff.omen_of_clarity.react&energy.max-energy>=60)|energy.max-energy>=80
				//	actions+=/incarnation,if=cooldown.berserk.remains<10&energy.time_to_max>1
				//	actions+=/shadowmeld,if=dot.rake.remains<4.5&energy>=35&dot.rake.pmultiplier<2&(buff.bloodtalons.up|!talent.bloodtalons.enabled)&(!talent.incarnation.enabled|cooldown.incarnation.remains>15)&!buff.king_of_the_jungle.up
				//	# Keep Rip from falling off during execute range.
				//	actions+=/ferocious_bite,cycle_targets=1,if=dot.rip.ticking&dot.rip.remains<3&target.health.pct<25
				//	actions+=/healing_touch,if=talent.bloodtalons.enabled&buff.predatory_swiftness.up&(combo_points>=4|buff.predatory_swiftness.remains<1.5)
				//	actions+=/savage_roar,if=buff.savage_roar.down
				//	actions+=/pool_resource,for_next=1
				//	actions+=/thrash_cat,cycle_targets=1,if=remains<4.5&(active_enemies>=2&set_bonus.tier17_2pc|active_enemies>=4)
				//	actions+=/call_action_list,name=finisher,if=combo_points=5
				//	actions+=/savage_roar,if=buff.savage_roar.remains<gcd
				//	actions+=/call_action_list,name=maintain,if=combo_points<5
				//	actions+=/pool_resource,for_next=1
				//	actions+=/thrash_cat,cycle_targets=1,if=remains<4.5&active_enemies>=2
				//	actions+=/call_action_list,name=generator,if=combo_points<5


				// actions+=/use_item,slot=trinket2,if=(prev.tigers_fury&(target.time_to_die>trinket.stat.any.cooldown|target.time_to_die<45))|prev.berserk|(buff.king_of_the_jungle.up&time<10)
				// actions+=/potion,name=draenic_agility,if=(buff.berserk.remains>10&(target.time_to_die<180|(trinket.proc.all.react&target.health.pct<25)))|target.time_to_die<=40
				// actions+=/blood_fury,sync=tigers_fury
				// actions+=/berserking,sync=tigers_fury
				// if (CastSelf("Berserking", () => Me.HasAura("Tiger's Fury") && (IsPlayer || IsElite))) return; // id 26297 Troll Racial
				// actions+=/arcane_torrent,sync=tigers_fury
				if (CastSelf ("Arcane Torrent", () => Me.HasAura ("Tiger's Fury") && (IsPlayer || IsElite)))
					return;
				// actions+=/tigers_fury,if=(!buff.omen_of_clarity.react&energy.max-energy>=60)|energy.max-energy>=80
				if (CastSelf ("Tiger's Fury", () => (!Me.HasAura ("Clearcasting") && EnergyMax - Energy >= 60) || EnergyMax - Energy >= 80))
					return;
				// actions+=/incarnation,if=cooldown.berserk.remains<10&energy.time_to_max>1
				if (CastSelf ("Incarnation: King of the Jungle", () => HasSpell ("Incarnation: King of the Jungle") && SpellCooldown ("Berserk") < 10 && EnergyTimeToMax > 1))
					return;
				// actions+=/shadowmeld,if=dot.rake.remains<4.5&energy>=35&dot.rake.pmultiplier<2&(buff.bloodtalons.up|!talent.bloodtalons.enabled)&(!talent.incarnation.enabled|cooldown.incarnation.remains>15)&!buff.king_of_the_jungle.up
				if (CastSelf ("Shadowmeld", () => HasSpell ("Shadowmeld") && Target.AuraTimeRemaining ("Rake", true) < 4.5 && Energy >= 35 && (Me.HasAura ("Bloodtalons") || !HasSpell ("Bloodtalons") && (!HasSpell ("Incarnation: King of the Jungle") || SpellCooldown ("Incarnation: King of the Jungle") > 15)) && !Me.HasAura ("King of the Jungle")))
					return;
				// # Keep Rip from falling off during execute range.
				// actions+=/ferocious_bite,cycle_targets=1,if=dot.rip.ticking&dot.rip.remains<3&target.health.pct<25
				// if ((Energy >= 25 || Me.HasAura("Clearcasting")) && ComboPoints > 0) {
				CycleTarget = targets.Where (target => target.IsInCombatRangeAndLoS && target.AuraTimeRemaining ("Rip", true) < 3 && target.HealthFraction < 0.25).OrderBy (target => target.HealthFraction).DefaultIfEmpty (null).FirstOrDefault ();
				if (Cast ("Ferocious Bite", CycleTarget, () => CycleTarget != null))
					return; 
				// }
				// actions+=/healing_touch,if=talent.bloodtalons.enabled&buff.predatory_swiftness.up&(combo_points>=4|buff.predatory_swiftness.remains<1.5)
				if (CastSelf ("Healing Touch", () => HasSpell ("Bloodtalons") && Me.HasAura ("Predatory Swiftness") && (ComboPoints >= 4 || Me.AuraTimeRemaining ("Predatory Swiftness") < 1.5)))
					return;
				// actions+=/savage_roar,if=buff.savage_roar.down
				if (CastSelf ("Savage Roar", () => Energy >= 25 && ComboPoints > 0 && !Me.HasAura ("Savage Roar")))
					return;
				// actions+=/pool_resource,for_next=1
				//			if (Energy < 50 && !Me.HasAura ("Clearcasting") && Target.AuraTimeRemaining ("Thrash", true) + TimeToRegen (50) < 4.5 && Enemy (8) >= 4) {
				//				Sleep = 50;
				//				ToSkill = 1;
				//				return;
				//			}
				// actions+=/thrash_cat,cycle_targets=1,if=remains<4.5&(active_enemies>=2&set_bonus.tier17_2pc|active_enemies>=4)
				// if (Cast("Thrash", () => (Energy >= 50 || Me.HasAura("Clearcasting")) && Target.AuraTimeRemaining("Thrash", true) < 4.5 && Enemy(8) >= 4)) return;
				if (Cast ("Thrash", () => Target.AuraTimeRemaining ("Thrash", true) < 4.5 && EnemyInRange (8) >= 4))
					return;
				// actions+=/call_action_list,name=finisher,if=combo_points=5
				if (ComboPoints == 5)
					Finishers ();
				// actions+=/savage_roar,if=buff.savage_roar.remains<gcd
				// if (CastSelf("Savage Roar", () => Energy >= 25 && ComboPoints > 0 && Me.AuraTimeRemaining("Savage Roar") < 1)) return;
				if (CastSelf ("Savage Roar", () => ComboPoints > 0 && Me.AuraTimeRemaining ("Savage Roar") < 1))
					return;
				// actions+=/call_action_list,name=maintain,if=combo_points<5
				if (ComboPoints < 5)
					Maintains ();
				// actions+=/pool_resource,for_next=1
				//			if (Energy < 50 && !Me.HasAura ("Clearcasting") && Target.AuraTimeRemaining ("Thrash", true) + TimeToRegen (50) < 4.5 && Enemy (8) >= 2) {
				//				Sleep = 50;
				//				ToSkill = 2;
				//				return;
				//			}
				// actions+=/thrash_cat,cycle_targets=1,if=remains<4.5&active_enemies>=2
				// if (Cast("Thrash", () => (Energy >= 50 || Me.HasAura("Clearcasting")) && Target.AuraTimeRemaining("Thrash", true) < 4.5 && Enemy(8) >= 2)) return;
				if (Cast ("Thrash", () => Target.AuraTimeRemaining ("Thrash", true) < 4.5 && EnemyInRange (8) >= 2))
					return;
				// actions+=/call_action_list,name=generator,if=combo_points<5
				if (ComboPoints < 5)
					Generators ();

				// if (Cast("Moonfire", () => Moonfire && TargetHealth <= 0.1 && Target.IsElite())) return;
			}
		}

		public void Finishers ()
		{
			var targets = Adds;
			targets.Add (Target);

			//	actions.finisher=ferocious_bite,cycle_targets=1,max_energy=1,if=target.health.pct<25&dot.rip.ticking
			//	actions.finisher+=/rip,cycle_targets=1,if=remains<7.2&persistent_multiplier>dot.rip.pmultiplier&target.time_to_die-remains>18
			//	actions.finisher+=/rip,cycle_targets=1,if=remains<7.2&persistent_multiplier=dot.rip.pmultiplier&(energy.time_to_max<=1|!talent.bloodtalons.enabled)&target.time_to_die-remains>18
			//	actions.finisher+=/rip,cycle_targets=1,if=remains<2&target.time_to_die-remains>18
			//	actions.finisher+=/savage_roar,if=(energy.time_to_max<=1|buff.berserk.up|cooldown.tigers_fury.remains<3)&buff.savage_roar.remains<12.6
			//	actions.finisher+=/ferocious_bite,max_energy=1,if=(energy.time_to_max<=1|buff.berserk.up|cooldown.tigers_fury.remains<3)



			// actions.finisher=ferocious_bite,cycle_targets=1,max_energy=1,if=target.health.pct<25&dot.rip.ticking
			// if ((Energy >= 50 || Me.HasAura("Clearcasting"))) {
			CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.25 && u.HasAura ("Rip", true)).DefaultIfEmpty (null).FirstOrDefault ();
			if (Cast ("Ferocious Bite", CycleTarget, () => CycleTarget != null))
				return; 
			// }
			// actions.finisher+=/rip,cycle_targets=1,if=remains<7.2&persistent_multiplier>dot.rip.pmultiplier&target.time_to_die-remains>18
			// if (Energy >= 30) {
			// 	CycleTargets = targets.Where(target => target.IsInCombatRangeAndLoS && target.CombatRange <= 6 && target.AuraTimeRemaining("Rip", true) < 7.2 && TimeToDie(target) - target.AuraTimeRemaining("Rip", true) > 18).OrderBy(target => target.HealthFraction).DefaultIfEmpty(null).FirstOrDefault();
			// 	if (Cast("Rip", CycleTargets, () => CycleTargets != null)) return;
			// }
			// actions.finisher+=/rip,cycle_targets=1,if=remains<2&target.time_to_die-remains>18
			CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rip", true) < 2).DefaultIfEmpty (null).FirstOrDefault ();
			if (Cast ("Rip", CycleTarget, () => CycleTarget != null))
				return;
			// actions.finisher+=/savage_roar,if=(energy.time_to_max<=1|buff.berserk.up|cooldown.tigers_fury.remains<3)&buff.savage_roar.remains<12.6
			if (CastSelf ("Savage Roar", () => (EnergyTimeToMax <= 1 || Me.HasAura ("Berserk") || SpellCooldown ("Tiger's Fury") < 3) && Me.AuraTimeRemaining ("Savage Roar") < 12.6))
				return;
			// actions.finisher+=/ferocious_bite,max_energy=1,if=(energy.time_to_max<=1|buff.berserk.up|cooldown.tigers_fury.remains<3)
			// if (Cast("Ferocious Bite", () => (Energy >= 50 || Me.HasAura("Clearcasting")) && (EnergyTimeToMax <= 1 || Me.HasAura("Berserk") || Me.AuraTimeRemaining("Tiger's Fury") < 3))) return;
			if (Cast ("Ferocious Bite", () => (EnergyTimeToMax <= 1 || Me.HasAura ("Berserk") || SpellCooldown ("Tiger's Fury") < 3)))
				return;
		}

		public void Maintains ()
		{
			var targets = Adds;
			targets.Add (Target);

			// actions.maintain=rake,cycle_targets=1,if=remains<3&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
			// actions.maintain+=/rake,cycle_targets=1,if=remains<4.5&(persistent_multiplier>=dot.rake.pmultiplier|(talent.bloodtalons.enabled&(buff.bloodtalons.up|!buff.predatory_swiftness.up)))&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
			// actions.maintain+=/moonfire_cat,cycle_targets=1,if=remains<4.2&active_enemies<=5&target.time_to_die-remains>tick_time*5
			// actions.maintain+=/rake,cycle_targets=1,if=persistent_multiplier>dot.rake.pmultiplier&active_enemies=1&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)


			// actions.maintain=rake,cycle_targets=1,if=remains<3&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
			// if (Energy >= 35 || Me.HasAura("Clearcasting")) {
			CycleTargets = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rake", true) < 3).DefaultIfEmpty (null).FirstOrDefault ();
			if (Cast ("Rake", CycleTargets, () => CycleTargets != null))
				return;
			// }
			// actions.maintain+=/rake,cycle_targets=1,if=remains<4.5&(persistent_multiplier>=dot.rake.pmultiplier|(talent.bloodtalons.enabled&(buff.bloodtalons.up|!buff.predatory_swiftness.up)))&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
			// if (Energy >= 35 || Me.HasAura("Clearcasting") ) {
			CycleTargets = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Rake", true) < 4.5 && (HasSpell ("Bloodtalons") && (Me.HasAura ("Bloodtalons") || !Me.HasAura ("Predatory Swiftness")))).DefaultIfEmpty (null).FirstOrDefault ();
			if (Cast ("Rake", CycleTargets, () => CycleTargets != null))
				return;
			// }
			// actions.maintain+=/moonfire_cat,cycle_targets=1,if=remains<4.2&active_enemies<=5&target.time_to_die-remains>tick_time*5
			if (UseMoonfire && EnemyInRange (40) <= 5) {
				CycleTargets = targets.Where (u => u.IsInLoS && u.CombatRange <= 40 && u.AuraTimeRemaining ("Moonfire", true) < 4.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Cast ("Moonfire", CycleTargets, () => CycleTargets != null))
					return;
			}
			// actions.maintain+=/rake,cycle_targets=1,if=persistent_multiplier>dot.rake.pmultiplier&active_enemies=1&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
			// actions.maintain+=/rake,cycle_targets=1,if=persistent_multiplier>dot.rake.pmultiplier&active_enemies=1&((target.time_to_die-remains>3&active_enemies<3)|target.time_to_die-remains>6)
		}

		public void Generators ()
		{
			//	actions.generator=swipe,if=active_enemies>=3
			//	actions.generator+=/shred,if=active_enemies<3



			// actions.generator=swipe,if=active_enemies>=3
			// if (Cast("Swipe", () => (Energy >= 45 || Me.HasAura("Clearcasting")) && Enemy(8) >= 3)) return;
			if (Cast ("Swipe", () => EnemyInRange (8) >= 3))
				return;
			// actions.generator+=/shred,if=active_enemies<3
			// if (Cast("Shred", () => (Energy >= 40 || Me.HasAura("Clearcasting")) && Enemy(8) < 3)) return;
			if (Cast ("Shred", () => EnemyInRange (8) < 3))
				return;
		}

		public void RunToEnemy ()
		{
			// // if (CastSelfPreventDouble("Stealth", () => !Me.InCombat && !HasAura("Stealth"))) return;
			// if (Cast("Shadowstep", () => !HasAura("Sprint") && HasSpell("Shadowstep"))) return;
			// // if (CastSelf("Sprint", () => !HasAura("Sprint") && !HasAura("Burst of Speed"))) return;
			// // if (CastSelf("Burst of Speed", () => !HasAura("Sprint") && !HasAura("Burst of Speed") && HasSpell("Burst of Speed") && Energy > 20)) return;
			// if (Cast(RangedAtk, () => Energy >= 40 && !HasAura("Stealth") && Target.IsInLoS)) return;
		}
	}
}
