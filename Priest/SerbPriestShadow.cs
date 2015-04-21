using ReBot.API;
using System.Linq;
using Geometry;
using Newtonsoft.Json;

namespace ReBot.Priest
{
	[Rotation ("Serb Priest Shadow SC", "ReBot", WoWClass.Priest, Specialization.PriestShadow, 40, 25)]

	public class SerbPriestShadowSc : SerbPriest
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Auto target")]
		public bool UseAutoTarget = true;
		[JsonProperty ("Fight in instance")]
		public bool FightInInstance = true;
		[JsonProperty ("Heal %/100")]
		public double HealPr = 0.8;
		[JsonProperty ("Heal tank %/100")]
		public double TankPr = 0.9;

		public SerbPriestShadowSc ()
		{
			GroupBuffs = new string[] {
				"Power Word: Fortitude"
			};
			PullSpells = new string[] {
				"Shadow Word: Pain",
			};

		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/power_word_fortitude,if=!aura.stamina.up
			if (PowerWordFortitude ())
				return true;
			//	actions.precombat+=/shadowform,if=!buff.shadowform.up
			if (Shadowform ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_intellect
			//	actions.precombat+=/mind_spike

			if (Me.FallingTime > 2) {
				if (Levitate ())
					return true;
			}

//			if (API.GetNaviTarget() != Vector3.Zero && HasSpell("Levitate")) {
//				if (!API.IsNaviTargetInWater()) {
//					if (CastSelf("Levitate", () => Me.Race != WoWRace.Tauren && Me.IsSwimming && !HasAura("Levitate"))) return true;
//				} else if (HasAura("Levitate")) {
//					CancelAura("Levitate");
//				}
//			}
//
//			if (CastSelfPreventDouble("Flash Heal", () => Health <= 0.75 && !Me.IsMoving)) return true;
//			if (CastSelf("Desperate Prayer", () => HasSpell("Desperate Prayer") && Health <= 0.75 && !Me.IsMoving)) return true;
//
//			// if (CastSelf("Fear Ward", () => CurrentBotName == "PvP" && !HasAura("Fear Ward"))) return true;
//			if (CastOnTerrain("Angelic Feather", Me.PositionPredicted, () => Me.MovementSpeed > 0 && !HasAura("Angelic Feather"))) return true;
//
//			if (API.HasItem(CrystalOfInsanity) && !HasAura("Visions of Insanity") && API.ItemCooldown(CrystalOfInsanity) == 0) {
//				API.UseItem(CrystalOfInsanity);
//				return true;
//			}
//
//			if (API.HasItem(OraliusWhisperingCrystal) && !HasAura(OraliusWhisperingCrystalBuff) && API.ItemCooldown(OraliusWhisperingCrystal) == 0) {
//				API.UseItem(OraliusWhisperingCrystal);
//				return true;
//			}

			return false;
		}

		public override void Combat ()
		{
			if (ShadowApparitions > 0) {
				API.Print (ShadowApparitions);
			}

			//	actions=shadowform,if=!buff.shadowform.up
			if (Shadowform ())
				return;
			//	actions+=/potion,name=draenic_intellect,if=buff.bloodlust.react|target.time_to_die<=40
			//	actions+=/power_infusion,if=talent.power_infusion.enabled
			PowerInfusion ();
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/call_action_list,name=pvp_dispersion,if=set_bonus.pvp_2pc
			//	actions+=/call_action_list,name=decision
			Decision ();
		}

		public bool Decision ()
		{
			//	actions.decision=call_action_list,name=main,if=!talent.clarity_of_power.enabled&!talent.void_entropy.enabled
			if (!HasSpell ("Clarity of Power") && !HasSpell ("Void Entropy")) {
				if (Main ())
					return true;
			}
			//	actions.decision+=/call_action_list,name=vent,if=talent.void_entropy.enabled&!talent.clarity_of_power.enabled&!talent.auspicious_spirits.enabled
			if (HasSpell ("Void Entropy") && !HasSpell ("Clarity of Power") && !HasSpell ("Auspicious Spirits")) {
				if (Vent ())
					return true;
			}
			//	actions.decision+=/call_action_list,name=cop,if=talent.clarity_of_power.enabled&!talent.insanity.enabled
			if (HasSpell ("Clarity of Power") && !HasSpell ("Insanity")) {
				if (COP ())
					return true;
			}
			//	actions.decision+=/call_action_list,name=cop_dotweave,if=talent.clarity_of_power.enabled&talent.insanity.enabled&target.health.pct>20&active_enemies<=6
			if (HasSpell ("Clarity of Power") && HasSpell ("Insanity") && Health (Target) > 0.2 && EnemyInRange (40) <= 6) {
				if (COP_DotWeave ())
					return true;
			}
			//	actions.decision+=/call_action_list,name=cop_insanity,if=talent.clarity_of_power.enabled&talent.insanity.enabled
			if (HasSpell ("Clarity of Power") && HasSpell ("Insanity")) {
				if (COP_Insanity ())
					return true;
			}

			return false;
		}

		public bool PVP_Dispersion ()
		{
			//	actions.pvp_dispersion=call_action_list,name=decision,if=cooldown.dispersion.remains>0
			if (Cooldown ("Dispersion") > 0) {
				if (Decision ())
					return true;
			}
			//	actions.pvp_dispersion+=/dispersion,interrupt=1
			//	actions.pvp_dispersion+=/call_action_list,name=decision
			if (Decision ())
				return true;

			return false;
		}

		public bool Main ()
		{
			var targets = Adds;
			targets.Add (Target);

			//	actions.main=mindbender,if=talent.mindbender.enabled
			if (Mindbender ())
				return true;
			//	actions.main+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend ())
					return true;
			} 
			//	actions.main+=/shadow_word_death,if=natural_shadow_word_death_range&shadow_orb<=4,cycle_targets=1
			if (Orb <= 4) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (ShadowWordDeath (CycleTarget))
						return true;
				}
			}
			//	actions.main+=/mind_blast,if=glyph.mind_harvest.enabled&shadow_orb<=2&active_enemies<=5&cooldown_react
			if (HasGlyph (162532) && Orb <= 2 && EnemyInRange (40) <= 5) {
				if (MindBlast ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb=5&!target.dot.devouring_plague_dot.ticking&(talent.surge_of_darkness.enabled|set_bonus.tier17_4pc),cycle_targets=1
			if (Orb == 5 && (HasSpell ("Surge of Darkness") || HasSpell ("Mental Instinct"))) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Devouring Plague", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (DevouringPlague (CycleTarget))
						return true;
				}
			}
			//	actions.main+=/devouring_plague,if=shadow_orb=5
			if (Orb == 5) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=3&talent.auspicious_spirits.enabled&shadowy_apparitions_in_flight>=3
			if (Orb >= 3 && HasSpell ("Auspicious Spirits") && ShadowApparitions >= 3) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=4&talent.auspicious_spirits.enabled&shadowy_apparitions_in_flight>=2
			if (Orb >= 4 && HasSpell ("Auspicious Spirits") && ShadowApparitions >= 2) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=3&buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
			if (Orb >= 3 && Me.AuraTimeRemaining ("Mental Instinct") < 1.5 && Me.AuraTimeRemaining ("Mental Instinct") > (1.5 * 0.7) && Me.HasAura ("Mental Instinct")) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=4&talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd))&!target.dot.devouring_plague_tick.ticking&talent.surge_of_darkness.enabled,cycle_targets=1
			if (Orb >= 4 && HasSpell ("Auspicious Spirits")) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (Health (u) < 0.2 && Cooldown ("Shadow Word: Death") < 1.5)) && !u.HasAura ("Devouring Plague", true) && HasSpell ("Surge of Darkness")).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (DevouringPlague (CycleTarget))
						return true;
				}
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=4&talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(target.health.pct<20&cooldown.shadow_word_death.remains<gcd))
			if (Orb >= 4 && HasSpell ("Auspicious Spirits") && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (Health (Target) < 0.2 && Cooldown ("Shadow Word Death") < 1.5))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=3&!talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd))&!target.dot.devouring_plague_tick.ticking&talent.surge_of_darkness.enabled,cycle_targets=1
			if (Orb >= 3 && !HasSpell ("Auspicious Spirits") && HasSpell ("Surge of Darkness")) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (u.HealthFraction < 0.2 && Cooldown ("Shadow Word: Death") < 1.5)) && !u.HasAura ("Devouring Plague", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (DevouringPlague (CycleTarget))
						return true;
				}
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=3&!talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(target.health.pct<20&cooldown.shadow_word_death.remains<gcd))
			if (Orb >= 3 && !HasSpell ("Auspicious Spirits") && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (Health (Target) < 0.2 && Cooldown ("Shadow Word Death") < 1.5))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0,cycle_targets=1
			if (HasGlyph (162532)) {
				CycleTarget = targets.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Glyph of Mind Blast", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (MindBlast (CycleTarget))
						return true;
				}
			}
			//	actions.main+=/mind_blast,if=talent.auspicious_spirits.enabled&active_enemies<=4&cooldown_react
			if (HasSpell ("Auspicious Spirits") && EnemyInRange (40) <= 4 && Cooldown ("Mind Blast") == 0) {
				if (MindBlast ())
					return true;
			}
			//	actions.main+=/shadow_word_pain,if=talent.auspicious_spirits.enabled&remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react,cycle_targets=1,max_cycle_targets=7
			if (HasSpell ("Auspicious Spirits")) {
				MaxCycle = targets.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Shadow Word: Pain", true) < 18 * 0.3);
				if (MaxCycle.ToList ().Count <= 7) {
					CycleTarget = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (ShadowWordPain (CycleTarget))
							return true;
					}
				}
			}
			//	actions.main+=/mind_blast,if=cooldown_react
			if (Cooldown ("Mind Blast") == 0) {
				if (MindBlast ())
					return true;
			}
			//	actions.main+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && EnemyInRange (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				var bestTarget = BestTarget (40, 10, 3);
				if (bestTarget != null) {
					if (MindSear (bestTarget)) {
						Interrupt = "ChainMS";
						return true;
					}
				}
			}
			//	actions.main+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && EnemyInRange (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				var bestTarget = BestTarget (40, 10, 3);
				if (bestTarget != null) {
					if (MindSear (bestTarget)) {
						Interrupt = "ChainMS";
						return true;
					}
				}
			}
			//	actions.main+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && EnemyInRange (40) <= 2) {
				if (MindFlay ()) {
					Interrupt = "ChainMSO";
					return true;
				}
			}
			//	actions.main+=/insanity,chain=1,if=active_enemies<=2,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			if (Me.HasAura ("Insanity") && EnemyInRange (40) <= 2) {
				if (MindFlay ()) {
					Interrupt = "ChainMSO";
					return true;
				}
			}
			//	actions.main+=/halo,if=talent.halo.enabled&target.distance<=30&active_enemies>2
			//	actions.main+=/cascade,if=talent.cascade.enabled&active_enemies>2&target.distance<=40
			//	actions.main+=/divine_star,if=talent.divine_star.enabled&active_enemies>4&target.distance<=24
			//	actions.main+=/shadow_word_pain,if=!talent.auspicious_spirits.enabled&remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&active_enemies<=5,cycle_targets=1,max_cycle_targets=5
			//	actions.main+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5,cycle_targets=1,max_cycle_targets=5
			//	actions.main+=/devouring_plague,if=!talent.void_entropy.enabled&shadow_orb>=3&ticks_remain<=1
			//	actions.main+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react=3
			//	actions.main+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
			//	actions.main+=/cascade,if=talent.cascade.enabled&(active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11
			//	actions.main+=/divine_star,if=talent.divine_star.enabled&(active_enemies>1&target.distance<=24)
			//	actions.main+=/wait,sec=cooldown.shadow_word_death.remains,if=natural_shadow_word_death_range&cooldown.shadow_word_death.remains<0.5&active_enemies<=1,cycle_targets=1
			//	actions.main+=/wait,sec=cooldown.mind_blast.remains,if=cooldown.mind_blast.remains<0.5&cooldown.mind_blast.remains&active_enemies<=1
			//	actions.main+=/mind_spike,if=buff.surge_of_darkness.react&active_enemies<=5
			//	actions.main+=/divine_star,if=talent.divine_star.enabled&target.distance<=28&active_enemies>1
			//	actions.main+=/mind_sear,chain=1,if=active_enemies>=4,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			//	actions.main+=/shadow_word_pain,if=shadow_orb>=2&ticks_remain<=3&target.time_to_die>(18*0.75)&talent.insanity.enabled
			//	actions.main+=/vampiric_touch,if=shadow_orb>=2&ticks_remain<=3.5&target.time_to_die>(15*0.75+cast_time)&talent.insanity.enabled
			//	actions.main+=/mind_flay,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			//	actions.main+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
			//	actions.main+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
			//	actions.main+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
			//	actions.main+=/divine_star,moving=1,if=talent.divine_star.enabled&target.distance<=28
			//	actions.main+=/cascade,moving=1,if=talent.cascade.enabled&target.distance<=40
			//	actions.main+=/shadow_word_pain,moving=1,cycle_targets=1

