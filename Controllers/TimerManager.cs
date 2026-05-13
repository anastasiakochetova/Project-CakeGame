using System.Windows.Forms;
using CakeGame.Models;

namespace CakeGame.Controllers
{
    public class TimerManager
    {
        private readonly GameController controller;
        private Timer convTimer, beltSpawnTimer, dispTimer, flashTimer, ovenTimer, partTimer, pulseTimer;
        private int flashTarget = -1;

        public TimerManager(GameController controller)
        {
            this.controller = controller;
        }

        public void StartAll(Form form)
        {
            convTimer = new Timer { Interval = 16 };
            convTimer.Tick += (s, e) => { controller.UpdateConveyor(); form.Invalidate(); };
            convTimer.Start();

            beltSpawnTimer = new Timer { Interval = 2200 };
            beltSpawnTimer.Tick += (s, e) => { controller.SpawnBeltCake(); form.Invalidate(); };
            beltSpawnTimer.Start();

            dispTimer = new Timer { Interval = 20 };
            dispTimer.Tick += (s, e) => { controller.UpdateDispenserDrips(); form.Invalidate(); };
            dispTimer.Start();

            flashTimer = new Timer { Interval = 400 };
            flashTimer.Tick += (s, e) => { controller.ResetFlash(); form.Invalidate(); };

            ovenTimer = new Timer { Interval = 80 };
            ovenTimer.Tick += (s, e) => { controller.UpdateOvenFlame(); form.Invalidate(); };
            ovenTimer.Start();

            partTimer = new Timer { Interval = 16 };
            partTimer.Tick += (s, e) => { controller.UpdateParticles(); form.Invalidate(); };
            partTimer.Start();

            pulseTimer = new Timer { Interval = 16 };
            pulseTimer.Tick += (s, e) => { controller.UpdateDispPulse(); form.Invalidate(); };
            pulseTimer.Start();
        }

        public void StopAll()
        {
            convTimer?.Stop();
            beltSpawnTimer?.Stop();
            dispTimer?.Stop();
            flashTimer?.Stop();
            ovenTimer?.Stop();
            partTimer?.Stop();
            pulseTimer?.Stop();
        }

        public void StartFlash(int target)
        {
            flashTarget = target;
            flashTimer?.Stop();
            flashTimer?.Start();
        }

        public void StopFlash()
        {
            flashTimer?.Stop();
            controller.SetDispActive(flashTarget, false);
            flashTarget = -1;
        }
    }
}