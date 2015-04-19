using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;

namespace ReBot.Hunter
{
	[Rotation ("Serb Hunter Beastmastery SC", "Serb", WoWClass.Hunter, Specialization.HunterBeastMastery, 40, 40)]

	public class SerbHunterBeastmasterSc : SerbHunter
	{
		[JsonProperty ("Use Pet"), JsonConverter (typeof(StringEnumConverter))]							
		public UsePet Pet = UsePet.UsePet;
		[JsonProperty ("Pet Slot"), JsonConverter (typeof(StringEnumConverter))]							
		public PetSlot Slot = PetSlot.PetSlot1;
		[JsonProperty ("Exotic Munitions"), JsonConverter (typeof(StringEnumConverter))]							
		public ExoticMunitionsType Exo = ExoticMunitionsType.NoExoticMunitions;
		[JsonProperty ("Use Misdirection")]
		public bool UseMd = true;
		[JsonProperty ("Use fire trap")]
		public bool FireTrap;
		[JsonProperty ("Use ice trap")]
		public bool IceTrap;

		public SerbHunterBeastmasterSc ()
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

			if (MeIsBusy ())
				return;

			if (Interrupt ())
				return;

			if (!Me.HasAura ("Trap Launcher")) {
				if (TrapLauncher ())
					return;
			}

			if (!InArena && !InBg && Me.HasAlivePet && UseMd) {
				if (Misdirection ())
					return;
			}

			if (!InRaid && !InInstance && Target.IsFleeing) {
				if (ConcussiveShot ())
					return;
			}

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
				if ((InArena || InBg) && Usable ("Freezing Trap") && EnemyWithTarget (Target, 15) == 0) {
					CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.IsPlayer && x != Target && x.CanParticipateInCombat).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (FreezingTrap (CycleTarget))
							return;
					}
				}
			}

			// if (API.LuaIf("IsLeftControlKeyDown()")) {
			//              string Click = "CameraOrSelectOrMoveStart(); CameraOrSelectOrMoveStop()";
			// 	if (Cast("Ice Trap", () => IceTrap)) { API.ExecuteLua(Click); return; }
			//          }

			if (Me.CanNotParticipateInCombat ()) {
				if (Freedom ())
					return;
			}

			//Heal
			if (Me.HasAlivePet) {
				if (Health (Me.Pet) <= 0.8) {
					if (MendPet ())
						return;
				}
				if (Health (Me.Pet) <= 0.3) {
					if (LastStand ())
						return;
				}
				if (Health () <= 0.3) {
					if (RoarofSacrifice ())
						return;
				}
			}
			if (Health () <= 0.3) {
				if (Exhilaration ())
					return;
			}
			if (Health () < 0.4) {
				if (Deterrence ())
					return;
			}
			if (Health () <= 0.2) {
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
			if (Me.HasAura ("Bloodlust") || Me.HasAura ("Focus Fire")) {
				if (Stampede ())
					return;
			}
			//	actions+=/dire_beast
			if (DireBeast ())
				return;
			//	actions+=/focus_fire,if=buff.focus_fire.down&((cooldown.bestial_wrath.remains<1&buff.bestial_wrath.down)|(talent.stampede.enabled&buff.stampede.remains)|pet.cat.buff.frenzy.remains<1)
			if (!Me.HasAura ("Focus Fire") && ((Cooldown ("Bestial Wrath") < 1 && !Me.HasAura ("Bestial Wrath")) || (HasSpell ("Stampede") && Me.HasAura ("Stampede")) || Me.Pet.AuraTimeRemaining ("Frenzy") < 1)) {
				if (FocusFire ())
					return;
			}
			//	actions+=/bestial_wrath,if=focus>30&!buff.bestial_wrath.up
			if (Focus > 30 && !Me.HasAura ("Bestial Wrath")) {
				if (BestialWrath ())
					return;
			}
			//	actions+=/multishot,if=active_enemies>1&pet.cat.buff.beast_cleave.remains<0.5
			if (EnemyWithTarget (Target, 10) > 1 && Me.Pet.AuraTimeRemaining ("Beast Cleave") < 0.5) {
				if (MultiShot ())
					return;
			}
			//	actions+=/focus_fire,five_stacks=1,if=buff.focus_fire.down
//			Me.HasAura ("Frenzy", false, 5)

			//	actions+=/barrage,if=active_enemies>1
			if (EnemyWithTarget (Target, 25) > 1) {
				if (Barrage ())
					return;
			}
			//	actions+=/explosive_trap,if=active_enemies>5
			if (FireTrap && (EnemyWithTarget (Target, 8) > 5 || IsPlayer () || IsElite ())) {
				if (ExplosiveTrap (Target))
					return;
			}
			//	actions+=/multishot,if=active_enemies>5
			if (EnemyWithTarget (Target, 15) > 5) {
				if (MultiShot ())
					return;
			}
			//	actions+=/kill_command
			if (KillCommand ())
				return;
			//	actions+=/a_murder_of_crows
			if (AMurderofCrows ())
				return;
			//	actions+=/kill_shot,if=focus.time_to_max>gcd
			if (FocusDeflict / FocusRegen > 1) {
				if (KillShot ())
					return;
			}
			//	actions+=/focusing_shot,if=focus<50
			if (Focus < 50) {
				if (FocusingShot ())
					return;
			}
			//	# Cast a second shot for steady focus if that won't cap us.
			//	actions+=/cobra_shot,if=buff.pre_steady_focus.up&buff.steady_focus.remains<7&(14+cast_regen)<focus.deficit
			if (Me.HasAura ("Steady Focus") && Me.AuraTimeRemaining ("Steady Focus") < 7 && (14 + 2 * FocusRegen) <= FocusDeflict) {
				if (CobraShot ())
					return;
			}
			//	actions+=/explosive_trap,if=active_enemies>1
			if (FireTrap && (EnemyWithTarget (Target, 8) > 1 || IsPlayer () || IsElite ())) {
				if (ExplosiveTrap (Target))
					return;
			}
			//	# Prepare for steady focus refresh if it is running out.
			//	actions+=/cobra_shot,if=talent.steady_focus.enabled&buff.steady_focus.remains<4&focus<50
			if (HasSpell ("Steady Focus") && Me.AuraTimeRemaining ("Steady Focus") < 4 && Focus < 50) {
				if (CobraShot ())
					return;
			}
			//	actions+=/glaive_toss
			if (GlaiveToss ())
				return;
			//	actions+=/barrage
			if (Barrage ())
				return;
			//	actions+=/powershot,if=focus.time_to_max>cast_time
			if (FocusDeflict / FocusRegen > 2.25) {
				if (Powershot ())
					return;
			}
			//	actions+=/cobra_shot,if=active_enemies>5
			if (EnemyInRange (40) > 5) {
				if (CobraShot ())
					return;
			}
			//	actions+=/arcane_shot,if=(buff.thrill_of_the_hunt.react&focus>35)|buff.bestial_wrath.up
			if ((Me.HasAura ("Thrill of the Hunt") && Focus > 35) || Me.HasAura ("Bestial Wrath")) {
				if (ArcaneShot ())
					return;
			}
			//	actions+=/arcane_shot,if=focus>=75
			if (Focus >= 75) {
				if (ArcaneShot ())
					return;
			}
			//	actions+=/cobra_shot
			if (CobraShot ())
				return;
		}
	}
}
