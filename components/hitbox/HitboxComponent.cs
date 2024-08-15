using Godot;
using Godot.Collections;
namespace ProjectMina;

[GlobalClass]
public partial class HitboxComponent : Area3D
{
	[Export] public DamageComponent DamageComponent { get; protected set; }
	
	public bool CanHit { get; private set; } = false;

	private Array<CharacterBase> _hitCharacters;
	private Array<Rid> _exclude;

	public void SetExclude(Array<Rid> rid)
	{
		_exclude = rid;
	}
	
	public void EnableHit()
	{
		CanHit = true;
	}

	public void DisableHit()
	{
		CanHit = false;
		_hitCharacters = new();
	}
	
	public override void _Ready()
	{
		BodyEntered += _CheckHit;
		_hitCharacters = new();
	}

	private void _CheckHit(Node3D body)
	{
		GD.Print("HIT SOMETHING: ", body.Name);
		if (!CanHit) return;
		GD.Print("HIT SOMETHING");
		if (body is CharacterBase c)
		{
			GD.Print("HIT A CHARACTER");
			// do nothing if this is the wielder
			if (_exclude.Contains(c.GetRid())) return;

			GD.Print("HIT SOMETHING NOT IN EXCLUDE");
			if (_hitCharacters.Contains(c)) return;
			GD.Print("HIT SOMETHING NOT IN HIT CHARACTERS");
			_hitCharacters.Add(c);
			GD.Print("hit character: ", c.Name);
		} else if (body.HasNode("HealthComponent"))
		{
			// handle a situation like a breakble door or something like that
		}
		else
		{
			GD.Print(body.Name);
		}
	}
}
