using ReBot.API;
using System.Linq;
using Newtonsoft.Json;

namespace ReBot
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
			GroupBuffs = new [] {
				"Power Word: Fortitude"
			};
			PullSpells = new [] {
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

			if (Health (Me) < 0.9) {
				if (FlashHeal (Me))
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

			Spell = "";
			IfInterrupt = "";
			InterruptTarget = null;

			return false;
		}

		public override void Combat ()
		{
			if (ShadowApparitions > 0) {
				API.Print (ShadowApparitions);
			}

			if (Spell.Length == 0) {
				if (HasSpell (Spell) && Cooldown (Spell) != 0)
					return;
				CastSpell (Spell);
				Spell = "";
				return;
			}

			if (Gcd && HasGlobalCooldown ())
				return;
			
			if (Interrupt ())
				return;

			if (Health (Me) < 0.8) {
				if (ShadowHeal ())
					return;
			}

			if (IfInterrupt.Length == 0) {
				if (!CaseInterrupt (InterruptTarget))
					return;
			}

			if (Mana () < 0.1) {
				if (Dispersion ())
					return;
			}

			//	actions=shadowform,if=!buff.shadowform.up
			if (!Me.HasAura ("Shadowform")) {
				if (Shadowform ())
					return;
			}
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
			if (HasSpell ("Clarity of Power") && HasSpell ("Insanity") && Health (Target) > 0.2 && ActiveEnemies (40) <= 6) {
				if (COPDotWeave ())
					return true;
			}
			//	actions.decision+=/call_action_list,name=cop_insanity,if=talent.clarity_of_power.enabled&talent.insanity.enabled
			if (HasSpell ("Clarity of Power") && HasSpell ("Insanity")) {
				if (COPInsanity ())
					return true;
			}

			return false;
		}

		public bool PVPDispersion ()
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
			var Enemy = Adds;
			Enemy.Add (Target);

			//	actions.main=mindbender,if=talent.mindbender.enabled
			if (HasSpell ("Mindbender")) {
				if (Mindbender ())
					return true;
			}
			//	actions.main+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend ())
					return true;
			} 
			//	actions.main+=/shadow_word_death,if=natural_shadow_word_death_range&shadow_orb<=4,cycle_Enemy=1
			if (Orb <= 4) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.main+=/mind_blast,if=glyph.mind_harvest.enabled&shadow_orb<=2&active_enemies<=5&cooldown_react
			if (HasGlyph (162532) && Orb <= 2 && ActiveEnemies (40) <= 5 && Usable ("Mind Blast")) {
				if (MindBlast ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb=5&!target.dot.devouring_plague_dot.ticking&(talent.surge_of_darkness.enabled|set_bonus.tier17_4pc),cycle_Enemy=1
			if (Orb == 5 && (HasSpell ("Surge of Darkness") || HasSpell ("Mental Instinct"))) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Devouring Plague", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
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
			//	actions.main+=/devouring_plague,if=shadow_orb>=4&talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd))&!target.dot.devouring_plague_tick.ticking&talent.surge_of_darkness.enabled,cycle_Enemy=1
			if (Orb >= 4 && HasSpell ("Auspicious Spirits")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (Health (u) < 0.2 && Cooldown ("Shadow Word: Death") < 1.5)) && !u.HasAura ("Devouring Plague", true) && HasSpell ("Surge of Darkness")).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=4&talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(target.health.pct<20&cooldown.shadow_word_death.remains<gcd))
			if (Orb >= 4 && HasSpell ("Auspicious Spirits") && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (Health (Target) < 0.2 && Cooldown ("Shadow Word Death") < 1.5))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=3&!talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd))&!target.dot.devouring_plague_tick.ticking&talent.surge_of_darkness.enabled,cycle_Enemy=1
			if (Orb >= 3 && !HasSpell ("Auspicious Spirits") && HasSpell ("Surge of Darkness")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (Health (u) < 0.2 && Cooldown ("Shadow Word: Death") < 1.5)) && !u.HasAura ("Devouring Plague", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.main+=/devouring_plague,if=shadow_orb>=3&!talent.auspicious_spirits.enabled&((cooldown.mind_blast.remains<gcd&!set_bonus.tier17_2pc)|(target.health.pct<20&cooldown.shadow_word_death.remains<gcd))
			if (Orb >= 3 && !HasSpell ("Auspicious Spirits") && ((Cooldown ("Mind Blast") < 1.5 && !HasSpell ("Mental Instinct")) || (Health (Target) < 0.2 && Cooldown ("Shadow Word Death") < 1.5))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0,cycle_Enemy=1
			if (HasGlyph (162532)) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Glyph of Mind Blast", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (MindBlast (Unit))
						return true;
				}
			}
			//	actions.main+=/mind_blast,if=talent.auspicious_spirits.enabled&active_enemies<=4&cooldown_react
			if (HasSpell ("Auspicious Spirits") && ActiveEnemies (40) <= 4 && Usable ("Mind Blast")) {
				if (MindBlast ())
					return true;
			}
			//	actions.main+=/shadow_word_pain,if=talent.auspicious_spirits.enabled&remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react,cycle_Enemy=1,max_cycle_Enemy=7
			if (HasSpell ("Auspicious Spirits")) {
				MaxCycle = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Shadow Word: Pain", true) < 18 * 0.3);
				if (MaxCycle.ToList ().Count <= 7) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (ShadowWordPain (Unit))
							return true;
					}
				}
			}
			//	actions.main+=/mind_blast,if=cooldown_react
			if (Usable ("Mind Blast")) {
				if (MindBlast ())
					return true;
			}
			//	actions.main+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.main+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.main+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && ActiveEnemies (40) <= 2) {
				if (Insanity ()) {
					IfInterrupt = "ChainMSO";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.main+=/insanity,chain=1,if=active_enemies<=2,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			if (Me.HasAura ("Insanity") && ActiveEnemies (40) <= 2) {
				if (Insanity ()) {
					IfInterrupt = "ChainMSO";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.main+=/halo,if=talent.halo.enabled&target.distance<=30&active_enemies>2
			if (HasSpell ("Halo") && Range (30) && ActiveEnemies (30) > 2) {
				if (Halo ())
					return true;
			}
			//	actions.main+=/cascade,if=talent.cascade.enabled&active_enemies>2&target.distance<=40
			if (HasSpell ("Cascade") && ActiveEnemies (40) > 2 && Range (40)) {
				if (Cascade ())
					return true;
			}
			//	actions.main+=/divine_star,if=talent.divine_star.enabled&active_enemies>4&target.distance<=24
			if (HasSpell ("Divine Star") && ActiveEnemies (24) > 4 && Range (24)) {
				if (DivineStar ())
					return true;
			}
			//	actions.main+=/shadow_word_pain,if=!talent.auspicious_spirits.enabled&remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&active_enemies<=5,cycle_Enemy=1,max_cycle_Enemy=5
			if (!HasSpell ("Auspicious Spirits") && ActiveEnemies (40) <= 5) {
				MaxCycle = Enemy.Where (u => Range (40, u) && u.AuraTimeRemaining ("Shadow Word: Pain", true) < 18 * 0.3 && TimeToDie (u) > (18 * 0.75));
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (ShadowWordPain (Unit))
							return true;
					}
				}
			}
			//	actions.main+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5,cycle_Enemy=1,max_cycle_Enemy=5
			if (ActiveEnemies (40) <= 5) {
				MaxCycle = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Vampiric Touch", true) < (15 * 0.3 + CastTime (34914)) && TimeToDie (u) > (15 * 0.75 + CastTime (34914)));
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (VampiricTouch (Unit))
							return true;
					}
				}
			}
			//	actions.main+=/devouring_plague,if=!talent.void_entropy.enabled&shadow_orb>=3&ticks_remain<=1
			if (!HasSpell ("Void Entropy") && Orb >= 3 && Target.AuraTimeRemaining ("Devouring Plague", true) <= 1) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.main+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react=3
			if (ActiveEnemies (40) <= 5 && AuraStackCount ("Surge of Darkness") == 3) {
				if (MindSpike ())
					return true;
			}
			//	actions.main+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
			if (HasSpell ("Halo") && Range (30, Target, 17)) {
				if (Halo ())
					return true;
			}
			//	actions.main+=/cascade,if=talent.cascade.enabled&(active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11
			if (HasSpell ("Cascade") && (ActiveEnemies (40) > 1 || Range (28, Target, 40)) && Range (40, Target, 11)) {
				if (Cascade ())
					return true;
			}
			//	actions.main+=/divine_star,if=talent.divine_star.enabled&(active_enemies>1&target.distance<=24)
			if (HasSpell ("Divine Star") && ActiveEnemies (24) > 1 && Range (24)) {
				if (Halo ())
					return true;
			}
			//	actions.main+=/wait,sec=cooldown.shadow_word_death.remains,if=natural_shadow_word_death_range&cooldown.shadow_word_death.remains<0.5&active_enemies<=1,cycle_Enemy=1
			if (Health (Target) <= 0.2 && Cooldown ("Shadow Word: Death") < 0.5 && ActiveEnemies (40) <= 1) {
				Spell = "Shadow Word: Death";
				return true;
			}
			//	actions.main+=/wait,sec=cooldown.mind_blast.remains,if=cooldown.mind_blast.remains<0.5&cooldown.mind_blast.remains&active_enemies<=1
			if (Cooldown ("Mind Blast") < 0.5 && Cooldown ("Mind Blast") > 0 && ActiveEnemies (40) <= 1) {
				Spell = "Mind Blast";
				return true;
			}
			//	actions.main+=/mind_spike,if=buff.surge_of_darkness.react&active_enemies<=5
			if (Me.HasAura ("Surge of Darkness") && ActiveEnemies (40) <= 5) {
				if (MindSpike ())
					return true;
			}
			//	actions.main+=/divine_star,if=talent.divine_star.enabled&target.distance<=28&active_enemies>1
			if (HasSpell ("Divine Star") && Range (28) && ActiveEnemies (24) > 1) {
				if (DivineStar ())
					return true;
			}
			//	actions.main+=/mind_sear,chain=1,if=active_enemies>=4,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			if (ActiveEnemies (40) >= 4) {
				Unit = BestAOETarget (40, 10, 4);
				if (Unit != null) {
					if (MindSear (Unit)) {
						IfInterrupt = "ChainMSO";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.main+=/shadow_word_pain,if=shadow_orb>=2&ticks_remain<=3&target.time_to_die>(18*0.75)&talent.insanity.enabled
			if (Orb >= 2 && Target.AuraTimeRemaining ("Shadow Word: Pain", true) <= 3 && TimeToDie () > (18 * 0.75) && HasSpell ("Insanity")) {
				if (ShadowWordPain ())
					return true;
			}
			//	actions.main+=/vampiric_touch,if=shadow_orb>=2&ticks_remain<=3.5&target.time_to_die>(15*0.75+cast_time)&talent.insanity.enabled
			if (Orb >= 2 && Target.AuraTimeRemaining ("Vampiric Touch", true) <= 3.5 && TimeToDie () > (15 * 0.75 + CastTime (34914)) && HasSpell ("Insanity")) {
				if (VampiricTouch ())
					return true;
			}
			//	actions.main+=/mind_flay,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1|shadow_orb=5)
			if (MindFlay ()) {
				IfInterrupt = "ChainMSO";
				InterruptTarget = Target;
				return true;
			}
			if (Me.IsMoving) {
				//	actions.main+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
				//	actions.main+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
				if (HasSpell ("Body and Soul")) {
					if (PowerWordShield (Me))
						return true;
				}
				//	actions.main+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
				if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Range (30)) {
					if (Halo ())
						return true;
				}
				//	actions.main+=/divine_star,moving=1,if=talent.divine_star.enabled&target.distance<=28
				if (HasSpell ("Divine Star") && (IsBoss () || IsPlayer ()) && Range (24)) {
					if (DivineStar ())
						return true;
				}
				//	actions.main+=/cascade,moving=1,if=talent.cascade.enabled&target.distance<=40
				if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && Range (40)) {
					if (Cascade ())
						return true;
				}
				//	actions.main+=/shadow_word_pain,moving=1,cycle_Enemy=1
				if (Usable ("Shadow Word: Pain")) {
					Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Shadow Word: Pain", true)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (ShadowWordPain (Unit))
							return true;
					}
				}
			}

			return false;
		}

		public bool Vent ()
		{
			var Enemy = Adds;
			Enemy.Add (Target);
			//	actions.vent=void_entropy,if=shadow_orb=3&!ticking&target.time_to_die>60&active_enemies=1
			if (Orb == 3 && !Target.HasAura ("Void Entropy", true) && TimeToDie () > 60 && ActiveEnemies (40) == 1) {
				if (VoidEntropy ())
					return true;
			}
			//	actions.vent+=/void_entropy,if=!dot.void_entropy.ticking&shadow_orb=5&active_enemies>=1&target.time_to_die>60,cycle_Enemy=1,max_cycle_Enemy=6
			if (Orb == 5 && ActiveEnemies (40) >= 1 && Usable ("Void Entropy")) {
				MaxCycle = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Void Entropy", true) && TimeToDie (u) > 60);
				if (MaxCycle.ToList ().Count <= 6) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (VoidEntropy ())
							return true;
					}
				}
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.ticking&dot.void_entropy.remains<=gcd*2&cooldown_react&active_enemies=1
			if (Orb == 5 && Target.HasAura ("Void Entropy", true) && Target.AuraTimeRemaining ("Void Entropy", true) <= 1.5 * 2 && Usable ("Devouring Plague") && ActiveEnemies (40) == 1) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.vent+=/devouring_plague,if=dot.void_entropy.ticking&dot.void_entropy.remains<=gcd*2&cooldown_react&active_enemies>1,cycle_Enemy=1
			if (ActiveEnemies (40) > 1 && Usable ("Devouring Plague") && Orb >= 3) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.HasAura ("Void Entropy", true) && u.AuraTimeRemaining ("Void Entropy", true) <= 1.5 * 2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague ())
						return true;
				}
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<5&active_enemies>1,cycle_Enemy=1
			if (Orb == 5 && ActiveEnemies (40) > 1 && Usable ("Devouring Plague")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Void Entropy", true) < 5).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague ())
						return true;
				}
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<10&active_enemies>2,cycle_Enemy=1
			if (Orb == 5 && ActiveEnemies (40) > 2 && Usable ("Devouring Plague")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Void Entropy", true) < 10).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague ())
						return true;
				}
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<15&active_enemies>3,cycle_Enemy=1
			if (Orb == 5 && ActiveEnemies (40) > 3 && Usable ("Devouring Plague")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Void Entropy", true) < 15).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague ())
						return true;
				}
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains<20&active_enemies>4,cycle_Enemy=1
			if (Orb == 5 && ActiveEnemies (40) > 4 && Usable ("Devouring Plague")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Void Entropy", true) < 20).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague ())
						return true;
				}
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains&(cooldown.mind_blast.remains<=gcd*2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<=gcd*2))&active_enemies=1
			if (Orb == 5 && Target.HasAura ("Void Entropy", true) && (Cooldown ("Mind Blast") <= 1.5 * 2 || (Health (Target) < 0.2 && Cooldown ("Shadow Word: Death") <= 1.5 * 2)) && ActiveEnemies (40) == 1) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb=5&dot.void_entropy.remains&(cooldown.mind_blast.remains<=gcd*2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<=gcd*2))&active_enemies>1,cycle_Enemy=1
			if (Orb == 5 && ActiveEnemies (40) > 1 && Usable ("Devouring Plague")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.HasAura ("Void Entropy", true) && (Cooldown ("Mind Blast") <= 1.5 * 2 || (Health (u) < 0.2 && Cooldown ("Shadow Word: Death") <= 1.5 * 2))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague ())
						return true;
				}
			}
			//	actions.vent+=/devouring_plague,if=shadow_orb>=3&dot.void_entropy.ticking&active_enemies=1&buff.mental_instinct.remains<(gcd*1.4)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
			if (Orb >= 3 && Target.HasAura ("Void Entropy", true) && ActiveEnemies (40) == 1 && Me.AuraTimeRemaining ("Mental Instinct") < (1.5 * 1.4) && Me.AuraTimeRemaining ("Mental Instinct") > (1.5 * 0.7) && Me.HasAura ("Mental Instinct")) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.vent+=/mindbender,if=talent.mindbender.enabled&cooldown.mind_blast.remains>=gcd
			if (HasSpell ("Mindbender") && Cooldown ("Mind Blast") >= 1.5) {
				if (Mindbender ())
					return true;
			}
			//	actions.vent+=/shadowfiend,if=!talent.mindbender.enabled&cooldown.mind_blast.remains>=gcd
			if (!HasSpell ("Mindbender") && Cooldown ("Mind Blast") >= 1.5) {
				if (Shadowfiend ())
					return true;
			}
			//	actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30&active_enemies>=4
			if (HasSpell ("Halo") && Range (30) && ActiveEnemies (30) >= 4) {
				if (Halo ())
					return true;
			}
			//	actions.vent+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb<=2,cycle_Enemy=1
			if (HasGlyph (162532) && Orb <= 2 && Usable ("Mind Blast")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Glyph of Mind Blast", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (MindBlast (Unit))
						return true;
				}
			}
			//	actions.vent+=/devouring_plague,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb>=3,cycle_Enemy=1
			if (HasGlyph (162532) && Orb >= 3 && Usable ("Devouring Plague")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Glyph of Mind Blast", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.vent+=/mind_blast,if=active_enemies<=10&cooldown_react&shadow_orb<=4
			if (ActiveEnemies (40) <= 10 && Usable ("Mind Blast") && Orb <= 4) {
				if (MindBlast ())
					return true;
			}
			//	actions.vent+=/shadow_word_death,if=natural_shadow_word_death_range&cooldown_react&shadow_orb<=4,cycle_Enemy=1
			if (Usable ("Shadow Word: Death") && Orb <= 4) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.vent+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.vent+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.vent+=/shadow_word_pain,if=shadow_orb=4&remains<(18*0.50)&set_bonus.tier17_2pc&cooldown.mind_blast.remains<1.2*gcd&cooldown.mind_blast.remains>0.2*gcd
			if (Orb == 4 && Target.AuraTimeRemaining ("Shadow Word: Pain", true) < (18 * 0.5) && HasSpell (165628) && Cooldown ("Mind Blast") < (1.2 * 1.5) && Cooldown ("Mind Blast") > 0.2 * 1.5) {
				if (ShadowWordPain ())
					return true;
			}
			//	actions.vent+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < (0.5 * 1.5) && ActiveEnemies (40) <= 3 && Cooldown ("Mind Blast") > (0.5 * 1.5)) {
				if (Insanity ()) {
					IfInterrupt = "ChainMS";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.vent+=/insanity,chain=1,if=active_enemies<=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && ActiveEnemies (40) <= 3 && Cooldown ("Mind Blast") > (0.5 * 1.5)) {
				if (Insanity ()) {
					IfInterrupt = "ChainMS";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.vent+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react=3
			if (ActiveEnemies (40) <= 5 && AuraStackCount ("Surge of Darkness") == 3) {
				if (MindSpike ())
					return true;
			}
			//	actions.vent+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react,cycle_Enemy=1,max_cycle_Enemy=5
			if (Usable ("Shadow Word: Pain")) {
				MaxCycle = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Shadow Word: Pain", true) <= (18 * 0.3) && TimeToDie (u) > (18 * 0.75));
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (ShadowWordPain (Unit))
							return true;
					}
				}
			}
			//	actions.vent+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react,cycle_Enemy=1,max_cycle_Enemy=5
			if (Usable ("Vampiric Touch")) {
				MaxCycle = Enemy.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Vampiric Touch", true) < (15 * 0.3 + CastTime (34914)) && TimeToDie (u) > (15 * 0.75 + CastTime (34914)));
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (VampiricTouch (Unit))
							return true;
					} 
				}
			}
			//	actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30&cooldown.mind_blast.remains>0.5*gcd
			if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Range (30) && Cooldown ("Mind Blast") > (0.5 * 1.5)) {
				if (Halo ())
					return true;
			}
			//	actions.vent+=/cascade,if=talent.cascade.enabled&target.distance<=40&cooldown.mind_blast.remains>0.5*gcd
			if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && Range (40) && Cooldown ("Mind Blast") > (0.5 * 1.5)) {
				if (Cascade ())
					return true;
			}
			//	actions.vent+=/divine_star,if=talent.divine_star.enabled&active_enemies>4&target.distance<=24&cooldown.mind_blast.remains>0.5*gcd
			if (HasSpell ("Divine Star") && ActiveEnemies (24) > 4 && Range (24) && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				if (DivineStar ())
					return true;
			}
			//	actions.vent+=/mind_spike,if=active_enemies<=5&buff.surge_of_darkness.react&cooldown.mind_blast.remains>0.5*gcd
			if (ActiveEnemies (40) <= 5 && Me.HasAura ("Surge of Darkness") && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				if (MindSpike ())
					return true;
			}
			//	actions.vent+=/mind_sear,chain=1,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5 && Usable ("Mind Sear")) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (MindSear (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.vent+=/mind_flay,if=cooldown.mind_blast.remains>0.5*gcd,interrupt=1,chain=1
			if (Cooldown ("Mind Blast") > 0.5 * 1.5) {
				if (MindFlay ()) {
					IfInterrupt = "ChainM";
					InterruptTarget = Target;
					return true;
				}
			}
			if (Me.IsMoving) {
				//	actions.vent+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
				//	actions.vent+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
				if (HasSpell ("Body and Soul")) {
					if (PowerWordShield (Me))
						return true;
				}
				//	actions.vent+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
				if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Range (30)) {
					if (Halo ())
						return true;
				}
				//	actions.vent+=/divine_star,moving=1,if=talent.divine_star.enabled&target.distance<=28
				if (HasSpell ("Divine Star") && (IsBoss () || IsPlayer ()) && Range (24)) {
					if (DivineStar ())
						return true;
				}
				//	actions.vent+=/cascade,moving=1,if=talent.cascade.enabled&target.distance<=40
				if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && Range (40)) {
					if (Cascade ())
						return true;
				}
				//	actions.vent+=/shadow_word_pain,moving=1,cycle_Enemy=1
				if (Usable ("Shadow Word: Pain")) {
					Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Shadow Word: Pain", true)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (ShadowWordPain (Unit))
							return true;
					}
				}
			}

			return false;
		}

		public bool COPDotWeave ()
		{
			var Enemy = Adds;
			Enemy.Add (Target);

			//	actions.cop_dotweave=devouring_plague,if=target.dot.vampiric_touch.ticking&target.dot.shadow_word_pain.ticking&shadow_orb=5&cooldown_react
			if (Target.HasAura ("Vampiric Touch", true) && Target.HasAura ("Shadow Word: Pain", true) && Orb == 5 && Usable ("Devouring Plague")) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop_dotweave+=/devouring_plague,if=buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
			if (Me.AuraTimeRemaining ("Mental Instinct") < 1.5 && Me.AuraTimeRemaining ("Mental Instinct") > (1.5 * 0.7) && Me.HasAura ("Mental Instinct", true)) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop_dotweave+=/devouring_plague,if=(target.dot.vampiric_touch.ticking&target.dot.shadow_word_pain.ticking&!buff.insanity.remains&cooldown.mind_blast.remains>0.4*gcd)
			if ((Target.HasAura ("Vampiric Touch", true) && Target.HasAura ("Shadow Word: Pain", true) && !Me.HasAura ("Insanity") && Cooldown ("Mind Blast") > 0.4 * 1.5)) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop_dotweave+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2 && !Target.HasAura ("Shadow Word: Pain", true) && !Target.HasAura ("Vampiric Touch", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.cop_dotweave+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.cop_dotweave+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0&shadow_orb<=2,cycle_Enemy=1
			if (HasGlyph (162532) && Orb <= 2 && Usable ("Mind Blast")) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Glyph of Mind Blast", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (MindBlast (Unit))
						return true;
				}
			}
			//	actions.cop_dotweave+=/mind_blast,if=shadow_orb<=4&cooldown_react
			if (Orb <= 4 && Usable ("Mind Blast")) {
				if (MindBlast ())
					return true;
			}
			//	actions.cop_dotweave+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.cop_dotweave+=/searing_insanity,if=active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.cop_dotweave+=/shadowfiend,if=!talent.mindbender.enabled&!buff.insanity.remains
			if (!HasSpell ("Mindbender") && !Me.HasAura ("Insanity")) {
				if (Shadowfiend ())
					return true;
			}
			//	actions.cop_dotweave+=/mindbender,if=talent.mindbender.enabled&!buff.insanity.remains
			if (HasSpell ("Mindbender") && !Me.HasAura ("Insanity")) {
				if (Mindbender ())
					return true;
			}
			//	actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb=4&set_bonus.tier17_2pc&!target.dot.shadow_word_pain.ticking&!target.dot.devouring_plague.ticking&cooldown.mind_blast.remains<gcd&cooldown.mind_blast.remains>0
			if (Orb == 4 && HasSpell (165628) && !Target.HasAura ("Shadow Word: Pain", true) && !Target.HasAura ("Devouring Plague", true) && Cooldown ("Mind Blast") < 1.5 && Cooldown ("Mind Blast") > 0) {
				if (ShadowWordPain ())
					return true;
			}
			//	actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb=5&!target.dot.devouring_plague.ticking&!target.dot.shadow_word_pain.ticking
			if (Orb == 5 && !Target.HasAura ("Devouring Plague", true) && !Target.HasAura ("Shadow Word: Pain", true)) {
				if (ShadowWordPain ())
					return true;
			}
			//	actions.cop_dotweave+=/vampiric_touch,if=shadow_orb=5&!target.dot.devouring_plague.ticking&!target.dot.vampiric_touch.ticking
			if (Orb == 5 && !Target.HasAura ("Devouring Plague", true) && !Target.HasAura ("Vampiric Touch", true)) {
				if (VampiricTouch ())
					return true;
			}
			//	actions.cop_dotweave+=/insanity,if=buff.insanity.remains,chain=1,interrupt_if=cooldown.mind_blast.remains<=0.1
			if (Me.HasAura ("Insanity")) {
				if (Insanity ()) {
					IfInterrupt = "ChainM";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.cop_dotweave+=/shadow_word_pain,if=shadow_orb>=2&target.dot.shadow_word_pain.remains>=6&cooldown.mind_blast.remains>0.5*gcd&target.dot.vampiric_touch.remains&buff.bloodlust.up&!set_bonus.tier17_2pc
			if (Orb >= 2 && Target.AuraTimeRemaining ("Shadow Word: Pain", true) >= 6 && Cooldown ("Mind Blast") > 0.5 * 1.5 && Target.HasAura ("Vampiric Touch", true) && Me.HasAura ("Bloodlust") && !HasSpell (165628)) {
				if (ShadowWordPain ())
					return true;
			}
			//	actions.cop_dotweave+=/vampiric_touch,if=shadow_orb>=2&target.dot.vampiric_touch.remains>=5&cooldown.mind_blast.remains>0.5*gcd&buff.bloodlust.up&!set_bonus.tier17_2pc
			if (Orb >= 2 && Target.AuraTimeRemaining ("Vampiric Touch", true) >= 5 && Cooldown ("Mind Blast") > 0.5 * 1.5 && Me.HasAura ("Bloodlust") && !HasSpell (165628)) {
				if (VampiricTouch ())
					return true;
			}
			//	actions.cop_dotweave+=/halo,if=cooldown.mind_blast.remains>0.5*gcd&talent.halo.enabled&target.distance<=30&target.distance>=17
			if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Cooldown ("Mind Blast") > 0.5 * 1.5 && Range (30, Target, 17)) {
				if (Halo ())
					return true;
			}
			//	actions.cop_dotweave+=/cascade,if=cooldown.mind_blast.remains>0.5*gcd&talent.cascade.enabled&((active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11)
			if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && Cooldown ("Mind Blast") > 0.5 * 1.5 && ((ActiveEnemies (40) > 1 || Range (40, Target, 28)) && Range (40, Target, 11))) {
				if (Cascade ())
					return true;
			}
			//	actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&cooldown.mind_blast.remains>0.5*gcd&active_enemies>3&target.distance<=24
			if (HasSpell ("Divine Star") && Cooldown ("Mind Blast") > 0.5 * 1.5 && ActiveEnemies (24) > 3 && Range (24)) {
				if (DivineStar ())
					return true;
			}
			//	actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0&!ticking,cycle_Enemy=1,max_cycle_Enemy=5
			if (Usable ("Shadow Word: Pain")) {
				MaxCycle = Adds.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Shadow Word: Pain", true) && u != Target);
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (ShadowWordPain (Unit))
							return true;
					}
				}
			}
			//	actions.cop_dotweave+=/vampiric_touch,if=primary_target=0&!ticking,cycle_Enemy=1,max_cycle_Enemy=5
			if (Usable ("Vampiric Touch")) {
				MaxCycle = Adds.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Vampiric Touch", true) && u != Target);
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (VampiricTouch (Unit))
							return true;
					}
				}
			}
			//	actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&cooldown.mind_blast.remains>0.5*gcd&active_enemies=3&target.distance<=24
			if (HasSpell ("Divine Star") && Cooldown ("Mind Blast") > 0.5 * 1.5 && ActiveEnemies (24) == 3 && Range (24)) {
				if (DivineStar ())
					return true;
			}
			//	actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0&(!ticking|remains<=18*0.3)&target.time_to_die>(18*0.75),cycle_Enemy=1,max_cycle_Enemy=5
			if (Usable ("Shadow Word: Pain")) {
				MaxCycle = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && (!u.HasAura ("Shadow Word: Pain", true) || u.AuraTimeRemaining ("Shadow Word: Pain", true) <= 18 * 0.3) && TimeToDie (u) > (18 * 0.75));
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && ShadowWordPain (Unit))
						return true;
				}
			}
			//	actions.cop_dotweave+=/vampiric_touch,if=primary_target=0&(!ticking|remains<=15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time),cycle_Enemy=1,max_cycle_Enemy=5
			if (Usable ("Vampiric Touch")) {			
				MaxCycle = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && (!u.HasAura ("Vampiric Touch", true) || u.AuraTimeRemaining ("Vampiric Touch", true) <= 15 * 0.3 + CastTime (34914)) && TimeToDie (u) > (15 * 0.75 + CastTime (34914)));
				if (MaxCycle.ToList ().Count <= 5) {
					Unit = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && VampiricTouch (Unit))
						return true;
				}
			}
			//	actions.cop_dotweave+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Usable ("Mind Sear") && ActiveEnemies (40) >= 8) {
				Unit = BestAOETarget (40, 10, 8);
				if (Unit != null) {
					if (MindSear (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.cop_dotweave+=/mind_spike
			if (MindSpike ())
				return true;
			//	actions.cop_dotweave+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2 && !Target.HasAura ("Shadow Word: Pain", true) && !Target.HasAura ("Vampiric Touch", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}

			if (Me.IsMoving) {
				//	actions.cop_dotweave+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
				//	actions.cop_dotweave+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
				if (HasSpell ("Body and Soul")) {
					if (PowerWordShield (Me))
						return true;
				}
				//	actions.cop_dotweave+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
				if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Range (30)) {
					if (Halo ())
						return true;
				}
				//	actions.cop_dotweave+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
				if (HasSpell ("Divine Star") && (IsBoss () || IsPlayer ()) && Range (24)) {
					if (DivineStar ())
						return true;
				}
				//	actions.cop_dotweave+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
				if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && Range (40)) {
					if (Cascade ())
						return true;
				}
				//	actions.cop_dotweave+=/devouring_plague,moving=1
				if (DevouringPlague ())
					return true;
				//	actions.cop_dotweave+=/shadow_word_pain,if=primary_target=0,moving=1,cycle_Enemy=1
				if (Usable ("Shadow Word: Pain")) {
					Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && !u.HasAura ("Shadow Word: Pain", true)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && ShadowWordPain (Unit))
						return true;
				}
			}
			return false;
		}

		public bool COPInsanity ()
		{
			//	actions.cop_insanity=devouring_plague,if=shadow_orb=5|(active_enemies>=5&!buff.insanity.remains)
			if (Orb == 5 || (ActiveEnemies (40) >= 5 && !Me.HasAura ("Insanity"))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop_insanity+=/devouring_plague,if=buff.mental_instinct.remains<(gcd*1.7)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains
			if (Me.AuraTimeRemaining ("Mental Instinct") < (1.5 * 1.7) && Me.AuraTimeRemaining ("Mental Instinct") > (1.5 * 0.7) && Me.HasAura ("Mental Instinct")) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop_insanity+=/mind_blast,if=glyph.mind_harvest.enabled&mind_harvest=0,cycle_Enemy=1
			if (HasGlyph (162532)) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Glyph of Mind Blast", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (MindBlast (Unit))
						return true;
				}
			}
			//	actions.cop_insanity+=/mind_blast,if=active_enemies<=5&cooldown_react
			if (ActiveEnemies (40) <= 5 && Usable ("Mind Blast")) {
				if (MindBlast ())
					return true;
			}
			//	actions.cop_insanity+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2 && !Target.HasAura ("Shadow Word: Pain", true) && !Target.HasAura ("Vampiric Touch", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.cop_insanity+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.cop_insanity+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<gcd|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd)),cycle_Enemy=1
			if (Orb >= 3 && !HasSpell (165628) && !HasSpell (165629)) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && (Cooldown ("Mind Blast") < 1.5 || (Health (u) < 0.2 && Cooldown ("Shadow Word: Death") < 1.5))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop_insanity+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=2|(natural_shadow_word_death_range&cooldown.shadow_word_death.remains<gcd)),cycle_Enemy=1
			if (Orb >= 3 && HasSpell (165628) && !HasSpell (165629)) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && (Cooldown ("Mind Blast") <= 2 || (Health (u) < 0.2 && Cooldown ("Shadow Word: Death") < 1.5))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop_insanity+=/searing_insanity,if=buff.insanity.remains<0.5*gcd&active_enemies>=3&cooldown.mind_blast.remains>0.5*gcd,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && ActiveEnemies (40) >= 3 && Cooldown ("Mind Blast") > 0.5 * 1.5) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.cop_insanity+=/searing_insanity,if=active_enemies>=5,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Usable ("Mind Sear") && Me.HasAura ("Insanity") && ActiveEnemies (40) >= 5) {
				Unit = BestAOETarget (40, 10, 5);
				if (Unit != null) {
					if (SearingInsanity (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.cop_insanity+=/mindbender,if=talent.mindbender.enabled
			if (HasSpell ("Mindbender")) {
				if (Mindbender ())
					return true;
			}
			//	actions.cop_insanity+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend ())
					return true;
			} 
			//	actions.cop_insanity+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&active_enemies<=5&primary_target=0,cycle_Enemy=1,max_cycle_Enemy=5
			if (ActiveEnemies (40) <= 5) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u.AuraTimeRemaining ("Shadow Word: Pain", true) < (18 * 0.3) && TimeToDie (u) > (18 * 0.75)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordPain (Unit))
						return true;
				}
			}
			//	actions.cop_insanity+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5&primary_target=0,cycle_Enemy=1,max_cycle_Enemy=5
			if (ActiveEnemies (40) <= 5) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && u.AuraTimeRemaining ("Vampiric Touch", true) < (15 * 0.3 + CastTime (34914)) && TimeToDie (u) > (15 * 0.75 + CastTime (34914))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (VampiricTouch (Unit))
						return true;
				}
			}
			//	actions.cop_insanity+=/insanity,if=buff.insanity.remains<0.5*gcd&active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
			if (Me.HasAura ("Insanity") && Me.AuraTimeRemaining ("Insanity") < 0.5 * 1.5 && ActiveEnemies (40) <= 2) {
				if (Insanity ()) {
					IfInterrupt = "ChainMSH";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.cop_insanity+=/insanity,if=active_enemies<=2,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|(cooldown.shadow_word_death.remains<=0.1&target.health.pct<20))
			if (Me.HasAura ("Insanity") && ActiveEnemies (40) <= 2) {
				if (Insanity ()) {
					IfInterrupt = "ChainMSH";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.cop_insanity+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
			if (HasSpell ("Halo") && Range (30, Target, 17)) {
				if (Halo ())
					return true;
			}
			//	actions.cop_insanity+=/cascade,if=talent.cascade.enabled&((active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11)
			if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && ((ActiveEnemies (40) > 1 || Range (40, Target, 28)) && Range (40, Target, 11))) {
				if (Cascade ())
					return true;
			}
			//	actions.cop_insanity+=/divine_star,if=talent.divine_star.enabled&active_enemies>2&target.distance<=24
			if (HasSpell ("Divine Star") && (IsBoss () || IsPlayer ()) && ActiveEnemies (24) > 2 && Range (24)) {
				if (DivineStar ())
					return true;
			}
			//	actions.cop_insanity+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Usable ("Mind Sear") && ActiveEnemies (40) >= 8) {
				Unit = BestAOETarget (40, 10, 8);
				if (Unit != null) {
					if (MindSear (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.cop_insanity+=/mind_spike
			if (MindSpike ())
				return true;
			//	actions.cop_insanity+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2 && !Target.HasAura ("Shadow Word: Pain", true) && !Target.HasAura ("Vampiric Touch", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}

			if (Me.IsMoving) {
				//	actions.cop_insanity+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
				//	actions.cop_insanity+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
				if (HasSpell ("Body and Soul")) {
					if (PowerWordShield (Me))
						return true;
				}
				//	actions.cop_insanity+=/halo,if=talent.halo.enabled&target.distance<=30,moving=1
				if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Range (30)) {
					if (Halo ())
						return true;
				}
				//	actions.cop_insanity+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
				if (HasSpell ("Divine Star") && (IsBoss () || IsPlayer ()) && Range (24)) {
					if (DivineStar ())
						return true;
				}
				//	actions.cop_insanity+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
				if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && Range (40)) {
					if (Cascade ())
						return true;
				}
				//	actions.cop_insanity+=/devouring_plague,moving=1
				if (DevouringPlague ())
					return true;
				//	actions.cop_insanity+=/shadow_word_pain,if=primary_target=0,moving=1,cycle_Enemy=1
				if (Usable ("Shadow Word: Pain")) {
					Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && !u.HasAura ("Shadow Word: Pain", true)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null) {
						if (ShadowWordPain (Unit))
							return true;
					}
				}
			}

			return false;
		}

		public bool COP ()
		{
			var Enemy = Adds;
			Enemy.Add (Target);

			//	actions.cop=devouring_plague,if=shadow_orb=5&primary_target=0&!target.dot.devouring_plague_dot.ticking&target.time_to_die>=(gcd*4*7%6),cycle_Enemy=1
			if (Orb == 5) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && !u.HasAura ("Devouring Plague", true) && TimeToDie (u) >= (1.5 * 4 * 7 / 6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb=5&primary_target=0&target.time_to_die>=(gcd*4*7%6)&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20)),cycle_Enemy=1
			if (Orb == 5) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && TimeToDie (u) >= (1.5 * 4 * 7 / 6) && (Cooldown ("Mind Blast") <= 1.5 || Cooldown ("Shadow Word: Death") <= 1.5 && Health (u) < 0.2)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb=5&!set_bonus.tier17_2pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
			if (Orb == 5 && !HasSpell (165628) && (Cooldown ("Mind Blast") <= 1.5 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (Target) < 0.2))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb=5&set_bonus.tier17_2pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
			if (Orb == 5 && HasSpell (165628) && (Cooldown ("Mind Blast") <= 1.5 * 2 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (Target) < 0.2))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop+=/devouring_plague,if=primary_target=0&buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains&active_enemies>1,cycle_Enemy=1
			if (Me.AuraTimeRemaining ("Mental Instinct") < 1.5 && Me.AuraTimeRemaining ("Mental Instinct") > (1.5 * 0.7) && Me.HasAura ("Mental Instinct") && ActiveEnemies (40) > 1) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop+=/devouring_plague,if=buff.mental_instinct.remains<gcd&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains&active_enemies>1
			if (Me.AuraTimeRemaining ("Mental Instinct") < 1.5 && Me.AuraTimeRemaining ("Mental Instinct") > (1.5 * 0.7) && Me.HasAura ("Mental Instinct", true) && ActiveEnemies (40) > 1) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6),cycle_Enemy=1
			if (Orb >= 3 && !HasSpell (165628) && !HasSpell (165629)) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && (Cooldown ("Mind Blast") <= 1.5 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (u) < 0.2))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&!set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))
			if (Orb >= 3 && !HasSpell (165628) && !HasSpell (165629) && (Cooldown ("Mind Blast") <= 1.5 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (Target) < 0.2))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6)&active_enemies>1,cycle_Enemy=1
			if (Orb >= 3 && HasSpell (165628) && !HasSpell (165629) && ActiveEnemies (40) > 1) {
				Unit = Adds.Where (u => u != Target && Cooldown ("Mind Blast") <= 1.5 * 2 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (u) < 0.2) && TimeToDie (u) >= (1.5 * 4 * 7 / 6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&!set_bonus.tier17_4pc&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&active_enemies>1
			if (Orb >= 3 && HasSpell (165628) && !HasSpell (165629) && (Cooldown ("Mind Blast") <= 1.5 * 2 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (Target) < 0.2)) && ActiveEnemies (40) > 1) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&talent.mindbender.enabled&!target.dot.devouring_plague_dot.ticking&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6)&active_enemies=1,cycle_Enemy=1
			if (Orb >= 3 && HasSpell (165628) && HasSpell ("Mindbender") && ActiveEnemies (40) == 2) {
				Unit = Adds.Where (u => u != Target && !u.HasAura ("Devouring Plague") && (Cooldown ("Mind Blast") <= 1.5 * 2 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (u) < 0.2)) && TimeToDie (u) >= (1.5 * 4 * 7 / 6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&talent.mindbender.enabled&!target.dot.devouring_plague_dot.ticking&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&active_enemies=1
			if (Orb >= 3 && HasSpell (165628) && HasSpell ("Mindbender") && !Target.HasAura ("Devouring Plague") && (Cooldown ("Mind Blast") <= 1.5 * 2 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (Target) < 0.2))) {
				if (DevouringPlague ())
					return true;
			}
			//	actions.cop+=/devouring_plague,if=shadow_orb>=3&set_bonus.tier17_2pc&talent.surge_of_darkness.enabled&buff.mental_instinct.remains<(gcd*1.4)&buff.mental_instinct.remains>(gcd*0.7)&buff.mental_instinct.remains&(cooldown.mind_blast.remains<=gcd*2|(cooldown.shadow_word_death.remains<=gcd&target.health.pct<20))&primary_target=0&target.time_to_die>=(gcd*4*7%6)&active_enemies=1,cycle_Enemy=1
			if (Orb >= 3 && HasSpell (165628) && HasSpell ("Surge of Darkness") && ActiveEnemies (40) == 2 && Me.AuraTimeRemaining ("Mental Instinct") < (1.5 * 1.4) && Me.AuraTimeRemaining ("Mental Instinct") > (1.5 * 0.7) && Me.HasAura ("Mental Instinct")) {
				Unit = Adds.Where (u => u != Target && (Cooldown ("Mind Blast") <= 1.5 * 2 || (Cooldown ("Shadow Word: Death") <= 1.5 && Health (u) < 0.2)) && TimeToDie (u) >= (1.5 * 4 * 7 / 6)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (DevouringPlague (Unit))
						return true;
				}
			}
			//	actions.cop+=/mind_blast,if=mind_harvest=0,cycle_Enemy=1
			if (HasGlyph (162532)) {
				Unit = Enemy.Where (u => u.IsInCombatRangeAndLoS && !u.HasAura ("Glyph of Mind Blast", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (MindBlast (Unit))
						return true;
				}
			}
			//	actions.cop+=/mind_blast,if=cooldown_react
			if (Cooldown ("Mind Blast") == 0) {
				if (MindBlast ())
					return true;
			}
			//	actions.cop+=/shadow_word_death,if=natural_shadow_word_death_range&!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2 && !Target.HasAura ("Shadow Word: Pain", true) && !Target.HasAura ("Vampiric Touch", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.cop+=/shadow_word_death,if=natural_shadow_word_death_range,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null) {
					if (ShadowWordDeath (Unit))
						return true;
				}
			}
			//	actions.cop+=/mindbender,if=talent.mindbender.enabled
			if (HasSpell ("Mindbender")) {
				if (Mindbender ())
					return true;
			}
			//	actions.cop+=/shadowfiend,if=!talent.mindbender.enabled
			if (!HasSpell ("Mindbender")) {
				if (Shadowfiend ())
					return true;
			}
			//	actions.cop+=/halo,if=talent.halo.enabled&target.distance<=30&target.distance>=17
			if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Range (30, Target, 17)) {
				if (Halo ())
					return true;
			}
			//	actions.cop+=/cascade,if=talent.cascade.enabled&(active_enemies>1|target.distance>=28)&target.distance<=40&target.distance>=11
			if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && (ActiveEnemies (40) > 1 || Range (40, Target, 28)) && Range (40, Target, 11)) {
				if (Cascade ())
					return true;
			}
			//	actions.cop+=/divine_star,if=talent.divine_star.enabled&active_enemies>3&target.distance<=24
			if (HasSpell ("Divine Star") && ActiveEnemies (24) > 3 && Range (24)) {
				if (DivineStar ())
					return true;
			}
			//	actions.cop+=/shadow_word_pain,if=remains<(18*0.3)&target.time_to_die>(18*0.75)&miss_react&!ticking&active_enemies<=5&primary_target=0,cycle_Enemy=1,max_cycle_Enemy=5
			if (ActiveEnemies (40) <= 5) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && u.AuraTimeRemaining ("Shadow Word: Pain", true) < (18 * 0.3) && TimeToDie (u) > (18 * 0.75)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && ShadowWordPain (Unit))
					return true;
			}
			//	actions.cop+=/vampiric_touch,if=remains<(15*0.3+cast_time)&target.time_to_die>(15*0.75+cast_time)&miss_react&active_enemies<=5&primary_target=0,cycle_Enemy=1,max_cycle_Enemy=5
			if (ActiveEnemies (40) <= 5) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && u != Target && u.AuraTimeRemaining ("Vampiric Touch", true) < (15 * 0.3 + CastTime (34914)) && TimeToDie (u) > (15 * 0.75 + CastTime (34914))).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && VampiricTouch (Unit))
					return true;
			}
			//	actions.cop+=/divine_star,if=talent.divine_star.enabled&active_enemies=3&target.distance<=24
			if (HasSpell ("Divine Star") && ActiveEnemies (24) == 3 && Range (24)) {
				if (DivineStar ())
					return true;
			}
			//	actions.cop+=/mind_spike,if=active_enemies<=4&buff.surge_of_darkness.react
			if (ActiveEnemies (40) <= 4 && Me.HasAura ("Surge of Darkness")) {
				if (MindSpike ())
					return true;
			}
			//	actions.cop+=/mind_sear,if=active_enemies>=8,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (ActiveEnemies (40) >= 8) {
				Unit = BestAOETarget (40, 10, 3);
				if (Unit != null) {
					if (MindSear (Unit)) {
						IfInterrupt = "ChainMS";
						InterruptTarget = Unit;
						return true;
					}
				}
			}
			//	actions.cop+=/mind_spike,if=target.dot.devouring_plague_tick.remains&target.dot.devouring_plague_tick.remains<cast_time
			if (Target.HasAura ("Devouring Plague", true) && Target.AuraTimeRemaining ("Devouring Plague") < 1.5) {
				if (MindSpike ())
					return true;
			}
			//	actions.cop+=/mind_flay,if=target.dot.devouring_plague_tick.ticks_remain>1&active_enemies>1,chain=1,interrupt_if=(cooldown.mind_blast.remains<=0.1|cooldown.shadow_word_death.remains<=0.1)
			if (Target.AuraTimeRemaining ("Devouring Plague", true) > 1 && ActiveEnemies (40) > 1) {
				if (MindFlay ()) {
					IfInterrupt = "ChainMS";
					InterruptTarget = Target;
					return true;
				}
			}
			//	actions.cop+=/mind_spike
			if (MindSpike ())
				return true;
			//	actions.cop+=/shadow_word_death,moving=1,if=!target.dot.shadow_word_pain.ticking&!target.dot.vampiric_touch.ticking,cycle_Enemy=1
			if (Usable ("Shadow Word: Death")) {
				Unit = Adds.Where (u => u.IsInCombatRangeAndLoS && Health (u) < 0.2 && !Target.HasAura ("Shadow Word: Pain", true) && !Target.HasAura ("Vampiric Touch", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && ShadowWordDeath (Unit))
					return true;
			}

			if (Me.IsMoving) {
				//	actions.cop+=/shadow_word_death,moving=1,if=movement.remains>=1*gcd
				if (Health (Target) < 0.2) {
					if (ShadowWordDeath ())
						return true;
				}
				//	actions.cop+=/power_word_shield,moving=1,if=talent.body_and_soul.enabled&movement.distance>=25
				if (HasSpell ("Body and Soul")) {
					if (PowerWordShield (Me))
						return true;
				}
				//	actions.cop+=/halo,moving=1,if=talent.halo.enabled&target.distance<=30
				if (HasSpell ("Halo") && (IsBoss () || IsPlayer ()) && Range (30)) {
					if (Halo ())
						return true;
				}
				//	actions.cop+=/divine_star,if=talent.divine_star.enabled&target.distance<=28,moving=1
				if (HasSpell ("Divine Star") && (IsBoss () || IsPlayer ()) && Range (24)) {
					if (DivineStar ())
						return true;
				}
				//	actions.cop+=/cascade,if=talent.cascade.enabled&target.distance<=40,moving=1
				if (HasSpell ("Cascade") && (IsBoss () || IsPlayer ()) && Range (40)) {
					if (Cascade ())
						return true;
				}
				//	actions.cop+=/devouring_plague,moving=1
				if (DevouringPlague ())
					return true;
			}
			return false;
		}
	}
}