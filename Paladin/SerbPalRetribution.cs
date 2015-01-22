// Target.timeToDie = function()
// 	local hp = Target.healthArray[1] - Target.healthArray[#Target.healthArray]
// 	if hp > 0 then
// 		return UnitHealth('target') / (hp / 3)
// 	end
// 	return 600
// end

using System;
using System.Linq;
using ReBot.API;

namespace ReBot
{
	[Rotation("Serb Retribution Paladin", "Serb", WoWClass.Paladin, Specialization.PaladinRetribution)]

	public class SerbPaladinRetribution : CombatRotation
	{
		string mySealSpell;

		public Int32 CrystalOfInsanity = 86569;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

		public DateTime StartBattle;
		public bool InCombat;

		public bool IsBoss(UnitObject o)
		{
			return(o.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || o.Level >= Me.Level + BossLevelIncrease;
		}

		public double Health {
			get { return Me.HealthFraction; }
		}

		public double TargetHealth {
			get { return Target.HealthFraction; }
		}

		public int HolyPower {
			get { return Me.GetPower (WoWPowerType.PaladinHolyPower); }
		}

		public bool IsPlayer {
			get { return Target.IsPlayer; }
		}

		public bool IsElite {
			get { return Target.IsElite(); }
		}

		public int Time {
			get { 
				TimeSpan CombatTime = DateTime.Now.Subtract(StartBattle);
				return Convert.ToInt32(CombatTime.TotalSeconds);
			}
		}

		public SerbPaladinRetribution()
		{
			GroupBuffs = new[] { "Blessing of Kings" };
			PullSpells = new[] { "Judgment" };

			if (HasSpell("Seal of Truth"))
				mySealSpell = "Seal of Truth";
			else
				mySealSpell = "Seal of Command";
		}

		public override bool OutOfCombat()
		{
			if (API.HasItem(CrystalOfInsanity) && !HasAura("Visions of Insanity") && API.ItemCooldown(CrystalOfInsanity) == 0) {
				API.UseItem(CrystalOfInsanity);
				return true;
			}

			// # Executed before combat begins. Accepts non-harmful actions only.

			// actions.precombat=flask,type=greater_draenic_strength_flask
			// actions.precombat+=/food,type=sleeper_surprise
			// actions.precombat+=/blessing_of_kings,if=!aura.str_agi_int.up
			// actions.precombat+=/blessing_of_might,if=!aura.mastery.up
			bool hasKings = Me.HasAura("Blessing of Kings") || Me.HasAura(115921) || Me.HasAura(1126); // monk buff & druid buff
			if (hasKings) {
				if (CastSelf("Blessing of Might", () => !HasAura("Blessing of Kings", true) && !HasAura("Blessing of Might")))
					return true;
			} else if (Me.HasAura("Blessing of Might")) {
				if (CastSelf("Blessing of Kings", () => !HasAura("Blessing of Might", true) && !HasAura("Blessing of Kings")))
					return true;
			} else {
				if (Me.Level < Me.MaxLevel) {
					if (CastSelf("Blessing of Kings"))
						return true;
				} else {
					if (CastSelf("Blessing of Might"))
						return true;
				}
			}
			// actions.precombat+=/seal_of_truth,if=active_enemies<2
			// actions.precombat+=/seal_of_righteousness,if=active_enemies>=2
			// # Snapshot raid buffed stats before combat begins and pre-potting is done.
			// actions.precombat+=/snapshot_stats
			// actions.precombat+=/potion,name=draenic_strength
			
			// Heal
			
			if (CastSelfPreventDouble("Flash of Light", () => Health <= 0.75)) return true;
			if (Cast(mySealSpell, () => !IsInShapeshiftForm(mySealSpell))) return true;
			if (CastSelfPreventDouble("Flash of Light", () => Health <= 0.75)) return true;

			if (CastSelf("Cleanse", () => Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains(x.DebuffType)))) return true;
			if (CastSelf("Righteous Fury", () => CombatRole == CombatRole.Tank && !Me.HasAura("Righteous Fury"))) return true;

			InCombat = false;

			return false;
		}

