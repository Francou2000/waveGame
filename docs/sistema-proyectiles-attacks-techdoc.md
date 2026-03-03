# Tech Doc (Formal) — Sistema de Proyectiles / Attacks

- **Engine:** Unity 6 + URP
- **Plataforma:** PC
- **Target:** 60 FPS cap (16.67 ms/frame)
- **Juego:** 3ra persona, hordas, colliders simples en enemigos

## 1) Alcance y definiciones

### 1.1 Qué es “proyectil” en este documento

Se considera “proyectil” cualquier entidad/ataque que:

- se mueve o existe en el espacio (bala, misil homing, rayo, aura, campo en suelo),
- puede impactar enemigos y/o mundo,
- dispara VFX/SFX,
- aplica daño / estados / knockback.

> **Nota de performance:** para escalar, los “proyectiles” deben tratarse como instancias livianas simuladas por un sistema central, no como `GameObjects` con `Update` individual.

### 1.2 Objetivos

- **Escalabilidad:** soportar cientos/miles de instancias.
- **Consistencia:** colisión estable (anti-tunneling) y reglas de hit claras.
- **Extensibilidad:** sumar arquetipos sin reescribir el core.
- **Performance:** 0 allocs en gameplay; sin `O(N²)` involuntario.

## 2) Principios de performance (reglas de oro)

1. Sin `Instantiate/Destroy` durante gameplay → **pooling**.
2. Sin `Update` por entidad (si es posible) → **`ProjectileSystem` central**.
3. Colisión por segmento (`casts`), no por `triggers` masivos.
4. Targeting no per-frame: **lock-on + retarget por intervalos**.
5. Queries NonAlloc (`SphereCastNonAlloc`, `OverlapSphereNonAlloc`, etc.).
6. Caps globales + degradación graciosa (VFX/UI).

## 3) Arquitectura: datos vs runtime vs resolución

### 3.1 Componentes principales

- **`ProjectileDefinition`** (`ScriptableObject`): datos y reglas configurables por diseño.
- **`ProjectileInstance`** (`struct`/clase liviana): estado runtime (`pos`, `counters`, `target`, etc.).
- **`ProjectileSystem`** (manager): simula todas las instancias activas.
- **`HitResolver` / `DamageSystem`**: aplica daño/status y dispara eventos.
- **Pools** (`VFX/Projectile visuals/UI`): reutilización sin GC.

### 3.2 Flujo de responsabilidades

```text
[Weapon/Ability]
   -> Spawn(ProjectileDefinition, SpawnContext)
        -> ProjectileSystem.AddInstance(instance)

ProjectileSystem.Update(dt)
   -> Archetype.Simulate(instance, dt)
   -> HitResolver.Resolve(hitContext)
   -> VFX/SFX via Pools
   -> Recycle instance (cuando termina)
```

## 4) Modelo de datos propuesto

### 4.1 `ProjectileDefinition` (`ScriptableObject`)

#### General

- `ArchetypeType` (enum): `Straight`, `Homing`, `Beam`, `AoE`, `Aura`, `Chain`, `Ricochet`, `Orbital`, `GroundField`, `Hitscan`, etc.
- `LifetimeSeconds`
- `MaxDistance`

#### Movimiento

- `Speed`
- `Acceleration`
- `TurnRate` (homing)
- `GravityScale` (opcional)
- `Drag`

#### Colisión

- `Radius` (SphereCast / grosor)
- `HitMask` (Enemy + World)
- `StopOnWorld` (`bool`)
- `StopOnEnemy` (`bool`)

#### Impact rules

- `PierceCount` (`int`)
- `BounceCount` (`int`)
- `MaxHitsTotal` (`int`)
- `FalloffPerPierce` (`%`)
- `FalloffPerBounce` (`%`)

#### Damage

- `BaseDamage`
- `DamageType` (enum)
- `CritChance`
- `CritMultiplier`
- `StatusEffects[]` (SOs o IDs)
- `KnockbackForce` (opcional)

#### Tick / Multi-hit control

- `TickInterval` (beams/auras/fields)
- `HitCooldownPerTarget` (ej. `0.1s`)

#### Targeting (si aplica)

