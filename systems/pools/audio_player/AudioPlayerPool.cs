using System.Collections;
using GDebugPanelGodot.Extensions;
using Godot;
using Godot.Collections;

namespace ProjectMina;

public partial class AudioPlayerPool : PoolBase
{
    [Signal] public delegate void PoolEmptiedEventHandler();

    [Export] public int QueuePlayerCount = 64;
    public static AudioPlayerPool Manager { get; private set; }
    
    private Array<SoundQueue3D> _queuePool3D = new();
    private Array<SoundQueue2D> _queuePool2D = new();
    private Array<SoundPlayer3D> _playerPool3D = new();
    private Array<SoundPlayer2D> _playerPool2D = new();

    // tracks instance ids of all players (queues and players) created and managed by this pool
    private Array<ulong> _managedPlayers = new();

    public void PlaySoundAtPosition(Vector3 position, AudioStream stream)
    {
        var player = GetPlayer3D();
        
        Global.Data.MainScene.AddChild(player);
        player.SetStream(stream);
        player.GlobalPosition = position;
        player.Play();
    }

    public SoundQueue3D GetQueue3D()
    {
        SoundQueue3D q;
        
        if (_queuePool3D.Count > 0)
        {
            q = _queuePool3D[0];
            _queuePool3D.RemoveAt(0);
        }
        else
        {
            EmitSignal(SignalName.PoolEmptied);
            
            if (EnableDebug) GD.PushError("Audio Pool Manager has drained its Queue3D pool");

            if (AllowOverflow)
            {
                q = new SoundQueue3D();
                _managedPlayers.Add(q.GetInstanceId());
                if (EnableDebug) GD.PushError("Audio Pool Manager creating a new Queue3D");
            }
            else
            {
                if (EnableDebug) GD.PushError("Audio Pool Manager attempted to overflow Queue3D but was denied.");
                return null;
            }
        }

        return q;
    }

    public void ReturnQueue3D(SoundQueue3D q)
    {
        // if we have space in the queue for the returned queue, add it and dip
        if (_queuePool3D.Count < PoolSize && _managedPlayers.Contains(q.GetInstanceId()))
        {
            _queuePool3D.Add(q);
            return;
        }

        if (_managedPlayers.Contains(q.GetInstanceId()))
        {
            _managedPlayers.Remove(q.GetInstanceId());
        }
        
        q.QueueFree();
    }

