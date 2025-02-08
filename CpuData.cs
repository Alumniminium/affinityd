namespace affinity_daemon;
 
public record CpuData(List<Cpu> cpus);
public record Cpu(int cpu, int core, float maxmhz);