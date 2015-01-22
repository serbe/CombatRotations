using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;

namespace ReBot {
	[Rotation("Serb Subtlety Rogue", "Serb", WoWClass.Rogue, Specialization.RogueSubtlety)]

	public class SerbRogueSubtlety : CombatRotation {
		public enum MainHandPoison {
	        DeadlyPoison = 2823,
	        WoundPoison = 8679,
	        InstantPoison = 157584,
	    }
	    public enum OffHandPoison {
	        CripplingPoison = 3408,
	        LeechingPoison = 108211,
	    }

		[JsonProperty("Main hand Poison"), JsonConverter(typeof(StringEnumConverter))]
        public MainHandPoison MainHand = MainHandPoison.DeadlyPoison;
        [JsonProperty("Off hand Poison"), JsonConverter(typeof(StringEnumConverter))]
        public OffHandPoison OffHand = OffHandPoison.CripplingPoison;
		[JsonProperty("Use range attack")]
		public bool UseRange = false;
		[JsonProperty("Run to enemy")]
		public bool Run = false;
		[JsonProperty("Max energy")]
		public int EnergyMax = 100;
		[JsonProperty("Solo DPS")]
		public Int32 SoloDPS = 10000;
		[JsonProperty("Instance DPS")]
		public Int32 InstanceDPS = 60000;
		[JsonProperty("Raid DPS")]
		public Int32 RaidDPS = 200000;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public int Sleep = 0;
		public double SleepSec = 0;
		public int ToSkill = 0;

