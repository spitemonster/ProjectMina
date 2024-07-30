using System.Threading.Tasks;
using Godot;

namespace ProjectMina.BehaviorTree;

[Tool]
[GlobalClass,Icon("res://_dev/icons/icon--sequence.svg")]
public partial class Sequence : Composite
{
	
	private int _runningIndex = 0;
	protected override EActionStatus _Tick(AIControllerComponent controller, BlackboardComponent blackboard)
	{
		GD.Print("child actions: ", ChildActions);
		foreach (var action in ChildActions)
		{
			// skip 
			if (action.GetIndex() < _runningIndex)
			{
				GD.Print("skipping");
				continue;
			}
			
			GD.Print("ticking action: ", action.Name);
			var result = action.Tick(controller, blackboard);
		
			switch (result)
			{
				case EActionStatus.Failed:
					SetStatus(EActionStatus.Failed);
					return Status;
				case EActionStatus.Succeeded:
					break;
				case EActionStatus.Running:
					_runningIndex = action.GetIndex();
					SetStatus(EActionStatus.Running);
					return Status;
			}
		}

		SetStatus(EActionStatus.Succeeded);
		_runningIndex = 0;
		return Status;
	}
}