- `AcquireRadius`
- `RetargetInterval`
- `PreferForwardAngle` (`0..1` o grados)

#### Presentation

- `VisualPrefabId` (pool)
- `ImpactVfxId`
- `LoopVfxId` (beam, aura)
- `SfxId`

### 4.2 `ProjectileInstance` (runtime)

Estado runtime recomendado (evitar referencias pesadas):

- `definitionId` (o referencia directa, según diseño)
- `ownerEntityId` / `team`
- `pos`, `dir`, `speed`
- `travelled`, `timeAlive`
- `piercesLeft`, `bouncesLeft`, `hitsDone`
- `targetId` (homing)
- `nextRetargetTime`
- `nextTickTime`
- `damageScale` (falloff acumulado)
- `visualHandle` (id a pool / `transform` si aplica)

**Anti double-hit:** usar cooldown por target mediante `HitRegistry` (sección 6.4).

## 5) Pipeline de simulación (core loop)

### 5.1 Actualización por sistema central

`ProjectileSystem.Update(dt)`:

1. Avanza timers globales.
2. Itera lista de instancias activas.
3. Llama a `Archetype.Simulate(instance, dt)`.
4. Maneja reciclado (`lifetime`, `maxDistance`, caps).
5. Actualiza render/VFX (opcional; costo mínimo).

### 5.2 Flujo general por instancia

```text
For each instance:
  timeAlive += dt
  if timeAlive > lifetime -> Recycle

  if maxDistance && travelled > maxDistance -> Recycle

  Archetype.Simulate(...)
     -> may generate HitEvents (0..n)
     -> may request VFX/SFX
     -> may mark instance as finished

  Apply queued HitEvents via HitResolver
  If finished -> Recycle
```

## 6) Colisiones, hits y reglas de impacto

### 6.1 Segment collision (anti tunneling)

Para `straight/homing/ricochet`:

```text
prevPos = pos
nextPos = pos + velocity * dt
SphereCastNonAlloc(prevPos, radius, dir, distance(prev,next), hitMask)
```

**Por qué `SphereCast`:** en 3ra persona “perdona” levemente el aiming y mejora la sensación de impacto.

### 6.2 Resolución de hit (orden recomendado)

1. Tomar el `RaycastHit` más cercano.
2. Clasificar: `Enemy` o `World`.
3. Aplicar reglas:
   - `World` + `StopOnWorld` → terminar.
   - `Enemy` → aplicar daño y luego:
     - si `piercesLeft > 0` → decrementar y seguir,
     - si no → terminar.
4. Si `ricochet`: reflejar dirección y decrementar `bouncesLeft`.
5. Disparar VFX/SFX por pools.

### 6.3 Penetración y multi-hit sin bugs

Problema: un mismo enemigo puede recibir 2 hits en el mismo tramo/tick.

Solución: `HitCooldownPerTarget`.

- Cada instancia respeta un cooldown por target.
- Si `now < lastHitTime[targetId] + cooldown` → ignorar hit.

### 6.4 `HitRegistry` (recomendado)

Evitar `HashSet` por proyectil (costoso/alloc). Usar registro global reutilizable:

- Mapa: `Key = (projectileInstanceId, targetId) -> lastTime`
- O tabla circular por instancia de pocos slots para ultra-performance

Reglas:

- **Obligatorio** para `beams/auras/campos`.
- **Recomendado** para `pierce` con casts complejos.

## 7) Targeting (evitar O(N²))

### 7.1 Lock-on + retarget interval (homing)

- Al spawn: adquirir target 1 sola vez (`OverlapSphereNonAlloc` con Enemy mask).
- Guardar `targetId`.
- En update: steering barato hacia target.
- Retarget solo cuando:
  - target inválido/muerto, o
  - `Time >= nextRetargetTime` (ej. `0.25–0.5s`).

### 7.2 Flujo homing

```text
Spawn:
  target = AcquireTarget()
  nextRetargetTime = now + RetargetInterval

Update:
  if !targetValid:
     target = AcquireTarget()
  else if now >= nextRetargetTime:
     maybe retarget (opcional)
     nextRetargetTime = now + RetargetInterval

  dir = steer(dir, targetPos - pos, turnRate)
  move + SphereCast segment
```

