# Guía paso a paso — Implementación del Sistema de Proyectiles (Unity 3D)

Esta guía explica **cómo funciona** el sistema actual y cómo integrarlo en una escena de Unity 3D de forma incremental.

## 1) Qué piezas existen y para qué sirven

### Núcleo
- `ProjectileSystem`:
  - Simula todos los proyectiles activos en un solo `Update`.
  - Ejecuta arquetipos (`Straight`, `Homing`, `Beam`, etc.).
  - Limita physics queries por frame para mantener presupuesto.
  - Encola y resuelve eventos de daño (`HitEvent` + `HitResolver`).

### Datos configurables
- `ProjectileDefinition` (`ScriptableObject`): define comportamiento de cada ataque.
- `ProjectileGlobalConfig` (`ScriptableObject`): define caps globales y tamaños de buffers.

### Runtime y daño
- `ProjectileInstance`: estado liviano en runtime.
- `HitRegistry`: evita double-hit por target/proyectil en ventanas cortas.
- `IDamageable`: contrato para enemigos/objetivos dañables.
- `DamageableBehaviour`: implementación simple con vida.
- `DestroyOnProjectileHit`: implementación para prototipo (destrucción al primer impacto).

### Arquetipos implementados
- `StraightProjectileArchetype`
- `HomingProjectileArchetype`
- `HitscanProjectileArchetype`
- `AoEProjectileArchetype`
- `BeamProjectileArchetype`
- `AuraProjectileArchetype`

## 2) Preparación en Unity (escena mínima)

1. Crea una escena de prueba vacía.
2. Agrega un `GameObject` llamado `CombatRoot`.
3. Añade el componente `ProjectileSystem` a `CombatRoot`.
4. Crea un asset `ProjectileGlobalConfig` y asígnalo al campo `globalConfig`.
5. Configura `enemyMask` en `ProjectileSystem` para que incluya la layer de enemigos.

## 3) Preparar enemigos para recibir daño

1. Crea un prefab de enemigo con `Collider` (y opcional `Rigidbody` si lo necesitás en tu juego).
2. Para este prototipo, añade `DestroyOnProjectileHit` al prefab.
   - Si más adelante querés vida/estadísticas, reemplazalo por `DamageableBehaviour`.
3. Configura:
   - `entityId` único por instancia (en producción conviene asignarlo desde un sistema de entidades).
   - `teamId` del enemigo (por ejemplo `2`).
   - Si usás `DamageableBehaviour`, también definir `health` inicial.
4. Verifica que el enemigo esté en la layer incluida por `enemyMask` / `HitMask`.

## 4) Crear un proyectil (data-driven)

1. Crea un asset `ProjectileDefinition`.
2. Configura mínimo:
   - `ArchetypeType`.
   - `Speed`, `LifetimeSeconds`, `MaxDistance`.
   - `Radius`, `HitMask`.
   - `BaseDamage`, `DamageType`.
3. Para homing:
   - `AcquireRadius`, `RetargetInterval`, `TurnRate`.
4. Para beam/aura:
   - `TickInterval`, `HitCooldownPerTarget`.

## 5) Integración de disparo (arma)

1. En el arma o jugador agrega `ProjectileWeaponEmitter`.
2. Referencia:
   - `projectileSystem` (el de `CombatRoot`).
   - `projectileDefinition` (asset del ataque).
   - `ownerEntityId` y `teamId` del jugador.
3. Llama `Fire()` desde input/anim event/ability.

## 6) Flujo interno en runtime (qué pasa cuando disparás)

1. `ProjectileWeaponEmitter.Fire()` construye `ProjectileSpawnContext`.
2. `ProjectileSystem.TrySpawn(...)` crea `ProjectileInstance` y lo agrega a la lista activa.
3. Cada frame, `ProjectileSystem.Update()`:
   - avanza `timeAlive`;
   - descarta por `lifetime`/`maxDistance`;
   - ejecuta `archetype.Simulate(...)`;
   - encola `HitEvent`;
   - resuelve hits en lote con `HitResolver`.
4. `HitResolver` calcula crit y aplica daño al `IDamageable` objetivo.

## 7) Recomendación de rollout en proyecto real

1. Activar primero `Straight` + `AoE`.
2. Validar colisiones/cooldowns y presupuesto de física.
3. Integrar `Homing`.
4. Integrar `Beam`/`Aura` con tick controlado.
5. Recién después sumar arquetipos faltantes (`Chain`, `Ricochet`, `Orbital`, `GroundField`).

## 8) Checklist de producción

- [ ] No usar `Instantiate/Destroy` en gameplay para proyectiles/VFX/UI (usar pooling).
- [ ] Validar que `HitMask` y layers estén bien configurados.
- [ ] IDs de entidad únicos y consistentes.
- [ ] `MaxPhysicsQueriesPerFrame` calibrado con profiler.
- [ ] Sin GC spikes durante combate sostenido.
- [ ] Tests automatizados de reglas de hit y cooldown.

## 9) Controlador de jugador básico (WASD + Mouse0)

1. Crea un `GameObject` Player con:
   - `CharacterController`
   - `ProjectileWeaponEmitter`
   - `BasicPlayerController`
2. En `ProjectileWeaponEmitter` asigna:
   - `projectileSystem`
   - `projectileDefinition`
   - `ownerEntityId` y `teamId` del jugador
3. En `BasicPlayerController` asigna:
   - `weaponEmitter` (el del mismo player)
   - `cameraTransform` (opcional; si queda vacío usa `Camera.main`)
4. Controles:
   - Movimiento: `WASD`
   - Disparo: mantener `Mouse Izquierdo`

## 10) Setup de escena de testing (auto-fire + auto-aim)

1. Crea assets data-driven:
   - `WeaponDefinition`: `BaseCooldown`, `ProjectilesPerShot`, `Range`, `MuzzleLocalOffset`
   - `FirePatternSO`: por ejemplo `Single` o `SpreadCone`
   - `TargetingDefinitionSO`: por ejemplo `Nearest` o `ForwardConeNearest`
   - Asignar `ProjectileDefinition` al `WeaponDefinition`
2. En Player agrega `AutoFireWeaponEmitter`:
   - asignar `ProjectileSystem`
   - asignar `WeaponDefinition`
   - `ownerEntityId/teamId`
   - para pruebas manuales, activar `requireMouseHold`
3. Crea prefab de enemigo simple con:
   - `Collider`
   - `DestroyOnProjectileHit`
4. Crea un `GameObject` con `TestingArenaSpawner` y asigna el prefab enemigo.
5. Ejecuta escena:
   - mover con WASD (`BasicPlayerController`)
   - mantener Mouse0 (si `requireMouseHold = true`) para auto-fire
   - verificar auto-aim hacia enemigos y destrucción al impacto

## 11) Qué testear rápido en esta escena

- Cooldown estable (ritmo parejo, sin drift visible).
- Cambio de `TargetingModeSO` (`Nearest`, `ForwardConeNearest`, `RandomInRange`).
- `ProjectilesPerShot` + `SpreadAngle` + `BurstCount`.
- Diferencia entre `Straight` (no corrige) y `Homing` (corrige en vuelo).


> Nota de targeting: `ForwardConeNearest` aplica filtro duro por ángulo; `RandomInRange` usa dirección aleatoria planar (XZ) para conservar comportamiento top-down estable.
