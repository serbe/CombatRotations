using System;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;
using System.Collections.Generic;

namespace ReBot
{
	public abstract class SerbHunter : CombatRotation
	{
		// Vars

		public enum ExoticMunitionsType
		{
			NoExoticMunitions,
			PoisonedAmmo,
			IncendiaryAmmo,
			FrozenAmmo,
		}

		public enum PetSlot
		{
			PetSlot1,
			PetSlot2,
			PetSlot3,
			PetSlot4,
			PetSlot5,
		}

		public enum UsePet
		{
			UsePet,
			NoPet,
			LoneWolfCrit,
			LoneWolfMastery,
			LoneWolfHaste,
			LoneWolfStats,
			LoneWolfStamina,
			LoneWolfMultistrike,
			LoneWolfVersatility,
			LoneWolfSpellpower,
		}

		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Run to enemy")]
		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool Aoe = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public PlayerObject Player;
		public UnitObject Unit;
		public Int32 OraliusWhisperingCrystalId = 118922;
		public Int32 CrystalOfInsanityId = 86569;

		// Check

		public bool C (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (Cast (s, u))
				return true;
			API.Print ("False Cast " + s + " with " + u.CombatRange + " range and " + Focus + " focus, with " + u.Distance + " distance");
			return false;
		}

		public bool CPD (string s, UnitObject u = null, int d = 800)
		{
			u = u ?? Target;
			if (CastPreventDouble (s, null, u, d))
				return true;
			API.Print ("False CastPreventDouble " + s + " with " + u.CombatRange + " range " + d + " delay");
			return false;
		}

		public bool CS (string s)
		{
			if (CastSelf (s))
				return true;
			API.Print ("False CastSelf " + s);
			return false;
		}

		public bool CSPD (string s, int d = 800)
		{
			if (CastSelfPreventDouble (s, null, d))
				return true;
			API.Print ("False CastSelfPreventDouble " + s + " with " + d + " delay");
			return false;
		}

		public bool COT (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (CastOnTerrain (s, u.Position))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range and " + Focus + " focus, with " + u.Distance + " distance");
			return false;
		}

		public bool COTPD (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (CastOnTerrainPreventDouble (s, u.Position))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range and " + Focus + " focus, with " + u.Distance + " distance");
			return false;
		}

		public bool Range (int r, UnitObject u = null, int l = 0)
		{
			u = u ?? Target;
			if (l != 0)
				return u.IsInLoS && u.CombatRange <= r && u.CombatRange >= l;
			return u.IsInLoS && u.CombatRange <= r;
		}

		public bool Danger (UnitObject u = null, int r = 0, int e = 2)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		public bool DangerBoss (UnitObject u = null, int r = 0, int e = 6)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		public bool InRaid {
			get {
				return API.MapInfo.Type == MapType.Raid;
			}
		}

		public bool InInstance {
			get {
				return API.MapInfo.Type == MapType.Instance;
			}
		}

		public bool InArena {
			get {
				return API.MapInfo.Type == MapType.Arena;
			}
		}

		public bool InBg {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsPlayer;
		}

		public bool IsElite (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsElite ();
		}

		public bool IsInEnrage (UnitObject o)
		{
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			return false;
		}

		public bool IsDispeling (UnitObject o)
		{
			bool result1 = false;
			bool result2 = false;
			if (o.HasAura ("Freezing Fog") || o.HasAura ("Mark of the Wild") || o.HasAura ("Rejuvenation") || o.HasAura ("Predator's Swiftness") || o.HasAura ("Nature's Swiftness") || o.HasAura ("Arcane Intellect") || o.HasAura ("Ice Barrier") || o.HasAura ("Earth Shield") || o.HasAura ("Spiritwalker's Grace") || o.HasAura ("Dark Intent") || o.HasAura ("Alter Time") || o.HasAura ("Arcane Power") || o.HasAura ("Presence of Mind") || o.HasAura ("Brain Freeze") || o.HasAura ("Icy Veins") || o.HasAura ("Hand of Protection") || o.HasAura ("Hand of Freedom") || o.HasAura ("Hand of Sacrifice") || o.HasAura ("Blessing of Might") || o.HasAura ("Eternal Flame") || o.HasAura ("Selfless Healer"))
				result1 = true;
			if (o.HasAura ("Execution Sentence") || o.HasAura ("Hand of Purity") || o.HasAura ("Speed of Light") || o.HasAura ("Long Arm of the Law") || o.HasAura ("Illuminated Healing") || o.HasAura ("Power Word: Shield") || o.HasAura ("Power Word: Fortitude") || o.HasAura ("Fear Ward") || o.HasAura ("Prayer of Mending") || o.HasAura ("Power Infusion") || o.HasAura ("Angelic Feather") || o.HasAura ("Body and Soul") || o.HasAura ("Borrowed Time") || o.HasAura ("Unleash Fury") || o.HasAura ("Elemental Overload") || o.HasAura ("Ancestral Swiftness"))
				result2 = true;
			return result1 || result2;
		}

