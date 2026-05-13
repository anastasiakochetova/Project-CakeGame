using System.Collections.Generic;

namespace CakeGame.Models
{
    public class CakeModel
    {
        public List<string> SampleLayers { get; private set; } = new List<string>();
        public List<string> PlayerLayers { get; private set; } = new List<string>();

        public void ClearPlayerCake() => PlayerLayers.Clear();
        public void ClearSampleCake() => SampleLayers.Clear();
        public void ClearAll() { PlayerLayers.Clear(); SampleLayers.Clear(); }

        public void AddSpongeLayer(int spongeType) => PlayerLayers.Add($"sponge_{spongeType}");
        public void AddCreamLayer(int creamColor) => PlayerLayers.Add($"cream_{creamColor}");
        public void AddSprinkleLayer() => PlayerLayers.Add("sprinkle");

        public bool HasAnyLayers() => PlayerLayers.Count > 0;
    }
}