    public SoundPlayer3D GetPlayer3D()
    {
        SoundPlayer3D p;

        if (_playerPool3D.Count > 0)
        {
            p = _playerPool3D[0];
            _playerPool3D.RemoveAt(0);
        }
        else
        {
            if (EnableDebug) GD.PushError("Audio Pool Manager has drained its Player3D pool");
            
            if (AllowOverflow)
            {
                p = new SoundPlayer3D();
                _managedPlayers.Add(p.GetInstanceId());
                if (EnableDebug) GD.PushError("Audio Pool Manager creating a new Player3D");
            }
            else
            {
                if (EnableDebug) GD.PushError("Audio Pool Manager attempted to overflow Player3D but was denied.");
                return null;
            }
        }
        
        return p;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p">the player to return to the queue</param>
    /// <returns>whether the player was added to the pool</returns>
    public void ReturnPlayer3D(SoundPlayer3D p)
    {
        // don't accept players we don't manage
        if (!_managedPlayers.Contains(p.GetInstanceId()))
        {
            if (EnableDebug) GD.PushError("Attempted to return a SoundPlayer3D to the audio player pool that is not managed by it.");
            return;
        } 
        
        //  or players that loop or players that are playing
        // basically this means before we return an audio player to the queue we 
        if (p.Loop || p.Playing)
        {
            if (EnableDebug) GD.PushError("You must stop and disable looping on any SoundPlayer3Ds returned to the Audio Player Pool.");
            return;
        }
        
        // otherwise if we've got space, send it back to the pool
        if (_playerPool3D.Count < PoolSize)
        {
            if (EnableDebug) GD.Print("Successfully returned audio player.");
            p.Reparent(this);
            _playerPool3D.Add(p);
            return;
        }
        
        // if we end up here the pool is full so we can torch this player
        _managedPlayers.Remove(p.GetInstanceId());
        p.QueueFree();
    }
    
    public SoundQueue2D GetQueue2D()
    {
        SoundQueue2D q;
        
        if (_queuePool2D.Count > 0)
        {
            q = _queuePool2D[0];
            _queuePool2D.RemoveAt(0);
        }
        else
        {
            EmitSignal(SignalName.PoolEmptied);
            
            if (EnableDebug) GD.PushError("Audio Pool Manager has drained its Queue2D pool");

            if (AllowOverflow)
            {
                q = new SoundQueue2D();
                _managedPlayers.Add(q.GetInstanceId());
                if (EnableDebug) GD.PushError("Audio Pool Manager creating a new Queue2D");
            }
            else
            {
                if (EnableDebug) GD.PushError("Audio Pool Manager attempted to overflow Queue2D but was denied.");
                return null;
            }
        }

        return q;
    }

    public void ReturnQueue2D(SoundQueue2D q)
    {
        // if we have space in the queue for the returned queue, add it and dip
        if (_queuePool2D.Count < PoolSize && _managedPlayers.Contains(q.GetInstanceId()))
        {
            _queuePool2D.Add(q);
            return;
        }
        
        if (_managedPlayers.Contains(q.GetInstanceId()))
        {
            _managedPlayers.Remove(q.GetInstanceId());
        }
        
        q.QueueFree();
    }

    public SoundPlayer2D GetPlayer2D()
    {
        SoundPlayer2D p;

        // same as above
        if (_playerPool2D.Count > 0)
        {
            p = _playerPool2D[0];
            _playerPool2D.RemoveAt(0);
        }
        else
        {
            if (EnableDebug) GD.PushError("Audio Pool Manager has drained its Player2D pool");
            
            if (AllowOverflow)
            {
                p = new SoundPlayer2D();
                _managedPlayers.Add(p.GetInstanceId());
                if (EnableDebug) GD.PushError("Audio Pool Manager creating a new Player2D");
            }
            else
            {
                if (EnableDebug) GD.PushError("Audio Pool Manager attempted to overflow Player2D but was denied.");
                return null;
            }
        }

        return p;
    }

    public bool ReturnPlayer2D(SoundPlayer2D p)
    {
        // same as 3d
        if (!_managedPlayers.Contains(p.GetInstanceId()))
        {
            if (EnableDebug) GD.PushError("Attempted to return a SoundPlayer2D to the audio player pool that is not managed by it.");
            return false;
        }

        if (p.Loop || p.Playing)
        {
            if (EnableDebug) GD.PushError("You must stop and disable looping on any SoundPlayer3Ds returned to the Audio Player Pool.");
            return false;
        }
        
        // if we've got space in the pool and it's one of our players, add it back to the pool
        if (_playerPool2D.Count < PoolSize)
        {
            _playerPool2D.Add(p);
            return true;
        }

        _managedPlayers.Remove(p.GetInstanceId());
        
        // otherwise destroy it
        p.QueueFree();
        return false;
    }
    
    public override void _Ready()
    {
        PoolSize = 64;

        CallDeferred("SetupPlayers");
    }

    private void SetupPlayers()
    {
        for (var i = 0; i < PoolSize; i++)
        {
            SoundQueue3D q3D = new();
            SoundQueue2D q2D = new();
            var p3D = _CreatePlayer3D();
            SoundPlayer2D p2D = new();

            q3D.PlayerCount = QueuePlayerCount;
            q2D.PlayerCount = QueuePlayerCount;
            
            q3D.AddChild(new SoundPlayer3D());
            q3D.AddChild(new SoundPlayer2D());
            
            // AddChild(q3D);
            // AddChild(q2D);
            // AddChild(p3D);
            // AddChild(p2D);
            //
            _queuePool3D.Add(q3D);
            _queuePool2D.Add(q2D);

            
            
            _playerPool3D.Add(p3D);
            _playerPool2D.Add(p2D);
            
            _managedPlayers.Add(q3D.GetInstanceId());
            _managedPlayers.Add(q2D.GetInstanceId());
            _managedPlayers.Add(p3D.GetInstanceId());
            _managedPlayers.Add(p2D.GetInstanceId());
            
            
        }
    }

    private SoundPlayer3D _CreatePlayer3D(bool FreeOnFinish = true)
    {
        SoundPlayer3D p3D = new();
        if (!FreeOnFinish)
        {
            p3D.FreeOnFinish = false;
            p3D.PlayerFinished += ReturnPlayer3D;
        }

        return p3D;
    }
    
    public override void _EnterTree()
    {
        if (Manager != null)
        {
            QueueFree();
        }
        Manager = this;
    }
}
