using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quest.Interaction;

public class TimerBar : ProgressBar
{
    public event Action<float>? TimerComplete;
    public float CurrentTime { get; set; }
    public float Time { get; set; }
    public bool IsRunning { get; set; }
    public int RepeatCount { get; set; }
    public int MaxRepeatCount { get; set; }
    public TimerBar(Point location, float time, Point size, Color bg, Color fg, int border, int reps = 1) : base(location, size, bg, fg, border)
    {
        Time = time;
        CurrentTime = 0;
        IsRunning = false;
        MaxRepeatCount = reps;
    }
    public override void Update(UserInterface ui)
    {
        // Running
        if (IsRunning)
            CurrentTime += GameManager.DeltaTime;

        // Progress
        Progress = CurrentTime / Time;

        // Complete
        if (Progress >= 1)
        {
            TimerComplete?.Invoke(Time);
            RepeatCount++;
            CurrentTime = 0;
            if (RepeatCount >= MaxRepeatCount)
                Stop();
        }
    }
    public override void Draw(UserInterface ui)
    {
        // Border
        ui.Batch.DrawRectangle(Bounds, Color.Black, Border);
        // Background
        ui.Batch.FillRectangle(Bounds, Background);
        // Foreground
        ui.Batch.FillRectangle(Bounds.Location.ToVector2(), new(Bounds.Width * Progress, Bounds.Height), Foreground);
    }
    public void Restart()
    {
        RepeatCount = 0;
        Start();
    }
    public void Start() => IsRunning = true;
    public void Stop() => IsRunning = false;
}
