using System;
using System.Collections.Generic;
using System.Drawing;
using CakeGame.Helpers;
using CakeGame.Models;

namespace CakeGame.Controllers
{
    public class GameController
    {
        private readonly GameModel model;
        private readonly CakeModel cakeModel;
        private readonly Random rng = new();

        // Анимационные состояния
        public bool[] DispActive { get; } = new bool[2];
        public float[] DispDripY { get; } = new float[2];
        public bool[] DispDripping { get; } = new bool[2];
        public float ConvOffset { get; set; } = 0f;
        public int OvenFlame { get; set; } = 0;
        public int ActiveDispPulse { get; set; } = 0;
        public List<Particle> Particles { get; } = new();
        public List<BeltCake> BeltCakes { get; } = new();

        // Состояния UI
        public bool ShowWrongFlash { get; set; } = false;
        public int WrongFlashTimer { get; set; } = 0;
        public int ShakeTimer { get; set; } = 0;
        public bool NewGameFlash { get; set; } = false;
        public bool CompareFlash { get; set; } = false;
        public bool TrashHover { get; set; } = false;

        public event Action OnStateChanged;

        public GameController(GameModel model, CakeModel cakeModel)
        {
            this.model = model;
            this.cakeModel = cakeModel;
        }

        public void UpdateConveyor()
        {
            ConvOffset += 1.4f;
            if (ConvOffset > 40) ConvOffset -= 40;
            for (int i = BeltCakes.Count - 1; i >= 0; i--)
            {
                BeltCakes[i].X += 1.4f;
                if (BeltCakes[i].X > Constants.FORM_W + 120)
                    BeltCakes.RemoveAt(i);
            }
            OnStateChanged?.Invoke();
        }

        public void SpawnBeltCake()
        {
            int maxCakes = model.Combo >= 5 ? 4 : model.Combo >= 3 ? 3 : 2;
            if (BeltCakes.Count < maxCakes)
            {
                BeltCakes.Add(new BeltCake
                {
                    X = Constants.OVEN_X + Constants.OVEN_W + 10f,
                    SpongeIdx = rng.Next(4),
                    HasCream = rng.NextDouble() > 0.4,
                    HasSprinkle = rng.NextDouble() > 0.6
                });
            }
            OnStateChanged?.Invoke();
        }

        public void UpdateDispenserDrips()
        {
            for (int i = 0; i < 2; i++)
            {
                if (DispDripping[i])
                {
                    DispDripY[i] += 40f;
                    if (DispDripY[i] > Constants.WORK_Y + Constants.WORK_H - Constants.DispRects[i].Bottom - 20)
                    {
                        DispDripping[i] = false;
                        DispDripY[i] = 0;
                    }
                }
            }
            OnStateChanged?.Invoke();
        }

        public void ResetFlash()
        {
            for (int i = 0; i < DispActive.Length; i++)
                DispActive[i] = false;
            OnStateChanged?.Invoke();
        }

        public void SetDispActive(int index, bool value)
        {
            if (index >= 0 && index < DispActive.Length)
                DispActive[index] = value;
        }

        public void UpdateOvenFlame()
        {
            OvenFlame = (OvenFlame + 1) % 4;
            OnStateChanged?.Invoke();
        }

        public void UpdateDispPulse()
        {
            ActiveDispPulse = (ActiveDispPulse + 1) % 40;
            OnStateChanged?.Invoke();
        }

        public void UpdateParticles()
        {
            for (int i = Particles.Count - 1; i >= 0; i--)
            {
                var p = Particles[i];
                p.X += p.Vx;
                p.Y += p.Vy;
                p.Vy += 0.18f;
                p.Life -= p.Decay;
                if (p.Life <= 0 || p.Y > Constants.FORM_H)
                    Particles.RemoveAt(i);
            }
            OnStateChanged?.Invoke();
        }

        public void SpawnParticles(string type, int x = -1, int y = -1)
        {
            if (x == -1) x = Constants.FORM_W / 2;
            if (y == -1) y = Constants.FORM_H / 2;

            int count = type switch
            {
                "sprinkle" or "correct" or "wrong" => 30,
                "trash" => 25,
                _ => 14
            };

            for (int i = 0; i < count; i++)
            {
                Color col = type switch
                {
                    "sponge" => Constants.SPONGE_COLORS[model.SelectedSponge],
                    "cream" => Constants.CREAM_COLORS[model.SelectedCreamColor],
                    "correct" => Color.FromArgb(0x4c, 0xaf, 0x50),
                    "wrong" => Color.FromArgb(0xff, 0x44, 0x44),
                    "trash" => Color.FromArgb(0x88, 0x66, 0x44),
                    _ => Constants.SPRINKLE_COLORS[rng.Next(Constants.SPRINKLE_COLORS.Length)]
                };

                Particles.Add(new Particle
                {
                    X = x + (float)(rng.NextDouble() - 0.5) * 30,
                    Y = y,
                    Vx = (float)(rng.NextDouble() - 0.5) * 2f,
                    Vy = 1.5f + (float)rng.NextDouble() * 3f,
                    Size = (type == "sprinkle" || type == "correct" || type == "wrong") ? 3 : 6,
                    Col = col,
                    Life = 1f,
                    Decay = 0.015f + (float)rng.NextDouble() * 0.01f,
                    Type = type
                });
            }
            OnStateChanged?.Invoke();
        }

        public void TriggerDispenser(int idx)
        {
            DispActive[idx] = true;
            DispDripping[idx] = true;
            DispDripY[idx] = 0;

            string type = idx == 0 ? "cream" : "sprinkle";
            SpawnParticles(type, Constants.DispRects[idx].X + Constants.DispRects[idx].Width / 2, Constants.DispRects[idx].Bottom + 6);

            if (type == "cream")
                cakeModel.PlayerLayers.Add($"cream_{model.SelectedCreamColor}");
            else
                cakeModel.PlayerLayers.Add(type);

            OnStateChanged?.Invoke();
        }

        public void AddLayerSponge()
        {
            SpawnParticles("sponge", Constants.SPONGE_ICONS_X + Constants.SPONGE_ICON_SIZE / 2, Constants.SPONGE_ICONS_Y + 100);
            cakeModel.PlayerLayers.Add($"sponge_{model.SelectedSponge}");
            OnStateChanged?.Invoke();
        }

        public void ClearTrash()
        {
            if (cakeModel.PlayerLayers.Count > 0)
            {
                cakeModel.PlayerLayers.Clear();
                SpawnParticles("trash");
            }
        }

        public void UpdateWrongFlash()
        {
            if (WrongFlashTimer > 0)
            {
                WrongFlashTimer--;
                if (WrongFlashTimer == 0)
                    ShowWrongFlash = false;
            }
        }

        public void UpdateShake()
        {
            if (ShakeTimer > 0)
                ShakeTimer--;
        }

        public void SetWrongFlash(int duration)
        {
            ShowWrongFlash = true;
            WrongFlashTimer = duration;
        }

        public void SetShake(int duration)
        {
            ShakeTimer = duration;
        }
    }
}