## 8) Arquetipos (módulos) y contrato `Simulate()`

### 8.1 Pseudo-API

```csharp
public interface IProjectileArchetype
{
    void Simulate(ref ProjectileInstance p, float dt, IProjectileContext ctx);
}
```

`IProjectileContext` debe exponer:

- tiempo actual,
- wrapper de `Physics` con `NonAlloc` queries,
- `target provider` (`targetId -> pos`),
- cola de `HitEvent`,
- pools (VFX/visual handles),
- caps y config global.

## 9) Arquetipos incluidos

### 9.1 Straight Projectile (bullet)

- Movimiento por segmento + `SphereCast`.
- Reglas `stop/pierce`.
- Variantes: stop primer enemigo, `pierce X`, `falloff per pierce`.

### 9.2 Homing Projectile

- Steering hacia target.
- Retarget por intervalo.
- `SphereCast` por segmento igual a bullet.

### 9.3 Beam (laser persistente)

- Vive por `lifetime`.
- Cada `tickInterval`: `RaycastNonAlloc`/`CapsuleCastNonAlloc`.
- Por cada target: daño solo si cooldown OK.
- VFX pooled (`LineRenderer`/mesh beam).

> Regla clave: daño por tick, no por frame.

### 9.4 Hitscan (instantáneo)

- En spawn: un raycast + resolver.
- No requiere update continuo.
- Tracer VFX opcional.

### 9.5 AoE Explosion (instantáneo)

- Una vez: `OverlapSphereNonAlloc`.
- Daño a todos (falloff opcional).
- Reciclado inmediato.

### 9.6 Aura (persistente alrededor de owner)

- Sigue al owner.
- Cada `tickInterval`: overlap sphere.
- Cooldown por target.
- Loop VFX pooled.

### 9.7 Chain (encadenado)

- Primer hit (raycast o bullet).
- Repite `chainCount`:
  - query de vecinos del último target,
  - elegir próximo no repetido,
  - aplicar daño con falloff.
- VFX de rayos cortos entre targets.

### 9.8 Ricochet

- Cast.
- Si world hit: `reflect(dir)` y decrementa rebotes.
- Enemy hit: según config (`ricochet`, `pierce`, `stop`).

### 9.9 Orbital (satélites)

- `pos = ownerPos + rotación * offset`.
- Tick overlap + cooldown.
- VFX simple pooled (mesh + trail).

### 9.10 Ground Field (piso)

- Fijo o siguiendo punto objetivo.
- Tick overlap + cooldown.
- `lifetime/caps`.
- Controlar overdraw de VFX.

## 10) Eventos: `HitEvent` y resolución de daño

### 10.1 `HitEvent` (estructura)

- `projectileId`
- `ownerId`
- `targetId`
- `hitPoint`, `hitNormal`
- `baseDamage`, `damageScale`
- `damageType`, `statusEffects`
- `isCritCandidate` (opcional)
- `tags` (sinergias)

### 10.2 `HitResolver`

Orden recomendado:

1. Validaciones (`friendly fire`, invulnerable, etc.).
2. Cálculo de daño final (crit, resistencias, multiplicadores).
3. Aplicar a `IDamageable`.
4. Aplicar estados.
5. Emitir `OnHit/OnKill` (sinergias).

Separar simulación de aplicación de daño habilita batching y evita bugs de orden.

## 11) Presentation (VFX/SFX/UI) sin matar FPS

- Todo VFX/SFX por pool.
- Runtime de proyectil no crea `GameObjects`.
- Damage numbers:
  - cap visible,
  - pooling,
  - evitar rebuild masivo de canvas.

Degradación graciosa:

- si `ActiveImpactVfx > cap`: omitir VFX (mantener daño),
- si `ActiveDamagePopups > cap`: mostrar solo crits o agrupar.

## 12) Caps y budgets recomendados (config global)

`ProjectileGlobalConfig` sugerido:

- `MaxActiveProjectiles`
- `MaxActiveBeams`
- `MaxActiveImpactVfx`
- `MaxActiveDamagePopups`
- `MaxPhysicsQueriesPerFrame` (opcional)

Estrategias al superar cap:

