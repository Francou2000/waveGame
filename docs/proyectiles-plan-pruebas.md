# Plan de pruebas — Sistema de Proyectiles / Attacks

Documento de casos de prueba para validar funcionalidad, estabilidad y performance.

## 1) Alcance

## 0) Modo prototipo para esta instancia

Para esta iteración, los enemigos pueden ser `GameObject` simples con `Collider` + `DestroyOnProjectileHit` (sin barra de vida, stats ni IA compleja).

Valida:
- Simulación centralizada.
- Colisión por segmento / overlap non-alloc.
- Reglas de impacto (stop, pierce, cooldown por target).
- Targeting homing con retarget por intervalo.
- Daño por tick (beam/aura).
- Presupuesto de queries y caps.

## 2) Entorno de prueba recomendado

- Unity 6 + URP.
- Build PC Development.
- VSync off, target 60 FPS.
- Profiler conectado (CPU, Physics, GC, Memory).

## 3) Matriz de pruebas funcionales

## F-01 — Straight impacta enemigo y termina
**Setup**
- 1 enemigo estático enfrente.
- Proyectil `Straight`, `StopOnEnemy = true`, `PierceCount = 0`.

**Pasos**
1. Disparar una vez.
2. Observar impacto y vida del enemigo.

**Resultado esperado**
- Se aplica 1 hit.
- El proyectil se recicla al primer impacto.

## F-02 — Straight con pierce atraviesa N enemigos
**Setup**
- 5 enemigos en línea.
- `PierceCount = 2`, `StopOnEnemy = false`.

**Pasos**
1. Disparar una vez.

**Esperado**
- Daño a 3 enemigos máximo (1 + 2 pierces).
- Luego el proyectil termina.

## F-03 — World collision con StopOnWorld
**Setup**
- Muro entre shooter y enemigos.
- `StopOnWorld = true`.

**Pasos**
1. Disparar al muro.

**Esperado**
- El proyectil finaliza en world hit.
- No hay daño detrás del muro.

## F-04 — Homing adquiere target y retargetea por intervalo
**Setup**
- 2 enemigos móviles.
- Proyectil `Homing` con `AcquireRadius` suficiente y `RetargetInterval = 0.3`.

**Pasos**
1. Disparar.
2. Matar/deshabilitar target actual.

**Esperado**
- Se adquiere target inicial.
- Retarget solo al cumplirse condición de invalidez o intervalo.

## F-05 — Hitscan aplica daño instantáneo
**Setup**
- Enemigo frente al disparo.
- `ArchetypeType = Hitscan`.

**Pasos**
1. Disparar.

**Esperado**
- Daño en el mismo frame de spawn.
- Proyectil se recicla inmediatamente.

## F-06 — AoE daña múltiples objetivos en radio
**Setup**
- 6 enemigos alrededor del punto de explosión.
- `ArchetypeType = AoE`.

**Pasos**
1. Ejecutar el AoE.

**Esperado**
- Se dañan solo enemigos dentro del radio.
- Instancia AoE termina en el mismo frame.

## F-07 — Beam daña por tick y no por frame
**Setup**
- Beam persistente 2s, `TickInterval = 0.1`.

**Pasos**
1. Mantener beam sobre 1 enemigo por 2s.
2. Contar hits.

**Esperado**
- Hits ~ 20 (±1 por timing).
- No escala con FPS.

## F-08 — Aura respeta cooldown por target
**Setup**
- Aura sobre player con enemigo dentro del radio.
- `TickInterval = 0.05`, `HitCooldownPerTarget = 0.2`.

**Pasos**
1. Mantener enemigo dentro 2s.

**Esperado**
- Hits aprox 10 (2s / 0.2), no 40.

## F-09 — Friendly fire bloqueado por team
**Setup**
- Ally y enemy dentro del área.

**Pasos**
1. Disparar proyectil del player.

**Esperado**
- No daño a `TeamId` aliado.
- Sí daño a enemigo.

## F-10 — Lifetime y maxDistance reciclan correctamente
**Setup**
- Sin objetivos.

**Pasos**
1. Disparar proyectiles con `LifetimeSeconds` bajo.
2. Disparar proyectiles con `MaxDistance` corto.

**Esperado**
- Se reciclan por condición correcta, sin quedar “zombies”.

## 4) Pruebas de robustez / edge cases

## R-01 — Enemigo destruido entre cast y resolución
**Esperado**
- `HitResolver` ignora objetivo inválido sin excepción.

## R-02 — Buffer de hits completo
**Setup**
- Gran cantidad de colliders en el segmento.

**Esperado**
- No hay allocs ni crash.
- Resultado truncado al tamaño del buffer, comportamiento estable.

## R-03 — Sin target en homing
**Esperado**
- Proyectil sigue trayectoria sin NRE.

## R-04 — Query budget agotado
**Setup**
- Bajar `MaxPhysicsQueriesPerFrame` a valor muy bajo.

**Esperado**
- El sistema no crashea.
- Algunas simulaciones no consultan física ese frame (degradación controlada).

## 5) Pruebas de performance (stress)

## P-01 — Stress arena objetivo del doc
**Escenario**
- 800 enemigos (colliders simples)
- 600 straight projectiles
- 10 beams (`tick 0.1`)
- 5 auras (`tick 0.2`)

**Métricas esperadas**
- `GC Alloc` ~ 0 durante gameplay sostenido.
- Sin spikes relevantes durante 1–2 min.
- Physics queries dentro del budget.

## P-02 — Soak test 10 minutos
**Esperado**
- Sin crecimiento de memoria no acotado.
- Sin degradación progresiva severa de FPS.

## 6) Sugerencia de automatización (PlayMode)

Crear tests PlayMode (NUnit + Unity Test Framework) para:
1. `HitRegistry.CanHit()` con distintos cooldowns.
2. `Straight` hit único vs pierce.
3. `Beam/Aura` daño por tick.
4. `ProjectileSystem` recycling por lifetime/maxDistance.

> Nota: para tests deterministas, inyectar dobles (`fake IProjectileTargetProvider`, `fake IDamageable`) y evitar dependencia excesiva de escena física real.

## 7) Criterio de aceptación final

- Todos los casos funcionales críticos (F-01..F-10) pasan.
- Casos robustez (R-01..R-04) pasan sin excepciones.
- Stress P-01 cumple presupuesto general de frame y GC.
- Soak P-02 sin regresiones graves.