		public override void Combat()
		{
			if (InCombat == false) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			if (CastSelf("Flash of Light", () => Health <= 0.7 && AuraStackCount("Selfless Healer") >= 3)) return;

			// # Executed every time the actor is available.
			// actions=rebuke
			if (Cast("Rebuke", () => Target.IsCastingAndInterruptible())) return;
			if (Cast("Fist of Justice", () => Target.IsCastingAndInterruptible())) return;
			// actions+=/potion,name=draenic_strength,if=(buff.bloodlust.react|buff.avenging_wrath.up|target.time_to_die<=40)
			// actions+=/auto_attack
			// actions+=/speed_of_light,if=movement.distance>5
			if (CastSelf("Speed of Light", () => HasSpell("Speed of Light") && Target.CombatRange > 5)) return;
			// actions+=/judgment,if=talent.empowered_seals.enabled&time<2
			if (Cast("Judgment", () => HasGlyph(54922) && Time < 2)) return;
			// actions+=/execution_sentence
			if (Cast("Execution Sentence", () => IsElite || IsPlayer)) return; 
			// actions+=/lights_hammer
			if (Cast("Light's Hammer", () => HasSpell("Light's Hammer") && (IsElite || IsPlayer))) return;
			// actions+=/holy_avenger,sync=seraphim,if=talent.seraphim.enabled
			// actions+=/holy_avenger,if=holy_power<=2&!talent.seraphim.enabled
			if (Cast("Holy Avenger", () => HolyPower <= 2 && (IsElite || IsPlayer))) return;
			// actions+=/avenging_wrath,sync=seraphim,if=talent.seraphim.enabled
			if (Cast("Avenging Wrath", () => HasSpell("Seraphim") && Me.HasAura("Seraphim"))) return;
			// actions+=/avenging_wrath,if=!talent.seraphim.enabled
			if (Cast("Avenging Wrath", () => !HasSpell("Seraphim") && (IsElite || IsPlayer))) return;
			// actions+=/blood_fury
			if (Cast("Blood Fury", () => IsElite || IsPlayer)) return;
			// actions+=/berserking
			if (Cast("Berserking", () => IsElite || IsPlayer)) return;
			// actions+=/arcane_torrent
			if (Cast("Arcane Torrent", () => IsElite || IsPlayer)) return;
			// actions+=/seraphim
			if (Cast("Seraphim", () => HasSpell("Seraphim"))) return;
			// actions+=/wait,sec=cooldown.seraphim.remains,if=talent.seraphim.enabled&cooldown.seraphim.remains>0&cooldown.seraphim.remains<gcd.max&holy_power>=5
			if (HasSpell("Seraphim") && SpellCooldown("Seraphim") > 0 && SpellCooldown("Seraphim") < 1.5 && HolyPower >= 5) return;

			if (CastSelf("Divine Shield", () => Health <= 0.3 && !Me.HasAura("Immunity"))) return;
			if (CastSelf("Flash of Light", () => Health <= 0.6 && Me.HasAura("Divine Shield"))) return;
			if (CastSelf("Lay on Hands", () => Health <= 0.2 && !Me.HasAura("Divine Shield") && !Me.HasAura("Immunity"))) return;
			if (CastSelf("Divine Protection", () => Health <= 0.3 && Target.IsCasting && !Me.HasAura("Divine Shield"))) return;
			if (CastSelf("Hand of Freedom", () => Me.CanParticipateInCombat && !Target.IsInCombatRange && Me.MovementSpeed > 0 && Me.MovementSpeed < MovementSpeed.NormalRunning)) return;
			if (CastSelf("Emancipate", () => Me.CanParticipateInCombat && !Target.IsInCombatRange && Me.MovementSpeed > 0 && Me.MovementSpeed < MovementSpeed.NormalRunning)) return;

			//GLOBAL CD CHECK
			// if (HasGlobalCooldown())
			// 	return;

			// Cancel fury aura in retribution spec!! (first check is to know if we even have the spell)
			if (CombatRole != CombatRole.Tank && HasSpell("Righteous Fury") && Me.HasAura("Righteous Fury"))
				CancelAura("Righteous Fury");

			int manyAdds = Adds.Count + 1;
			// dmg buff
			//int nearbyAdds = Adds.Where(x => x.IsInCombatRangeAndLoS).ToList().Count + 1;
			int nearbyAdds = Adds.Count(x => x.DistanceSquared <= 8 * 8) + 1;

			if (CastSelf("Word of Glory", () => HolyPower >= 3 && Health <= 0.5)) return;
			// Stun if Possible
			if (Cast("Hammer of Justice", () => IsElite || IsPlayer)) return;

			// actions+=/call_action_list,name=cleave,if=active_enemies>=3
			if (nearbyAdds > 3) {
				// actions.cleave=final_verdict,if=buff.final_verdict.down&holy_power=5
				if (Cast("Final Verdict", () => !Me.HasAura("Final Verdict") && HolyPower == 5)) return;
				// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&buff.final_verdict.up
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && HolyPower == 5 && Me.HasAura("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=holy_power=5&buff.final_verdict.up
				if (Cast("Divine Storm", () => HolyPower == 5 && Me.HasAura("Final Verdict")))	return;
				// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && HolyPower == 5 && !HasSpell("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=holy_power=5&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => HolyPower == 5 && (!HasSpell("Seraphim") || Me.AuraTimeRemaining("Seraphim") > 6) && !HasSpell("Final Verdict"))) return;
				// actions.cleave+=/hammer_of_wrath
				// if (Cast("Hammer of Wrath", () => TargetHealth <= 0.35 || Me.HasAura("Avenging Wrath"))) return;
				if (Cast("Hammer of Wrath")) return;
				// actions.cleave+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
				if (Cast("Exorcism", () => Me.HasAura("Blazing Contempt") && HolyPower <= 2 && !Me.HasAura("Holy Avenger"))) return;
				// actions.cleave+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<=5
				if (Cast("Judgment", () => HasSpell("Empowered Seals") && IsInShapeshiftForm("Seal of Righteousness") && Me.AuraTimeRemaining("Liadrin's Righteousness") <= 5)) return;
				// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && Me.HasAura("Final Verdict") && (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35))) return;
				// actions.cleave+=/divine_storm,if=buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
				if (Cast("Divine Storm", () => Me.HasAura("Final Verdict") && (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35))) return;
				// actions.cleave+=/final_verdict,if=buff.final_verdict.down&(buff.avenging_wrath.up|target.health.pct<35)
				if (Cast("Final Verdict", () => !Me.HasAura("Final Verdict") && (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35))) return;
				// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35) && !HasSpell("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=(buff.avenging_wrath.up|target.health.pct<35)&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35) && (!HasSpell("Seraphim") || Me.AuraTimeRemaining("Seraphim") > 7.5) && !HasSpell("Final Verdict"))) return;
				// actions.cleave+=/hammer_of_the_righteous,if=active_enemies>=4&holy_power<5
				if (Cast("Hammer of the Righteous", () => nearbyAdds >= 4 && HolyPower < 5)) return;
				// actions.cleave+=/crusader_strike,if=holy_power<5
				if (Cast("Crusader Strike", () => HolyPower < 5)) return;
				// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && Me.HasAura("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=buff.divine_purpose.react&buff.final_verdict.up
				if (Cast("Divine Storm", () => Me.HasAura("Divine Purpose") && Me.HasAura("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=holy_power>=4&buff.final_verdict.up
				if (Cast("Divine Storm", () => HolyPower >= 4 && Me.HasAura("Final Verdict"))) return;
				// actions.cleave+=/final_verdict,if=buff.divine_purpose.react&buff.final_verdict.down
				if (Cast("Final Verdict", () => Me.HasAura("Divine Purpose") && !Me.HasAura("Final Verdict"))) return;
				// actions.cleave+=/final_verdict,if=holy_power>=4&buff.final_verdict.down
				if (Cast("Final Verdict", () => HolyPower >= 4 && !Me.HasAura("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && !HasSpell("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => HolyPower >= 4 && (!HasSpell("Seraphim") || AuraTimeRemaining("Seraphim") > 7.5) && !HasSpell("Final Verdict"))) return;
				// actions.cleave+=/exorcism,if=glyph.mass_exorcism.enabled&holy_power<5
				if (Cast("Exorcism", () => HasGlyph (122028) && HolyPower < 5)) return;
				// actions.cleave+=/judgment,cycle_targets=1,if=glyph.double_jeopardy.enabled&holy_power<5
				if (HasGlyph (54922) && HolyPower < 5) {
					UnitObject CycleTargets = Adds.Where(x => x.IsInCombatRangeAndLoS).ToList().FirstOrDefault(x => x != Target);
					if (CycleTargets != null)
				    	if (Cast("Judgment", CycleTargets)) return;
				}
				// actions.cleave+=/judgment,if=holy_power<5
				if (Cast("Judgment", () => HolyPower < 5)) return;
				// actions.cleave+=/exorcism,if=holy_power<5
				if (Cast("Exorcism", () => HolyPower < 5)) return;
				// actions.cleave+=/divine_storm,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => HolyPower >= 3 && (!HasSpell("Seraphim") || AuraTimeRemaining("Seraphim") > 9) && !HasSpell("Final Verdict"))) return;
				// actions.cleave+=/divine_storm,if=holy_power>=3&buff.final_verdict.up
				if (Cast("Divine Storm", () => HolyPower >= 3 && Me.HasAura("Final Verdict"))) return;
				// actions.cleave+=/final_verdict,if=holy_power>=3&buff.final_verdict.down
				if (Cast("Final Verdict", () => HolyPower >= 3 && !Me.HasAura("Final Verdict"))) return;
			}
			// actions+=/call_action_list,name=single
			else {
				// actions.single=divine_storm,if=buff.divine_crusader.react&holy_power=5&buff.final_verdict.up
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && HolyPower == 5 && Me.HasAura("Final Verdict"))) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&active_enemies=2&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && HolyPower == 5 && nearbyAdds == 2 && !HasSpell("Final Verdict"))) return;
				// actions.single+=/divine_storm,if=holy_power=5&active_enemies=2&buff.final_verdict.up
				if (Cast("Divine Storm", () => HolyPower == 5 && nearbyAdds == 2 && Me.HasAura("Final Verdict"))) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&holy_power=5&(talent.seraphim.enabled&cooldown.seraphim.remains<gcd*4)
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && HolyPower == 5 && (!HasSpell("Seraphim") || AuraTimeRemaining("Seraphim") > 6))) return;
				// actions.single+=/templars_verdict,if=holy_power=5|buff.holy_avenger.up&holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*4)
				if (Cast("Templar's Verdict", () => HolyPower == 5 || Me.HasAura("Holy Avenger") && HolyPower >= 3 && (!HasSpell("Seraphim") || AuraTimeRemaining("Seraphim") > 6))) return;
				// actions.single+=/templars_verdict,if=buff.divine_purpose.react&buff.divine_purpose.remains<3
				if (Cast("Templar's Verdict", () => Me.HasAura("Divine Purpose") && Me.AuraTimeRemaining("Divine Purpose") < 3)) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.divine_crusader.remains<3&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && Me.AuraTimeRemaining("Divine Crusader") < 3 && !HasSpell("Final Verdict"))) return;
				// actions.single+=/final_verdict,if=holy_power=5|buff.holy_avenger.up&holy_power>=3
				if (Cast("Final Verdict", () => HolyPower == 5 || Me.HasAura("Holy Avenger") && HolyPower >= 3)) return;
				// actions.single+=/final_verdict,if=buff.divine_purpose.react&buff.divine_purpose.remains<3
				if (Cast("Final Verdict", () => Me.HasAura("Divine Purpose") && Me.AuraTimeRemaining("Divine Purpose") < 3)) return;
				// actions.single+=/hammer_of_wrath
				if (Cast("Hammer of Wrath")) return;
				// actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.truth&buff.maraads_truth.remains<cooldown.judgment.duration
				if (Cast("Judgment", () => HasSpell("Empowered Seals") && IsInShapeshiftForm("Seal of Truth") && Me.AuraTimeRemaining("Maraad's Truth") < SpellCooldown("Judgment"))) return;
				// actions.single+=/judgment,if=talent.empowered_seals.enabled&seal.righteousness&buff.liadrins_righteousness.remains<cooldown.judgment.duration
				if (Cast("Judgment", () => HasSpell("Empowered Seals") && IsInShapeshiftForm("Seal of Righteousness") && Me.AuraTimeRemaining("Liadrin's Righteousness") < SpellCooldown("Judgment"))) return;
				// actions.single+=/exorcism,if=buff.blazing_contempt.up&holy_power<=2&buff.holy_avenger.down
				if (Cast("Exorcism", () => Me.HasAura("Blazing Contempt") && HolyPower <= 2 && !Me.HasAura("Holy Avenger"))) return;
				// actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.down
				if (Cast("Seal of Truth", () => HasSpell("Empowered Seals") && !Me.HasAura("Maraad's Truth"))) return;
				// actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.liadrins_righteousness.down&!buff.avenging_wrath.up&!buff.bloodlust.up
				if (Cast("Seal of Righteousness", () => HasSpell("Empowered Seals") && !Me.HasAura("Liadrin's Righteousness") && !Me.HasAura("Avenging Wrath") && !Me.HasAura("Bloodlust"))) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && Me.HasAura("Final Verdict") && (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35))) return;
				// actions.single+=/divine_storm,if=active_enemies=2&buff.final_verdict.up&(buff.avenging_wrath.up|target.health.pct<35)
				if (Cast("Divine Storm", () => nearbyAdds == 2 && Me.HasAura("Final Verdict") && (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35))) return;
				// actions.single+=/final_verdict,if=buff.avenging_wrath.up|target.health.pct<35
				if (Cast("Final Verdict", () => Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35)) return;
				// actions.single+=/templars_verdict,if=buff.avenging_wrath.up|target.health.pct<35&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
				if (Cast("Templar's Verdict", () => Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35 && (!HasSpell("Seraphim") || Me.AuraTimeRemaining("Seraphim") > 7.5))) return;
				// actions.single+=/crusader_strike,if=holy_power<5
				if (Cast("Crusader Strike", () => HolyPower < 5)) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&(buff.avenging_wrath.up|target.health.pct<35)&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && (Me.HasAura("Avenging Wrath") || TargetHealth <= 0.35) && !HasSpell("Final Verdict"))) return;
				// actions.single+=/judgment,cycle_targets=1,if=last_judgment_target!=target&glyph.double_jeopardy.enabled&holy_power<5
				if (Adds.Count + 1 > 1 && HasGlyph (54922) && HolyPower < 5) {
					UnitObject CycleTargets = Adds.Where(x => x.IsInCombatRangeAndLoS).ToList().FirstOrDefault(x => x != Target);
					if (CycleTargets != null)
				    	if (Cast("Judgment", CycleTargets)) return;
				}
				// actions.single+=/exorcism,if=glyph.mass_exorcism.enabled&active_enemies>=2&holy_power<5&!glyph.double_jeopardy.enabled
				if (Cast("Exorcism", () => HasGlyph (122028) && nearbyAdds >= 2 && HolyPower < 5 && !HasGlyph (54922))) return;
				// actions.single+=/judgment,,if=holy_power<5
				if (Cast("Judgment", () => HolyPower < 5)) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&buff.final_verdict.up
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && Me.HasAura("Final Verdict"))) return;
				// actions.single+=/divine_storm,if=active_enemies=2&holy_power>=4&buff.final_verdict.up
				if (Cast("Divine Storm", () => nearbyAdds == 2 && HolyPower >= 4 && Me.HasAura("Final Verdict"))) return;
				// actions.single+=/final_verdict,if=buff.divine_purpose.react
				if (Cast("Final Verdict", () => Me.HasAura("Divine Purpose"))) return;
				// actions.single+=/final_verdict,if=holy_power>=4
				if (Cast("Final Verdict", () => HolyPower >= 4)) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&active_enemies=2&holy_power>=4&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && nearbyAdds == 2 && HolyPower >= 4 && !HasSpell("Final Verdict"))) return;
				// actions.single+=/templars_verdict,if=buff.divine_purpose.react
				if (Cast("Templar's Verdict", () => Me.HasAura("Divine Purpose"))) return;
				// actions.single+=/divine_storm,if=buff.divine_crusader.react&!talent.final_verdict.enabled
				if (Cast("Divine Storm", () => Me.HasAura("Divine Crusader") && !HasSpell("Final Verdict"))) return;
				// actions.single+=/templars_verdict,if=holy_power>=4&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*5)
				if (Cast("Templar's Verdict", () => HolyPower >= 4 && (!HasSpell("Seraphim") || SpellCooldown("Seraphim") > 7.5))) return;
				// actions.single+=/seal_of_truth,if=talent.empowered_seals.enabled&buff.maraads_truth.remains<cooldown.judgment.duration
				if (Cast("Seal of Truth", () => HasSpell("Empowered Seals") && Me.AuraTimeRemaining("Maraad's Truth") < SpellCooldown("Judgment"))) return;
				// actions.single+=/seal_of_righteousness,if=talent.empowered_seals.enabled&buff.liadrins_righteousness.remains<cooldown.judgment.duration&!buff.bloodlust.up
				if (Cast("Seal of Righteousness", () => HasSpell("Empowered Seals") && Me.AuraTimeRemaining("Liadrin's Righteousness") < SpellCooldown("Judgment") && !Me.HasAura("Bloodlust"))) return;
				// actions.single+=/exorcism,if=holy_power<5
				if (Cast("Exorcism", () => HolyPower < 5)) return;
				// actions.single+=/divine_storm,if=active_enemies=2&holy_power>=3&buff.final_verdict.up
				if (Cast("Divine Storm", () => nearbyAdds == 2 && HolyPower >= 3 && Me.HasAura("Final Verdict"))) return;
				// actions.single+=/final_verdict,if=holy_power>=3
				if (Cast("Final Verdict", () => HolyPower >= 3)) return;
				// actions.single+=/templars_verdict,if=holy_power>=3&(!talent.seraphim.enabled|cooldown.seraphim.remains>gcd*6)
				if (Cast("Templar's Verdict", () => HolyPower >= 3 && (!HasSpell("Seraphim") || SpellCooldown("Seraphim") > 9))) return;
				// actions.single+=/holy_prism
				if (Cast("Holy Prism")) return;
			}

			if (CastSelfPreventDouble("Flash of Light", () => Health <= 0.5)) return;
		}
	}
}
