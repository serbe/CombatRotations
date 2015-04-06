using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;
using Geometry;

namespace ReBot
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
		public int TTD = 10;
		[JsonProperty ("Run to enemy")]
		public bool Run;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool AOE = true;
		[JsonProperty ("Use Burst Of Speed in no combat")]
		public bool UseBurstOfSpeed = true;
		[JsonProperty ("Use GCD")]
		public bool GCD = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public DateTime StartSleepTime;
		public bool InCombat;
		public UnitObject CycleTarget;
		public String RangedAttack = "Throw";
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;
		public int FocusMax = 100;

		public bool IsSolo {
			get {
				return Group.GetNumGroupMembers () != 1;
			}
		}

		//		public int EnergyMax {
		//			get {
		//				int energy = 100;
		//				if (HasSpell ("Venom Rush"))
		//					energy = energy + 15;
		//				if (HasGlyph (159634))
		//					energy = energy + 20;
		//				return energy;
		//			}
		//		}

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

		public bool InBG {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		public double Health {
			get {
				return Me.HealthFraction;
			}
		}

		public double TargetHealth {
			get {
				return Target.HealthFraction;
			}
		}

		public bool IsBoss (UnitObject o)
		{
			return(o.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || o.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer {
			get {
				return Target.IsPlayer;
			}
		}

		public bool IsElite {
			get {
				return Target.IsElite ();
			}
		}

		// public bool isInterruptable { get { return Target.IsCastingAndInterruptible(); } }
		public bool IsInEnrage (UnitObject o)
		{
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			else
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
			else
				return false;
		}

		public int Focus {
			get {
				return Me.GetPower (WoWPowerType.Focus);
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

		// public bool TargettingMe { get { return Target.Target ==(UnitObject)Me; } }

		public double Time {
			get {
				TimeSpan CombatTime = DateTime.Now.Subtract (StartBattle);
				return CombatTime.TotalSeconds;
			}
		}

		public double SleepTime {
			get {
				TimeSpan CurrentSleepTime = DateTime.Now.Subtract (StartSleepTime);
				return CurrentSleepTime.TotalSeconds;
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
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead) {
					x++;
				}
			}
			return x;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && IsNotForDamage (mob)) {
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

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public bool Usable (string s, double d)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) <= d;
		}

		//		public int AmbushCost {
		//			get {
		//				int Cost = 60;
		//				if (HasSpell ("Shadow Focus"))
		//					Cost = 15;
		//				if (HasSpell ("Shadow Dance") && Me.HasAura ("Shadow Dance"))
		//					Cost = 40;
		//				return Cost;
		//			}
		//		}
		//
		//		public double Cost (double i)
		//		{
		//			if (Me.HasAura ("Shadow Focus"))
		//				i = Math.Floor (i * 0.25);
		//			return i;
		//		}

		public bool HasFocus (double i)
		{
			if (Me.HasAura ("Burning Adrenaline"))
				i = 0;
			return Focus >= i;
		}

		// Multi-Shot and Aimed Shot
		public bool HasAMFocus (double i)
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
			return o.Health / TTD;
		}

		public SerbHunter ()
		{
		}

		public virtual bool ExoticMunitions (ExoticMunitionsType e)
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

		public virtual bool SummonPet (PetSlot s)
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

		public virtual bool LoneWolf (UsePet p)
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


		public virtual bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public virtual bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityID) && !HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityID) == 0)
				return (API.UseItem (CrystalOfInsanityID));
			return false;
		}

		public virtual bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalID) && !HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalID) == 0)
				return API.UseItem (OraliusWhisperingCrystalID);
			return false;
		}

		public virtual bool MendPet ()
		{
			return Cast ("Mend Pet", () => Usable ("Mend Pet") && Me.HasAlivePet && Me.Pet.HasAura ("Mend Pet") && Me.Pet.CombatRange <= 45);
		}

		public virtual bool Misdirection ()
		{
			if (Usable ("Misdirection")) {
				if (!IsSolo) {
					CycleTarget = Group.GetGroupMemberObjects ().Where (x => !x.IsDead && x.IsInLoS && x.CombatRange < 100 && x.IsTank).DefaultIfEmpty (null).FirstOrDefault ();
					if (Cast ("Misdirection", CycleTarget, () => CycleTarget != null))
						return true;
				}
				
				if (Cast ("Misdirection", Me.Focus, () => Me.Focus != null))
					return true;
				if (CastPreventDouble ("Misdirection", () => HasGlyph (56829), Me.Pet, 8000))
					return true;
				if (Cast ("Misdirection", Me.Pet))
					return true;
			}
			return false;
		}

		public virtual bool TrapLauncher ()
		{
			return CastSelf ("Trap Launcher", () => Usable ("Trap Launcher"));
		}

		public virtual bool ConcussiveShot()
		{
			return Cast ("Concussive Shot", () => Usable ("Concussive Shot") && !Target.HasAura ("Concussive Shot"));
		}

		public virtual bool Interrupt() {
			var targets = Adds;
			targets.Add(Target);

			if (Usable("Counter Shot")) {
				CycleTarget = targets.Where(x => x.IsInCombatRangeAndLoS && x.IsCastingAndInterruptible() && x.RemainingCastTime > 0).DefaultIfEmpty(null).FirstOrDefault();
				if (CycleTarget != null) {
					if (CounterShot (CycleTarget))
						return true;
				}
			}

			return false;
		}

		public virtual bool CounterShot(UnitObject u) {
			return Cast ("Counter Shot", u, () => Usable ("Counter Shot") && u.IsInLoS && u.CombatRange <= 40);
		}

		public virtual bool Tranquilizing() {
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Tranquilizing Shot")) {
				if (InArena || InBG) {
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

		public virtual bool TranquilizingShot(UnitObject u) {
			return Cast ("Tranquilizing Shot", u, () => Usable ("Tranquilizing Shot") && (HasGlyph (119384) || HasFocus (50)) && u.IsInLoS && u.CombatRange <= 40);
		}

		public virtual bool BindingShot(UnitObject u) {
			return CastOnTerrain ("Binding Shot", u.Position, () => Usable ("Binding Shot") && u.IsInLoS && u.CombatRange <= 30);
		}

		public virtual bool FreezingTrap(UnitObject u) {
			return CastOnTerrain ("Freezing Trap", u.Position, () => Usable ("Freezing Trap") && u.IsInLoS && u.CombatRange <= 40);
		}

		public virtual bool Freedom() {
			return WilloftheForsaken () || EveryManforHimself () || MastersCall();
		}

		public virtual bool WilloftheForsaken() {
			return CastSelf("Will of the Forsaken", () => Usable("Will of the Forsaken"));
		}

		public virtual bool EveryManforHimself() {
			return CastSelf("Every Man for Himself", () => Usable("Every Man for Himself"));
		}

		public virtual bool MastersCall() {
			return CastSelf ("Master's Call", () => Usable ("Master's Call") && Me.HasAlivePet && Me.Pet.CombatRange <= 40);
		}

		public virtual bool LastStand() {
			return CastSelf ("Last Stand", () => Usable ("Last Stand") && Me.HasAlivePet);
		}

		public virtual bool RoarofSacrifice() {
			return CastSelf ("Roar of Sacrifice", () => Usable ("Roar of Sacrifice") && Me.HasAlivePet && Me.Pet.CombatRange <= 40);
		}

		public virtual bool Exhilaration() {
			return Cast ("Exhilaration", () => Usable ("Exhilaration"));
		}

		public virtual bool Deterrence() {
			return Cast ("Deterrence", () => Usable ("Deterrence"));
		}

		public virtual bool FeignDeath() {
			return Cast ("Feign Death", () => Usable ("Feign Death") && !Me.HasAura("Feign Death"));
		}

		public virtual bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool Stampede() {
			return Cast ("Stampede", () => Usable ("Stampede") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange(10) > 2));
		}

		public virtual bool DireBeast() {
			return Cast ("Dire Beast", () => Usable ("Dire Beast"));
		}

		public virtual bool FocusFire() {
			return Cast ("Focus Fire", () => Usable ("Focus Fire"));
		}

		public virtual bool BestialWrath() {
			return Cast ("Bestial Wrath", () => Usable ("Bestial Wrath"));
		}

		public virtual bool MultiShot() {
			return Cast ("MultiShot", () => Usable ("MultiShot") && HasAMFocus(40) && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool Barrage() {
			return Cast ("Barrage", () => Usable ("Barrage") && Focus >= 60 && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool ExplosiveTrap(UnitObject u) {
			return CastOnTerrain ("Explosive Trap", u.Position, () => Usable ("Explosive Trap") && u.IsInLoS && u.CombatRange <= 40);
		}

		public virtual bool KillCommand() {
			return Cast ("Kill Command", () => Usable ("Kill Command") && Me.HasAlivePet && HasFocus(40));
		}
	
		public virtual bool AMurderofCrows() {
			return Cast ("A Murder of Crows", () => Usable ("A Murder of Crows") && Focus >= 30 && Target.IsInLoS && Target.CombatRange <= 40);
		}
	
		public virtual bool KillShot() {
			return Cast ("Kill Shot", () => Usable ("Kill Shot") && (Target.HealthFraction < 0.20 || (HasSpell("Enhanced Kill Shot") && Target.HealthFraction < 0.35)) && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool FocusingShot() {
			return Cast ("Focusing Shot", () => Usable ("Focusing Shot") && !Me.IsMoving && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool CobraShot() {
			return Cast ("Cobra Shot", () => Usable ("Cobra Shot") && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool GlaiveToss() {
			return Cast ("Glaive Toss", () => Usable ("Glaive Toss") && Focus >= 15 && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool Powershot() {
			return Cast ("Powershot", () => Usable ("Powershot") && Focus >= 15 && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool ArcaneShot() {
			return Cast ("Arcane Shot", () => Usable ("Arcane Shot") && HasArcaneFocus(30) && Target.IsInLoS && Target.CombatRange <= 40);
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

