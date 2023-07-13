using Godot;

namespace ProjectMina;
[GlobalClass]
public partial class HitboxComponent : Area3D
{
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

	private Godot.Collections.Array<Node3D> _hitNodes = new();
	private Godot.Collections.Array<Node3D> exclude;
	private CharacterBase _owner;
	private bool _canHit = false;

	public void SetOwner(CharacterBase newOwner)
	{
		_owner = newOwner;
	}

	public override void _Ready()
	{
		BodyEntered += CheckHit;
	}

	private void CheckHit(Node3D body)
	{
		if (_hitNodes.Contains(body) || body == _owner || !CanHit)
		{
			return;
		}
		Dev.UI.PushDevNotification("AI character hit player with sword!");

		_hitNodes.Add(body);

		if (body is PlayerCharacter p)
		{
			p.CharacterHealthComponent.ChangeHealth(10.0, true);
		}
	}
}
