# Tech Doc — Controladores de Player y Enemigos (Base Survivors 3D)

## 1) Milestone implementado

### Player
- `PlayerMotor` con `CharacterController` (WASD no relativo a cámara + gravedad manual).
- `PlayerAim` para orientar hacia dirección de movimiento (modo base).
- `PlayerCombatAnchorProvider` para exponer `Position`, `Forward` y `CombatAnchor`.
- `PlayerStatsRuntime` + `PlayerStatsDefinitionSO` (vida, velocidad, attack speed multiplier).

### Enemy
- `EnemyRuntime` con `ITargetable + IDamageable`.
- `EnemySystem` centralizado (sin `Update` por enemigo) con:
  - seek hacia player,
  - separación local con `OverlapSphereNonAlloc` por ticks.
- `EnemySpawner` con pooling y budget de spawn por segundo.

## 2) Contratos de combate

- `ITargetable`
  - `EntityId`
  - `GetAimPoint()`
  - `IsAlive`
- `IDamageable`
  - `TeamId`, `Position`
  - `ApplyDamage(DamageEvent e)`

## 3) Integración con armas auto-fire

`AutoFireWeaponEmitter` ahora puede tomar:
- `PlayerCombatAnchorProvider` para origen/forward de disparo,
- `PlayerStatsRuntime` para `AttackSpeedMultiplier` runtime.

Esto permite que weapon + targeting consuman datos reales de player.

## 4) Escena de testing recomendada

1. Crear Player prefab con:
   - `CharacterController`
   - `PlayerStatsRuntime` (+ opcional `PlayerStatsDefinitionSO`)
   - `PlayerMotor`
   - `PlayerAim`
   - hijo `CombatAnchor` + `PlayerCombatAnchorProvider`
   - `AutoFireWeaponEmitter`
2. Crear Enemy prefab con:
   - `EnemyRuntime`
   - `CapsuleCollider` (layer de enemigos)
3. En escena agregar:
   - `ProjectileSystem`
   - `EnemySystem` (con referencia al transform del player)
   - `EnemySpawner` (prefab + referencia a EnemySystem)

## 5) Próximos pasos

- Evitar `Transform` writes individuales con batches si se llega a miles de enemigos.
- Agregar avoidance de paredes simple en `EnemySystem`.
- Migrar estructura física de scripts a `Assets/Game/Combat/*`.
