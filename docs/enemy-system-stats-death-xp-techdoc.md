# Tech Doc — Enemy System (Stats, Death, XP Orbs, Variants, Boss base)

## Implementado en este repo (estado actual)

### 1) Data-driven de enemigos
Se agregaron ScriptableObjects:
- `EnemyDefinitionSO` (identidad, categoría, stats, hitbox/presencia, visual set, drops, boss flags).
- `EnemyDropTableSO` (estrategia XP + variación).
- `EnemyVisualSetSO` (variantes de material/tint/escala).
- `BossPhaseProfileSO` y `BossLootSO` como base de extensibilidad.

### 2) Runtime de enemigo
`EnemyRuntime` ahora:
- consume `EnemyDefinitionSO`,
- aplica daño con reducción (`DamageReduction`),
- emite `DeathRequested` al entrar en estado de muerte,
- deja la finalización de muerte en `FinalizeDeath()` para pipeline central,
- soporta variación visual por `EnemyVisualSetSO`.

### 3) Death pipeline
`EnemyDeathSystem`:
- escucha `DeathRequested`,
- calcula XP drop via `EnemyRuntime.EvaluateXpDrop()`,
- delega spawn de XP a `XpOrbSystem`,
- finaliza muerte del enemigo sin `Instantiate` en ese punto.

### 4) XP Orbs
`XpOrbSystem` + `XpOrbRuntime`:
- pooling de orbes,
- cap global (`maxActiveOrbs`),
- merge on spawn por cercanía,
- magnet/pickup hacia player,
- suma XP en `PlayerStatsRuntime.AddXp()`.

### 5) Hordas
Se mantiene `EnemySystem` centralizado (seek + separación tickeada) y `EnemySpawner` con pooling + spawn budget.
`EnemySpawner` ahora además:
- aplica `EnemyDefinitionSO` a instancias,
- registra enemigos en `EnemyDeathSystem`.

## Qué ya cumple del spec
- Stats data-driven por tipo de enemigo.
- Muerte desacoplada del enemigo (pipeline por `DeathRequested`).
- XP drops por sistema dedicado y pooling.
- Variación visual de minions sin duplicar prefabs.
- Base de bosses separada en perfiles SO.

## Qué queda pendiente
- Resistencias por tipo de daño completas (hoy hay `DamageReduction` general).
- Fases reales de boss (state machine y ataques).
- Loot especial de boss (hoy solo base SO).
- Sistema de score/quests escuchando muerte.
- Merge periódico por celdas para orbes (hoy merge por proximidad al spawn).
