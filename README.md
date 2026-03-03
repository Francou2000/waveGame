# waveGame

## Documentation

- Tech spec (arquitectura): `docs/sistema-proyectiles-attacks-techdoc.md`
- Guía paso a paso de implementación: `docs/proyectiles-guia-implementacion-paso-a-paso.md`
- Plan/casos de prueba: `docs/proyectiles-plan-pruebas.md`
- Data-driven starter alignment: `docs/data-driven-starter-alignment.md`
- Controladores Player/Enemy (milestone): `docs/controladores-player-enemigos-techdoc.md`

## Unity implementation

Core scripts under `Assets/Scripts/Combat/Projectiles`:

- `ProjectileSystem` centralizado (simula todas las instancias)
- `ProjectileDefinition` y `ProjectileGlobalConfig` (`ScriptableObject`)
- `ProjectileInstance`, `HitEvent`, `HitResolver`, `HitRegistry`
- Arquetipos implementados: Straight, Homing, Hitscan, AoE, Beam, Aura
- `ProjectileWeaponEmitter` para disparo manual desde armas
- `AutoFireWeaponEmitter` (auto-fire + auto-aim estilo survivors)
- `WeaponDefinition` (cadencia y composición data-driven)
- `FirePatternSO` / `TargetingDefinitionSO` (patrón y targeting desacoplados)
- `BasicPlayerController` (WASD + disparo Mouse0 para prototipo)
- `TestingArenaSpawner` (escena rápida de testing)

Damage contracts/components under `Assets/Scripts/Combat/Damage`:

- `IDamageable`
- `DamageableBehaviour` (implementación con vida)
- `DestroyOnProjectileHit` (enemigo simple que se destruye en el primer impacto)


## Combat folder layout (target)
- `Assets/Game/Combat/*` reservado para migración al layout del starter spec.