//			if (ToSkill == 1) {
//				ToSkill = 0;
//				goto Skill_1;
//			}
//			if (ToSkill == 2) {
//				ToSkill = 0;
//				goto Skill_2;
//			}
//
//			// actions.main+=/halo,if=talent.halo.enabled&target.distance<=30&active_enemies>2
//			if (Cast("Halo", () => HasSpell("Halo") && Range <= 30 && EnemyInRange(30) > 2)) return;
//			// actions.main+=/cascade,if=talent.cascade.enabled&active_enemies>2&target.distance<=40
//			if (Cast("Cascade", () => HasSpell("Cascade") && EnemyInRange(40) > 2 && Range <= 40)) return;
//			// actions.main+=/divine_star,if=talent.divine_star.enabled&active_enemies>4&target.distance<=24
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && EnemyInRange(24) > 4 && Range <= 24)) return;
//			// actions.main+=/shadow_word_pain,if=!talent.auspicious_spirits.enabled&remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&active_enemies<=5,cycle_targets=1,max_cycle_targets=5
//			if (!HasSpell("Auspicious Spirits") && EnemyInRange(40) <= 5) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Shadow Word: Pain", true) < 18 * 0.3).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.main+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5,cycle_targets=1,max_cycle_targets=5
//			if (EnemyInRange(40) <= 5) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Vampiric Touch", true) < (15 * 0.3 + 1.5)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Vampiric Touch", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.main+=/devouring_plague,if=!talent.void_entropy.enabled&shadow_orb>=3&ticks_remain<=1
//			if (Cast("Devouring Plague", () => !HasSpell("Void Entropy") && Orb >= 3 && Target.AuraTimeRemaining("Devouring Plague", true) <= 1)) return;
//			// actions.main+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react=3
//			if (Cast("Mind Spike", () => EnemyInRange(40) <= 5 && AuraStackCount("Surge of Darkness") == 3)) return;
//			// actions.main+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
//			if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30 && Range >= 17)) return;
//			// actions.main+=/cascade,if=talent.cascade.enabled&(active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11
//			if (Cast("Cascade", () => HasSpell("Cascade") && (EnemyInRange(40) > 2 && Range <= 40))) return;
//			// actions.main+=/divine_star,if=talent.divine_star.enabled&(active_enemies>1&target.distance<=24)
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && EnemyInRange(24) > 1 && Range <= 24)) return;
//			// actions.main+=/wait,sec=cooldown.shadow_word_death.remains,if=natural_shadow_word_death_range&cooldown.shadow_word_death.remains<0.5&active_enemies<=1,cycle_targets=1
//			// wait
//			if (TargetHealth < 0.2 && Cooldown("Shadow Word: Death") < 0.5 && Cooldown("Shadow Word: Death") > 0 && EnemyInRange(40) == 1) {
//				ToSkill = 1;
//				return;
//			}
//			Skill_1:
//			if (Cast("Shadow Word: Death")) return;
//			// actions.main+=/wait,sec=cooldown.mind_blast.remains,if=cooldown.mind_blast.remains<0.5&cooldown.mind_blast.remains&active_enemies<=1
//			// wait
//			if (TargetHealth < 0.2 && Cooldown("Mind Blast") < 0.5 && Cooldown("Mind Blast") > 0 && EnemyInRange(40) == 1) {
//				ToSkill = 2;
//				return;
//			}
//			Skill_2:
//			if (Cast("Mind Blast")) return;
//			// actions.main+=/mind_spike,if=buff.surge_of_darkness.react&active_enemies<=5
//			if (Cast("Mind Spike", () => Me.HasAura("Surge of Darkness") && EnemyInRange(40) <= 5)) return;
//			// actions.main+=/divine_star,if=talent.divine_star.enabled&target.distance<=28&active_enemies>1
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && Range <= 24 && EnemyInRange(24) > 1)) return;
//			// actions.main+=/mind_sear,chain=1,if=active_enemies>=4,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
//			if (EnemyInRange(40) >= 4) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 4) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMSO = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.main+=/shadow_word_pain,if=shadow_orb>=2&ticks_remain<=3&target.time_to_die>(18*0.75)&talent.insanity.enabled
//			if (Cast("Shadow Word: Pain", () => Orb >= 2 && Target.AuraTimeRemaining("Shadow Word: Pain") <= 3 && HasSpell("Insanity"))) return;
//			// actions.main+=/vampiric_touch,if=shadow_orb>=2&ticks_remain<=3.5&target.time_to_die>(15*0.75+cast_time)&talent.insanity.enabled
//			if (Cast("Vampiric Touch", () => Orb >= 2 && Target.AuraTimeRemaining("Vampiric Touch") <= 3.5 && HasSpell("Insanity"))) return;
//			// actions.main+=/mind_flay,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
//			if (Cast("Mind Flay")) {
//				ChainMSO = true;
//				return;
//			}
//			if (Me.IsMoving) {
//				// actions.main+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
//				if (Cast("Shadow Word: Death", () => TargetHealth < 0.2)) return;
//				// actions.main+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
//				if (Cast("Power Word: Shield", () => HasSpell("Body and Soul"))) return;
//				// actions.main+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
//				if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30)) return;
//				// actions.main+=/divine_star,moving=1,if=talent.divine_star.enabled&target.distance<=28
//				if (Cast("Divine Star", () => HasSpell("Divine Star") && Range <= 24)) return;
//				// actions.main+=/cascade,moving=1,if=talent.cascade.enabled&target.distance<=40
//				if (Cast("Cascade", () => HasSpell("Cascade") && Range <= 40)) return;
//				// actions.main+=/shadow_word_pain,moving=1,cycle_targets=1
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Shadow Word: Pain", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}

			return false;
		}

		public bool Vent ()
		{
			//	actions.vent=void_entropy,if=shadow_orb=3&!ticking&target.time_to_die>60&active_enemies=1
			//	actions.vent+=/void_entropy,if=!dot.void_entropy.ticking&shadow_orb=5&active_enemies>=1&target.time_to_die>60,cycle_targets=1,max_cycle_targets=6
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.ticking&dot.void_entropy.remains<=gcd*2&cooldown_react&active_enemies=1
			//	actions.vent+=/devouring_plague,if=dot.void_entropy.ticking&dot.void_entropy.remains<=gcd*2&cooldown_react&active_enemies>1,cycle_targets=1
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<5&active_enemies>1,cycle_targets=1
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<10&active_enemies>2,cycle_targets=1
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<15&active_enemies>3,cycle_targets=1
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<20&active_enemies>4,cycle_targets=1
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains&(cooldown.mind_blast.remains<=gcd*2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<=gcd*2))&active_enemies=1
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains&(cooldown.mind_blast.remains<=gcd*2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<=gcd*2))&active_enemies>1,cycle_targets=1
			//	actions.vent+=/devouring_plague,if=shadow_orb>=3&dot.void_entropy.ticking&active_enemies=1&buff.mental_instinct.remains<(gcd*1.4)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
			//	actions.vent+=/mindbender,if=talent.mindbender.enabled&cooldown.mind_blast.remains>=gcd
			//	actions.vent+=/shadowfiend,if=!talent.mindbender.enabled&cooldown.mind_blast.remains>=gcd
			//	actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30&active_enemies>=4
			//	actions.vent+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb<=2,cycle_targets=1
			//	actions.vent+=/devouring_plague,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb>=3,cycle_targets=1
			//	actions.vent+=/mind_blast,if=active_enemies<=10&cooldown_react&shadow_orb<=4
			//	actions.vent+=/shadow_word_death,if=natural_shadow_word_death_range&cooldown_react&shadow_orb<=4,cycle_targets=1
			//	actions.vent+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.vent+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.vent+=/shadow_word_pain,if=shadow_orb=4&remains<(18*0.50)&set_bonus.tier17_2pc&cooldown.mind_blast.remains<1.2*gcd&cooldown.mind_blast.remains>0.2*gcd
			//	actions.vent+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.vent+=/insanity,chain=1,if=active_enemies<=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.vent+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react=3
			//	actions.vent+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react,cycle_targets=1,max_cycle_targets=5
			//	actions.vent+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react,cycle_targets=1,max_cycle_targets=5
			//	actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30&cooldown.mind_blast.remains>0.5*gcd
			//	actions.vent+=/cascade,if=talent.cascade.enabled&target.distance<=40&cooldown.mind_blast.remains>0.5*gcd
			//	actions.vent+=/divine_star,if=talent.divine_star.enabled&active_enemies>4&target.distance<=24&cooldown.mind_blast.remains>0.5*gcd
			//	actions.vent+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react&cooldown.mind_blast.remains>0.5*gcd
			//	actions.vent+=/mind_sear,chain=1,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.vent+=/mind_flay,if=cooldown.mind_blast.remains>0.5*gcd,interrupt=1,chain=1
			//	actions.vent+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
			//	actions.vent+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
			//	actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
			//	actions.vent+=/divine_star,moving=1,if=talent.divine_star.enabled&target.distance<=28
			//	actions.vent+=/cascade,moving=1,if=talent.cascade.enabled&target.distance<=40
			//	actions.vent+=/shadow_word_pain,moving=1,cycle_targets=1

