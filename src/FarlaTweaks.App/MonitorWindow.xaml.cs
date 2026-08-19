using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using FarlaTweaks.Core.Diagnostics;
using FarlaTweaks.Core.Monitoring;

namespace FarlaTweaks.App;

public partial class MonitorWindow : Window
{
    private readonly PerformanceSampler _sampler = new();
    private readonly CopilotEngine _copilot = new();
    private readonly DispatcherTimer _timer;
    private readonly Queue<double> _cpuHistory = new();
    private readonly Queue<double> _memoryHistory = new();

    public MonitorWindow()
    {
        InitializeComponent();
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += Timer_OnTick;
        Loaded += MonitorWindow_OnLoaded;
        Closed += MonitorWindow_OnClosed;
    }

    private void MonitorWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
        _timer.Start();
        _ = SampleAsync();
    }

    private async void Timer_OnTick(object? sender, EventArgs e) => await SampleAsync();

    private async Task SampleAsync()
    {
        try
        {
            var sample = await Task.Run(_sampler.Sample);
            var observation = _copilot.Observe(sample);
            Add(_cpuHistory, sample.CpuPercent);
            Add(_memoryHistory, sample.MemoryPercent);

            CpuText.Text = $"{sample.CpuPercent:0}%";
            MemoryText.Text = $"{sample.MemoryPercent:0}%";
            GpuText.Text = sample.GpuPercent.HasValue ? $"{sample.GpuPercent:0}%" : "N/A";
            StatusText.Text = $"{observation.Title}  ·  {observation.Detail}";
            StatusText.Foreground = observation.State == "attention"
                ? (Brush)FindResource("FarlaWarning")
                : (Brush)FindResource("FarlaMuted");
            DrawGraph();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Monitor unavailable: {ex.Message}";
        }
    }

    private static void Add(Queue<double> queue, double value)
    {
        queue.Enqueue(value);
        while (queue.Count > 90)
            queue.Dequeue();
    }

    private void DrawGraph()
    {
        GraphCanvas.Children.Clear();
        var width = Math.Max(20, GraphCanvas.ActualWidth);
        var height = Math.Max(20, GraphCanvas.ActualHeight);
        DrawLine(_cpuHistory, width, height, 0);
        DrawLine(_memoryHistory, width, height, 1);
    }

    private void DrawLine(Queue<double> values, double width, double height, int layer)
    {
        if (values.Count < 2)
            return;

        var points = new PointCollection();
        var snapshot = values.ToArray();
        for (var i = 0; i < snapshot.Length; i++)
        {
            var x = i * (width / Math.Max(1, snapshot.Length - 1));
            var y = height - snapshot[i] / 100d * height;
            points.Add(new Point(x, y));
        }

        var line = new Polyline
        {
            Points = points,
            Stroke = layer == 0
                ? new SolidColorBrush(Color.FromRgb(196, 204, 196))
                : new SolidColorBrush(Color.FromRgb(120, 132, 122)),
            StrokeThickness = layer == 0 ? 2 : 1.5,
            Opacity = layer == 0 ? 0.95 : 0.8
        };
        GraphCanvas.Children.Add(line);
    }

    private void MonitorWindow_OnClosed(object? sender, EventArgs e)
    {
        _timer.Stop();
        _sampler.Dispose();
    }

    private void DragArea_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e) => Close();
}
