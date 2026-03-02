# waveGame

## Documentation

- Tech spec (arquitectura): `docs/sistema-proyectiles-attacks-techdoc.md`
- Guía paso a paso de implementación: `docs/proyectiles-guia-implementacion-paso-a-paso.md`
- Plan/casos de prueba: `docs/proyectiles-plan-pruebas.md`

## Unity implementation

Core scripts under `Assets/Scripts/Combat/Projectiles`:

- `ProjectileSystem` centralizado (simula todas las instancias)
- `ProjectileDefinition` y `ProjectileGlobalConfig` (`ScriptableObject`)
- `ProjectileInstance`, `HitEvent`, `HitResolver`, `HitRegistry`
- Arquetipos implementados: Straight, Homing, Hitscan, AoE, Beam, Aura
- `ProjectileWeaponEmitter` para disparo desde armas
- `BasicPlayerController` (WASD + disparo Mouse0 para prototipo)

Damage contracts/components under `Assets/Scripts/Combat/Damage`:

- `IDamageable`
- `DamageableBehaviour` (implementación con vida)
- `DestroyOnProjectileHit` (enemigo simple que se destruye en el primer impacto)
