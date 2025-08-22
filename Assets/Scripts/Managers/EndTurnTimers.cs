using System.Collections.Generic;

public static class EndTurnTimers {
    public static List<EndTurnTimer> timers = new();

    public static void CreateTimer(int duration, System.Action action) {
        timers.Add(new EndTurnTimer(duration, action));
    }
    public static void RemoveTimer(EndTurnTimer timer) { timers.Remove(timer); }

    public static void ClearTimers() { timers.ForEach(x => x.UnSubscribe()); timers.Clear();}
}
public class EndTurnTimer
{
    int duration = 0;
    public System.Action action;
    public EndTurnTimer(int duration, System.Action action)
    {
        this.duration = duration;
        this.action = action;
        GameManager.OnEndTurn += OnEndTurn;
    }
    public void OnEndTurn()
    {
        duration--;
        if (duration <= 0)
        {
            action?.Invoke();
            Clear();
        }
    }
    public void Clear()
    {
        UnSubscribe();
        EndTurnTimers.RemoveTimer(this);
    }
    public void UnSubscribe() {
        action = null;
        GameManager.OnEndTurn -= OnEndTurn;
    }
}