//			var targets = Adds;
//			targets.Add(Target);
//			// actions.vent=void_entropy,if=shadow_orb=3&!ticking&target.time_to_die>60&active_enemies=1
//			if (Cast("Void Entropy", () => Orb == 3 && !Target.HasAura("Void Entropy", true) && EnemyInRange(40) == 1)) return;
//			// actions.vent+=/void_entropy,if=!dot.void_entropy.ticking&shadow_orb=5&active_enemies>=1&target.time_to_die>60,cycle_targets=1,max_cycle_targets=6
//			if (Orb == 5 && EnemyInRange(40) >= 1 && Cooldown("Devouring Plague") == 0) {
//				MaxCycle = targets.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Void Entropy", true));
//				if (MaxCycle.ToList().Count <= 6) {
//					CycleTarget = MaxCycle.DefaultIfEmpty(null).FirstOrDefault();
//					if (Cast("Void Entropy", CycleTarget, () => CycleTarget != null)) return;
//				}
//			}
//			// actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.ticking&dot.void_entropy.remains<=gcd*2&cooldown_react&active_enemies=1
//			if (Cast("Devouring Plague", () => Orb == 5 && Target.HasAura("Void Entropy", true) && Target.AuraTimeRemaining("Void Entropy", true) <= 1.5 * 2 && EnemyInRange(40) == 1)) return;
//			// actions.vent+=/devouring_plague,if=dot.void_entropy.ticking&dot.void_entropy.remains<=gcd*2&cooldown_react&active_enemies>1,cycle_targets=1
//			if (EnemyInRange(40) > 1 && Cooldown("Devouring Plague") == 0) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HasAura("Void Entropy", true) && u.AuraTimeRemaining("Void Entropy", true) <= 1.5 *2 && Cooldown("Devouring Plague") == 0).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<5&active_enemies>1,cycle_targets=1
//			if (Orb == 5 && EnemyInRange(40) > 1) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Void Entropy", true) < 5).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<10&active_enemies>2,cycle_targets=1
//			if (Orb == 5 && EnemyInRange(40) > 2) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Void Entropy", true) < 10).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<15&active_enemies>3,cycle_targets=1
//			if (Orb == 5 && EnemyInRange(40) > 3) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Void Entropy", true) < 15).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<20&active_enemies>4,cycle_targets=1
//			if (Orb == 5 && EnemyInRange(40) > 4) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Void Entropy", true) < 20).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains&(cooldown.mind_blast.remains<=gcd*2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<=gcd*2))&active_enemies=1
//			if (Cast("Devouring Plague", () => Orb == 5 && Target.HasAura("Void Entropy", true) && (Cooldown("Mind Blast") <= 1.5 * 2 || (TargetHealth < 0.2 && Cooldown("Shadow Word: Death") <= 1.5 * 2)) && EnemyInRange(40) == 1)) return;
//			// actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains&(cooldown.mind_blast.remains<=gcd*2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<=gcd*2))&active_enemies>1,cycle_targets=1
//			if (Orb == 5 && EnemyInRange(40) > 1) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HasAura("Void Entropy", true) && (Cooldown("Mind Blast") <= 1.5 * 2 || (u.HealthFraction < 0.2 && Cooldown("Shadow Word: Death") <= 1.5 * 2))).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/devouring_plague,if=shadow_orb>=3&dot.void_entropy.ticking&active_enemies=1&buff.mental_instinct.remains<(gcd*1.4)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
//			if (Cast("Devouring Plague", () => Orb >= 3 && Target.HasAura("Void Entropy", true) && EnemyInRange(40) == 1 && Me.AuraTimeRemaining("Mental Instinct") < 1.5 * 1.4 && Me.AuraTimeRemaining("Mental Instinct") > 1.5 * 0.7 && Me.HasAura("Mental Instinct"))) return;
//			// actions.vent+=/mindbender,if=talent.mindbender.enabled&cooldown.mind_blast.remains>=gcd
//			if (Cast("Mindbender", () => HasSpell("Mindbender") && Cooldown("Mind Blast") >= 1.5)) return;
//			// actions.vent+=/shadowfiend,if=!talent.mindbender.enabled&cooldown.mind_blast.remains>=gcd
//			if (Cast("Shadowfiend", () => !HasSpell("Mindbender") && Cooldown("Mind Blast") >= 1.5)) return;
//			// actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30&active_enemies>=4
//			if (Cast("Halo", () => HasSpell("Halo") && Range <= 30 && EnemyInRange(30) >= 4)) return;
//			// actions.vent+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb<=2,cycle_targets=1
//			if (HasGlyph(162532) && Orb <= 2) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Glyph of Mind Blast", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Mind Blast", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/devouring_plague,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb>=3,cycle_targets=1
//			if (HasGlyph(162532) && Orb >= 3) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Glyph of Mind Blast", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/mind_blast,if=active_enemies<=10&cooldown_react&shadow_orb<=4
//			if (Cast("Mind Blast", () => EnemyInRange(40) <= 10 && Cooldown("Mind Blast") == 0 && Orb <= 4)) return;
//			// actions.vent+=/shadow_word_death,if=natural_shadow_word_death_range&cooldown_react&shadow_orb<=4,cycle_targets=1
//			if (Cooldown("Shadow Word: Death") == 0 && Orb <= 4) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Me.HasAura("Insanity") && EnemyInRange(40) >= 3 && Me.AuraTimeRemaining("Insanity") < 0.5 * 1.5 && Cooldown("Mind Blast") > 0.5 * 1.5) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						// if (Cast("Searing Insanity", bestTarget, () => bestTarget != null)) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.vent+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Me.HasAura("Insanity") && EnemyInRange(40) >= 3 && Cooldown("Mind Blast") > 0.5 * 1.5) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.vent+=/shadow_word_pain,if=shadow_orb=4&remains<(18*0.50)&set_bonus.tier17_2pc&cooldown.mind_blast.remains<1.2*gcd&cooldown.mind_blast.remains>0.2*gcd
//			if (Cast("Shadow Word: Pain", () => Orb == 4 && Target.AuraTimeRemaining("Shadow Word: Pain") < (18 * 0.5) && HasSpell(165628) && Cooldown("Mind Blast") < (1.2 * 1.5) && Cooldown("Mind Blast") > 0.2 * 1.5)) return;
//			// actions.vent+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Cast("Mind Flay", () => Me.HasAura("Insanity") && Me.AuraTimeRemaining("Insanity") < (0.5 * 1.5) && EnemyInRange(40) <= 3 && Cooldown("Mind Blast") > (0.5 * 1.5))) {
//				ChainMS = true;
//				return;
//			}
//			// actions.vent+=/insanity,chain=1,if=active_enemies<=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Cast("Mind Flay", () => Me.HasAura("Insanity") && EnemyInRange(40) <= 3 && Cooldown("Mind Blast") > (0.5 * 1.5))) {
//				ChainMS = true;
//				return;
//			}
//			// actions.vent+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react=3
//			if (Cast("Mind Spike", () => EnemyInRange(40) <= 5 && AuraStackCount("Surge of Darkness") == 3)) return;
//			// actions.vent+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react,cycle_targets=1,max_cycle_targets=5
//			MaxCycle = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Shadow Word: Pain", true) <= (18 * 0.3));
//			if (MaxCycle.ToList().Count <= 5) {
//				CycleTarget = MaxCycle.DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}
//			if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			// actions.vent+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react,cycle_targets=1,max_cycle_targets=5
//			MaxCycle = targets.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Vampiric Touch", true) < (15 * 0.3 + 1.5));
//			if (MaxCycle.ToList().Count <= 5) {
//				CycleTarget = MaxCycle.DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Vampiric Touch", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30&cooldown.mind_blast.remains>0.5*gcd
//			if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30 && Cooldown("Mind Blast") > (0.5 * 1.5))) return;
//			// actions.vent+=/cascade,if=talent.cascade.enabled&target.distance<=40&cooldown.mind_blast.remains>0.5*gcd
//			if (Cast("Cascade", () => HasSpell("Cascade") && Range <= 40 && Cooldown("Mind Blast") > (0.5 * 1.5))) return;
//			// actions.vent+=/divine_star,if=talent.divine_star.enabled&active_enemies>4&target.distance<=24&cooldown.mind_blast.remains>0.5*gcd
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && EnemyInRange(24) > 4 && Range <= 24 && Cooldown("Mind Blast") > 0.5 * 1.5)) return;
//			// actions.vent+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react&cooldown.mind_blast.remains>0.5*gcd
//			if (Cast("Mind Spike", () => EnemyInRange(40) <= 5 && Me.HasAura("Surge of Darkness") && Cooldown("Mind Blast") > 0.5 * 1.5)) return;
//			// actions.vent+=/mind_sear,chain=1,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (EnemyInRange(40) >= 3 && Cooldown("Mind Blast") > 0.5 * 1.5) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.vent+=/mind_flay,if=cooldown.mind_blast.remains>0.5*gcd,interrupt=1,chain=1
//			if (Cast("Mind Flay", () => Cooldown("Mind Blast") > 0.5 * 1.5)) {
//				ChainM = true;
//				return;
//			}
//			if (Me.IsMoving) {
//				// actions.vent+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
//				if (Cast("Shadow Word: Death", () => TargetHealth < 0.2)) return;
//				// actions.vent+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
//				if (Cast("Power Word: Shield", () => HasSpell("Body and Soul"))) return;
//				// actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
//				if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30)) return;
//				// actions.vent+=/divine_star,moving=1,if=talent.divine_star.enabled&target.distance<=28
//				if (Cast("Divine Star", () => HasSpell("Divine Star") && Range <= 24)) return;
//				// actions.vent+=/cascade,moving=1,if=talent.cascade.enabled&target.distance<=40
//				if (Cast("Cascade", () => HasSpell("Cascade") && Range <= 40)) return;
//				// actions.vent+=/shadow_word_pain,moving=1,cycle_targets=1
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Shadow Word: Pain", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}

			return false;
		}

		public bool COP_DotWeave ()
		{
			//	actions.cop_dotweave=devouring_plague,if=target.dot.vampiric_touch.ticking&target.dot.shadow_word_pain.ticking&shadow_orb=5&cooldown_react
			//	actions.cop_dotweave+=/devouring_plague,if=buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
			//	actions.cop_dotweave+=/devouring_plague,if=(target.dot.vampiric_touch.ticking&target.dot.shadow_word_pain.ticking&!buff.insanity.remains&cooldown.mind_blast.remains>0.4*gcd)
			//	actions.cop_dotweave+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
			//	actions.cop_dotweave+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_targets=1
			//	actions.cop_dotweave+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb<=2,cycle_targets=1
			//	actions.cop_dotweave+=/mind_blast,if=shadow_orb<=4&cooldown_react
			//	actions.cop_dotweave+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.cop_dotweave+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.cop_dotweave+=/shadowfiend,if=!talent.mindbender.enabled&!buff.insanity.remains
			//	actions.cop_dotweave+=/mindbender,if=talent.mindbender.enabled&!buff.insanity.remains
			//	actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb=4&set_bonus.tier17_2pc&!target.dot.shadow_word_pain.ticking&!target.dot.devouring_plague.ticking&cooldown.mind_blast.remains<gcd&cooldown.mind_blast.remains>0
			//	actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb=5&!target.dot.devouring_plague.ticking&!target.dot.shadow_word_pain.ticking
			//	actions.cop_dotweave+=/vampiric_touch,if=shadow_orb=5&!target.dot.devouring_plague.ticking&!target.dot.vampiric_touch.ticking
			//	actions.cop_dotweave+=/insanity,if=buff.insanity.remains,chain=1,interrupt_if=cooldown.mind_blast.remains<=0.1
			//	actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb>=2&target.dot.shadow_word_pain.remains>=6&cooldown.mind_blast.remains>0.5*gcd&target.dot.vampiric_touch.remains&buff.bloodlust.up&!set_bonus.tier17_2pc
			//	actions.cop_dotweave+=/vampiric_touch,if=shadow_orb>=2&target.dot.vampiric_touch.remains>=5&cooldown.mind_blast.remains>0.5*gcd&buff.bloodlust.up&!set_bonus.tier17_2pc
			//	actions.cop_dotweave+=/halo,if=cooldown.mind_blast.remains>0.5*gcd&talent.halo.enabled&target.distance<=30&target.distance>=17
			//	actions.cop_dotweave+=/cascade,if=cooldown.mind_blast.remains>0.5*gcd&talent.cascade.enabled&((active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11)
			//	actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&cooldown.mind_blast.remains>0.5*gcd&active_enemies>3&target.distance<=24
			//	actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0&!ticking,cycle_targets=1,max_cycle_targets=5
			//	actions.cop_dotweave+=/vampiric_touch,if=primary_target=0&!ticking,cycle_targets=1,max_cycle_targets=5
			//	actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&cooldown.mind_blast.remains>0.5*gcd&active_enemies=3&target.distance<=24
			//	actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0&(!ticking|remains<=18*0.3)&target.time_to_die>(18*0.75),cycle_targets=1,max_cycle_targets=5
			//	actions.cop_dotweave+=/vampiric_touch,if=primary_target=0&(!ticking|remains<=15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time),cycle_targets=1,max_cycle_targets=5
			//	actions.cop_dotweave+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.cop_dotweave+=/mind_spike
			//	actions.cop_dotweave+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
			//	actions.cop_dotweave+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
			//	actions.cop_dotweave+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
			//	actions.cop_dotweave+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
			//	actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
			//	actions.cop_dotweave+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
			//	actions.cop_dotweave+=/devouring_plague,moving=1
			//	actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0,moving=1,cycle_targets=1

//			var targets = Adds;
//			targets.Add(Target);
//			// actions.cop_dotweave=devouring_plague,if=target.dot.vampiric_touch.ticking&target.dot.shadow_word_pain.ticking&shadow_orb=5&cooldown_react
//			if (Cast("Devouring Plague", () => Target.HasAura("Vampiric Touch", true) && Target.HasAura("Shadow Word: Pain", true) && Orb == 5 && Cooldown("Devouring Plague") == 0)) return;
//			// actions.cop_dotweave+=/devouring_plague,if=buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
//			if (Cast("Devouring Plague", () => Me.AuraTimeRemaining("Mental Instinct") < 1.5 && Me.AuraTimeRemaining("Mental Instinct") > (1.5 * 0.7) && Me.HasAura("Mental Instinct", true))) return;
//			// actions.cop_dotweave+=/devouring_plague,if=(target.dot.vampiric_touch.ticking&target.dot.shadow_word_pain.ticking&!buff.insanity.remains&cooldown.mind_blast.remains>0.4*gcd)
//			if (Cast("Devouring Plague", () => (Target.HasAura("Vampiric Touch", true) && Target.HasAura("Shadow Word: Pain", true) && !Me.HasAura("Insanity") && Cooldown("Mind Blast") > 0.4 * 1.5))) return;
//			// actions.cop_dotweave+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
//			CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2 && !Target.HasAura("Shadow Word: Pain", true) && !Target.HasAura("Vampiric Touch", true)).DefaultIfEmpty(null).FirstOrDefault();
//			if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//			// actions.cop_dotweave+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_targets=1
//			CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2).DefaultIfEmpty(null).FirstOrDefault();
//			if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//			// actions.cop_dotweave+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb<=2,cycle_targets=1
//			if (HasGlyph(162532) && Orb <= 2) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Glyph of Mind Blast", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Mind Blast", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_dotweave+=/mind_blast,if=shadow_orb<=4&cooldown_react
//			if (Cast("Mind Blast", () => Orb <= 4 && Cooldown("Mind Blast") == 0)) return;
//			// actions.cop_dotweave+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Me.HasAura("Insanity") && Me.AuraTimeRemaining("Insanity") < 0.5 * 1.5 && EnemyInRange(40) >= 3 && Cooldown("Mind Blast") > 0.5 * 1.5) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.cop_dotweave+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Me.HasAura("Insanity") && EnemyInRange(40) >= 3 && Cooldown("Mind Blast") > 0.5 * 1.5) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.cop_dotweave+=/shadowfiend,if=!talent.mindbender.enabled&!buff.insanity.remains
//			if (Cast("Shadowfiend", () => !HasSpell("Mindbender") && !Me.HasAura("Insanity"))) return;
//			// actions.cop_dotweave+=/mindbender,if=talent.mindbender.enabled&!buff.insanity.remains
//			if (Cast("Mindbender", () => HasSpell("Mindbender") && !Me.HasAura("Insanity"))) return;
//			// actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb=4&set_bonus.tier17_2pc&!target.dot.shadow_word_pain.ticking&!target.dot.devouring_plague.ticking&cooldown.mind_blast.remains<gcd&cooldown.mind_blast.remains>0
//			if (Cast("Shadow Word: Pain", () => Orb == 4 && HasSpell(165628) && !Target.HasAura("Shadow Word: Pain", true) && !Target.HasAura("Devouring Plague", true) && Cooldown("Mind Blast") < 1.5 && Cooldown("Mind Blast") > 0)) return;
//			// actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb=5&!target.dot.devouring_plague.ticking&!target.dot.shadow_word_pain.ticking
//			if (Cast("Shadow Word: Pain", () => Orb == 5 && !Target.HasAura("Devouring Plague", true) && !Target.HasAura("Shadow Word: Pain", true))) return;
//			// actions.cop_dotweave+=/vampiric_touch,if=shadow_orb=5&!target.dot.devouring_plague.ticking&!target.dot.vampiric_touch.ticking
//			if (Cast("Vampiric Touch", () => Orb == 5 && !Target.HasAura("Devouring Plague", true) && !Target.HasAura("Vampiric Touch", true))) return;
//			// actions.cop_dotweave+=/insanity,if=buff.insanity.remains,chain=1,interrupt_if=cooldown.mind_blast.remains<=0.1
//			if (Cast("Mind Flay", () => Me.HasAura("Insanity"))) {
//				ChainM = true;
//				return;
//			}
//			// actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb>=2&target.dot.shadow_word_pain.remains>=6&cooldown.mind_blast.remains>0.5*gcd&target.dot.vampiric_touch.remains&buff.bloodlust.up&!set_bonus.tier17_2pc
//			if (Cast("Shadow Word: Pain", () => Orb >= 2 && Target.AuraTimeRemaining("Shadow Word: Pain", true) >= 6 && Cooldown("Mind Blast") > 0.5 * 1.5 && Target.HasAura("Vampiric Touch", true) && Me.HasAura("Bloodlust") && !HasSpell(165628))) return;
//			// actions.cop_dotweave+=/vampiric_touch,if=shadow_orb>=2&target.dot.vampiric_touch.remains>=5&cooldown.mind_blast.remains>0.5*gcd&buff.bloodlust.up&!set_bonus.tier17_2pc
//			if (Cast("Vampiric Touch", () => Orb >= 2 && Target.AuraTimeRemaining("Vampiric Touch", true) >= 5 && Cooldown("Mind Blast") > 0.5 * 1.5 && Me.HasAura("Bloodlust") && !HasSpell(165628))) return;
//			// actions.cop_dotweave+=/halo,if=cooldown.mind_blast.remains>0.5*gcd&talent.halo.enabled&target.distance<=30&target.distance>=17
//			if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Cooldown("Mind Blast") > 0.5 * 1.5 && Range <= 30 && Range >= 17)) return;
//			// actions.cop_dotweave+=/cascade,if=cooldown.mind_blast.remains>0.5*gcd&talent.cascade.enabled&((active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11)
//			if (Cast("Cascade", () => HasSpell("Cascade") && Cooldown("Mind Blast") > 0.5 * 1.5 && ((EnemyInRange(40) > 1 || Range >= 28) && Range <= 40 && Range >= 11))) return;
//			// actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&cooldown.mind_blast.remains>0.5*gcd&active_enemies>3&target.distance<=24
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && Cooldown("Mind Blast") > 0.5 * 1.5 && EnemyInRange(24) > 3 && Range <= 24)) return;
//			// actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0&!ticking,cycle_targets=1,max_cycle_targets=5
//			MaxCycle = Adds.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Shadow Word: Pain", true));
//			if (MaxCycle.ToList().Count <= 5) {
//				CycleTarget = MaxCycle.DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_dotweave+=/vampiric_touch,if=primary_target=0&!ticking,cycle_targets=1,max_cycle_targets=5
//			MaxCycle = Adds.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Vampiric Touch", true) && u != Target);
//			if (MaxCycle.ToList().Count <= 5) {
//				CycleTarget = MaxCycle.DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Vampiric Touch", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&cooldown.mind_blast.remains>0.5*gcd&active_enemies=3&target.distance<=24
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && Cooldown("Mind Blast") > 0.5 * 1.5 && EnemyInRange(24) == 3 && Range <= 24)) return;
//			// actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0&(!ticking|remains<=18*0.3)&target.time_to_die>(18*0.75),cycle_targets=1,max_cycle_targets=5
//			MaxCycle = Adds.Where(u => u.IsInCombatRangeAndLoS && (!u.HasAura("Shadow Word: Pain", true) || u.AuraTimeRemaining("Shadow Word: Pain", true) <= 18 * 0.75));
//			if (MaxCycle.ToList().Count <= 5) {
//				CycleTarget = MaxCycle.DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_dotweave+=/vampiric_touch,if=primary_target=0&(!ticking|remains<=15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time),cycle_targets=1,max_cycle_targets=5
//			MaxCycle = Adds.Where(u => u.IsInCombatRangeAndLoS && u != Target && (!u.HasAura("Vampiric Touch", true) || u.AuraTimeRemaining("Vampiric Touch", true) <= 15 * 0.3 + 1.5));
//			if (MaxCycle.ToList().Count <= 5) {
//				CycleTarget = MaxCycle.DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Vampiric Touch", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_dotweave+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (EnemyInRange(40) >= 8) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.cop_dotweave+=/mind_spike
//			if (Cast("Mind Spike")) return;
//			if (Me.IsMoving) {
//				// actions.cop_dotweave+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2 && !Target.HasAura("Shadow Word: Pain", true) && !Target.HasAura("Vampiric Touch", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//				// actions.cop_dotweave+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
//				if (Cast("Shadow Word: Death", () => TargetHealth < 0.2)) return;
//				// actions.cop_dotweave+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
//				if (Cast("Power Word: Shield", () => HasSpell("Body and Soul"))) return;
//				// actions.cop_dotweave+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
//				if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30)) return;
//				// actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
//				if (Cast("Divine Star", () => HasSpell("Divine Star") && Range <= 24)) return;
//				// actions.cop_dotweave+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
//				if (Cast("Cascade", () => HasSpell("Cascade") && Range <= 40)) return;
//				// actions.cop_dotweave+=/devouring_plague,moving=1
//				if (Cast("Devouring Plague")) return;
//				// actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0,moving=1,cycle_targets=1
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Shadow Word: Pain", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}

			return false;
		}

		public bool COP_Insanity ()
		{
			//	actions.cop_insanity=devouring_plague,if=shadow_orb=5|(active_enemies>=5&!buff.insanity.remains)
			//	actions.cop_insanity+=/devouring_plague,if=buff.mental_instinct.remains<(gcd*1.7)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
			//	actions.cop_insanity+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0,cycle_targets=1
			//	actions.cop_insanity+=/mind_blast,if=active_enemies<=5&cooldown_react
			//	actions.cop_insanity+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
			//	actions.cop_insanity+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_targets=1
			//	actions.cop_insanity+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<gcd|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd)),cycle_targets=1
			//	actions.cop_insanity+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd)),cycle_targets=1
			//	actions.cop_insanity+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.cop_insanity+=/searing_insanity,if=active_enemies>=5,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.cop_insanity+=/mindbender,if=talent.mindbender.enabled
			//	actions.cop_insanity+=/shadowfiend,if=!talent.mindbender.enabled
			//	actions.cop_insanity+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
			//	actions.cop_insanity+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
			//	actions.cop_insanity+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
			//	actions.cop_insanity+=/insanity,if=active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
			//	actions.cop_insanity+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
			//	actions.cop_insanity+=/cascade,if=talent.cascade.enabled&((active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11)
			//	actions.cop_insanity+=/divine_star,if=talent.divine_star.enabled&active_enemies>2&target.distance<=24
			//	actions.cop_insanity+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.cop_insanity+=/mind_spike
			//	actions.cop_insanity+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
			//	actions.cop_insanity+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
			//	actions.cop_insanity+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
			//	actions.cop_insanity+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
			//	actions.cop_insanity+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
			//	actions.cop_insanity+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
			//	actions.cop_insanity+=/devouring_plague,moving=1
			//	actions.cop_insanity+=/shadow_word_pain,if=primary_target=0,moving=1,cycle_targets=1

//			var targets = Adds;
//			targets.Add(Target);
//			// actions.cop_insanity=devouring_plague,if=shadow_orb=5|(active_enemies>=5&!buff.insanity.remains)
//			if (Cast("Devouring Plague", () => Orb == 5 || (EnemyInRange(40) >= 5 && !Me.HasAura("Insanity")))) return;
//			// actions.cop_insanity+=/devouring_plague,if=buff.mental_instinct.remains<(gcd*1.7)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
//			if (Cast("Devouring Plague", () => Me.AuraTimeRemaining("Mental Instinct") < (1.5 * 1.7) && Me.AuraTimeRemaining("Mental Instinct") > (1.5 * 0.7) && Me.HasAura("Mental Instinct", true))) return;
//			// actions.cop_insanity+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0,cycle_targets=1
//			if (HasGlyph(162532)) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Glyph of Mind Blast", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Mind Blast", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_insanity+=/mind_blast,if=active_enemies<=5&cooldown_react
//			if (Cast("Mind Blast", () => EnemyInRange(40) <= 5 && Cooldown("Mind Blast") == 0)) return;
//			// actions.cop_insanity+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
//			CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2 && !Target.HasAura("Shadow Word: Pain", true) && !Target.HasAura("Vampiric Touch", true)).DefaultIfEmpty(null).FirstOrDefault();
//			if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//			// actions.cop_insanity+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_targets=1
//			CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2).DefaultIfEmpty(null).FirstOrDefault();
//			if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//			// actions.cop_insanity+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<gcd|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd)),cycle_targets=1
//			if (Orb >= 3 && !HasSpell(165628) && !HasSpell(165629)) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && (Cooldown("Mind Blast") < 1.5 || (u.HealthFraction < 0.2 && Cooldown("Shadow Word: Death") < 1.5))).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}			
//			// actions.cop_insanity+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd)),cycle_targets=1
//			if (Orb >= 3 && HasSpell(165628) && !HasSpell(165629)) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && (Cooldown("Mind Blast") < 2 || (u.HealthFraction < 0.2 && Cooldown("Shadow Word: Death") < 1.5))).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}			
//			// actions.cop_insanity+=/searing_insanity,if=buff.shadow_word_insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Me.HasAura("Insanity") && EnemyInRange(40) >= 3 && Me.AuraTimeRemaining("Shadow Word: Insanity") < 0.5 * 1.5 && Cooldown("Mind Blast") > 0.5 * 1.5) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.cop_insanity+=/searing_insanity,if=active_enemies>=5,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Me.HasAura("Insanity") && EnemyInRange(40) >= 5) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.cop_insanity+=/mindbender,if=talent.mindbender.enabled
//			if (Cast("Mindbender", () => HasSpell("Mindbender"))) return;
//			// actions.cop_insanity+=/shadowfiend,if=!talent.mindbender.enabled
//			if (Cast("Shadowfiend", () => !HasSpell("Mindbender"))) return;
//			// actions.cop_insanity+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
//			if (EnemyInRange(40) <= 5) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Shadow Word: Pain", true) < (18 * 0.3)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_insanity+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
//			if (EnemyInRange(40) <= 5) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && u != Target && u.AuraTimeRemaining("Vampiric Touch", true) < (15 * 0.3 + 1.5)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Vampiric Touch", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop_insanity+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
//			if (Cast("Mind Flay", () => Me.HasAura("Insanity") && Me.AuraTimeRemaining("Insanity") < 0.5 * 1.5 && EnemyInRange(40) <= 2)) {
//				ChainMSH = true;
//				return;
//			}
//			// actions.cop_insanity+=/insanity,if=active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
//			if (Cast("Mind Flay", () => Me.HasAura("Insanity") && EnemyInRange(40) <= 2)) {
//				ChainMSH = true;
//				return;
//			}
//			// actions.cop_insanity+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
//			if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30 && Range >= 17)) return;
//			// actions.cop_insanity+=/cascade,if=talent.cascade.enabled&((active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11)
//			if (Cast("Cascade", () => HasSpell("Cascade") && IsBoss(Target) && ((EnemyInRange(40) > 1 || Range >= 28) && Range <= 40 && Range >= 11))) return;
//			// actions.cop_insanity+=/divine_star,if=talent.divine_star.enabled&active_enemies>2&target.distance<=24
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && EnemyInRange(24) > 2 && Range <= 24)) return;
//			// actions.cop_insanity+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (EnemyInRange(40) >= 8) {
//				var bestTarget = targets
//					.Where(u => u.IsInCombatRangeAndLoS && u.DistanceSquared <= SpellMaxRangeSq("Mind Sear"))
//					.OrderByDescending(u => targets.Count(o => Vector3.DistanceSquared(u.Position, o.Position) <= 10 * 10)).DefaultIfEmpty(null).FirstOrDefault();
//				if (bestTarget != null) {
//					if (targets.Where(u => Vector3.DistanceSquared(u.Position, bestTarget.Position) <= 10 * 10).ToList().Count >= 3) {
//						if (Cast("Mind Sear", bestTarget, () => bestTarget != null)) {
//							ChainMS = true;
//							return;
//						}
//					}
//				}
//			}
//			// actions.cop_insanity+=/mind_spike
//			if (Cast("Mind Spike")) return;
//			if (Me.IsMoving) {
//				// actions.cop_insanity+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2 && !Target.HasAura("Shadow Word: Pain", true) && !Target.HasAura("Vampiric Touch", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//				// actions.cop_insanity+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
//				if (Cast("Shadow Word: Death", () => TargetHealth < 0.2)) return;
//				// actions.cop_insanity+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
//				if (Cast("Power Word: Shield", () => HasSpell("Body and Soul"))) return;
//				// actions.cop_insanity+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
//				if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30)) return;
//				// actions.cop_insanity+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
//				if (Cast("Divine Star", () => HasSpell("Divine Star") && Range <= 24)) return;
//				// actions.cop_insanity+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
//				if (Cast("Cascade", () => HasSpell("Cascade") && Range <= 40)) return;
//				// actions.cop_insanity+=/devouring_plague,moving=1
//				if (Cast("Devouring Plague")) return;
//				// actions.cop_insanity+=/shadow_word_pain,if=primary_target=0,moving=1,cycle_targets=1
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Shadow Word: Pain", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}

			return false;
		}

		public bool COP ()
		{
			//	actions.cop=devouring_plague,if=shadow_orb=5&primary_target=0&!target.dot.devouring_plague_dot.ticking&target.time_to_die>=(gcd*4*7%6),cycle_targets=1
			//	actions.cop+=/devouring_plague,if=shadow_orb=5&primary_target=0&target.time_to_die>=(gcd*4*7%6)&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20)),cycle_targets=1
			//	actions.cop+=/devouring_plague,if=shadow_orb=5&!set_bonus.tier17_2pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
			//	actions.cop+=/devouring_plague,if=shadow_orb=5&set_bonus.tier17_2pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
			//	actions.cop+=/devouring_plague,if=primary_target=0&buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains&active_enemies>1,cycle_targets=1
			//	actions.cop+=/devouring_plague,if=buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains&active_enemies>1
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6),cycle_targets=1
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6)&active_enemies>1,cycle_targets=1
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&active_enemies>1
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&talent.mindbender.enabled&!target.dot.devouring_plague_dot.ticking&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6)&active_enemies=1,cycle_targets=1
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&talent.mindbender.enabled&!target.dot.devouring_plague_dot.ticking&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&active_enemies=1
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&talent.surge_of_darkness.enabled&buff.mental_instinct.remains<(gcd*1.4)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6)&active_enemies=1,cycle_targets=1
			//	actions.cop+=/mind_blast,if=mind_harvest=0,cycle_targets=1
			//	actions.cop+=/mind_blast,if=cooldown_react
			//	actions.cop+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
			//	actions.cop+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_targets=1
			//	actions.cop+=/mindbender,if=talent.mindbender.enabled
			//	actions.cop+=/shadowfiend,if=!talent.mindbender.enabled
			//	actions.cop+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
			//	actions.cop+=/cascade,if=talent.cascade.enabled&(active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11
			//	actions.cop+=/divine_star,if=talent.divine_star.enabled&active_enemies>3&target.distance<=24
			//	actions.cop+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&!ticking&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
			//	actions.cop+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
			//	actions.cop+=/divine_star,if=talent.divine_star.enabled&active_enemies=3&target.distance<=24
			//	actions.cop+=/mind_spike,if=active_enemies<=4&buff.surge_of_darkness.react
			//	actions.cop+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (EnemyInRange (40) >= 8) {
				var bestTarget = BestTarget (40, 10, 3);
				if (bestTarget != null) {
					if (MindSear (bestTarget)) {
						Interrupt = "ChainMS";
						return true;
					}
				}
			}
			//	actions.cop+=/mind_spike,if=target.dot.devouring_plague_tick.remains&target.dot.devouring_plague_tick.remains<cast_time
			//	actions.cop+=/mind_flay,if=target.dot.devouring_plague_tick.ticks_remain>1&active_enemies>1,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			//	actions.cop+=/mind_spike
			//	actions.cop+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
			//	actions.cop+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
			//	actions.cop+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
			//	actions.cop+=/halo,moving=1,if=talent.halo.enabled&target.distance<=30
			//	actions.cop+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
			//	actions.cop+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
			//	actions.cop+=/devouring_plague,moving=1

