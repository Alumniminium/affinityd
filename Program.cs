using affinity_daemon;

ThreadPool.QueueUserWorkItem((x) => Kernel.Initialize());

Version version = typeof(Program).Assembly.GetName().Version!;

while (true)
{
    Console.WriteLine($"Affinity Daemon Version {version.ToString(3)}");
    Kernel.ReadConfig();
    HyprWindows.GetState();
    Kernel.SetAffinities();
    Console.Clear();
    Thread.Sleep(5000);
}