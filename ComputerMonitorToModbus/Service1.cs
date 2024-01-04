using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.Hardware;
using EasyModbus;

namespace ComputerMonitorToModbus
{
 
    public partial class Service1 : ServiceBase
    {
        private static float? Temperature;
        private static float? CPULoad;
        private static float? RAMAvailable;
        private static float? RAMLoad;
        private static float? HDDLoad;
        ModbusServer ModServer;

        public Service1()
        {
            InitializeComponent();
            ModServer = new ModbusServer();
            ModServer.Listen();
            ModbusServer.HoldingRegisters data = ModServer.holdingRegisters;
            
            while (true)
            {
                GetSystemInfo();
                data[1] = Convert.ToInt16(Temperature);
                data[2] = Convert.ToInt16(CPULoad*100);
                data[3] = Convert.ToInt16(RAMAvailable*100);
                data[4] = Convert.ToInt16(RAMLoad * 100);
                data[5] = Convert.ToInt16(HDDLoad * 100);
            }
        }

        static void GetSystemInfo()
        {
            UpdateVisitor updateVisitor = new UpdateVisitor();
            Computer computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.RAMEnabled = true;
            computer.HDDEnabled = true;
            computer.MainboardEnabled = true;
            computer.Accept(updateVisitor);

            Temperature = computer.Hardware[0].Sensors[9].Value;
            CPULoad = computer.Hardware[0].Sensors[4].Value;
            RAMAvailable = computer.Hardware[1].Sensors[2].Value;
            RAMLoad = computer.Hardware[1].Sensors[0].Value;
            HDDLoad = computer.Hardware[2].Sensors[0].Value;

            Console.WriteLine("CPU Temperature= " + Temperature.ToString() + "°C");
            Console.WriteLine("CPU Load= " + "%" + CPULoad.ToString());
            Console.WriteLine("RAM Available= " + RAMAvailable.ToString() + " GB");
            Console.WriteLine("RAM Load= " + "%" + RAMLoad.ToString());
            Console.WriteLine("HDD Load= " + "%" + HDDLoad.ToString());
            Console.WriteLine("*********************");
            computer.Close();

        }

        protected override void OnStart(string[] args)
        {


        }

        protected override void OnStop()
        {
            ModServer.StopListening();
            ModServer = null;
        }
    }
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