//			var targets = Adds;
//			targets.Add(Target);
//			// actions.cop=devouring_plague,if=shadow_orb=5&primary_target=0&!target.dot.devouring_plague_dot.ticking&target.time_to_die>=(gcd*4*7%6),cycle_targets=1
//			if (Orb == 5) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Devouring Plague", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/devouring_plague,if=shadow_orb=5&!target.dot.devouring_plague_dot.ticking
//			if (Cast("Devouring Plague", () => Orb == 5 && !Target.HasAura("Devouring Plague", true))) return;
//			// actions.cop+=/devouring_plague,if=shadow_orb=5&primary_target=0&target.time_to_die>=(gcd*4*7%6)&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20)),cycle_targets=1
//			if (Orb == 5) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && (Cooldown("Mind Blast") <= 1.5 || Cooldown("Shadow Word: Death") <= 1.5 && u.HealthFraction < 0.2)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/devouring_plague,if=shadow_orb=5&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
//			if (Cast("Devouring Plague", () => Orb == 5 && (Cooldown("Mind Blast") <= 1.5 || (Cooldown("Shadow Word: Death") <= 1.5 && TargetHealth < 0.2)))) return;
//			// actions.cop+=/devouring_plague,if=primary_target=0&buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains,cycle_targets=1
//			if (Me.AuraTimeRemaining("Mental Instinct") < 1.5 && Me.AuraTimeRemaining("Mental Instinct") > (1.5 * 0.7) && Me.HasAura("Mental Instinct", true)) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/devouring_plague,if=buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
//			if (Cast("Devouring Plague", () => Me.AuraTimeRemaining("Mental Instinct") < 1.5 && Me.AuraTimeRemaining("Mental Instinct") > 1.5 * 0.7 && Me.HasAura("Mental Instinct", true))) return;
//			// actions.cop+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6),cycle_targets=1
//			if (Orb >= 3 && !HasSpell(165628) && !HasSpell(165629)) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && (Cooldown("Mind Blast") <= 1.5 || (Cooldown("Shadow Word: Death") <= 1.5 && u.HealthFraction < 0.2))).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
//			if (Cast("Devouring Plague", () => Orb >= 3 && !HasSpell(165628) && !HasSpell(165629) && (Cooldown("Mind Blast") <= 1.5 || (Cooldown("Shadow Word: Death") <= 1.5 && Target.HealthFraction < 0.2)))) return;
//			// actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6),cycle_targets=1
//			if (Orb >= 3 && HasSpell(165628) && !HasSpell(165629)) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && (Cooldown("Mind Blast") <= 2 * 1.5 || (Cooldown("Shadow Word: Death") <= 1.5 && u.HealthFraction < 0.2))).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Devouring Plague", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
//			if (Cast("Devouring Plague", () => Orb >= 3 && HasSpell(165628) && !HasSpell(165629) && (Cooldown("Mind Blast") <= 2 * 1.5 || (Cooldown("Shadow Word: Death") <= 1.5 && Target.HealthFraction < 0.2)))) return;
//			// actions.cop+=/mind_blast,if=mind_harvest=0,cycle_targets=1
//			if (HasGlyph(162532)) {
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && !u.HasAura("Glyph of Mind Blast", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Mind Blast", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/mind_blast,if=cooldown_react
//			if (Cast("Mind Blast", () => Cooldown("Mind Blast") == 0)) return;
//			// actions.cop+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
//			CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2 && !Target.HasAura("Shadow Word: Pain", true) && !Target.HasAura("Vampiric Touch", true)).DefaultIfEmpty(null).FirstOrDefault();
//			if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//			// actions.cop+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_targets=1
//			CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2).DefaultIfEmpty(null).FirstOrDefault();
//			if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//			// actions.cop+=/mindbender,if=talent.mindbender.enabled
//			if (Cast("Mindbender", () => HasSpell("Mindbender"))) return;
//			// actions.cop+=/shadowfiend,if=!talent.mindbender.enabled
//			if (Cast("Shadowfiend", () => !HasSpell("Mindbender"))) return;
//			// actions.cop+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
//			if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30 && Range >= 17)) return;
//			// actions.cop+=/cascade,if=talent.cascade.enabled&(active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11
//			if (Cast("Cascade", () => HasSpell("Cascade") && IsBoss(Target) && ((EnemyInRange(40) > 1 || Range >= 28) && Range <= 40 && Range >= 11))) return;
//			// actions.cop+=/divine_star,if=talent.divine_star.enabled&active_enemies>3&target.distance<=24
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && EnemyInRange(24) > 3 && Range <= 24)) return;
//			// actions.cop+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&!ticking&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
//			if (EnemyInRange(40) <= 5) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining("Shadow Word: Pain", true) < (18 * 0.3)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Pain", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5&primary_target=0,cycle_targets=1,max_cycle_targets=5
//			if (EnemyInRange(40) <= 5) {
//				CycleTarget = Adds.Where(u => u.IsInCombatRangeAndLoS && u != Target && u.AuraTimeRemaining("Vampiric Touch", true) < (15 * 0.3 + 1.5)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Vampiric Touch", CycleTarget, () => CycleTarget != null)) return;
//			}
//			// actions.cop+=/divine_star,if=talent.divine_star.enabled&active_enemies=3&target.distance<=24
//			if (Cast("Divine Star", () => HasSpell("Divine Star") && EnemyInRange(24) == 3 && Range <= 24)) return;
//			// actions.cop+=/mind_spike,if=active_enemies<=4&buff.surge_of_darkness.react
//			if (Cast("Mind Spike", () => EnemyInRange(40) <= 4 && Me.HasAura("Surge of Darkness"))) return;
//			// actions.cop+=/mind_spike,if=target.dot.devouring_plague_tick.remains&target.dot.devouring_plague_tick.remains<cast_time
//			if (Cast("Mind Sear", () => Target.HasAura("Devouring Plague", true) && Target.AuraTimeRemaining("Devouring Plague") < 5)) return;
//			// actions.cop+=/mind_flay,if=target.dot.devouring_plague_tick.ticks_remain>1&active_enemies=1,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (Cast("Mind Flay", () => Target.HasAura("Devouring Plague", true) && EnemyInRange(40) == 1)) {
//				ChainMS = true;
//				return;
//			}
//			// actions.cop+=/mind_spike
//			if (Cast("Mind Spike")) return;
//			if (Me.IsMoving) {
//				// actions.cop+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_targets=1
//				CycleTarget = targets.Where(u => u.IsInCombatRangeAndLoS && u.HealthFraction < 0.2 && !Target.HasAura("Shadow Word: Pain", true) && !Target.HasAura("Vampiric Touch", true)).DefaultIfEmpty(null).FirstOrDefault();
//				if (Cast("Shadow Word: Death", CycleTarget, () => CycleTarget != null)) return;
//				// actions.cop+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
//				if (Cast("Shadow Word: Death", () => TargetHealth < 0.2)) return;
//				// actions.cop+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
//				if (Cast("Power Word: Shield", () => HasSpell("Body and Soul"))) return;
//				// actions.cop+=/halo,moving=1,if=talent.halo.enabled&target.distance<=30
//				if (Cast("Halo", () => HasSpell("Halo") && IsBoss(Target) && Range <= 30)) return;
//				// actions.cop+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
//				if (Cast("Divine Star", () => HasSpell("Divine Star") && Range <= 24)) return;
//				// actions.cop+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
//				if (Cast("Cascade", () => HasSpell("Cascade") && Range <= 40)) return;
//				// actions.cop+=/devouring_plague,moving=1
//				if (Cast("Devouring Plague")) return;
//			}

			return false;
		}
	}
}





