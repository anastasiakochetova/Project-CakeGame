using System;
using System.Collections.Generic;
using CakeGame.Helpers;

namespace CakeGame.Models
{
    public class GameModel
    {
        private readonly Random rng = new();
        private readonly CakeModel cakeModel;

        public int CorrectStreak { get; private set; } = 0;
        public int Lives { get; private set; } = 3;
        public int Combo { get; private set; } = 0;
        public int ResultPercent { get; set; } = 0;
        public string ResultText { get; set; } = "";
        public bool ShowResult { get; set; } = false;

        public int SelectedSponge { get; set; } = 0;
        public int SelectedCreamColor { get; set; } = 3;

        public event Action OnGameStateChanged;

        public GameModel(CakeModel cakeModel)
        {
            this.cakeModel = cakeModel;
        }

        public void NewGame()
        {
            cakeModel.ClearAll();
            CorrectStreak = 0;
            Combo = 0;
            Lives = 3;
            ShowResult = false;
            GenerateNewSample();
            OnGameStateChanged?.Invoke();
        }

        public void GenerateNewSample()
        {
            cakeModel.SampleLayers.Clear();
            int layerCount = 2 + rng.Next(3);

            for (int i = 0; i < layerCount; i++)
            {
                cakeModel.SampleLayers.Add($"sponge_{rng.Next(4)}");
                cakeModel.SampleLayers.Add($"cream_{rng.Next(Constants.CREAM_COLORS.Length)}");
                if (i == layerCount - 1 || rng.NextDouble() < 0.5)
                    cakeModel.SampleLayers.Add("sprinkle");
            }
        }

        public void NextRound()
        {
            cakeModel.ClearPlayerCake();
            GenerateNewSample();
            OnGameStateChanged?.Invoke();
        }

        public void Compare()
        {
            if (!cakeModel.HasAnyLayers()) return;

            int matches = 0;
            for (int i = 0; i < Math.Min(cakeModel.SampleLayers.Count, cakeModel.PlayerLayers.Count); i++)
            {
                string s = cakeModel.SampleLayers[i];
                string p = cakeModel.PlayerLayers[i];
                if (s == p) matches++;
                else if (s.StartsWith("sponge") && p.StartsWith("sponge") && s == p) matches++;
                else if (s.StartsWith("cream") && p.StartsWith("cream") && s == p) matches++;
            }

            ResultPercent = (int)Math.Round(matches / (double)Math.Max(cakeModel.SampleLayers.Count, cakeModel.PlayerLayers.Count) * 100);

            if (ResultPercent == 100)
            {
                CorrectStreak++;
                Combo = CorrectStreak;
                ResultText = $"ИДЕАЛЬНО! +1\nПравильных подряд: {CorrectStreak}";
            }
            else
            {
                CorrectStreak = 0;
                Combo = 0;
                Lives--;
                ResultText = $"ОШИБКА! Счетчик сброшен\nПравильных подряд: {CorrectStreak}";
            }

            if (Lives <= 0)
                ResultText = $"ИГРА ОКОНЧЕНА!\nНажмите ОК чтобы начать заново";

            ShowResult = true;
            OnGameStateChanged?.Invoke();
        }

        public int CalculateSimilarity()
        {
            int maxLen = Math.Max(cakeModel.SampleLayers.Count, cakeModel.PlayerLayers.Count);
            if (maxLen == 0) return 0;

            int matches = 0;
            for (int i = 0; i < Math.Min(cakeModel.SampleLayers.Count, cakeModel.PlayerLayers.Count); i++)
            {
                string s = cakeModel.SampleLayers[i];
                string p = cakeModel.PlayerLayers[i];
                if (s == p) matches++;
                else if (s.StartsWith("sponge") && p.StartsWith("sponge") && s == p) matches++;
                else if (s.StartsWith("cream") && p.StartsWith("cream") && s == p) matches++;
            }
            return (int)Math.Round(matches / (double)maxLen * 100);
        }

        public List<string> GetSampleLayers() => cakeModel.SampleLayers;
        public List<string> GetPlayerLayers() => cakeModel.PlayerLayers;
        public void AddPlayerLayer(string layer) => cakeModel.PlayerLayers.Add(layer);
        public void ClearPlayerLayers() => cakeModel.PlayerLayers.Clear();
    }
}