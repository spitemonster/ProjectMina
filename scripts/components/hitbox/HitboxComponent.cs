using Godot;
using Godot.Collections;
namespace ProjectMina;

public partial class HitboxComponent : ComponentBase
{
	[Export] public double Damage = 10.0;

	public Array<Node3D> Exclude = new();

	[Signal] public delegate void HitCharacterEventHandler(CharacterBase character);
	[Signal] public delegate void HitNodeEventHandler(Node3D node);

	public bool CanHit
	{
		get { return _canHit; }
		set
		{
			_canHit = value;
			if (!_canHit)
			{
				_hitNodes.Clear();
			}
		}
	}

	private Array<Node3D> _hitNodes = new();
	private CharacterBase _owner;
	private bool _canHit = false;

	public void SetOwner(CharacterBase newOwner)
	{
		_owner = newOwner;
	}

	public override void _Ready()
	{
		base._Ready();
		if (!Active)
		{
			return;
		}

		// BodyEntered += CheckHit;
	}

	private void CheckHit(Node3D body)
	{
		// if we already hit the body, the body should be ignored
		if (_hitNodes.Contains(body) || Exclude.Contains(body))
		{
			return;
		}

		// this feels like something else should happen
		if (!CanHit)
		{
			return;
		}

		_hitNodes.Add(body);

		if (body is CharacterBase c && c != _owner)
		{
			c.CharacterHealth.ChangeHealth(Damage, true);
			EmitSignal(SignalName.HitCharacter, c);
		}

		EmitSignal(SignalName.HitNode, body);
	}
}