//		public override void Combat() {
//			// interrupt_if=cooldown.mind_blast.remains<=0.1
//			if (ChainM) {
//				if (Cooldown("Mind Blast") == 0.1) {
//					ChainM = false;
//					API.ExecuteMacro("/stopcasting");
//				} else {
//					return;
//				}
//			}
//			// interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
//			if (ChainMS) {
//				if (Cooldown("Mind Blast") == 0 || Cooldown("Shadow Word: Death") == 0) {
//					ChainMS = false;
//					API.ExecuteMacro("/stopcasting");
//				} else {
//					return;
//				}
//			}
//			// interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
//			if (ChainMSO) {
//				if (Cooldown("Mind Blast") == 0 || Cooldown("Shadow Word: Death") == 0 || Orb == 5) {
//					ChainMSO = false;
//					API.ExecuteMacro("/stopcasting");
//				} else {
//					return;
//				}
//			}
//			// interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
//			if (ChainMSH) {
//				if (Cooldown("Mind Blast") == 0 || (Cooldown("Shadow Word: Death") == 0 && TargetHealth < 0.2)) {
//					ChainMSH = false;
//					API.ExecuteMacro("/stopcasting");
//				} else {
//					return;
//				}
//			}
//
//			if (ToSkill == 1) {
//				if ((TargetHealth < 0.2 && Cooldown("Shadow Word: Death") == 0))
//					Main();
//				else
//					return;
//			}
//			if (ToSkill == 2) {
//				if (Cooldown("Mind Blast") == 0)
//					Main();
//				else
//					return;
//			}
//
//			var targets = Adds;
//			targets.Add(Target);
//
//			// Heal
//			if (Health < 0.45) {
//				API.UseItem(5512);
//				return;
//			} // 5512 = Healthstone
//			// if (CastSelf("Dispersion", () => Mana < 0.1)) return;
//			// if (Cast("Shadowfiend", () => Health < 0.5 && (IsElite || IsPlayer)))) return;
//

//
//			if (CastSelf("Power Word: Shield", () => Health <= 0.7 && !HasAura("Weakened Soul"))) return;
//			// if (CastSelfPreventDouble("Flash Heal", () => Health <= 0.5 && !Me.IsMoving)) return;
//
//			// if (CastOnTerrain("Halo", Me.Position, () => Health < 0.5 && (IsElite || IsPlayer))) return
//
//			// Interrupt
//			CycleTarget = targets.Where(x => x.IsInLoS && x.CombatRange <= 30 && x.IsCastingAndInterruptible() && x.RemainingCastTime > 0).DefaultIfEmpty(null).FirstOrDefault();
//			if (Cast("Silence", CycleTarget, () => CycleTarget != null)) return;
//
//		}
