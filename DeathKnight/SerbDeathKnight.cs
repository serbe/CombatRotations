using System;
using ReBot.API;
using Newtonsoft.Json;
using System.Linq;

namespace ReBot
{
	public abstract class DeathKnight : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int TTD = 10;

		public UnitObject CycleTarget;
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

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

		public bool IsBoss (UnitObject o)
		{
			return (o.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || o.Level >= Me.Level + BossLevelIncrease;
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

		public double Health {
			get {
				return Me.HealthFraction;
			}
		}

		public int RunicPower { 
			get { 
				return Me.GetPower (WoWPowerType.RunicPower);
			}
		}

		public bool HasBlood {
			get { 
				return (Me.Runes (RuneType.Blood) > 0);
			}
		}

		public bool HasUnholy {
			get { 
				return (Me.Runes (RuneType.Unholy) > 0);
			}
		}

		public bool HasFrost {
			get { 
				return (Me.Runes (RuneType.Frost) > 0);
			}
		}

		public bool HasDeath {
			get { 
				return (Me.Runes (RuneType.Death) > 0);
			}
		}

		public int Blood { 
			get {
				return Me.Runes (RuneType.Blood);
			}
		}

		public int Frost { 
			get { 
				return Me.Runes (RuneType.Frost);
			}
		}

		public int Unholy { 
			get { 
				return Me.Runes (RuneType.Unholy);
			}
		}

		public int Death { 
			get { 
				return Me.Runes (RuneType.Death);
			}
		}

		public double BloodFrac {
			get {
				double startTime_1 = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(1); return start");
				double duration = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(1); return duration");
				bool runeReady_1 = API.ExecuteLua<bool>("start, duration, runeReady = GetRuneCooldown(1); return runeReady");
				double startTime_2 = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(2); return start");
				bool runeReady_2 = API.ExecuteLua<bool>("start, duration, runeReady = GetRuneCooldown(2); return runeReady");
				double currentTime = API.ExecuteLua<double>("return GetTime()");
				double result;
				if (!runeReady_1) {
					result = (currentTime - startTime_1) / duration;
				} else
					result = 1;
				if (!runeReady_2) {
					result = result + (currentTime - startTime_1) / duration;
				} else
					result = result + 1;
				return result;
			}
		}

		public double FrostFrac {
			get {
				double startTime_1 = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(3); return start");
				double duration = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(3); return duration");
				bool runeReady_1 = API.ExecuteLua<bool>("start, duration, runeReady = GetRuneCooldown(3); return runeReady");
				double startTime_2 = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(4); return start");
				bool runeReady_2 = API.ExecuteLua<bool>("start, duration, runeReady = GetRuneCooldown(4); return runeReady");
				double currentTime = API.ExecuteLua<double>("return GetTime()");
				double result;
				if (!runeReady_1) {
					result = (currentTime - startTime_1) / duration;
				} else
					result = 1;
				if (!runeReady_2) {
					result = result + (currentTime - startTime_1) / duration;
				} else
					result = result + 1;
				return result;
			}
		}

		public double UnholyFrac {
			get {
				double startTime_1 = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(5); return start");
				double duration = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(5); return duration");
				bool runeReady_1 = API.ExecuteLua<bool>("start, duration, runeReady = GetRuneCooldown(5); return runeReady");
				double startTime_2 = API.ExecuteLua<double>("start, duration, runeReady = GetRuneCooldown(6); return start");
				bool runeReady_2 = API.ExecuteLua<bool>("start, duration, runeReady = GetRuneCooldown(6); return runeReady");
				double currentTime = API.ExecuteLua<double>("return GetTime()");
				double result;
				if (!runeReady_1) {
					result = (currentTime - startTime_1) / duration;
				} else
					result = 1;
				if (!runeReady_2) {
					result = result + (currentTime - startTime_1) / duration;
				} else
					result = result + 1;
				return result;
			}
		}

		public int BloodCharge { 
			get { 
				return AuraStackCount ("Blood Charge");
			}
		}

		public bool HasFrostDisease (UnitObject o)
		{
			return o.HasAura ("Frost Fever", true);
		}

		public bool HasBloodDisease (UnitObject o)
		{
			return o.HasAura ("Blood Plague", true);
		}

		public bool HasDisease (UnitObject o)
		{
			return HasBloodDisease (o) && HasFrostDisease (o);
		}

		public int Disease (UnitObject o)
		{
			var result = 0;
			if (HasBloodDisease (o))
				result = result + 1;
			if (HasFrostDisease (o))
				result = result + 1;
			return result;
		}

		public double FrostDiseaseRemaining (UnitObject o)
		{
			return HasFrostDisease (o) ? o.AuraTimeRemaining ("Frost Fever", true) : 0;
		}

		public double BloodDiseaseRemaining (UnitObject o)
		{
			return HasBloodDisease (o) ? o.AuraTimeRemaining ("Blood Plague", true) : 0;
		}

		public double MinDisease (UnitObject o)
		{
			return FrostDiseaseRemaining (o) < BloodDiseaseRemaining (o) ? FrostDiseaseRemaining (o) : BloodDiseaseRemaining (o);
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double TimeToDie (UnitObject o)
		{
			return o.Health / TTD;
		}

		public DeathKnight ()
		{
		}

		public virtual bool HornofWinter ()
		{
			return CastSelf ("Horn of Winter", () => Usable ("Horn of Winter") && !HasAura ("Horn of Winter") && !HasAura ("Battle Shout"));
		}

		public virtual bool RaiseDead ()
		{
			return CastSelf ("Raise Dead", () => Usable ("Raise Dead") && !Me.HasAlivePet);
		}

		public virtual bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return API.HasItem (5512) && API.ItemCooldown (5512) == 0 && API.UseItem (5512);
		}

		public virtual bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityID) && !Me.HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityID) == 0)
				return API.UseItem (CrystalOfInsanityID);
			return false;
		}

		public virtual bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalID) && !Me.HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalID) == 0)
				return API.UseItem (OraliusWhisperingCrystalID);
			return false;
		}

		public virtual bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool AntimagicShell ()
		{
			return CastSelf ("Anti-Magic Shell", () => Usable ("Anti-Magic Shell"));
		}

		public virtual bool UnholyBlight ()
		{
			return CastSelf ("Unholy Blight", () => Usable ("Unholy Blight") && Target.IsInLoS && Target.CombatRange <= 10);
		}

		public virtual bool Defile ()
		{
			return CastOnTerrain ("Defile", Target.Position, () => Usable ("Defile") && (Me.HasAura ("Crimson Scourge") || (HasUnholy || HasDeath)) && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool BloodBoil ()
		{
			return CastSelf ("Blood Boil", () => Usable ("Blood Boil") && (Me.HasAura ("Crimson Scourge") || (HasBlood || HasDeath)) && Target.IsInLoS && Target.CombatRange <= 10);
		}

		public virtual bool SummonGargoyle ()
		{
			return Cast ("Summon Gargoyle", () => Usable ("Summon Gargoyle") && Target.IsInLoS && Target.CombatRange <= 30 && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool DarkTransformation ()
		{
			return Cast ("Dark Transformation", () => Usable ("Dark Transformation") && !Me.Pet.HasAura("Dark Transformation") && Me.HasAlivePet && Me.GetAura ("Shadow Infusion").StackCount == 5 && (HasSpell ("Enhanced Dark Transformation") || (HasDeath || HasUnholy)));
		}

		public virtual bool BloodTap ()
		{
			return CastSelf ("Blood Tap", () => Usable ("Blood Tap") && BloodCharge >= 5);
		}

		public virtual bool DeathandDecay ()
		{
			return CastOnTerrain ("Death and Decay", Target.Position, () => Usable ("Death and Decay") && (Me.HasAura ("Crimson Scourge") || (HasUnholy || HasDeath)) && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool ScourgeStrike ()
		{
			return Cast ("Scourge Strike", () => Usable ("Scourge Strike") && (HasUnholy || HasDeath));
		}

		public virtual bool DeathCoil ()
		{
			return Cast ("Death Coil", () => Usable ("Death Coil") && (Me.HasAura ("Sudden Doom") || RunicPower >= 30) && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool IcyTouch ()
		{
			return Cast ("Icy Touch", () => Usable ("Icy Touch") && (HasFrost || HasDeath) && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool PlagueLeech ()
		{
			return Cast ("Plague Leech", () => Usable ("Plague Leech") && Target.HasAura ("Frost Fever", true) && Target.HasAura ("Blood Plague", true));
		}

		public virtual bool EmpowerRuneWeapon ()
		{
			return Cast ("Empower Rune Weapon", () => Usable ("Empower Rune Weapon") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool Outbreak ()
		{
			return Cast ("Outbreak", () => Usable ("Outbreak") && (!HasGlyph (59332) || RunicPower >= 30) && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool Outbreak (UnitObject u)
		{
			return Cast ("Outbreak", u, () => Usable ("Outbreak") && (!HasGlyph (59332) || RunicPower >= 30) && u.IsInLoS && u.CombatRange <= 30);
		}

		public virtual bool PlagueStrike ()
		{
			return Cast ("Plague Strike", () => Usable ("Plague Strike") && (HasUnholy || HasDeath));
		}

		public virtual bool FesteringStrike ()
		{
			return Cast ("Festering Strike", () => Usable ("Festering Strike") && ((HasFrost && HasBlood) || (HasFrost && HasDeath) || (HasDeath && HasBlood) || Death == 2));
		}

		public virtual bool Interrupt ()
		{
			var targets = Adds;
			targets.Add (Target);

			if (Usable ("Mind Freeze")) {
				if (InArena || InBG) {
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && u.IsInCombatRangeAndLoS && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (MindFreeze (CycleTarget))
							return true;
					}
				} else {
					CycleTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInCombatRangeAndLoS && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (MindFreeze (CycleTarget))
							return true;
					}
				}
			}

			if (Usable ("Strangulate")) {
				if (InArena || InBG) {
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsHealer && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Strangulate (CycleTarget))
							return true;
					}
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Strangulate (CycleTarget))
							return true;
					}
				} else {
					CycleTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Strangulate (CycleTarget))
							return true;
					}
				}
			}

			if (Usable ("Asphyxiate")) {
				if (InArena || InBG) {
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsHealer && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Asphyxiate (CycleTarget))
							return true;
					}
					CycleTarget = API.Players.Where (u => u.IsPlayer && u.IsEnemy && u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Asphyxiate (CycleTarget))
							return true;
					}
				} else {
					CycleTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.IsInLoS && u.CombatRange <= 30 && u.RemainingCastTime > 0).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Asphyxiate (CycleTarget))
							return true;
					}
				}
			}

			return false;
		}

		public virtual bool Heal ()
		{
			if (Health < 0.45) {
				if (Healthstone ())
					return true;
			}

			if (Health <= 0.6) {
				if (DeathSiphon ())
					return true;
			}
			if (!InRaid && Health < 0.9) {
				if (DeathStrike ())
					return true;
			}
			if (Health < 0.7) {
				if (Lichborne ())
					return true;
			}
			if (Health < 0.5) {
				if (VampiricBlood ())
					return true;
			}
			if (Health < 0.3 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Dancing Rune Weapon") && !Me.HasAura ("Vampiric Blood")) {
				if (IceboundFortitude ())
					return true;
			}
			if (Health < 0.8 && !Me.HasAura ("Army of the Dead") && !Me.HasAura ("Icebound Fortitude") && !Me.HasAura ("Bone Shield") && !Me.HasAura ("Vampiric Blood")) {
				if (DancingRuneWeapon ())
					return true;
			}
			if (Health < 0.5) {
				if (DeathPact ())
					return true;
			}
			
			return false;
		}

		public virtual bool MindFreeze (UnitObject u)
		{
			return Cast ("Mind Freeze", u, () => Usable ("MindFreeze"));
		}

		public virtual bool Strangulate (UnitObject u)
		{
			return Cast ("Strangulate", u, () => Usable ("Strangulate") && (HasBlood || HasDeath) && u.IsInLoS && u.CombatRange <= 30);
		}

		public virtual bool Asphyxiate (UnitObject u)
		{
			return Cast ("Asphyxiate", u, () => Usable ("Asphyxiate") && u.IsInLoS && u.CombatRange <= 30);
		}

		public virtual bool DeathSiphon ()
		{
			return Cast ("Death Siphon", () => Usable ("Death Siphon") && HasDeath && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool DeathStrike ()
		{
			return Cast ("Death Strike", () => Usable ("Death Strike") && ((HasFrost && HasUnholy) || (HasFrost && HasDeath) || (HasDeath && HasUnholy) || Death == 2));
		}

		public virtual bool BreathofSindragosa ()
		{
			return CastSelf ("Breath of Sindragosa", () => Usable ("Breath of Sindragosa") && RunicPower > 0 && (IsElite || IsPlayer) && Target.IsInLoS && Target.CombatRange <= 10);
		}

		public virtual bool Lichborne ()
		{
			return CastSelf ("Lichborne", () => Usable ("Lichborne"));
		}

		public virtual bool ArmyoftheDead ()
		{
			return CastSelf ("Army of the Dead", () => Usable ("Army of the Dead") && (IsPlayer || IsBoss (Target)) && ((HasBlood && HasFrost && HasUnholy) || (HasDeath && HasFrost && HasUnholy) || (HasBlood && HasDeath && HasUnholy) || (HasBlood && HasFrost && HasDeath) || ((HasBlood || HasFrost || HasUnholy) && Death >= 2) || Death >= 3));
		}

		public virtual bool VampiricBlood ()
		{
			return CastSelf ("Vampiric Blood", () => Usable ("Vampiric Blood"));
		}

		public virtual bool IceboundFortitude ()
		{
			return CastSelf ("Icebound Fortitude", () => Usable ("Icebound Fortitude"));
		}

		public virtual bool DancingRuneWeapon ()
		{
			return CastSelf ("Dancing Rune Weapon", () => Usable ("Dancing Rune Weapon") && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool DeathPact ()
		{
			return CastSelf ("Death Pact", () => Usable ("Death Pact"));
		}

		public virtual bool BoneShield() {
			return CastSelf ("Bone Shield", () => Usable ("Bone Shield"));
		}
	}
}

