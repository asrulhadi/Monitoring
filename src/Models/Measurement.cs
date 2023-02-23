using System;
using System.Collections.Generic;
using System.Linq;

using LibreHardwareMonitor.Hardware;

namespace Monitoring.Models;

public class Measurement
{
    public Measurement()
    {
        pc = new()
        {
            IsCpuEnabled = true,
            IsMotherboardEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
        };
        pc.Open();
        System.Console.WriteLine(pc.GetReport());
        SetSensors();
    }
    public void GetMeasurement(out double load, out double ram)
    {
        System.Console.WriteLine("Hello from measurement");
        this.Update();
        System.Console.WriteLine("CPU % {0}", CPUPercent.Count > 1 ? CPUPercent.Select(a => $"{a:N2}").Aggregate((a,b) => $"{a} {b}") : "");
        System.Console.WriteLine("CPU Temp {0}", CPUTemp.Count > 1 ? CPUTemp.Select(a => $"{a:N2}").Aggregate((a,b) => $"{a} {b}") : "");
        System.Console.WriteLine("GPU % {0}", GPUPercent);
        System.Console.WriteLine("GPU Temp {0}", GPUTemp);
        System.Console.WriteLine("Memory % {0:N2} => {1:N3}Gb/{2:N3}Gb", RamPercent, RamUsed, RamAvailable);
        load = CPUPercent.Count == 0 ? 0 : CPUPercent[0];
        ram = RamPercent;
    }

    #region Code from https://github.com/FanIT/SysMonAvalonia/blob/main/Hardware/Computer.cs - modified
    private Computer pc;
    private List<ISensor> CpuPercentSensor = new();
    private List<ISensor> CpuTempSensor = new();
    private ISensor GpuPercentSensor;
    private ISensor GpuTempSensor;
    private ISensor GpuRamUsedSensor;
    private ISensor RamUsedSensor;
    private ISensor RamAvailableSensor;
    private ISensor RamLoadSensor;

    public List<float> CPUPercent { get; private set; } = new();
    public List<float> CPUTemp { get; private set; } = new();
    public float GPUPercent { get; private set; } = 0;
    public float GPUTemp { get; private set; } = 0;
    public float GPUTotalRam { get; private set; } = 0;
    public float GPUUsedRam { get; private set; } = 0;
    public float GPUPercentRam { get; private set; } = 0;
    public double RamAvailable { get; private set; } = 0;
    public double RamUsed { get; private set; } = 0;
    public double RamPercent { get; private set; } = 0;

