using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GlassPane.Models;
using GlassPane.Native;

namespace GlassPane.Services
{
    public class VirtualDesktopManager : IDisposable
    {
        private static VirtualDesktopManager instance;
        private static readonly object lockObject = new object();

        private Dictionary<int, DesktopAssignment> assignments;
        private Windows11VirtualDesktopManager windows11Manager;
        private bool isDisposed;

        public event EventHandler AssignmentChanged;

        public static VirtualDesktopManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new VirtualDesktopManager();
                        }
                    }
                }
                return instance;
            }
        }

        private VirtualDesktopManager()
        {
            assignments = new Dictionary<int, DesktopAssignment>();
            windows11Manager = new Windows11VirtualDesktopManager();
            windows11Manager.AssignmentChanged += (s, e) => OnAssignmentChanged();
        }

        public void Start()
        {
            windows11Manager.Start();
        }

        public void Stop()
        {
            windows11Manager.Stop();
        }

        public void AssignWindowToDesktop(int desktopNumber)
        {
            windows11Manager.AssignWindowToDesktop(desktopNumber);
        }

        public void SwitchToDesktop(int desktopNumber)
        {
            windows11Manager.SwitchToDesktop(desktopNumber);
        }

        public async Task SwitchToDesktopAsync(int desktopNumber)
        {
            await windows11Manager.SwitchToDesktopAsync(desktopNumber);
        }

        public void RemoveAssignment(int desktopNumber)
        {
            windows11Manager.RemoveAssignment(desktopNumber);
        }

        public void ClearAllAssignments()
        {
            windows11Manager.ClearAllAssignments();
        }

        public IEnumerable<DesktopAssignment> GetAllAssignments()
        {
            return windows11Manager.GetAllAssignments();
        }



        private void OnAssignmentChanged()
        {
            AssignmentChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                windows11Manager?.Dispose();
                isDisposed = true;
            }
        }
    }
} 