		public bool IsNotForDamage (UnitObject o)
		{
			if (o.HasAura ("Fear") || o.HasAura ("Polymorph") || o.HasAura ("Gouge") || o.HasAura ("Paralysis") || o.HasAura ("Blind") || o.HasAura ("Hex"))
				return true;
			return false;
		}

		public bool Usable (string s, double d = 0)
		{ 
			if (d == 0)
				return HasSpell (s) && Cooldown (s) == 0;
			return HasSpell (s) && Cooldown (s) <= d;
		}

		public bool HasFocus (double i)
		{
			if (Me.HasAura ("Burning Adrenaline"))
				i = 0;
			return Focus >= i;
		}

		// Multi-Shot and Aimed Shot
		public bool HasAmFocus (double i)
		{
			if (Me.HasAura ("Multi-Shot"))
				i = i - 20;
			if (Me.HasAura ("Thrill of the Hunt"))
				i = i - 20;
			if (Me.HasAura ("Burning Adrenaline"))
				i = 0;
			return Focus >= i;
		}

		// Arcane Shot
		public bool HasArcaneFocus (double i)
		{
			if (Me.HasAura ("Thrill of the Hunt"))
				i = i - 20;
			if (Me.HasAura ("Burning Adrenaline"))
				i = 0;
			return Focus >= i;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject u in API.CollectUnits(range)) {
				if ((u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && u.InCombat && IsNotForDamage (u)) {
					x++;
				}
			}
			return x > 0;
		}

		public bool MeIsBusy ()
		{
			if (Me.HasAura ("Feign Death"))
				return true; 
			if (Me.IsChanneling)
				return true;
			if (Me.IsCasting)
				return true;
			if (Me.HasAura ("Drink"))
				return true;

			return false;
		}


		// Getters

