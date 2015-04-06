using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;
using Newtonsoft.Json.Converters;

namespace ReBot
{
	[Rotation ("Serb Hunter Beastmastery", "Serb", WoWClass.Hunter, Specialization.HunterBeastMastery, 40, 40)]

	public class SerbHunterBeastmasterSC : SerbHunter
	{
		[JsonProperty ("Use Pet"), JsonConverter (typeof(StringEnumConverter))]							
		public UsePet Pet = UsePet.UsePet;
		[JsonProperty ("Pet Slot"), JsonConverter (typeof(StringEnumConverter))]							
		public PetSlot Slot = PetSlot.PetSlot1;
		[JsonProperty ("Exotic Munitions"), JsonConverter (typeof(StringEnumConverter))]							
		public ExoticMunitionsType Exo = ExoticMunitionsType.NoExoticMunitions;
		[JsonProperty ("Use Misdirection")]
		public bool UseMD = true;
		[JsonProperty ("Use fire trap")]
		public bool FireTrap = false;
		[JsonProperty ("Use ice trap")]
		public bool IceTrap = false;

		public SerbHunterBeastmasterSC ()
		{
			PullSpells = new[] {
				"Concussive Shot",
				"Arcane Shot",
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_agility_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/summon_pet
			if (!Me.HasAlivePet && !HasSpell ("Lone Wolf")) {
				if (SummonPet (Slot))
					return true;
			}
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			if (HasSpell ("Exotic Munitions")) {
				if (ExoticMunitions (Exo))
					return true;
			}
			//	actions.precombat+=/exotic_munitions,ammo_type=poisoned,if=active_enemies<3
			//	actions.precombat+=/exotic_munitions,ammo_type=incendiary,if=active_enemies>=3
			//	actions.precombat+=/potion,name=draenic_agility
			//	actions.precombat+=/glaive_toss
			//	actions.precombat+=/focusing_shot


			if (!Me.HasAura ("Trap Launcher")) {
				if (TrapLauncher ())
					return true;
			}

			if (HasSpell ("Lone Wolf")) {
				if (LoneWolf (Pet))
					return true;
			}

			if (Me.HasAlivePet && Me.Pet.HealthFraction <= 0.8) {
				if (MendPet ())
					return true;
			}

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (InCombat) {
				InCombat = false;
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

			if (HasAura ("Aspect of the Pack")) {
				CancelAura ("Aspect of the Pack");
			}
			if (HasAura ("Aspect of the Cheetah")) {
				CancelAura ("Aspect of the Cheetah");
			}

			if (!Me.HasAura ("Trap Launcher")) {
				if (TrapLauncher ())
					return;
			}

			if (!InArena && !InBG && Me.HasAlivePet && UseMD) {
				if (Misdirection ())
					return;
			}

			if (!InRaid && !InInstance && Target.IsFleeing) {
				if (ConcussiveShot ())
					return;
			}

			if (Interrupt ())
				return;

			if (Tranquilizing ())
				return;

			// if (CastOnTerrain("Explosive Trap", Target.Position, () => FireTrap && IsPlayer && Target.AuraTimeRemaining("Explosive Trap", true) < 5)) return;

			if (API.ExecuteLua<bool> ("return IsShiftKeyDown()")) {
				if (Target.CombatRange > 5 && Target.IsInLoS && Target.CombatRange <= 30) {
					if (BindingShot (Target))
						return;
				}

				// if (IceTrap && Me.HasAura("Trap Launcher") && (IsPlayer || IsElite) && Target.CombatRange > 10) {
				// 	var IceTarget = new Vector3((Target.Position.X + Me.Position.X) / 2, (Target.Position.Y + Me.Position.Y) / 2, (Target.Position.Z + Me.Position.Z) / 2);
				// 	if (CastOnTerrain("Ice Trap", IceTarget, () => Cooldown("Ice Trap") == 0)) return;
				// }
				if ((InArena || InBG) && Usable ("Freezing Trap") && EnemyWithTarget (Target, 15) == 0) {
					CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.IsPlayer && x != Target && x.CanParticipateInCombat).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (FreezingTrap(CycleTarget))
							return;
					}
				}
			}

			// if (API.LuaIf("IsLeftControlKeyDown()")) {
			//              string Click = "CameraOrSelectOrMoveStart(); CameraOrSelectOrMoveStop()";
			// 	if (Cast("Ice Trap", () => IceTrap)) { API.ExecuteLua(Click); return; }
			//          }

			if (Me.CanNotParticipateInCombat()) {
				if (Freedom ())
					return;
			}

			//Heal
			if (Me.HasAlivePet) {
				if (Me.Pet.HealthFraction <= 0.8) {
					if (MendPet ())
						return;
				}
				if (Me.Pet.HealthFraction <= 0.3) {
					if (LastStand ())
						return;
				}
				if (Health <= 0.3) {
					if (RoarofSacrifice ())
						return;
				}
			}
			if (Health <= 0.3) {
				if (Exhilaration ())
					return;
			}
			if (Health < 0.4) {
				if (Deterrence ())
					return;
			}
			if (Health <= 0.2) {
				if (FeignDeath ())
					return;
			}


			//	actions=auto_shot
			//	actions+=/use_item,name=beating_heart_of_the_mountain
			//	actions+=/arcane_torrent,if=focus.deficit>=30
			if (FocusDeflict >= 30) {
				if (ArcaneTorrent ())
					return;
			}
			//	actions+=/blood_fury
			if (BloodFury ())
				return;
			//	actions+=/berserking
			if (Berserking ())
				return;
			//	actions+=/potion,name=draenic_agility,if=!talent.stampede.enabled&buff.bestial_wrath.up&target.health.pct<=20|target.time_to_die<=20
			//	actions+=/potion,name=draenic_agility,if=talent.stampede.enabled&cooldown.stampede.remains<1&(buff.bloodlust.up|buff.focus_fire.up)|target.time_to_die<=25
			//	actions+=/stampede,if=buff.bloodlust.up|buff.focus_fire.up|target.time_to_die<=25
			if (Cast ("Stampede", () => HasSpell ("Stampede") && (Me.HasAura ("Bloodlust") || Me.HasAura ("Focus Fire")) && (IsElite || IsPlayer)))
				return;
			//	actions+=/dire_beast
			//	actions+=/focus_fire,if=buff.focus_fire.down&((cooldown.bestial_wrath.remains<1&buff.bestial_wrath.down)|(talent.stampede.enabled&buff.stampede.remains)|pet.cat.buff.frenzy.remains<1)
			//	actions+=/bestial_wrath,if=focus>30&!buff.bestial_wrath.up
			//	actions+=/multishot,if=active_enemies>1&pet.cat.buff.beast_cleave.remains<0.5
			//	actions+=/focus_fire,five_stacks=1,if=buff.focus_fire.down
			//	actions+=/barrage,if=active_enemies>1
			//	actions+=/explosive_trap,if=active_enemies>5
			//	actions+=/multishot,if=active_enemies>5
			//	actions+=/kill_command
			//	actions+=/a_murder_of_crows
			//	actions+=/kill_shot,if=focus.time_to_max>gcd
			//	actions+=/focusing_shot,if=focus<50
			//	# Cast a second shot for steady focus if that won't cap us.
			//	actions+=/cobra_shot,if=buff.pre_steady_focus.up&buff.steady_focus.remains<7&(14+cast_regen)<focus.deficit
			//	actions+=/explosive_trap,if=active_enemies>1
			//	# Prepare for steady focus refresh if it is running out.
			//	actions+=/cobra_shot,if=talent.steady_focus.enabled&buff.steady_focus.remains<4&focus<50
			//	actions+=/glaive_toss
			//	actions+=/barrage
			//	actions+=/powershot,if=focus.time_to_max>cast_time
			//	actions+=/cobra_shot,if=active_enemies>5
			//	actions+=/arcane_shot,if=(buff.thrill_of_the_hunt.react&focus>35)|buff.bestial_wrath.up
			//	actions+=/arcane_shot,if=focus>=75
			//	actions+=/cobra_shot



			// actions+=/dire_beast
			if (Cast ("Dire Beast", () => HasSpell ("Dire Beast")))
				return;
			// actions+=/explosive_trap,if=active_enemies>1
			if (CastOnTerrain ("Explosive Trap", Target.Position, () => FireTrap && (EnemyWithTarget (Target, 8) > 1 || IsPlayer || IsElite)))
				return;
			// actions+=/focus_fire,if=buff.focus_fire.down&(cooldown.bestial_wrath.remains<1|(talent.stampede.enabled&buff.stampede.remains))
			if (Cast ("Focus Fire", () => !Me.HasAura ("Focus Fire") && (Cooldown ("Bestial Wrath") < 1 || (HasSpell ("Stampede") && Me.HasAura ("Stampede")))))
				return;
			// actions+=/bestial_wrath,if=focus>30&!buff.bestial_wrath.up
			if (Cast ("Bestial Wrath", () => Focus > 30 && !Me.HasAura ("Bestial Wrath")))
				;
			// actions+=/multishot,if=active_enemies>1&pet.cat.buff.beast_cleave.down
			if (Cast ("Multi-Shot", () => Focus >= 40 && EnemyWithTarget (Target, 10) > 1 && !Me.Pet.HasAura ("Beast Cleave")))
				return;
			// actions+=/barrage,if=active_enemies>1
			if (Cast ("Barrage", () => HasSpell ("Barrage") && Focus >= 60 && EnemyWithTarget (Target, 15) > 1))
				return;
			// actions+=/multishot,if=active_enemies>5
			if (Cast ("Multi-Shot", () => Focus >= 40 && EnemyWithTarget (Target, 10) > 5))
				return;
			// actions+=/focus_fire,five_stacks=1
			if (CastSelf ("Focus Fire", () => Me.HasAura ("Frenzy", false, 5)))
				return;
			// actions+=/barrage,if=active_enemies>1
			if (Cast ("Barrage", () => HasSpell ("Barrage") && Focus >= 60 && EnemyWithTarget (Target, 10) > 1))
				return;
			// actions+=/kill_command
			if (Cast ("Kill Command", () => Focus >= 40))
				return;
			// actions+=/a_murder_of_crows
			if (Cast ("A Murder of Crows", () => Focus >= 30))
				return;
			// actions+=/kill_shot,if=focus.time_to_max>gcd
			if (Cast ("Kill Shot", () => TargetHealth <= 0.35 && FocusDeflict / FocusRegen > 1))
				return;
			// actions+=/focusing_shot,if=focus<50
			if (Cast ("Focusing Shot", () => HasSpell ("Focusing Shot") && Focus < 50))
				return;
			// # Cast a second shot for steady focus if that won't cap us.
			// actions+=/cobra_shot,if=buff.pre_steady_focus.up&(14+cast_regen)<=focus.deficit
			if (Cast ("Cobra Shot", () => Me.HasAura ("Steady Focus") && (14 + 2 * FocusRegen) <= FocusDeflict))
				return;
			// actions+=/glaive_toss
			if (Cast ("Glaive Toss",	() => Focus >= 15 && HasSpell ("Glaive Toss")))
				return;
			// actions+=/barrage
			if (Cast ("Barrage", () => Focus >= 60 && HasSpell ("Barrage")))
				return;
			// actions+=/powershot,if=focus.time_to_max>cast_time
			if (Cast ("Powershot", () => HasSpell ("Powershot") && FocusDeflict / FocusRegen > 2.25))
				return;
			// actions+=/cobra_shot,if=active_enemies>5
			if (Cast ("Cobra Shot", () => EnemyInRange (40) > 5))
				return;
			// actions+=/arcane_shot,if=(buff.thrill_of_the_hunt.react&focus>35)|buff.bestial_wrath.up
			if (Cast ("Arcane Shot", () => (Me.HasAura ("Thrill of the Hunt") && Focus > 35) || Me.HasAura ("Bestial Wrath")))
				return;
			// actions+=/arcane_shot,if=focus>=75
			if (Cast ("Arcane Shot", () => Focus >= 75))
				return;
			// actions+=/cobra_shot
			if (Cast ("Cobra Shot"))
				return;
		}
	}
}
