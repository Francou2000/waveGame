# Data-Driven Starter Spec — Estado de alineación

## Resumen
Se revisó el repositorio contra el spec de Auto-Fire + Projectiles + Targeting y se ajustó la implementación para acercarla al modelo propuesto.

## Alineado
- `ProjectileDefinition` data-driven para comportamiento de proyectil.
- `WeaponDefinition` con composición de:
  - `FirePatternSO`
  - `TargetingDefinitionSO`
  - `ProjectileDefinition`
- `AutoFireWeaponEmitter` con:
  - cooldown por timestamp,
  - burst,
  - spread/patrones,
  - cache de target por intervalo.
- `IDamageable` actualizado a `ApplyDamage(DamageEvent e)`.
- Nuevo `ITargetable` con `GetAimPoint()`.
- Escena de prueba rápida con `TestingArenaSpawner`.

## Cambios aplicados en esta iteración
- Nuevo `DamageEvent`.
- Nuevo `ITargetable`.
- Nuevos SOs: `FirePatternSO`, `TargetingDefinitionSO`.
- `AutoFireWeaponEmitter` migrado a composición data-driven.
- Corrección de auto-aim:
  - `ForwardConeNearest` usa filtro duro de cono.
  - `RandomInRange` usa dirección planar (XZ).

## Pendiente / siguiente paso sugerido
- Migrar físicamente scripts de `Assets/Scripts/Combat/*` a `Assets/Game/Combat/*`.
- Añadir `TargetingSystem` dedicado separado de provider de física.
- Agregar `MostDense` y `LowestHP` reales en targeting (hoy quedan como placeholders de modo).
- Implementar powerups `ModifierSO`/`PowerUpSO`.
- Crear `Starter content` (SOs iniciales: Pistol Auto, Homing Wand, Laser Beam).


## Player/Enemy controllers base (nuevo)
- `PlayerMotor`, `PlayerAim`, `PlayerCombatAnchorProvider`, `PlayerStatsRuntime` + `PlayerStatsDefinitionSO`.
- `EnemySystem` centralizado con seek + separación en ticks.
- `EnemySpawner` con pooling y spawn budget por segundo.


## Enemy system additions
- `EnemyDefinitionSO`, `EnemyDropTableSO`, `EnemyVisualSetSO`, `BossPhaseProfileSO`, `BossLootSO`.
- `EnemyDeathSystem` + `XpOrbSystem` + `XpOrbRuntime` con pooling y merge on spawn.


## Meta progression base
- `ContentCatalogSO`, `PassiveDefinitionSO`, `AbilityDefinitionSO`, `EvolutionRecipeSO`, `UnlockConditionSO`, `ItemRarityProfileSO`.
- `MetaProgressionSaveData` + `MetaProgressionSaveStore` + `MetaProgressionManager` + `RunSessionState`.