		public List<UnitObject> Enemy {
			get {
				var Enemy = Adds;
				Enemy.Add (Target);
				return Enemy;
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		public int Focus {
			get {
				return Me.GetPower (WoWPowerType.Focus);
			}
		}

		public int FocusMax {
			get {
				return API.ExecuteLua <int> ("return UnitPowerMax(\"player\");");
			}
		}

		public int FocusDeflict {
			get {
				return FocusMax - Focus;
			}
		}

		public double FocusRegen {
			get {
				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
				return Convert.ToDouble (activeRegen);
			}
		}

		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		public double SleepTime {
			get {
				TimeSpan currentSleepTime = DateTime.Now.Subtract (StartSleepTime);
				return currentSleepTime.TotalSeconds;
			}
		}

		public double TimeToStartBattle {
			get {
				return API.ExecuteLua<double> ("return GetBattlefieldInstanceRunTime()") / 1000;
			}
		}

		public int ActiveEnemies (int range)
		{
			int x = 0;
			foreach (UnitObject u in API.CollectUnits (range)) {
				if ((u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && u.InCombat) {
					x++;
				}
			}
			return x;
		}

		public int EnemyWithTarget (UnitObject o, int range)
		{
			return Adds.Where (x => x.IsInCombatRangeAndLoS && Vector3.Distance (x.Position, o.Position) <= range).ToList ().Count;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double CooldownById (Int32 i)
		{ 
			return SpellCooldown (i) < 0 ? 0 : SpellCooldown (i);
		}

		public double TimeToDie (UnitObject o)
		{
			if (o != null)
				return o.Health / Ttd;
			return 0;
		}

		// Combo

		public bool ExoticMunitions (ExoticMunitionsType e)
		{
			if (e == ExoticMunitionsType.PoisonedAmmo) {
				return Usable ("Poisoned Ammo") && !Me.HasAura ("Poisoned Ammo") && CSPD ("Poisoned Ammo");
			}
			if (e == ExoticMunitionsType.IncendiaryAmmo) {
				return Usable ("Incendiary Ammo") && !Me.HasAura ("Incendiary Ammo") && CSPD ("Incendiary Ammo");
			}
			if (e == ExoticMunitionsType.FrozenAmmo) {
				return Usable ("Frozen Ammo") && !Me.HasAura ("Frozen Ammo") && CSPD ("Frozen Ammo");
			}
			return false;
		}

		public bool SummonPet (PetSlot s)
		{
			if (CSPD ("Revive Pet", 5000))
				return true;
			if (s == PetSlot.PetSlot1 && CSPD ("Call Pet 1", 5000))
				return true;
			if (s == PetSlot.PetSlot2 && CSPD ("Call Pet 2", 5000))
				return true;
			if (s == PetSlot.PetSlot3 && CSPD ("Call Pet 3", 5000))
				return true;
			if (s == PetSlot.PetSlot4 && CSPD ("Call Pet 4", 5000))
				return true;
			if (s == PetSlot.PetSlot5 && CSPD ("Call Pet 5", 5000))
				return true;
			return false;
		}

		public bool LoneWolf (UsePet p)
		{
			if (Me.HasAlivePet) {
				Me.PetDismiss ();
				return true;
			}
			if (p == UsePet.LoneWolfCrit && !Me.HasAura ("Lone Wolf: Ferocity of the Raptor") && CSPD ("Lone Wolf: Ferocity of the Raptor", 1500))
				return true;
			if (p == UsePet.LoneWolfMastery && !Me.HasAura ("Lone Wolf: Grace of the Cat") && CSPD ("Lone Wolf: Grace of the Cat", 1500))
				return true;
			if (p == UsePet.LoneWolfHaste && !Me.HasAura ("Lone Wolf: Haste of the Hyena") && CSPD ("Lone Wolf: Haste of the Hyena", 1500))
				return true;
			if (p == UsePet.LoneWolfStats && !Me.HasAura ("Lone Wolf: Power of the Primates") && CSPD ("Lone Wolf: Power of the Primates", 1500))
				return true;
			if (p == UsePet.LoneWolfStamina && !Me.HasAura ("Lone Wolf: Fortitude of the Bear") && CSPD ("Lone Wolf: Fortitude of the Bear", 1500))
				return true;
			if (p == UsePet.LoneWolfMultistrike && !Me.HasAura ("Lone Wolf: Quickness of the Dragonhawk") && CSPD ("Lone Wolf: Quickness of the Dragonhawk", 1500))
				return true;
			if (p == UsePet.LoneWolfVersatility && !Me.HasAura ("Lone Wolf: Versatility of the Ravager") && CSPD ("Lone Wolf: Versatility of the Ravager", 1500))
				return true;
			if (p == UsePet.LoneWolfSpellpower && !Me.HasAura ("Lone Wolf: Wisdom of the Serpent") && CSPD ("Lone Wolf: Wisdom of the Serpent", 1500))
				return true;
			return false;
		}

		public bool Tranquilizing ()
		{
			if (Usable ("Tranquilizing Shot")) {
				if (InArena || InBg) {
					Unit = Enemy.Where (x => x.IsInCombatRangeAndLoS && x.IsPlayer && x.Auras.Any (a => a.IsStealable)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && TranquilizingShot (Unit))
						return true;
				} else {
					Unit = Enemy.Where (x => x.IsInCombatRangeAndLoS && x.Auras.Any (a => a.IsStealable)).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && TranquilizingShot (Unit))
						return true;
				}
			}
			return false;
		}

		public bool UseMisdirection ()
		{
			if (Usable ("Misdirection") && Me.HasAlivePet) {
				if (InInstance || InRaid) {
					Unit = Group.GetGroupMemberObjects ().Where (u => !u.IsDead && Range (100, u) && u.IsTank).DefaultIfEmpty (null).FirstOrDefault ();
					if (Unit != null && Misdirection (Unit))
						return true;
				}
				if (Me.Focus != null && Misdirection (Me.Focus))
					return true;
				if (HasGlyph (56829) && Misdirection (Me.Pet, 8000))
					return true;
				if (Misdirection (Me.Pet))
					return true;
			}
			return false;
		}

		public bool Interrupt ()
		{
			if (Usable ("Counter Shot")) {
				Unit = Enemy.Where (x => x.IsInCombatRangeAndLoS && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && CounterShot (Unit))
					return true;
			}
			return false;
		}

		public bool Freedom ()
		{
			return WilloftheForsaken () || EveryManforHimself () || MastersCall ();
		}


		// Spells

		public bool TrapLauncher ()
		{
			return Usable ("Trap Launcher") && CS ("Trap Launcher");
		}

		public bool ConcussiveShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Concussive Shot") && !u.HasAura ("Concussive Shot") && Range (40, u) && C ("Concussive Shot", u);
		}

		public bool CounterShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Counter Shot") && Range (40, u) && C ("Counter Shot", u);
		}

		public bool MendPet ()
		{
			return Usable ("Mend Pet") && Me.HasAlivePet && Me.Pet.HasAura ("Mend Pet") && Me.Pet.CombatRange <= 45 && C ("Mend Pet");
		}

		public bool TranquilizingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Tranquilizing Shot") && (HasGlyph (119384) || HasFocus (50)) && Range (40, u) && C ("Tranquilizing Shot", u);
		}

