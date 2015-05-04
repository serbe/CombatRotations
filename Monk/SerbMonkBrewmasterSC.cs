using System;
using ReBot.API;
using Newtonsoft.Json;
using System.Linq;

namespace ReBot
{
	[Rotation ("Serb Monk Brewmaster SC", "Serb", WoWClass.Monk, Specialization.MonkBrewmaster, 5, 25)]

	public class SerbMonkBrewmasterSC : SerbMonk
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;
		[JsonProperty ("Use Spinning Crane Kick")]
		public bool Sck = true;
		[JsonProperty ("Use Dizzying Haze")]
		public bool Dh = true;



		public SerbMonkBrewmasterSC ()
		{
			GroupBuffs = new[] { "Legacy of the White Tiger" };
			PullSpells = new[] { "Jab" };
		}

		public override bool OutOfCombat ()
		{
			if (Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))) {
				if (Detox ())
					return true;
			}

			if (Me.IsMoving) {
				if (TigersLust ())
					return true;
			}

			if (LegacyoftheWhiteTiger (Me))
				return true;

			if (Health (Me) <= 0.8 && !Me.IsMoving) {
				if (SurgingMist (Me))
					return true;
			}

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			//	actions.precombat=flask,type=greater_draenic_agility_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/stance,choose=sturdy_ox
			if (OxStance ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_armor
			//	actions.precombat+=/dampen_harm

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

			if (!Me.CanParticipateInCombat) {
				if (Freedom ())
					return;
			}

			if (Interrupt ())
				return;

			if (InInstance && Dh && CombatRole == CombatRole.Tank) {
				if (AggroDizzyingHaze ())
					return;
			}

			// Slow enemy player
//			if (Cast ("Disable", () => !Target.HasAura ("Disable") && IsFleeing && IsPlayer))
//				return;
//
			if (Health (Me) < 0.45) {
				if (Healthstone ())
					return;
			}

			if (HasGlobalCooldown () && Gcd)
				return;

			//	actions=auto_attack
			//	actions+=/blood_fury,if=energy<=40
			if (Energy <= 40)
				BloodFury ();
			//	actions+=/berserking,if=energy<=40
			if (Energy <= 40)
				Berserking ();
			//	actions+=/arcane_torrent,if=chi.max-chi>=1&energy<=40
			if (ChiMax - Chi >= 1 && Energy <= 40)
				ArcaneTorrent ();
			//	actions+=/chi_sphere,if=talent.power_strikes.enabled&buff.chi_sphere.react&chi<4
//			if (HasSpell ("Power Strikes") && Me.HasAura ("Chi Sphere") && Chi < 4) {
//				if (ChiSphere())
//				return;
//			}
			//	actions+=/chi_brew,if=talent.chi_brew.enabled&chi.max-chi>=2&buff.elusive_brew_stacks.stack<=10&((charges=1&recharge_time<5)|charges=2|(target.time_to_die<15&(cooldown.touch_of_death.remains>target.time_to_die|glyph.touch_of_death.enabled)))
			if (HasSpell ("Chi Brew") && ChiMax - Chi >= 2 && ElusiveBrewStacks <= 10 && ((SpellCharges ("Chi Brew") == 1 && Cooldown ("Chi Brew") < 5) || SpellCharges ("Chi Brew") == 2 || (TimeToDie () < 15 && (Cooldown ("Touch of Death") > TimeToDie () || HasGlyph (123391))))) {
				if (ChiBrew ())
					API.Print ("1 Chi Brew");
			}
			//	actions+=/chi_brew,if=(chi<1&stagger.heavy)|(chi<2&buff.shuffle.down)
			if ((Chi < 1 && Me.HasAura ("Heavy Stagger")) || (Chi < 2 && !Me.HasAura ("Shuffle"))) {
				if (ChiBrew ())
					API.Print ("2 Chi Brew");
			}
			//	actions+=/gift_of_the_ox,if=buff.gift_of_the_ox.react&incoming_damage_1500ms

			//	actions+=/diffuse_magic,if=incoming_damage_1500ms&buff.fortifying_brew.down
//			if (CastSelf ("Diffuse Magic", () => Time > 1.5 && !Me.HasAura ("Fortifying Brew")))
//				return;
			//	actions+=/dampen_harm,if=incoming_damage_1500ms&buff.fortifying_brew.down&buff.elusive_brew_activated.down
			if (Health (Me) < 0.8 && Time > 1.5 && !Me.HasAura ("Fortifying Brew") && !Me.HasAura (115308)) {
				if (DampenHarm ())
					API.Print ("3 Dampen Harm");
			}
			//	actions+=/fortifying_brew,if=incoming_damage_1500ms&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down
			if (Health (Me) < 0.4 && Time > 1.5 && (!Me.HasAura ("Dampen Harm") || !Me.HasAura ("Diffuse Magic")) && !Me.HasAura (115308)) {
				if (FortifyingBrew ())
					API.Print ("4 Fortifying Brew");
			}
			//	actions+=/use_item,name=tablet_of_turnbuckle_teamwork,if=incoming_damage_1500ms&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.fortifying_brew.down&buff.elusive_brew_activated.down
			//	actions+=/elusive_brew,if=buff.elusive_brew_stacks.react>=9&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down
			if (Health (Me) < 0.3 && ElusiveBrewStacks >= 9 && (!Me.HasAura ("Dampen Harm") || !Me.HasAura ("Diffuse Magic")) && !Me.HasAura (115308)) {
				if (ElusiveBrew ())
					API.Print ("5 Elusive Brew");
			}
			//	actions+=/invoke_xuen,if=talent.invoke_xuen.enabled&target.time_to_die>15&buff.shuffle.remains>=3&buff.serenity.down
			if (HasSpell ("Invoke Xuen, the White Tiger") && TimeToDie () > 15 && Me.AuraTimeRemaining ("Shuffle") >= 3 && !Me.HasAura ("Serenity"))
				InvokeXuentheWhiteTiger ();
			//	actions+=/serenity,if=talent.serenity.enabled&cooldown.keg_smash.remains>6
			if (HasSpell ("Serenity") && Cooldown ("Keg Smash") > 6)
				Serenity ();
			//	actions+=/potion,name=draenic_armor,if=(buff.fortifying_brew.down&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down)

			//	actions+=/touch_of_death,if=target.health.percent<10&cooldown.touch_of_death.remains=0&((!glyph.touch_of_death.enabled&chi>=3&target.time_to_die<8)|(glyph.touch_of_death.enabled&target.time_to_die<5))
			if (Health () < 0.1 && Cooldown ("Touch of Death") == 0 && ((!HasGlyph (123391) && Chi >= 3) || (HasGlyph (123391)))) {
				if (TouchofDeath ())
					return;
			}


			if (!Me.HasAura ("Tiger Power")) {
				if (TigerPalm ())
					return;
			}

			//	actions+=/call_action_list,name=st,if=active_enemies<3
			if (EnemyInRange (10) < 3)
				St ();
			//	actions+=/call_action_list,name=aoe,if=active_enemies>=3
			if (EnemyInRange (10) >= 3)
				AOE ();
		}

		public void St ()
		{
			var targets = Adds;
			targets.Add (Target);

			//	actions.st=purifying_brew,if=stagger.heavy
			if (Me.HasAura ("Heavy Stagger")) {
				if (PurifyingBrew ())
					API.Print ("6 Purifying Brew");
			}
			//	actions.st+=/blackout_kick,if=buff.shuffle.down
			if (!Me.HasAura ("Shuffle")) {
				if (BlackoutKick ())
					return;
			}
			//	actions.st+=/purifying_brew,if=buff.serenity.up
			if (Me.HasAura ("Serenity")) {
				if (PurifyingBrew ())
					API.Print ("7 Purifying Brew");
			}
			//	actions.st+=/chi_explosion,if=chi>=3
			if (Chi >= 3) {
				if (ChiExplosion ())
					return;
			}
			//	actions.st+=/purifying_brew,if=stagger.moderate&buff.shuffle.remains>=6
			if (Me.HasAura ("Moderate Stagger") && Me.AuraTimeRemaining ("Shuffle") >= 6) {
				if (PurifyingBrew ())
					API.Print ("8 Purifying Brew");
			}
			//	actions.st+=/guard,if=(charges=1&recharge_time<5)|charges=2|target.time_to_die<15
			if ((SpellCharges ("Guard") == 1 && Cooldown ("Guard") < 5) || SpellCharges ("Guard") == 2 || TimeToDie () < 15)
				Guard ();
			//	actions.st+=/guard,if=incoming_damage_10s>=health.max*0.5
			if (DamageTaken (10000) > Health (Me) * 0.5)
				Guard ();
			//	actions.st+=/chi_brew,if=target.health.percent<10&cooldown.touch_of_death.remains=0&chi.max-chi>=2&(buff.shuffle.remains>=6|target.time_to_die<buff.shuffle.remains)&!glyph.touch_of_death.enabled
			if (Health () < 0.1 && Cooldown ("Touch of Death") == 0 && ChiMax - Chi >= 2 && (Me.AuraTimeRemaining ("Shuffle") >= 6 || TimeToDie () < Me.AuraTimeRemaining ("Shuffle")) && !HasGlyph (123391)) {
				if (ChiBrew ())
					API.Print ("9 Chi Brew");
			}
			//	actions.st+=/keg_smash,if=chi.max-chi>=2&!buff.serenity.remains
			if (ChiMax - Chi >= 2 && !Me.HasAura ("Serenity")) {
				if (KegSmash ())
					return;
			}
			//	actions.st+=/blackout_kick,if=buff.shuffle.remains<=3&cooldown.keg_smash.remains>=gcd
			if (Me.AuraTimeRemaining ("Shuffle") <= 3 && Cooldown ("Keg Smash") >= 1.5) {
				if (BlackoutKick ())
					return;
			}
			//	actions.st+=/blackout_kick,if=buff.serenity.up
			if (Me.HasAura ("Serenity")) {
				if (BlackoutKick ())
					return;
			}
			//	actions.st+=/chi_burst,if=energy.time_to_max>2&buff.serenity.down
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity")) {
				if (ChiBurst ())
					return;
			}
			//	actions.st+=/chi_wave,if=energy.time_to_max>2&buff.serenity.down
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity")) {
				if (ChiWave ())
					return;
			}
			//	actions.st+=/zen_sphere,cycle_targets=1,if=!dot.zen_sphere.ticking&energy.time_to_max>2&buff.serenity.down
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity")) {
				var players = Group.GetGroupMemberObjects ();
				CycleTarget = players.Where (p => !p.IsDead && p.IsInLoS && Range (p) <= 40 && !p.HasAura ("Zen Sphere", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (ZenSphere (CycleTarget))
						return;
				}
				if (!Me.HasAura ("Zen Sphere", true)) {
					if (ZenSphere (Me))
						return;
				}
			}
			//	actions.st+=/blackout_kick,if=chi.max-chi<2
			if (ChiMax - Chi < 2) {
				if (BlackoutKick ())
					return;
			}
			//	actions.st+=/expel_harm,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
			if (ChiMax - Chi >= 1 && Cooldown ("Keg Smash") >= 1.5 && (Energy + (EnergyRegen * Cooldown ("Keg Smash"))) >= 80) {
				if (ExpelHarm ())
					return;
			}
			//	actions.st+=/jab,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&cooldown.expel_harm.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
			if (ChiMax - Chi >= 1 && Cooldown ("Keg Smash") >= 1.5 && Cooldown ("Expel Harm") >= 1.5 && (Energy + (EnergyRegen * Cooldown ("Keg Smash"))) >= 80) {
				if (Jab ())
					return;
			}
			//	actions.st+=/tiger_palm
			if (TigerPalm ())
				return;
		}

		public void AOE ()
		{
			var targets = Adds;
			targets.Add (Target);

			//	actions.aoe=purifying_brew,if=stagger.heavy
			if (Me.HasAura ("Heavy Stagger")) {
				if (PurifyingBrew ())
					API.Print ("10 Purifying Brew");
			}
			//	actions.aoe+=/blackout_kick,if=buff.shuffle.down
			if (!Me.HasAura ("Shuffle")) {
				if (BlackoutKick ())
					return;
			}
			//	actions.aoe+=/purifying_brew,if=buff.serenity.up
			if (Me.HasAura ("Serenity")) {
				if (PurifyingBrew ())
					API.Print ("11 Purifying Brew");
			}
			//	actions.aoe+=/chi_explosion,if=chi>=4
			if (Chi >= 4) {
				if (ChiExplosion ())
					return;
			}
			//	actions.aoe+=/purifying_brew,if=stagger.moderate&buff.shuffle.remains>=6
			if (Me.HasAura ("Moderate Stagger") && Me.AuraTimeRemaining ("Shuffle") >= 6) {
				if (PurifyingBrew ())
					API.Print ("12 Purifying Brew");
			}
			//	actions.aoe+=/guard,if=(charges=1&recharge_time<5)|charges=2|target.time_to_die<15
			if ((SpellCharges ("Guard") == 1 && Cooldown ("Guard") < 5) || SpellCharges ("Guard") == 2 || TimeToDie () < 15)
				Guard ();
			//	actions.aoe+=/guard,if=incoming_damage_10s>=health.max*0.5
			if (DamageTaken (10000) > Health (Me) * 0.5)
				Guard ();
			//	actions.aoe+=/chi_brew,if=target.health.percent<10&cooldown.touch_of_death.remains=0&chi<=3&chi>=1&(buff.shuffle.remains>=6|target.time_to_die<buff.shuffle.remains)&!glyph.touch_of_death.enabled
			if (Health () < 0.1 && Cooldown ("Touch of Death") == 0 && Chi <= 3 && Chi >= 1 && (Me.AuraTimeRemaining ("Shuffle") >= 6 || TimeToDie () < Me.AuraTimeRemaining ("Shuffle")) && !HasGlyph (123391)) {
				if (ChiBrew ())
					API.Print ("13 Chi Brew");
			}
			//	actions.aoe+=/keg_smash,if=chi.max-chi>=2&!buff.serenity.remains
			if (ChiMax - Chi >= 2 && !Me.HasAura ("Serenity")) {
				if (KegSmash ())
					return;
			}
			//	actions.aoe+=/blackout_kick,if=buff.shuffle.remains<=3&cooldown.keg_smash.remains>=gcd
			if (Me.AuraTimeRemaining ("Shuffle") <= 3 && Cooldown ("Keg Smash") >= 1.5) {
				if (BlackoutKick ())
					return;
			}
			//	actions.aoe+=/blackout_kick,if=buff.serenity.up
			if (Me.HasAura ("Serenity")) {
				if (BlackoutKick ())
					return;
			}
			//	actions.aoe+=/rushing_jade_wind,if=chi.max-chi>=1&buff.serenity.down
			if (ChiMax - Chi >= 1 && !Me.HasAura ("Serenity")) {
				if (RushingJadeWind ())
					return;
			}
			//	actions.aoe+=/chi_burst,if=energy.time_to_max>2&buff.serenity.down
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity")) {
				if (ChiBurst ())
					return;
			}
			//	actions.aoe+=/chi_wave,if=energy.time_to_max>2&buff.serenity.down
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity")) {
				if (ChiWave ())
					return;
			}
			//	actions.aoe+=/zen_sphere,cycle_targets=1,if=!dot.zen_sphere.ticking&energy.time_to_max>2&buff.serenity.down
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity")) {
				var players = Group.GetGroupMemberObjects ();
				CycleTarget = players.Where (p => !p.IsDead && p.IsInLoS && Range (p) <= 40 && !p.HasAura ("Zen Sphere", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (ZenSphere (CycleTarget))
						return;
				}
				if (!Me.HasAura ("Zen Sphere", true)) {
					if (ZenSphere (Me))
						return;
				}
			}
			//	actions.aoe+=/blackout_kick,if=chi.max-chi<2
			if (ChiMax - Chi < 2) {
				if (BlackoutKick ())
					return;
			}
			//	actions.aoe+=/expel_harm,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
			if (ChiMax - Chi >= 1 && Cooldown ("Keg Smash") >= 1.5 && (Energy + (EnergyRegen * Cooldown ("Keg Smash"))) >= 80) {
				if (ExpelHarm ())
					return;
			}
			//	actions.aoe+=/jab,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&cooldown.expel_harm.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
			if (ChiMax - Chi >= 1 && Cooldown ("Keg Smash") >= 1.5 && Cooldown ("Expel Harm") >= 1.5 && (Energy + (EnergyRegen * Cooldown ("Keg Smash"))) >= 80) {
				if (Jab ())
					return;
			}


			if (InInstance && CombatRole == CombatRole.Tank) {
				CycleTarget = targets.Where (u => u.Target != Me && u.IsInLoS && Range (u) <= 5).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (TigerPalm (CycleTarget))
						return;
				}
			}

			if (Sck) {
				if (SpinningCraneKick ())
					return;
			}

			//	actions.aoe+=/tiger_palm
			if (TigerPalm ())
				return;

		}
	}
}
