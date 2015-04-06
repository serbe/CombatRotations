using System;
using ReBot.API;

namespace ReBot
{
	public abstract class DeathKnight : CombatRotation
	{
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

		public int RunicPower { 
			get { 
				return Me.GetPower(WoWPowerType.RunicPower); } }

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public DeathKnight ()
		{
		}

		public virtual bool HornofWinter ()
		{
			return CastSelf ("Horn of Winter", () => Usable ("Horn of Winter") && !HasAura ("Horn of Winter") && !HasAura ("Battle Shout"));
		}

		public virtual bool RaiseDead()
		{
			return CastSelf("Raise Dead", () => Usable("Raise Dead") && !Me.HasAlivePet);
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

		public virtual bool AntimagicShell()
		{
			return CastSelf ("Anti-Magic Shell", () => Usable ("Anti-Magic Shell"));
		}
	}
}

