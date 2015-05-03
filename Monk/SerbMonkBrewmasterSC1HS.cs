using System;
using ReBot.API;
using Newtonsoft.Json;
using System.Linq;

namespace ReBot
{
	[Rotation ("Serb Monk Brewmaster 1H Serenity SC", "Serb", WoWClass.Monk, Specialization.MonkBrewmaster, 5, 25)]

	public class SerbMonkBrewmasterSC1HS : SerbMonk
	{
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public SerbMonkBrewmasterSC1HS ()
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
			if (HasSpell ("Chi Brew") && ChiMax - Chi >= 2 && Me.GetAura ("Elusive Brew").StackCount <= 10 && ((SpellCharges ("Chi Brew") == 1 && Cooldown ("Chi Brew") < 5) || SpellCharges ("Chi Brew") == 2 || (TimeToDie () < 15 && (Cooldown ("Touch of Death") > TimeToDie () || HasGlyph (123391)))))
				ChiBrew ();
			//	actions+=/chi_brew,if=(chi<1&stagger.heavy)|(chi<2&buff.shuffle.down)
			if ((Chi < 1 && Me.HasAura ("Heavy Stagger")) || (Chi < 2 && !Me.HasAura ("Shuffle")))
				ChiBrew ();
			//	actions+=/gift_of_the_ox,if=buff.gift_of_the_ox.react&incoming_damage_1500ms

			//	actions+=/diffuse_magic,if=incoming_damage_1500ms&buff.fortifying_brew.down
			//			if (CastSelf ("Diffuse Magic", () => Time > 1.5 && !Me.HasAura ("Fortifying Brew")))
			//				return;
			//	actions+=/dampen_harm,if=incoming_damage_1500ms&buff.fortifying_brew.down&buff.elusive_brew_activated.down
			if (Health (Me) < 0.8 && Time > 1.5 && !Me.HasAura ("Fortifying Brew") && !Me.HasAura ("Elusive Brew"))
				DampenHarm ();
			//	actions+=/fortifying_brew,if=incoming_damage_1500ms&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down
			if (Health (Me) < 0.4 && Time > 1.5 && (!Me.HasAura ("Dampen Harm") || !Me.HasAura ("Diffuse Magic")) && !Me.HasAura ("Elusive Brew"))
				FortifyingBrew ();
			//	actions+=/use_item,name=tablet_of_turnbuckle_teamwork,if=incoming_damage_1500ms&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.fortifying_brew.down&buff.elusive_brew_activated.down
			//	actions+=/elusive_brew,if=buff.elusive_brew_stacks.react>=9&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down
			if (Health (Me) < 0.3 && Me.GetAura ("Elusive Brew").StackCount >= 9 && (!Me.HasAura ("Dampen Harm") || !Me.HasAura ("Diffuse Magic")) && !Me.HasAura ("Elusive Brew"))
				ElusiveBrew ();
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
			if (Me.HasAura ("Heavy Stagger"))
				PurifyingBrew ();
			//	actions.st+=/blackout_kick,if=buff.shuffle.down
			if (!Me.HasAura ("Shuffle")) {
				if (BlackoutKick ())
					return;
			}
			//	actions.st+=/purifying_brew,if=buff.serenity.up
			if (Me.HasAura ("Serenity"))
				PurifyingBrew ();
			//	actions.st+=/chi_explosion,if=chi>=3
			if (Chi >= 3) {
				if (ChiExplosion ())
					return;
			}
			//	actions.st+=/purifying_brew,if=stagger.moderate&buff.shuffle.remains>=6
			if (Me.HasAura ("Moderate Stagger") && Me.AuraTimeRemaining ("Shuffle") >= 6)
				PurifyingBrew ();
			//	actions.st+=/guard,if=(charges=1&recharge_time<5)|charges=2|target.time_to_die<15
			if ((SpellCharges ("Guard") == 1 && Cooldown ("Guard") < 5) || SpellCharges ("Guard") == 2 || TimeToDie () < 15)
				Guard ();
			//	actions.st+=/guard,if=incoming_damage_10s>=health.max*0.5
			if (DamageTaken (10000) > Health (Me) * 0.5) {
				API.Print ("Damage Taken" + DamageTaken (10000));
				Guard ();
			}
			//	actions.st+=/chi_brew,if=target.health.percent<10&cooldown.touch_of_death.remains=0&chi.max-chi>=2&(buff.shuffle.remains>=6|target.time_to_die<buff.shuffle.remains)&!glyph.touch_of_death.enabled
			if (Health () < 0.1 && Cooldown ("Touch of Death") == 0 && ChiMax - Chi >= 2 && (Me.AuraTimeRemaining ("Shuffle") >= 6 || TimeToDie () < Me.AuraTimeRemaining ("Shuffle")) && !HasGlyph (123391))
				ChiBrew ();
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
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity") && !Me.HasAura ("Zen Sphere", true)) {
				if (ZenSphere ())
					return;
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
			if (Me.HasAura ("Heavy Stagger"))
				PurifyingBrew ();
			//	actions.aoe+=/blackout_kick,if=buff.shuffle.down
			if (!Me.HasAura ("Shuffle")) {
				if (BlackoutKick ())
					return;
			}
			//	actions.aoe+=/purifying_brew,if=buff.serenity.up
			if (Me.HasAura ("Serenity"))
				PurifyingBrew ();
			//	actions.aoe+=/chi_explosion,if=chi>=4
			if (Chi >= 4) {
				if (ChiExplosion ())
					return;
			}
			//	actions.aoe+=/purifying_brew,if=stagger.moderate&buff.shuffle.remains>=6
			if (Me.HasAura ("Moderate Stagger") && Me.AuraTimeRemaining ("Shuffle") >= 6)
				PurifyingBrew ();
			//	actions.aoe+=/guard,if=(charges=1&recharge_time<5)|charges=2|target.time_to_die<15
			if ((SpellCharges ("Guard") == 1 && Cooldown ("Guard") < 5) || SpellCharges ("Guard") == 2 || TimeToDie () < 15)
				Guard ();
			//	actions.aoe+=/guard,if=incoming_damage_10s>=health.max*0.5
			if (DamageTaken (10000) > Health (Me) * 0.5) {
				API.Print ("Damage Taken" + DamageTaken (10000));
				Guard ();
			}
			//	actions.aoe+=/chi_brew,if=target.health.percent<10&cooldown.touch_of_death.remains=0&chi<=3&chi>=1&(buff.shuffle.remains>=6|target.time_to_die<buff.shuffle.remains)&!glyph.touch_of_death.enabled
			if (Health () < 0.1 && Cooldown ("Touch of Death") == 0 && Chi <= 3 && Chi >= 1 && (Me.AuraTimeRemaining ("Shuffle") >= 6 || TimeToDie () < Me.AuraTimeRemaining ("Shuffle")) && !HasGlyph (123391))
				ChiBrew ();
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
			if (TimeToMaxEnergy > 2 && !Me.HasAura ("Serenity") && !Me.HasAura ("Zen Sphere", true)) {
				if (ZenSphere ())
					return;
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
			//	actions.aoe+=/tiger_palm
			if (TigerPalm ())
				return;

		}
	}
}

