using System;
using System.Linq;
using Geometry;
using Newtonsoft.Json;
using ReBot.API;

namespace ReBot.Hunter
{
	public abstract class SerbHunter : CombatRotation
	{
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
		//		[JsonProperty ("Use Burst Of Speed in no combat")]
		//		public bool UseBurstOfSpeed = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public UnitObject CycleTarget;
		public String RangedAttack = "Throw";
		public Int32 OraliusWhisperingCrystalId = 118922;
		public Int32 CrystalOfInsanityId = 86569;


		public bool IsSolo {
			get {
				return Group.GetNumGroupMembers () != 1;
			}
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

		public double Health (UnitObject u = null)
		{
			u = u ?? Me;
			return u.HealthFraction;
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

		// public bool isInterruptable { get { return Target.IsCastingAndInterruptible(); } }
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

		public int Focus {
			get {
				return Me.GetPower (WoWPowerType.Focus);
			}
		}

		public int FocusMax {
			get {
				int fm = 100;
				if (HasSpell ("Kindred Spirits"))
					fm = fm + 20;
				return fm;
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

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable) {
					x++;
				}
			}
			return x;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable && IsNotForDamage (mob)) {
					x++;
				}
			}
			return x > 0;
		}

		public int EnemyWithTarget (UnitObject o, int range)
		{
			return Adds.Where (x => x.IsInCombatRangeAndLoS && Vector3.DistanceSquared (x.Position, o.Position) <= range * range).ToList ().Count;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double CooldownById (Int32 i)
		{ 
			return SpellCooldown (i) < 0 ? 0 : SpellCooldown (i);
		}

		public bool Usable (string s, double d = 0)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (d == 0)
				// Analysis disable once CompareOfFloatsByEqualityOperator
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

		public double TimeToDie (UnitObject o)
		{
			if (o != null)
				return o.Health / Ttd;
			return 0;
		}


		public bool ExoticMunitions (ExoticMunitionsType e)
		{
			if (e == ExoticMunitionsType.PoisonedAmmo) {
				return CastSelfPreventDouble ("Poisoned Ammo", () => Usable ("Poisoned Ammo") && !Me.HasAura ("Poisoned Ammo"));
			}
			if (e == ExoticMunitionsType.IncendiaryAmmo) {
				return CastSelfPreventDouble ("Incendiary Ammo", () => Usable ("Incendiary Ammo") && !Me.HasAura ("Incendiary Ammo"));
			}
			if (e == ExoticMunitionsType.FrozenAmmo) {
				return CastSelfPreventDouble ("Frozen Ammo", () => Usable ("Frozen Ammo") && !Me.HasAura ("Frozen Ammo"));
			}
			return false;
		}

		public bool SummonPet (PetSlot s)
		{
			if (CastSelfPreventDouble ("Revive Pet", null, 5000))
				return true;
			if (CastSelfPreventDouble ("Call Pet 1", () => s == PetSlot.PetSlot1, 5000))
				return true;
			if (CastSelfPreventDouble ("Call Pet 2", () => s == PetSlot.PetSlot2, 5000))
				return true;
			if (CastSelfPreventDouble ("Call Pet 3", () => s == PetSlot.PetSlot3, 5000))
				return true;
			if (CastSelfPreventDouble ("Call Pet 4", () => s == PetSlot.PetSlot4, 5000))
				return true;
			if (CastSelfPreventDouble ("Call Pet 5", () => s == PetSlot.PetSlot5, 5000))
				return true;
			return false;
		}

		public bool LoneWolf (UsePet p)
		{
			if (Me.HasAlivePet) {
				Me.PetDismiss ();
				return true;
			}
			if (CastSelfPreventDouble ("Lone Wolf: Ferocity of the Raptor", () => p == UsePet.LoneWolfCrit && !Me.HasAura ("Lone Wolf: Ferocity of the Raptor"), 1500))
				return true;
			if (CastSelfPreventDouble ("Lone Wolf: Grace of the Cat", () => p == UsePet.LoneWolfMastery && !Me.HasAura ("Lone Wolf: Grace of the Cat"), 1500))
				return true;
			if (CastSelfPreventDouble ("Lone Wolf: Haste of the Hyena", () => p == UsePet.LoneWolfHaste && !Me.HasAura ("Lone Wolf: Haste of the Hyena"), 1500))
				return true;
			if (CastSelfPreventDouble ("Lone Wolf: Power of the Primates", () => p == UsePet.LoneWolfStats && !Me.HasAura ("Lone Wolf: Power of the Primates"), 1500))
				return true;
			if (CastSelfPreventDouble ("Lone Wolf: Fortitude of the Bear", () => p == UsePet.LoneWolfStamina && !Me.HasAura ("Lone Wolf: Fortitude of the Bear"), 1500))
				return true;
			if (CastSelfPreventDouble ("Lone Wolf: Quickness of the Dragonhawk", () => p == UsePet.LoneWolfMultistrike && !Me.HasAura ("Lone Wolf: Quickness of the Dragonhawk"), 1500))
				return true;
			if (CastSelfPreventDouble ("Lone Wolf: Versatility of the Ravager", () => p == UsePet.LoneWolfVersatility && !Me.HasAura ("Lone Wolf: Versatility of the Ravager"), 1500))
				return true;
			if (CastSelfPreventDouble ("Lone Wolf: Wisdom of the Serpent", () => p == UsePet.LoneWolfSpellpower && !Me.HasAura ("Lone Wolf: Wisdom of the Serpent"), 1500))
				return true;

			return false;
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

		public bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityId) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0)
				return (API.UseItem (CrystalOfInsanityId));
			return false;
		}

		public bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalId) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0)
				return API.UseItem (OraliusWhisperingCrystalId);
			return false;
		}

		public bool MendPet ()
		{
			return Cast ("Mend Pet", () => Usable ("Mend Pet") && Me.HasAlivePet && Me.Pet.HasAura ("Mend Pet") && Me.Pet.CombatRange <= 45);
		}

		public virtual bool Misdirection ()
		{
			if (Usable ("Misdirection")) {
				if (!IsSolo) {
					CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && x.IsInLoS && x.CombatRange < 100 && x.IsTank).DefaultIfEmpty (null).FirstOrDefault ();
					if (Cast ("Misdirection", () => CycleTarget != null, CycleTarget))
						return true;
				}
				
				if (Cast ("Misdirection", () => Me.Focus != null, Me.Focus))
					return true;
				if (CastPreventDouble ("Misdirection", () => HasGlyph (56829), Me.Pet, 8000))
					return true;
				if (Cast ("Misdirection", Me.Pet))
					return true;
			}
			return false;
		}

		public bool TrapLauncher ()
		{
			return CastSelf ("Trap Launcher", () => Usable ("Trap Launcher"));
		}

		public bool ConcussiveShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Concussive Shot", () => Usable ("Concussive Shot") && !u.HasAura ("Concussive Shot") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Interrupt ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Counter Shot")) {
				CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.IsCastingAndInterruptible () && x.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (CounterShot (CycleTarget))
						return true;
				}
			}

			return false;
		}

		public bool CounterShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Counter Shot", () => Usable ("Counter Shot") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Tranquilizing ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Tranquilizing Shot")) {
				if (InArena || InBg) {
					CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.IsPlayer && x.Auras.Any (a => a.IsStealable)).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (TranquilizingShot (CycleTarget))
							return true;
					}
				} else {
					CycleTarget = targets.Where (x => x.IsInCombatRangeAndLoS && x.Auras.Any (a => a.IsStealable)).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (TranquilizingShot (CycleTarget))
							return true;
					}
				}
			}
			return false;
		}

		public bool TranquilizingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Tranquilizing Shot", () => Usable ("Tranquilizing Shot") && (HasGlyph (119384) || HasFocus (50)) && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool BindingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Binding Shot", u.Position, () => Usable ("Binding Shot") && u.IsInLoS && u.CombatRange <= 30);
		}

		public bool FreezingTrap (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Freezing Trap", u.PositionPredicted, () => Usable ("Freezing Trap") && Me.HasAura ("Trap Launcher") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool Freedom ()
		{
			return WilloftheForsaken () || EveryManforHimself () || MastersCall ();
		}

		public bool WilloftheForsaken ()
		{
			return CastSelf ("Will of the Forsaken", () => Usable ("Will of the Forsaken"));
		}

		public bool EveryManforHimself ()
		{
			return CastSelf ("Every Man for Himself", () => Usable ("Every Man for Himself"));
		}

		public bool MastersCall ()
		{
			return CastSelf ("Master's Call", () => Usable ("Master's Call") && Me.HasAlivePet && Me.Pet.CombatRange <= 40);
		}

		public bool LastStand ()
		{
			return CastSelf ("Last Stand", () => Usable ("Last Stand") && Me.HasAlivePet);
		}

		public bool RoarofSacrifice ()
		{
			return CastSelf ("Roar of Sacrifice", () => Usable ("Roar of Sacrifice") && Me.HasAlivePet && Me.Pet.CombatRange <= 40);
		}

		public virtual bool Exhilaration ()
		{
			return Cast ("Exhilaration", () => Usable ("Exhilaration"));
		}

		public virtual bool Deterrence ()
		{
			return Cast ("Deterrence", () => Usable ("Deterrence"));
		}

		public virtual bool FeignDeath ()
		{
			return Cast ("Feign Death", () => Usable ("Feign Death") && !Me.HasAura ("Feign Death"));
		}

		public bool BloodFury (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool Berserking (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Berserking", () => Usable ("Berserking") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool ArcaneTorrent (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool Stampede (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Stampede", () => Usable ("Stampede") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2), u);
		}

		public bool DireBeast ()
		{
			return Cast ("Dire Beast", () => Usable ("Dire Beast"));
		}

		public bool FocusFire ()
		{
			return Cast ("Focus Fire", () => Usable ("Focus Fire"));
		}

		public virtual bool BestialWrath ()
		{
			return Cast ("Bestial Wrath", () => Usable ("Bestial Wrath"));
		}

		public bool MultiShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Multi-Shot", () => Usable ("Multi-Shot") && HasAmFocus (40) && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Barrage (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Barrage", () => Usable ("Barrage") && Focus >= 60 && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool ExplosiveTrap (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Explosive Trap", u.Position, () => Usable ("Explosive Trap") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool KillCommand (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Kill Command", () => Usable ("Kill Command") && Me.HasAlivePet && HasFocus (40) && Vector3.Distance (Me.Pet.Position, u.Position) <= 25, u);
		}

		public bool AMurderofCrows (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("A Murder of Crows", () => Usable ("A Murder of Crows") && Focus >= 30 && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool KillShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Kill Shot", () => Usable ("Kill Shot") && (Health (u) < 0.20 || (HasSpell ("Enhanced Kill Shot") && Health (u) < 0.35)) && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool FocusingShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Focusing Shot", () => Usable ("Focusing Shot") && !Me.IsMoving && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool CobraShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Cobra Shot", () => Usable ("Cobra Shot") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool GlaiveToss (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Glaive Toss", () => Usable ("Glaive Toss") && Focus >= 15 && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Powershot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Powershot", () => Usable ("Powershot") && Focus >= 15 && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool ArcaneShot (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Arcane Shot", () => Usable ("Arcane Shot") && HasArcaneFocus (30) && u.IsInLoS && u.CombatRange <= 40, u);
		}

		//		public virtual bool Exhilaration() {
		//			return Cast ("Exhilaration", () => Usable ("Exhilaration"));
		//		}
		//
		//		public virtual bool Exhilaration() {
		//			return Cast ("Exhilaration", () => Usable ("Exhilaration"));
		//		}
		//
		//		public virtual bool Exhilaration() {
		//			return Cast ("Exhilaration", () => Usable ("Exhilaration"));
		//		}
		//
		//		public virtual bool Exhilaration() {
		//			return Cast ("Exhilaration", () => Usable ("Exhilaration"));
		//		}
		//
		//		public virtual bool Exhilaration() {
		//			return Cast ("Exhilaration", () => Usable ("Exhilaration"));
		//		}
	}
}

