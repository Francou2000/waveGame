using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    [CreateAssetMenu(menuName = "WaveGame/Combat/Enemy Visual Set", fileName = "EnemyVisualSet")]
    public sealed class EnemyVisualSetSO : ScriptableObject
    {
        public Material[] MaterialVariants;
        [Min(0f)] public float MinScale = 0.95f;
        [Min(0f)] public float MaxScale = 1.05f;
        public Color[] TintOptions;

        public Material PickMaterial()
        {
            if (MaterialVariants == null || MaterialVariants.Length == 0)
            {
                return null;
            }

            return MaterialVariants[Random.Range(0, MaterialVariants.Length)];
        }

        public Color PickTint()
        {
            if (TintOptions == null || TintOptions.Length == 0)
            {
                return Color.white;
            }

            return TintOptions[Random.Range(0, TintOptions.Length)];
        }

        public float PickScaleMultiplier()
        {
            return Mathf.Max(0.01f, Random.Range(MinScale, Mathf.Max(MinScale, MaxScale)));
        }
    }
}
