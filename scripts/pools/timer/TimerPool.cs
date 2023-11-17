using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class TimerPool : PoolBase
{
	public static TimerPool Manager { get; private set; }
	
	private Array<Timer> _pool = new();

	/// <summary>
	/// retrieves a timer from the pool; creates a new timer if the pool is empty
	/// </summary>
	/// <param name="waitTime">wait time of timer</param>
	/// <param name="callback">method executed on timeout; also removed by clear timer method</param>
	/// <param name="autostart">whether the timer should start when added to tree</param>
	/// <param name="oneShot">whether the timer should repeat after finishing</param>
	/// <returns></returns>
	public Timer GetTimer(float waitTime, System.Action callback = null, bool autostart = false, bool oneShot = true, string who = "")
	{
		Timer t;

		if (who != "" && EnableDebug)
		{
			GD.PushWarning(who, " is requesting a timer");
		}
		
		if (_pool.Count > 0)
		{
			t = _pool[0];
			_pool.RemoveAt(0);	
		}
		else
		{
			GD.PushWarning("Timer Pool Manager has drained its pool, creating a new timer.");
			t = new Timer();
		}

		t.WaitTime = waitTime;
		t.Autostart = autostart;
		t.OneShot = oneShot;
		t.Timeout += callback;
		t.Timeout += () => ClearTimerAndReturnToQueue(t, callback, who);

		if (EnableDebug)
		{
			GD.PushWarning("Timer retrieved from pool. Pool size is now: ", _pool.Count, " timers.");	
		}
		
		return t;
	}

	public override void _Ready()
	{
		
		EnableDebug = true;
		for (var i = 0; i < PoolSize; i++)
		{
			Timer t = new()
			{
				Autostart = false
			};
			AddChild(t);
			_pool.Add(t);
		}
	}
	
	public override void _EnterTree()
	{
		if (Manager != null)
		{
			QueueFree();
		}
		Manager = this;
	}

	private void ClearTimerAndReturnToQueue(Timer t, System.Action callback, string who)
	{
		if (!t.OneShot && !t.IsStopped())
		{
			GD.PushWarning("attempted to return a looping timer that was not stopped before returning");
			return;
		}
		if (_pool.Count >= PoolSize)
		{
			if (_pool.Count > PoolSize && EnableDebug)
			{
				GD.PushWarning("Timer pool has exceeded its size of ", PoolSize, " timers.");
			}
			
			t.QueueFree();
			return;
		}

		if (who != "" && EnableDebug)
		{
			GD.PushError(who, " is returning a timer");
		}

		t.WaitTime = 0.1f;
		t.OneShot = true;
		t.Autostart = false;

		if (callback != null)
		{
			t.Timeout -= callback;
		}
		
		_pool.Add(t);
		
		if (EnableDebug)
		{
			GD.PushWarning("Added timer back to pool. Pool size is now: ", _pool.Count, " timers.");
		}
	}
}
