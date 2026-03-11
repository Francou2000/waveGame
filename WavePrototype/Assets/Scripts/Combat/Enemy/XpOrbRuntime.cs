using UnityEngine;

namespace WaveGame.Combat.Enemy
{
    public sealed class XpOrbRuntime : MonoBehaviour
    {
        [SerializeField] private float value = 1f;

        public float Value => value;
        public bool IsActive => gameObject.activeInHierarchy;

        public void Activate(Vector3 position, float xpValue)
        {
            transform.position = position;
            value = Mathf.Max(0f, xpValue);
            gameObject.SetActive(true);
        }

        public void AddValue(float delta)
        {
            value = Mathf.Max(0f, value + delta);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
    }
}