- reciclar el más viejo,
- no spawnear VFX,
- bajar tickrate de auras/campos,
- reducir frecuencia de retarget.

## 13) Plan de implementación (orden sugerido)

1. Core: `ProjectileDefinition`, `ProjectileInstance`, `ProjectileSystem`, pooling base.
2. Straight bullet (`SphereCast` segment) + stop/pierce.
3. AoE (`OverlapSphereNonAlloc`).
4. Homing (lock-on + retarget interval).
5. Beam (tick + cooldown per target).
6. Chain / Ricochet / Orbital / GroundField.
7. Stress arena + profiler + caps/degradación.

## 14) Stress Arena (criterio de aceptación)

En PC promedio, target 60 FPS:

- 800 enemigos con colliders simples
- 600 proyectiles straight
- 10 beams persistentes (`tick = 0.1s`)
- 5 auras (`tick = 0.2s`)

Criterios:

- `GC Alloc ~ 0` en gameplay,
- sin spikes notorios al minuto 1–2,
- queries de física dentro de budget.

---

## Apéndice A — Diagramas rápidos por arquetipo

### Straight bullet (pierce)

```text
MoveSegment -> SphereCast -> Hit?
  No -> pos = next
  Sí -> if Enemy:
          if cooldown ok: damage
          if piercesLeft>0: piercesLeft-- ; continue moving (opcional)
          else finish
        if World:
          if StopOnWorld finish else continue
```

### Beam (tick)

```text
Each Update:
  if now < nextTickTime -> return
  nextTickTime = now + tickInterval
  Cast beam volume -> hits[]
  foreach hit target:
     if cooldown ok -> damage
```

## 15) Extensión aplicada — Auto-Fire + Auto-Aim (Survivors-like)

### 15.1 Decisión de arquitectura

El disparo automático se implementa en un emisor de arma (`AutoFireWeaponEmitter`) y **no** en el proyectil.

Flujo:

`AutoFireWeaponEmitter.Update()` → adquisición de target/dirección → `ProjectileSystem.TrySpawn(...)` → simulación por arquetipo → `HitResolver`.

### 15.2 WeaponDefinition y cooldown estable

Se agrega `WeaponDefinition` (`ScriptableObject`) y composición data-driven:

- `WeaponDefinition`: `BaseCooldown`, `ProjectilesPerShot`, `Range`, `MuzzleLocalOffset`, `ProjectileDefinition`
- `FirePatternSO`: `Single`, `SpreadCone`, `Burst`, `Spiral`, `Alternating`, `RandomCone`
- `TargetingDefinitionSO`: `Nearest`, `ForwardConeNearest`, `RandomInRange`, etc.

Cooldown se maneja por timestamp:

`nextFireTime = now + cooldownFinal`

con:

`cooldownFinal = max(0.05, BaseCooldown / AttackSpeedMultiplier)`

### 15.3 Auto-aim y target cache

`AutoFireWeaponEmitter` mantiene `currentTargetId` y reacquire por intervalo (`RetargetInterval`), evitando scans por frame innecesarios.

- `TargetingModeSO.Nearest`: adquisición en 360°
- `TargetingModeSO.ForwardConeNearest`: adquisición limitada a cono (filtro duro por ángulo)
- `TargetingModeSO.RandomInRange`: forward aleatorio en plano XZ dentro del rango

Opcionalmente valida LoS con raycast al candidato final.

### 15.4 Spawn direction + targetId (consistencia straight/homing)

Al disparar:

1. Se calcula `aimDir = normalize(targetPos - muzzlePos)` (o `forward` fallback).
2. Se aplican patrones de tiro (`ProjectilesPerShot`, `SpreadAngle`, `Burst`).
3. Cada instancia se crea con:
   - `dir = aimDir` (o dirección con spread)
   - `targetId` en `ProjectileSpawnContext`

Comportamiento resultante:

- `Straight`: usa `dir` inicial y no corrige.
- `Homing`: usa `targetId` inicial y luego steering/retarget.

### 15.5 Scripts agregados para escena de testing

- `AutoFireWeaponEmitter`
- `WeaponDefinition`
- `TestingArenaSpawner` (spawn rápido de enemigos en anillo)

Con esto se puede validar end-to-end sin IA compleja.