		public bool InRaid { get { return API.MapInfo.Type == MapType.Raid; } }
		public bool InInstance { get { return API.MapInfo.Type == MapType.Instance; } }
		public bool InArena { get { return API.MapInfo.Type == MapType.Arena; } }
		public bool InBG { get { return API.MapInfo.Type == MapType.PvP; } }
		public double Health { get { return Me.HealthFraction; }	}
		public double TargetHealth { get { return Target.HealthFraction; } }
		public bool IsBoss(UnitObject o) { return(o.MaxHealth >= Me.MaxHealth *(BossHealthPercentage / 100f)) || o.Level >= Me.Level + BossLevelIncrease; }
		public bool IsPlayer { get { return Target.IsPlayer; } }
		public bool IsElite { get { return Target.IsElite(); } }
		public bool IsInEnrage(UnitObject o) {
			if (o.HasAura("Enrage") || o.HasAura("Berserker Rage") || o.HasAura("Demonic Enrage") || o.HasAura("Aspect of Thekal") || o.HasAura("Charge Rage") || o.HasAura("Electric Spur") || o.HasAura("Cornered and Enraged!") || o.HasAura("Draconic Rage") || o.HasAura("Brood Rage") || o.HasAura("Determination") || o.HasAura("Charged Fists") || o.HasAura("Beatdown") || o.HasAura("Consuming Bite") || o.HasAura("Delirious") || o.HasAura("Angry") || o.HasAura("Blood Rage") || o.HasAura("Berserking Howl") || o.HasAura("Bloody Rage") || o.HasAura("Brewrific") || o.HasAura("Desperate Rage") || o.HasAura("Blood Crazed") || o.HasAura("Combat Momentum") || o.HasAura("Dire Rage") || o.HasAura("Dominate Slave") || o.HasAura("Blackrock Rabies") || o.HasAura("Burning Rage") || o.HasAura("Bloodletting Howl")) return true;
			else return false;
		}
		public int 		Energy 			{ get { return Me.GetPower(WoWPowerType.Energy); } }
		public int 		ComboPoints 	{ get { return Me.ComboPoints; } }
		public int 		Anticipation 	{ get { return SpellCharges("Anticipation"); } }
		public bool 	isInterruptable { get { return Target.IsCastingAndInterruptible(); } }
		public bool 	TargettingMe 	{ get { return Target.Target == (UnitObject)Me; } }
		public UnitObject LastTarget;
		public double 	EnergyRegen 	{ get { string activeRegen = API.ExecuteLua<string>("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
			return Convert.ToDouble(activeRegen); }
		}
		public int 		Enemy 			{ get { return Adds.Where(x => x.DistanceSquared <= 8 * 8).ToList().Count + 1; } }
		public int 		Enemy10			{ get { return Adds.Where(x => x.DistanceSquared <= 10 * 10).ToList().Count + 1; } }
		// public int 		Enemy 			{ get { return Adds.Count + 1; } }
		public double 	TimeToDie 		{ get { 
			if (InRaid) return Target.MaxHealth * TargetHealth / RaidDPS;
			if (InInstance) return Target.MaxHealth * TargetHealth / InstanceDPS;
			return Target.MaxHealth * TargetHealth / SoloDPS; }
		}
		public double 	Time 			{ get {
			TimeSpan CombatTime = DateTime.Now.Subtract(StartBattle);
			return CombatTime.TotalSeconds; }
		}
		public double 	SleepTime 		{ get {
			TimeSpan CurrentSleepTime = DateTime.Now.Subtract(StartSleepTime);
			return CurrentSleepTime.TotalSeconds; }
		}
		public double 	TimeToRegen(double e)	{ 
			if (e > Energy) return (e - Energy) / EnergyRegen;
			else return 0;
		}
		public double 	Cooldown(string s)	{ 
			if (SpellCooldown(s) < 0) return 0;
			else return SpellCooldown(s);
		}
		public double 	CooldownById(Int32 i)	{ 
			if (SpellCooldown(i) < 0) return 0;
			else return SpellCooldown(i);
		}
		public UnitObject AddInRange;
		public String RangedAtk = "Throw";
		public bool needToStartAttack = false;
		public Int32 OraliusWhisperingCrystal = 118922;
		public Int32 OraliusWhisperingCrystalBuff = 176151;
		public Int32 CrystalOfInsanity = 86569;

		public SerbRogueSubtlety() {
			PullSpells = new string[] {
				"Throw",
			};
			if (HasSpell("Shuriken Toss"))
				RangedAtk = "Shuriken Toss";
		}

		// # Executed before combat begins. Accepts non-harmful actions only.
		public override bool OutOfCombat() {
			// actions.precombat=flask,type=greater_draenic_agility_flask
			// actions.precombat+=/food,type=calamari_crepes
			// actions.precombat+=/apply_poison,lethal=deadly
			if (CastSelfPreventDouble((int)MainHand, () => !Me.HasAura((int)MainHand))) return true;
			if (CastSelfPreventDouble((int)OffHand, () => !Me.HasAura((int)OffHand))) return true;
			// # Snapshot raid buffed stats before combat begins and pre-potting is done.
			// actions.precombat+=/snapshot_stats
			// actions.precombat+=/potion,name=draenic_agility
			// actions.precombat+=/stealth
			// actions.precombat+=/marked_for_death
			// if (Cast("Marked for Death")) return true;
			// actions.precombat+=/premeditation,if=!talent.marked_for_death.enabled
			// if (Cast("Premeditation", () => !HasSpell("Marked for Death"))) return true;
			// actions.precombat+=/slice_and_dice
			// if (Cast("Slice and Dice")) return true;
			// actions.precombat+=/premeditation
			// if (Cast("Premeditation")) return true;
			// # Proxy Honor Among Thieves action. Generates Combo Points at a mean rate of 2.2 seconds. Comment out to disable (and use the real Honor Among Thieves).
			// actions.precombat+=/honor_among_thieves,cooldown=2.2,cooldown_stddev=0.1

			if (CastSelf("Recuperate", () => Health < 0.8 && Energy >= 30 && ComboPoints > 0 && !Me.HasAura("Recuperate"))) return true;
			if (CastSelf("Cloak of Shadows", () => Me.Auras.Any(x => x.IsDebuff && x.DebuffType.Contains("magic")))) return true;

			if (API.HasItem(CrystalOfInsanity) && !Me.HasAura("Visions of Insanity") && API.ItemCooldown(CrystalOfInsanity) == 0) {
				API.UseItem(CrystalOfInsanity);
				return true;
			}

			if (API.HasItem(OraliusWhisperingCrystal) && !HasAura(OraliusWhisperingCrystalBuff) && API.ItemCooldown(OraliusWhisperingCrystal) == 0) {
				API.UseItem(OraliusWhisperingCrystal);
				return true;
			}

			if (InCombat == true) {
				InCombat = false;
				return true;
			}

			return false;
		}

		public override bool AfterCombat () {
			if (CastSelfPreventDouble("Stealth", () => InArena && !HasAura("Stealth"))) return true;
            return false;
		}

		// # Executed every time the actor is available.
		public override void Combat() {
			// if (needToStartAttack) {
   //              needToStartAttack = false;
   //              API.StartCombat(LastTarget);
   //          }

			if (InCombat == false) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			if (Energy < Sleep) return;
			else {
				Sleep = 0;
			}
			
			switch (ToSkill) {
				case 1:
					// API.Print("Skill_1");
					ToSkill = 0;
					goto Skill_1;
				case 3:
					// API.Print("Skill_3");
					ToSkill = 0;
					goto Skill_3;
				case 4:
					// API.Print("Skill_4");
					ToSkill = 0;
					goto Skill_4;
				case 5:
					// API.Print("Skill_5");
					ToSkill = 0;
					goto Skill_5;
				case 6:
					// API.Print("Skill_6");
					ToSkill = 0;
					goto Skill_6;
				case 7:
					// API.Print("Skill_7");
					ToSkill = 0;
					goto Skill_7;
				// default:
				//     break;
			}

			if (ToSkill == 8) {
				// API.Print("Skill_8");
				Generators();
			}

			if (ToSkill == 2) {
				if (SleepSec != 0) {
					if (SleepTime < SleepSec) return;
					else {
						// API.Print("Skill_2");
						SleepSec = 0;
						ToSkill = 0;
						goto Skill_2;
					}
				}
			}

			// // Could try with RunLua:
			// // MoveBackwardStart();

			// Heal
			if (Health < 0.45) {
				API.UseItem(5512);
				return;
			} // 5512 = Healthstone
			if (CastSelf("Recuperate", () => !InRaid && !InInstance && Health < 0.9 && Energy >= 30 && ComboPoints > 0 && !Me.HasAura("Recuperate"))) return;
			if (CastSelf("Recuperate", () => !InRaid && Health < 0.3 && Energy >= 30 && ComboPoints > 0 && !Me.HasAura("Recuperate"))) return;
			if (CastSelf("Cloak of Shadows", () => TargettingMe && Health < 0.75 && Target.IsCasting && Target.RemainingCastTime < 1)) return;
            if (CastSelf("Cloak of Shadows", () => Me.Auras.Any(x => x.IsDebuff && x.DebuffType.Contains("magic")))) return;

			// Protect
			if (CastSelf("Evasion", () => TargettingMe && Health < 0.4 && (IsBoss(Target) || IsPlayer))) return;
			if (Health < 0.15 && !Me.HasAura("Stealth") && IsPlayer) {
				CastSelf("Vanish");
				API.ExecuteMacro("/stopattack");
				return;
			}

			if (Cast("Cheap Shot", () => IsPlayer && Me.HasAura("Stealth"))) return;

			//------------------------------------
			// actions=potion,name=draenic_agility,if=buff.bloodlust.react|target.time_to_die<40|(buff.shadow_reflection.up|(!talent.shadow_reflection.enabled&buff.shadow_dance.up))&(trinket.stat.agi.react|trinket.stat.multistrike.react|buff.archmages_greater_incandescence_agi.react)|((buff.shadow_reflection.up|(!talent.shadow_reflection.enabled&buff.shadow_dance.up))&target.time_to_die<136)
			// actions+=/kick
			if (!Me.HasAura("Stealth")) {
				// actions+=/kick
				if (SpellCooldown("Kick") <= 0) {
					AddInRange = Adds.Where(x => x.IsInCombatRangeAndLoS && x.DistanceSquared <= 5 * 5).ToList().FirstOrDefault(x => x.IsCastingAndInterruptible());
					if (AddInRange != null)
						if (Cast("Kick", AddInRange)) return;
					else
						if (Cast("Kick", () => isInterruptable)) return;
				}
				// Dispel Enrage
				if (SpellCooldown("Shiv") <= 0 && Energy >= 20) {
					AddInRange = Adds.Where(x => x.IsInCombatRangeAndLoS && x.DistanceSquared <= 5 * 5).ToList().FirstOrDefault(x => IsInEnrage(x));
					if (AddInRange != null)
						if (Cast("Shiv", AddInRange)) return;
					else
						if (Cast("Shiv", () => !IsBoss(Target) && IsInEnrage(Target))) return;
				}
				if (SpellCooldown("Gouge") <= 0 && Energy >= 45 && (InArena || InBG)) {
					AddInRange = Adds.Where(x => x.IsInCombatRangeAndLoS && x.DistanceSquared <= 5 * 5).ToList().FirstOrDefault(x => x.IsCasting && !Me.IsNotInFront(x));
					if (AddInRange != null)
						if (Cast("Gouge", AddInRange)) return;
				}
				// if (Cast("Gouge", () => Energy > 45 && isInterruptable && IsPlayer && !Me.IsNotInFront(Target))) return;
				if (Cast("Kidney Shot", () => IsPlayer && ComboPoints > 4)) return;
			}
			// actions+=/use_item,slot=trinket1,if=buff.shadow_dance.up
			// actions+=/shadow_reflection,if=buff.shadow_dance.up
			if (Cast("Shadow Reflection", () => HasSpell("Shadow Reflection") && (IsElite || IsPlayer || IsBoss(Target)) && Me.HasAura("Shadow Dance"))) return;
			// actions+=/blood_fury,if=buff.shadow_dance.up
			if (Cast("Blood Fury", () => (IsElite || IsPlayer) && Me.HasAura("Shadow Dance"))) return;
			// actions+=/berserking,if=buff.shadow_dance.up
			if (Cast("Berserking", () => (IsElite || IsPlayer) && Me.HasAura("Shadow Dance"))) return;
			// actions+=/arcane_torrent,if=energy<60&buff.shadow_dance.up
			if (Cast("Arcane Torrent", () => Energy < 60 && (IsElite || IsPlayer) && Me.HasAura("Shadow Dance"))) return;
			// actions+=/premeditation,if=combo_points<4|(talent.anticipation.enabled&(combo_points+anticipation_charges<9))
			if (Cast("Premeditation", () => Me.HasAura("Stealth") && ComboPoints <= 4 && (HasSpell("Anticipation") && (ComboPoints + Anticipation < 9)))) return;
			// actions+=/pool_resource,for_next=1
			if (TimeToRegen(45) < 1 && Time < 1 && Me.HasAura("Stealth") && !Target.HasAura("Garrote")) {
				Sleep = 45;
				ToSkill = 1;
				return;
			}
		Skill_1:
			// actions+=/garrote,if=time<1
			if (Cast("Garrote", () => Energy >= 45 && Time < 1 && !IsPlayer && !Target.HasAura("Garrote"))) return;

			// actions+=/wait,sec=buff.subterfuge.remains-0.1,if=buff.subterfuge.remains>0.5&buff.subterfuge.remains<1.6&time>6
			if (Me.AuraTimeRemaining("Subterfuge") > 0.5 && Me.AuraTimeRemaining("Subterfuge") < 1.6 && Time > 6) {
				SleepSec = Me.AuraTimeRemaining("Subterfuge") - 0.1;
				StartSleepTime = DateTime.Now;
				ToSkill = 2;
				return;
			}
		Skill_2:
			// actions+=/pool_resource,for_next=1,extra_amount=50
			if (Cooldown("Shadow Dance") < TimeToRegen(50) && !Me.HasAura("Stealth") && Me.AuraTimeRemaining("Vanish") < TimeToRegen(50)) {
				Sleep = 50;
				ToSkill = 3;
				return;
			}
		Skill_3:
			// actions+=/shadow_dance,if=energy>=50&buff.stealth.down&buff.vanish.down&debuff.find_weakness.remains<2|(buff.bloodlust.up&(dot.hemorrhage.ticking|dot.garrote.ticking|dot.rupture.ticking))
			if (Cast("Shadow Dance", () => Energy >= 50 && !Me.HasAura("Stealth") && !Me.HasAura("Vanish") && Target.AuraTimeRemaining("Find Weakness") < 2 || (Me.HasAura("Bloodlust") && (Target.HasAura("Hemorrhage") || Target.HasAura("Garrote") || Target.HasAura("Rupture"))))) return;
			// actions+=/pool_resource,for_next=1,extra_amount=50
			
			if (Cooldown("Vanish") < TimeToRegen(50) && HasSpell("Shadow Focus")) {
				Sleep = 50;
				ToSkill = 4;
				return;
			}
		Skill_4:
			// actions+=/vanish,if=talent.shadow_focus.enabled&energy>=45&energy<=75&(combo_points<4|(talent.anticipation.enabled&combo_points+anticipation_charges<9))&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.remains<2
			if (Cast("Vanish", () => HasSpell("Shadow Focus") && Energy >= 45 && Energy <= 75 && (ComboPoints < 4 || (HasSpell("Anticipation") && ComboPoints + Anticipation < 9)) && !Me.HasAura("Shadow Dance") && !Me.HasAura("Master of Subtlety") && Target.AuraTimeRemaining("Find Weakness") < 2)) {
				// API.Print("Vanish 4");
				// needToStartAttack = true;
				// LastTarget = Target;
				return;
			}
			
			// actions+=/pool_resource,for_next=1,extra_amount=50
			if (HasSpell("Shadowmeld") && Cooldown("Shadowmeld") < TimeToRegen(50)) {
				Sleep = 50;
				ToSkill = 5;
				return;
			}
		Skill_5:
			// actions+=/shadowmeld,if=talent.shadow_focus.enabled&energy>=45&energy<=75&(combo_points<4|(talent.anticipation.enabled&combo_points+anticipation_charges<9))&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.remains<2
			if (Cast("Shadowmeld", () => HasSpell("Shadowmeld") && HasSpell("Shadow Focus") && Energy >= 45 && Energy <=75 && (ComboPoints < 4 || (HasSpell("Anticipation") && ComboPoints + Anticipation < 9)) && !Me.HasAura("Shadow Dance") && !Me.HasAura("Master of Subtlety") && Target.AuraTimeRemaining("Find Weakness") < 2)) return;
			// actions+=/pool_resource,for_next=1,extra_amount=90

			if (Cooldown("Vanish") < TimeToRegen(90) && HasSpell("Subterfuge") && Me.AuraTimeRemaining("Shadow Dance") < TimeToRegen(90) && Me.AuraTimeRemaining("Master of Subtlety") < TimeToRegen(90)) {
				Sleep = 90;
				ToSkill = 6;
				return;
			}
		Skill_6:
			// actions+=/vanish,if=talent.subterfuge.enabled&energy>=90&(combo_points<4|(talent.anticipation.enabled&combo_points+anticipation_charges<9))&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.remains<2
			if (CastSelf("Vanish", () => HasSpell("Subterfuge") && Energy >= 90 && (ComboPoints < 4 || (HasSpell("Anticipation") && ComboPoints + Anticipation < 9)) && !Me.HasAura("Shadow Dance") && !Me.HasAura("Master of Subtlety") && Target.AuraTimeRemaining("Find Weakness") < 2)) {
				// API.Print("Vanish 6");
				// needToStartAttack = true;
				// LastTarget = Target;
				return;
			}
			
			// actions+=/pool_resource,for_next=1,extra_amount=90
			if (HasSpell("Shadowmeld") && Cooldown("Shadowmeld") < TimeToRegen(90) && (ComboPoints < 4 || (HasSpell("Anticipation") && ComboPoints + Anticipation < 9)) && Me.AuraTimeRemaining("Shadow Dance") < TimeToRegen(90) && Me.AuraTimeRemaining("Master of Subtlety") < TimeToRegen(90) && Target.AuraTimeRemaining("Find Weakness") < TimeToRegen(90) + 2) {
				Sleep = 90;
				ToSkill = 7;
				return;
			}
		Skill_7:
			// actions+=/shadowmeld,if=talent.subterfuge.enabled&energy>=90&(combo_points<4|(talent.anticipation.enabled&combo_points+anticipation_charges<9))&buff.shadow_dance.down&buff.master_of_subtlety.down&debuff.find_weakness.remains<2
			if (Cast("Shadowmeld", () => HasSpell("Shadowmeld") && HasSpell("Subterfuge") && Energy >= 90 && (ComboPoints < 4 || (HasSpell("Anticipation") && ComboPoints + Anticipation < 9)) && !Me.HasAura("Shadow Dance") && !Me.HasAura("Master of Subtlety") && Target.AuraTimeRemaining("Find Weakness") < 2)) return;
			// actions+=/marked_for_death,if=combo_points=0
			if (Cast("Marked for Death", () => HasSpell("Marked for Death") && ComboPoints == 0)) return;

			// actions+=/run_action_list,name=finisher,if=combo_points=5
			if (ComboPoints >= 5)
				Finishers();
			// actions+=/run_action_list,name=generator,if=combo_points<4|(combo_points=4&cooldown.honor_among_thieves.remains>1&energy>70-energy.regen)|(talent.anticipation.enabled&anticipation_charges<4)
			if (ComboPoints < 4 || (ComboPoints == 4 && SpellCooldown("Honor Among Thieves") > 1 && Energy > 70 - EnergyRegen) || (HasSpell("Anticipation") && Anticipation < 4))
				Generators();
			// actions+=/run_action_list,name=pool
			Pool();

			if (Cast(RangedAtk, () => Energy >= 40 && !HasAura("Stealth") && Target.IsInLoS && Target.CombatRange > 10 && Target.CombatRange <= 30 && UseRange)) return;

			if (!Target.IsInCombatRangeAndLoS && Target.CombatRange > 10 && Run)
				RunToEnemy();
		}

		// # Combo point generators
		public void Generators() {
			if (ToSkill == 8) {
				ToSkill = 0;
				goto Skill_8;
			}
			// actions.generator=run_action_list,name=pool,if=buff.master_of_subtlety.down&buff.shadow_dance.down&debuff.find_weakness.down&(energy+cooldown.shadow_dance.remains*energy.regen<=energy.max|energy+cooldown.vanish.remains*energy.regen<=energy.max)
			if (!Me.HasAura("Master of Subtlety") && !Me.HasAura("Shadow Dance") && !Target.HasAura("Find Weakness") && (Energy + SpellCooldown("Shadow Dance") * EnergyRegen <= EnergyMax || Energy + SpellCooldown("Vanish") * EnergyRegen < EnergyMax))
				Pool();
			// actions.generator+=/pool_resource,for_next=1
			if (((Me.AuraTimeRemaining("Stealth") > TimeToRegen(60) || Me.AuraTimeRemaining("Vanish") > TimeToRegen(60)) && TimeToRegen(60) > 0) || (Me.AuraTimeRemaining("Shadow Dance") > TimeToRegen(40) && TimeToRegen(40) > 0)) {
				if (Me.AuraTimeRemaining("Shadow Dance") > TimeToRegen(40) && TimeToRegen(40) > 0)
					Sleep = 40;
				else
					Sleep = 60;
				ToSkill = 8;
				return;
			}
		Skill_8:
			// actions.generator+=/ambush
			if (Cast("Ambush", () => Me.HasAura("Stealth") || Me.HasAura("Vanish") || Me.HasAura("Shadow Dance"))) return;
			// # If simulating AoE, it is recommended to use Anticipation as the level 90 talent.
			// actions.generator+=/fan_of_knives,if=active_enemies>1
			if (Cast("Fan of Knives", () => Energy >= 35 && Enemy10 > 1)) return;
			// actions.generator+=/hemorrhage,if=(remains<duration*0.3&target.time_to_die>=remains+duration+8&debuff.find_weakness.down)|!ticking|position_front
			if (Cast("Hemorrhage", () => Energy >= 30 && (Target.AuraTimeRemaining("Hemorrhage") < 7.2 && TimeToDie > Target.AuraTimeRemaining("Hemorrhage") + 24 && !Target.HasAura("Find Weakness")) || !Target.HasAura("Hemorrhage") || !Me.IsNotInFront(Target))) return;
			// actions.generator+=/shuriken_toss,if=energy<65&energy.regen<16
			if (Cast("Shuriken Toss", () => HasSpell("Shuriken Toss") && Energy < 65 && EnergyRegen < 16)) return;
			// actions.generator+=/backstab,if=!talent.death_from_above.enabled|energy>=energy.max-energy.regen|target.time_to_die<10
			if (Cast("Backstab", () => Energy >= 35 && (!HasSpell("Death from Above") || Energy >= EnergyMax - EnergyRegen || TimeToDie < 10) && Me.IsNotInFront(Target))) return;
			// actions.generator+=/run_action_list,name=pool
			Pool();
		}

		// # Combo point finishers
		public void Finishers() {
			// actions.finisher=rupture,cycle_targets=1,if=((!ticking|remains<duration*0.3)&active_enemies<=8&(cooldown.death_from_above.remains>0|!talent.death_from_above.enabled)|(buff.shadow_reflection.remains>8&dot.rupture.remains<12&buff.shadow_reflection.remains<10))&target.time_to_die>=8
			if (Enemy > 1 && Energy >= 25) {
				AddInRange = Adds.Where(x => x.IsInCombatRangeAndLoS && x.DistanceSquared <= 5 * 5).ToList().FirstOrDefault(x => ((!x.HasAura("Rupture") || x.AuraTimeRemaining("Rupture") < (4 + 4 * ComboPoints) * 0.3) && Enemy <= 8 && (SpellCooldown("Death from Above") > 0 || !HasSpell("Death from Above")) || (Me.AuraTimeRemaining("Shadow Reflection") > 8 && x.AuraTimeRemaining("Rupture") < 12 && Me.AuraTimeRemaining("Shadow Reflection") < 10)) && TimeToDie >= 8);
				if (AddInRange != null)
					if (Cast("Rupture", AddInRange)) return;
			} else {
				if (Cast("Rupture", () => Energy >= 25 && ((!Target.HasAura("Rupture") || Target.AuraTimeRemaining("Rupture") < (4 + 4 * ComboPoints) * 0.3) && Enemy <= 8 && (SpellCooldown("Death from Above") > 0 || !HasSpell("Death from Above")) || (Me.AuraTimeRemaining("Shadow Reflection") > 8 && Target.AuraTimeRemaining("Rupture") < 12 && Me.AuraTimeRemaining("Shadow Reflection") < 10)) && TimeToDie >= 8)) return; }
			// actions.finisher+=/slice_and_dice,if=(buff.slice_and_dice.remains<10.8)&buff.slice_and_dice.remains<target.time_to_die
			if (Cast("Slice and Dice", () => Energy >= 25 && (Me.AuraTimeRemaining("Slice and Dice") < 10.8) && Me.AuraTimeRemaining("Slice and Dice") < TimeToDie)) return;
			// actions.finisher+=/death_from_above
			if (Cast("Death from Above", () => Energy >= 50 && HasSpell("Death from Above"))) return;
			// actions.finisher+=/crimson_tempest,if=(active_enemies>=2&debuff.find_weakness.down)|active_enemies>=3&(cooldown.death_from_above.remains>0|!talent.death_from_above.enabled)
			if (Cast("Crimson Tempest", () => Energy >= 35 && (Enemy10 >= 2 && !Target.HasAura("Find Weakness")) || Enemy10 >= 3 && Target.AuraTimeRemaining("Crimson Tempest") < 3 && (SpellCooldown("Death from Above") > 0 || !HasSpell("Death from Above")))) return;
			// actions.finisher+=/eviscerate
			if (Cast("Eviscerate", () => Energy >= 35)) return;
			// actions.finisher+=/run_action_list,name=pool			
			Pool();
		}

		// # Resource pooling
		public void Pool() {
			// actions.pool=preparation,if=!buff.vanish.up&cooldown.vanish.remains>60
			if (CastSelf("Preparation", () => !Me.HasAura("Vanish") && SpellCooldown("Vanish") > 60)) return;
		}

		public void RunToEnemy() {
			if (Cast("Shadowstep", () => !HasAura("Sprint") && HasSpell("Shadowstep"))) return;
			if (CastSelf("Sprint", () => !HasAura("Sprint") && !HasAura("Burst of Speed"))) return;
			if (CastSelf("Burst of Speed", () =>  Energy >= 20 && !HasAura("Sprint") && !HasAura("Burst of Speed") && HasSpell("Burst of Speed"))) return;
		}
	}
}