//	actions=auto_attack
//	actions+=/blood_fury,if=energy<=40
//	actions+=/berserking,if=energy<=40
//	actions+=/arcane_torrent,if=chi.max-chi>=1&energy<=40
//	actions+=/chi_sphere,if=talent.power_strikes.enabled&buff.chi_sphere.react&chi<4
//	actions+=/chi_brew,if=talent.chi_brew.enabled&chi.max-chi>=2&buff.elusive_brew_stacks.stack<=10&((charges=1&recharge_time<5)|charges=2|(target.time_to_die<15&(cooldown.touch_of_death.remains>target.time_to_die|glyph.touch_of_death.enabled)))
//	actions+=/chi_brew,if=(chi<1&stagger.heavy)|(chi<2&buff.shuffle.down)
//	actions+=/gift_of_the_ox,if=buff.gift_of_the_ox.react&incoming_damage_1500ms
//	actions+=/diffuse_magic,if=incoming_damage_1500ms&buff.fortifying_brew.down
//	actions+=/dampen_harm,if=incoming_damage_1500ms&buff.fortifying_brew.down&buff.elusive_brew_activated.down
//	actions+=/fortifying_brew,if=incoming_damage_1500ms&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down
//	actions+=/use_item,name=tablet_of_turnbuckle_teamwork,if=incoming_damage_1500ms&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.fortifying_brew.down&buff.elusive_brew_activated.down
//	actions+=/elusive_brew,if=buff.elusive_brew_stacks.react>=9&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down
//	actions+=/invoke_xuen,if=talent.invoke_xuen.enabled&target.time_to_die>15&buff.shuffle.remains>=3&buff.serenity.down
//	actions+=/serenity,if=talent.serenity.enabled&cooldown.keg_smash.remains>6
//	actions+=/potion,name=draenic_armor,if=(buff.fortifying_brew.down&(buff.dampen_harm.down|buff.diffuse_magic.down)&buff.elusive_brew_activated.down)
//	actions+=/touch_of_death,if=target.health.percent<10&cooldown.touch_of_death.remains=0&((!glyph.touch_of_death.enabled&chi>=3&target.time_to_die<8)|(glyph.touch_of_death.enabled&target.time_to_die<5))
//	actions+=/call_action_list,name=st,if=active_enemies<3
//	actions+=/call_action_list,name=aoe,if=active_enemies>=3
//
//	actions.st=purifying_brew,if=stagger.heavy
//	actions.st+=/blackout_kick,if=buff.shuffle.down
//	actions.st+=/purifying_brew,if=buff.serenity.up
//	actions.st+=/chi_explosion,if=chi>=3
//	actions.st+=/purifying_brew,if=stagger.moderate&buff.shuffle.remains>=6
//	actions.st+=/guard,if=(charges=1&recharge_time<5)|charges=2|target.time_to_die<15
//	actions.st+=/guard,if=incoming_damage_10s>=health.max*0.5
//	actions.st+=/chi_brew,if=target.health.percent<10&cooldown.touch_of_death.remains=0&chi.max-chi>=2&(buff.shuffle.remains>=6|target.time_to_die<buff.shuffle.remains)&!glyph.touch_of_death.enabled
//	actions.st+=/keg_smash,if=chi.max-chi>=2&!buff.serenity.remains
//	actions.st+=/blackout_kick,if=buff.shuffle.remains<=3&cooldown.keg_smash.remains>=gcd
//	actions.st+=/blackout_kick,if=buff.serenity.up
//	actions.st+=/chi_burst,if=energy.time_to_max>2&buff.serenity.down
//	actions.st+=/chi_wave,if=energy.time_to_max>2&buff.serenity.down
//	actions.st+=/zen_sphere,cycle_targets=1,if=!dot.zen_sphere.ticking&energy.time_to_max>2&buff.serenity.down
//	actions.st+=/blackout_kick,if=chi.max-chi<2
//	actions.st+=/expel_harm,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
//	actions.st+=/jab,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&cooldown.expel_harm.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
//	actions.st+=/tiger_palm
//
//	actions.aoe=purifying_brew,if=stagger.heavy
//	actions.aoe+=/blackout_kick,if=buff.shuffle.down
//	actions.aoe+=/purifying_brew,if=buff.serenity.up
//	actions.aoe+=/chi_explosion,if=chi>=4
//	actions.aoe+=/purifying_brew,if=stagger.moderate&buff.shuffle.remains>=6
//	actions.aoe+=/guard,if=(charges=1&recharge_time<5)|charges=2|target.time_to_die<15
//	actions.aoe+=/guard,if=incoming_damage_10s>=health.max*0.5
//	actions.aoe+=/chi_brew,if=target.health.percent<10&cooldown.touch_of_death.remains=0&chi<=3&chi>=1&(buff.shuffle.remains>=6|target.time_to_die<buff.shuffle.remains)&!glyph.touch_of_death.enabled
//	actions.aoe+=/keg_smash,if=chi.max-chi>=2&!buff.serenity.remains
//	actions.aoe+=/blackout_kick,if=buff.shuffle.remains<=3&cooldown.keg_smash.remains>=gcd
//	actions.aoe+=/blackout_kick,if=buff.serenity.up
//	actions.aoe+=/rushing_jade_wind,if=chi.max-chi>=1&buff.serenity.down
//	actions.aoe+=/chi_burst,if=energy.time_to_max>2&buff.serenity.down
//	actions.aoe+=/chi_wave,if=energy.time_to_max>2&buff.serenity.down
//	actions.aoe+=/zen_sphere,cycle_targets=1,if=!dot.zen_sphere.ticking&energy.time_to_max>2&buff.serenity.down
//	actions.aoe+=/blackout_kick,if=chi.max-chi<2
//	actions.aoe+=/expel_harm,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
//	actions.aoe+=/jab,if=chi.max-chi>=1&cooldown.keg_smash.remains>=gcd&cooldown.expel_harm.remains>=gcd&(energy+(energy.regen*(cooldown.keg_smash.remains)))>=80
//	actions.aoe+=/tiger_palm