		public bool BindingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Binding Shot") && Range (30, u) && COT ("Binding Shot", u);
		}

		public bool FreezingTrap (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Freezing Trap") && Me.HasAura ("Trap Launcher") && Range (40, u) && COT ("Freezing Trap", u);
		}

		public bool WilloftheForsaken ()
		{
			return Usable ("Will of the Forsaken") && CS ("Will of the Forsaken");
		}

		public bool EveryManforHimself ()
		{
			return Usable ("Every Man for Himself") && CS ("Every Man for Himself");
		}

		public bool MastersCall ()
		{
			return Usable ("Master's Call") && Me.HasAlivePet && Me.Pet.CombatRange <= 40 && CS ("Master's Call");
		}

		public bool LastStand ()
		{
			return Usable ("Last Stand") && Me.HasAlivePet && CS ("Last Stand");
		}

		public bool RoarofSacrifice ()
		{
			return Usable ("Roar of Sacrifice") && Me.HasAlivePet && Me.Pet.CombatRange <= 40 && CS ("Roar of Sacrifice");
		}

		public bool Exhilaration ()
		{
			return Usable ("Exhilaration") && C ("Exhilaration");
		}

		public bool Deterrence ()
		{
			return Usable ("Deterrence") && C ("Deterrence");
		}

		public bool FeignDeath ()
		{
			return Usable ("Feign Death") && !Me.HasAura ("Feign Death") && C ("Feign Death");
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("Blood Fury");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && CS ("Arcane Torrent");
		}

		public bool Stampede (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Stampede") && Danger (u) && C ("Stampede", u);
		}

		public bool DireBeast ()
		{
			return Usable ("Dire Beast") && C ("Dire Beast");
		}

		public bool FocusFire ()
		{
			return Usable ("Focus Fire") && C ("Focus Fire");
		}

		public bool BestialWrath ()
		{
			return Usable ("Bestial Wrath") && C ("Bestial Wrath");
		}

		public bool MultiShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Multi-Shot") && HasAmFocus (40) && Range (40, u) && C ("Multi-Shot", u);
		}

		public bool Barrage (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Barrage") && Focus >= 60 && Range (40, u) && C ("Barrage", u);
		}

		public bool ExplosiveTrap (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Explosive Trap") && Range (40, u) && COT ("Explosive Trap", u);
		}

		public bool KillCommand (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Kill Command") && Me.HasAlivePet && HasFocus (40) && Vector3.Distance (Me.Pet.Position, u.Position) <= 25 && C ("Kill Command", u);
		}

		public bool AMurderofCrows (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("A Murder of Crows") && Focus >= 30 && Range (40, u) && C ("A Murder of Crows", u);
		}

		public bool KillShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Kill Shot") && (Health (u) < 0.20 || (HasSpell ("Enhanced Kill Shot") && Health (u) < 0.35)) && Range (40, u) && C ("Kill Shot", u);
		}

		public bool FocusingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Focusing Shot") && !Me.IsMoving && Range (40, u) && C ("Focusing Shot", u);
		}

		public bool CobraShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Cobra Shot") && Range (40, u) && C ("Cobra Shot", u);
		}

		public bool GlaiveToss (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Glaive Toss") && Focus >= 15 && Range (40, u) && C ("Glaive Toss", u);
		}

		public bool Powershot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Powershot") && Focus >= 15 && Range (40, u) && C ("Powershot", u);
		}

		public bool ArcaneShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Arcane Shot") && HasArcaneFocus (30) && Range (40, u) && C ("Arcane Shot", u);
		}

		public bool Misdirection (UnitObject u = null, int d = 800)
		{
			u = u ?? Me.Pet;
			return Usable ("Misdirection") && Range (100, u) && CPD ("Misdirection", u, d);
		}

		// Items

		public bool Healthstone ()
		{
			return API.HasItem (5512) && API.ItemCooldown (5512) == 0 && API.UseItem (5512);
		}

		public bool CrystalOfInsanity ()
		{
			return !InArena && API.HasItem (CrystalOfInsanityId) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0 && (API.UseItem (CrystalOfInsanityId));
		}

		public bool OraliusWhisperingCrystal ()
		{
			return API.HasItem (OraliusWhisperingCrystalId) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0 && API.UseItem (OraliusWhisperingCrystalId);
		}
	}
}

