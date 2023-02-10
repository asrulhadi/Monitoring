using System;
using System.Threading.Tasks;
using LibreHardwareMonitor.Hardware;

namespace Monitoring.Models;

public class Measurement
{
    public Measurement()
    {
        this.Open();
    }
    public void GetMeasurement()
    {
        System.Console.WriteLine("Hello from measurement");
        this.Update();
        System.Console.WriteLine("CPU % {0}", CPUPercent);
        System.Console.WriteLine("CPU Temp {0}", CPUTemp);
        System.Console.WriteLine("GPU % {0}", GPUPercent);
        System.Console.WriteLine("GPU Temp {0}", GPUTemp);
    }

    #region Code from https://github.com/FanIT/SysMonAvalonia/blob/main/Hardware/Computer.cs - modified
    private Computer pc;
    private ISensor CpuPercentSensor;
    private ISensor CpuTempSensor;
    private ISensor GpuPercentSensor;
    private ISensor GpuTempSensor;
    private ISensor GpuRamUsedSensor;

    public float CPUPercent { get; private set; } = 0;
    public float CPUTemp { get; private set; } = 0;
    public float GPUPercent { get; private set; } = 0;
    public float GPUTemp { get; private set; } = 0;
    public float GPUTotalRam { get; private set; } = 0;
    public float GPUUsedRam { get; private set; } = 0;
    public float GPUPercentRam { get; private set; } = 0;
    public double RamTotal { get; private set; }
    public double RamUsed { get; private set; }
    public double RamFree { get; private set; }
    public byte RamPercent { get; private set; }


    public void Open()
    {
        if (pc is null)
        {
            pc = new()
            {
                IsCpuEnabled = true,
                IsMotherboardEnabled = true,
                IsGpuEnabled = true
            };
            pc.Open();
        }

        SetSensors();

        RamUpdate();
    }

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
                            if (sensor.SensorType == SensorType.Temperature && sensor.Index == 0)
                            {
                                CpuTempSensor = sensor;
                                CpuTempSensor.ValuesTimeWindow = TimeSpan.Zero;
                                break;
                            }
                        }

                        break;
                    }
                }
            }

            if (hardware.HardwareType == HardwareType.Cpu)
            {
                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Index == 0)
                    {
                        CpuPercentSensor = sensor;
                        CpuPercentSensor.ValuesTimeWindow = TimeSpan.Zero;
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
        }
    }


    private void RamUpdate()
    {
        double devider = 1073741824;

        // MemoryStatusEx msu = new();

        // try
        // { NativeMethods.GlobalMemoryStatusEx(msu); }
        // catch { return; }

        // RamTotal = msu.TotalPhys / devider;
        // RamFree = msu.AvailPhys / devider;
        // RamUsed = (msu.TotalPhys - msu.AvailPhys) / devider;
        // RamPercent = (byte)((msu.TotalPhys - msu.AvailPhys) / (msu.TotalPhys / 100));
    }

    public void Update()
    {
        try
        {
            CpuPercentSensor.Hardware.Update();
            CPUPercent = CpuPercentSensor.Value.Value;
        }
        catch { /* ignored */ }

        try
        {
            CpuTempSensor.Hardware.Update();
            CPUTemp = CpuTempSensor.Value.Value;
        }
        catch { /* ignored */ }

        try
        {
            GpuPercentSensor.Hardware.Update();
            GPUPercent = GpuPercentSensor.Value.Value;
        }
        catch { /* ignored */ }

        try
        {
            GpuTempSensor.Hardware.Update();
            GPUTemp = GpuTempSensor.Value.Value;
        }
        catch { /* ignored */ }

        try
        {
            GpuRamUsedSensor.Hardware.Update();
            GPUUsedRam = GpuRamUsedSensor.Value.Value;
        }
        catch { /* ignored */ }
        
        GPUPercentRam = GPUUsedRam / (GPUTotalRam / 100);

        RamUpdate();
    }

    public void Close()
    {
        pc.Close();
    }
    #endregion
}