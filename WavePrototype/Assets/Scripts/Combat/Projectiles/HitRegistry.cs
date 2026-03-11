using System.Collections.Generic;

namespace WaveGame.Combat.Projectiles
{
    public sealed class HitRegistry
    {
        private readonly Dictionary<ulong, float> _lastHitByPair = new(2048);

        public bool CanHit(int projectileId, int targetId, float now, float cooldown)
        {
            if (cooldown <= 0f)
            {
                return true;
            }

            var key = MakeKey(projectileId, targetId);
            if (!_lastHitByPair.TryGetValue(key, out var lastTime))
            {
                _lastHitByPair[key] = now;
                return true;
            }

            if (now - lastTime < cooldown)
            {
                return false;
            }

            _lastHitByPair[key] = now;
            return true;
        }

        public void ForgetProjectile(int projectileId)
        {
            var keysToRemove = ListPool<ulong>.Get();
            foreach (var pair in _lastHitByPair)
            {
                var keyProjectileId = (int)(pair.Key >> 32);
                if (keyProjectileId == projectileId)
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            for (var i = 0; i < keysToRemove.Count; i++)
            {
                _lastHitByPair.Remove(keysToRemove[i]);
            }

            ListPool<ulong>.Release(keysToRemove);
        }

        private static ulong MakeKey(int projectileId, int targetId)
        {
            unchecked
            {
                return ((ulong)(uint)projectileId << 32) | (uint)targetId;
            }
        }
    }

    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> Pool = new();

        public static List<T> Get()
        {
            return Pool.Count > 0 ? Pool.Pop() : new List<T>(32);
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            Pool.Push(list);
        }
    }
}