    private void SetSensors()
    {
        foreach (IHardware hardware in pc.Hardware)
        {
            hardware.Update();

            if (hardware.HardwareType == HardwareType.Motherboard)
            {
                foreach (IHardware sub in hardware.SubHardware)
                {
                    sub.Update();

                    if (sub.HardwareType == HardwareType.SuperIO)
                    {
                        foreach (ISensor sensor in sub.Sensors)
                        {
                            // if (sensor.SensorType == SensorType.Temperature && sensor.Index == 0)
                            if (sensor.SensorType == SensorType.Temperature)
                            {
                                sensor.ValuesTimeWindow = TimeSpan.Zero;
                                CpuTempSensor.Add(sensor);
                            }
                        }
                        break;
                    }
                }
            }
            if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
            {
                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Index == 0)
                    {
                        GpuPercentSensor = sensor;
                        GpuPercentSensor.ValuesTimeWindow = TimeSpan.Zero;
                    }
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        GpuTempSensor = sensor;
                        GpuTempSensor.ValuesTimeWindow = TimeSpan.Zero;
                    }
                    if (sensor.SensorType == SensorType.SmallData && sensor.Index == 2) GPUTotalRam = sensor.Value.Value;
                    if (sensor.SensorType == SensorType.SmallData && sensor.Index == 1)
                    {
                        GpuRamUsedSensor = sensor;
                        GpuRamUsedSensor.ValuesTimeWindow = TimeSpan.Zero;
                    }
                }
            }
            if (hardware.HardwareType == HardwareType.GpuIntel)
            {
                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Index == 0)
                    {
                        GpuPercentSensor = sensor;
                        GpuPercentSensor.ValuesTimeWindow = TimeSpan.Zero;
                    }
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        GpuTempSensor = sensor;
                        GpuTempSensor.ValuesTimeWindow = TimeSpan.Zero;
                    }
                    if (sensor.SensorType == SensorType.SmallData && sensor.Index == 2) GPUTotalRam = sensor?.Value ?? 0;
                    if (sensor.SensorType == SensorType.SmallData && sensor.Index == 1)
                    {
                        GpuRamUsedSensor = sensor;
                        GpuRamUsedSensor.ValuesTimeWindow = TimeSpan.Zero;
                    }
                }
            }
        }

        IEnumerable<ISensor> sensors;
        // CPU
        var cpu = pc.Hardware.Where(h => h.HardwareType == HardwareType.Cpu);
        // Load Sensors
        sensors = cpu.Where(c => c.Sensors.Length > 0)
                .SelectMany(c => c.Sensors, (_, s) => s)
                .Where(s => s.SensorType == SensorType.Load);
        CpuPercentSensor.AddRange(sensors.Where(s => s is not null).OrderBy(s => s.Index));
        CpuPercentSensor?.ForEach(s => s.ValuesTimeWindow = TimeSpan.Zero);
        // Temperature Sensors
        sensors = cpu.Where(c => c.Sensors.Length > 0)
                .SelectMany(c => c.Sensors, (_, s) => s)
                .Where(s => s.SensorType == SensorType.Temperature);
        CpuTempSensor.AddRange(sensors.Where(s => s is not null).OrderBy(s => s.Index));
        CpuTempSensor?.ForEach(s => s.ValuesTimeWindow = TimeSpan.Zero);

        // intel gpu
        var intelGpu = pc.Hardware.Where(h => h.HardwareType == HardwareType.GpuIntel);
        sensors = cpu.Where(c => c.Sensors.Length > 0).SelectMany(c => c.Sensors, (_, s) => s);

        // Memory
        var memory = pc.Hardware.Where(h => h.HardwareType == HardwareType.Memory);
        RamUsedSensor = memory.Select(m => m.Sensors[0]).FirstOrDefault();
        RamAvailableSensor = memory.Select(m => m.Sensors[1]).FirstOrDefault();
        RamLoadSensor = memory.Select(m => m.Sensors[2]).FirstOrDefault();
    }

    public void Update()
    {
        try
        {
            pc.Hardware.Where(h => h.HardwareType == HardwareType.Cpu).ToList().ForEach(h => h.Update());

            CPUPercent.Clear();
            CPUPercent.AddRange(CpuPercentSensor?.OrderBy(s => s.Index).Select(s => s.Value ?? 0) ?? Array.Empty<float>());

            CPUTemp.Clear();
            CPUTemp.AddRange(CpuTempSensor?.OrderBy(s => s.Index).Select(s => s.Value ?? 0) ?? Array.Empty<float>());
        }
        catch { /* ignored */ }

        try
        {
            //GpuPercentSensor?.Hardware.Update();
            //GPUPercent = GpuPercentSensor is null ? 0 : GpuPercentSensor.Value.Value;
        }
        catch { /* ignored */ }

        try
        {
            //GpuTempSensor?.Hardware.Update();
            //GPUTemp = GpuTempSensor is null ? 0 : GpuTempSensor.Value.Value;
        }
        catch { /* ignored */ }

        try
        {
            //GpuRamUsedSensor?.Hardware.Update();
            //GPUUsedRam = GpuRamUsedSensor is null ? 0 : GpuRamUsedSensor.Value.Value;
        }
        catch { /* ignored */ }

        try
        {
            RamUsedSensor?.Hardware.Update();
            RamUsed = RamUsedSensor?.Value ?? 0;
        }
        catch { /* ignored */ }

        try
        {
            RamAvailableSensor?.Hardware.Update();
            RamAvailable = RamAvailableSensor?.Value ?? 0;
        }
        catch { /* ignored */ }

        try
        {
            RamLoadSensor?.Hardware.Update();
            RamPercent = RamLoadSensor?.Value ?? 0;
        }
        catch { /* ignored */ }
        
        //GPUPercentRam = GPUUsedRam / (GPUTotalRam / 100);

    }

    public void Close()
    {
        pc.Close();
    }
    #endregion
}