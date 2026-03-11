# Tech Doc — Meta Progression (Unlocks persistentes para drops in-run)

## Implementado (base funcional)

### 1) Catálogo estático (data-driven)
Se agregó `ContentCatalogSO` con listas de:
- `WeaponDefinition`
- `PassiveDefinitionSO`
- `AbilityDefinitionSO`
- `EvolutionRecipeSO`
- `UnlockConditionSO`
- `ItemRarityProfileSO`

Además:
- `WeaponDefinition` ahora tiene `ContentId` estable + `Rarity`.
- `PassiveDefinitionSO` y `AbilityDefinitionSO` exponen el mismo contrato (`IContentDefinition`).

### 2) Save persistente meta
- `MetaProgressionSaveData` guarda:
  - `SaveVersion`
  - `MetaCurrency`
  - `UnlockedContentIds`
  - `DiscoveredContentIds`
  - `SystemUnlockIds`
- `MetaProgressionSaveStore` persiste/carga via `PlayerPrefs` JSON.

### 3) Run session state
`RunSessionState` (por partida) guarda:
- `AvailableWeaponIds`
- `AvailablePassiveIds`
- `BannedIds`
- `TakenThisRun`
- `Seed`

### 4) Manager de meta progression
`MetaProgressionManager`:
- carga save al iniciar,
- construye el run pool filtrando por unlocks,
- permite `UnlockContent`, `AddMetaCurrency`, `BanishFromRun`, `MarkTakenInRun`,
- resuelve evoluciones elegibles con `TryGetEligibleEvolution(...)`.

### 5) Evolutions
`EvolutionRecipeSO` implementa:
- `BaseWeaponId`
- `RequiredPassiveId`
- `MinBaseWeaponLevel`
- `TriggerSource` (`ChestOnly`, `LevelUpAllowed`, `BossReward`, `Any`)
- `ResultWeaponId`
- `Weight`

## Pendiente recomendado
- Evaluación real de `UnlockConditionSO` desde resultados de run.
- Integración completa con pantallas de rewards (level-up/chest/shop).
- Sistema de rarity rolls usando `ItemRarityProfileSO` en el reward selector.
- Versionado/migración robusta de save (fallbacks más finos por ID).
