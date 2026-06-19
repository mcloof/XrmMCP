# Architectural Decision Records (ADR)

Ce document enregistre les décisions architecturales importantes prises pour le projet XrmMCP.

Format inspiré de [Michael Nygard's ADR template](https://github.com/joelparkerhenderson/architecture-decision-record).

---

## ADR-001 : Choix de .NET 8 (Core) plutôt que .NET Framework

**Date** : 19 juin 2026  
**Statut** : ✅ Accepté  
**Décideurs** : Équipe XrmMCP  

### Contexte
Le SDK XRM traditionnel (CrmServiceClient) était historiquement basé sur .NET Framework. Le nouveau SDK Microsoft.PowerPlatform.Dataverse.Client supporte .NET Core.

### Décision
Utiliser **.NET 8 (LTS)** avec le nouveau SDK `Microsoft.PowerPlatform.Dataverse.Client`.

### Justification

**Avantages** :
- ✅ **Performance** : .NET 8 est 20-30% plus rapide que .NET Framework
- ✅ **Multi-plateforme** : Support Linux/macOS (Docker, Kubernetes)
- ✅ **Support long-terme** : .NET 8 LTS supporté jusqu'en novembre 2026
- ✅ **Async natif** : Meilleure gestion des opérations I/O
- ✅ **Mémoire** : Garbage collector optimisé
- ✅ **SDK moderne** : ServiceClient optimisé pour .NET Core

**Inconvénients** :
- ⚠️ Nécessite migration si code existant en .NET Framework
- ⚠️ Certaines anciennes dépendances peuvent être incompatibles

### Alternatives Considérées

1. **.NET Framework 4.8**
   - ❌ Pas multi-plateforme
   - ❌ Performance inférieure
   - ❌ Pas de support long-terme
   - ✅ Compatible avec ancien code

2. **.NET 10 (Preview)**
   - ❌ Pas stable (preview)
   - ❌ Pas LTS
   - ✅ Dernières features

### Conséquences

**Positives** :
- Peut tourner dans des containers Linux (coût infrastructure réduit)
- Meilleure scalabilité (async/await natif)
- Support communautaire actif

**Négatives** :
- Équipe doit connaître .NET Core (mais très similaire)
- Impossible de réutiliser certaines anciennes librairies .NET Framework

### Validation
- ✅ POC réussi (Phase 1) confirmera la viabilité
- ✅ Tests de performance comparés si nécessaire

---

## ADR-002 : Architecture Multi-Projets (Clean Architecture)

**Date** : 19 juin 2026  
**Statut** : ✅ Accepté  
**Décideurs** : Équipe XrmMCP  

### Contexte
Décider comment structurer le code : monolithe ou multi-projets ?

### Décision
Adopter une **architecture multi-projets** inspirée de Clean Architecture :
- `XrmMcp.Api` (présentation)
- `XrmMcp.Core` (domaine)
- `XrmMcp.Infrastructure` (implémentation)
- `XrmMcp.Tests.*` (tests)

### Justification

**Avantages** :
- ✅ **Séparation des responsabilités** : Chaque layer a un rôle clair
- ✅ **Testabilité** : Facilite les mocks et tests unitaires
- ✅ **Évolutivité** : Ajouter des features sans impacter le core
- ✅ **Indépendance du framework** : Core ne dépend pas d'ASP.NET
- ✅ **Réutilisabilité** : Core peut être utilisé dans une CLI ou Blazor

**Inconvénients** :
- ⚠️ Plus de complexité initiale (plusieurs projets)
- ⚠️ Temps de build légèrement plus long
- ⚠️ Courbe d'apprentissage pour nouveaux développeurs

### Alternatives Considérées

1. **Monolithe (1 projet)**
   - ✅ Plus simple au début
   - ❌ Difficile à tester
   - ❌ Couplage fort
   - ❌ Difficile à scale

2. **Microservices**
   - ❌ Trop complexe pour un MVP
   - ❌ Overhead réseau
   - ✅ Scalabilité maximale
   - ⚠️ Peut évoluer vers ça plus tard

### Diagramme de Dépendances

```
XrmMcp.Api
  ├─> XrmMcp.Core (interfaces)
  └─> XrmMcp.Infrastructure (implémentations)
	  └─> XrmMcp.Core (interfaces)

XrmMcp.Tests.Unit
  ├─> XrmMcp.Core
  ├─> XrmMcp.Infrastructure
  └─> XrmMcp.Api

Règle : Les dépendances pointent VERS le centre (Core)
```

### Conséquences

**Positives** :
- Code plus maintenable à long terme
- Facilite l'onboarding (structure claire)
- Tests plus rapides (unitaires isolés)

**Négatives** :
- Setup initial plus long (mitigé par templates)
- Besoin de gérer les références de projets

### Validation
- ✅ Structure créée en Phase 0
- ✅ Build réussit sans erreurs circulaires

---

## ADR-003 : MemoryCache pour MVP (plutôt que Redis immédiatement)

**Date** : 19 juin 2026  
**Statut** : ✅ Accepté  
**Décideurs** : Équipe XrmMCP  

### Contexte
Les métadonnées XRM sont lentes à récupérer (2-5 secondes). Un cache est nécessaire. Redis ou MemoryCache ?

### Décision
Utiliser **Microsoft.Extensions.Caching.Memory** pour le MVP (Phases 0-4).  
Migration vers **Redis** planifiée pour Phase 5+.

### Justification

**MemoryCache - Avantages** :
- ✅ **Simplicité** : Intégré dans .NET, pas de dépendance externe
- ✅ **Zéro configuration** : Pas de serveur Redis à gérer
- ✅ **Performance locale** : Accès mémoire ultra-rapide
- ✅ **Suffisant pour MVP** : 1 serveur, pas besoin de distribution

**MemoryCache - Inconvénients** :
- ⚠️ **Local uniquement** : Pas partagé entre instances
- ⚠️ **Perdu au redémarrage** : Pas de persistence
- ⚠️ **Limite mémoire** : Peut consommer beaucoup de RAM

**Redis - Avantages** :
- ✅ Cache distribué (multi-instances)
- ✅ Persistence optionnelle
- ✅ TTL avancé, patterns pub/sub

**Redis - Inconvénients** :
- ❌ Infrastructure supplémentaire (coût, maintenance)
- ❌ Latence réseau (vs mémoire locale)
- ❌ Complexité ajoutée pour le MVP

### Configuration Cache

```csharp
services.AddMemoryCache(options =>
{
	options.SizeLimit = 1024; // Limite : 1024 entrées
	options.CompactionPercentage = 0.25; // Éviction : 25% si limite atteinte
});
```

**Stratégie** :
- TTL : 1 heure pour metadata
- Invalidation manuelle après déploiement
- Clés : `{orgId}:entity:{logicalName}`

### Alternatives Considérées

1. **Redis dès le début**
   - ❌ Over-engineering pour MVP
   - ✅ Future-proof

2. **Pas de cache**
   - ❌ Performance inacceptable (5s par requête)

3. **SQLite cache**
   - ⚠️ Persistence mais plus lent que MemoryCache
   - ⚠️ Complexité I/O

### Plan de Migration vers Redis

**Trigger** : Quand le projet passe en production avec > 1 instance.

**Steps** :
1. Créer `IDistributedCache` interface (abstraction)
2. Implémenter `RedisCache : IDistributedCache`
3. Configuration dans `appsettings.Production.json`
4. Tests de charge pour valider

**Impact** : Minimal grâce à l'abstraction `IMetadataService`.

### Conséquences

**Positives** :
- MVP plus simple et rapide à développer
- Pas de dépendance infrastructure (Docker, Redis Server)
- Performance excellente pour 1 serveur

**Négatives** :
- Besoin de migrer si scaling horizontal
- Cache perdu à chaque redémarrage (recalculé)

### Validation
- ✅ Tests de performance en Phase 3
- ✅ Monitoring RAM usage

---

## ADR-004 : OAuth et Windows Auth comme premiers modes d'authentification

**Date** : 19 juin 2026  
**Statut** : ✅ Accepté  
**Décideurs** : Équipe XrmMCP  

### Contexte
XRM supporte plusieurs modes d'auth : OAuth, Client Secret, Certificate, Windows Auth, IFD.  
Lesquels implémenter en premier ?

### Décision
Supporter **OAuth (Online)** et **Windows Auth (OnPrem)** pour le POC (Phase 1).

### Justification

**OAuth** :
- ✅ Standard moderne (OpenID Connect)
- ✅ Recommandé par Microsoft pour Online
- ✅ Sécurisé (pas de password stocké)
- ✅ Support MFA

**Windows Auth** :
- ✅ Simple pour OnPrem dans un domaine AD
- ✅ Transparent pour l'utilisateur
- ✅ Pas de configuration complexe

**Client Secret / Certificate** :
- ⚠️ Ajoutés en Phase 2 si besoin
- ✅ Utiles pour automatisation (CI/CD)
- ⚠️ Nécessite App Registration Azure AD

### Connection String Examples

```csharp
// Online OAuth
"AuthType=OAuth;Url=https://org.crm4.dynamics.com;Username=user@tenant.onmicrosoft.com;Password=***;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost;LoginPrompt=Auto"

// OnPrem Windows Auth
"AuthType=AD;Url=http://crmdev1.pt.co.il/PetachTikva;Domain=PETACHTIKVA;Username=admin;Password=***"
```

### Alternatives Considérées

1. **Client Secret uniquement**
   - ❌ Pas user-friendly (besoin App Registration)
   - ✅ Bon pour automation

2. **Tous les modes en même temps**
   - ❌ Trop complexe pour POC
   - ⚠️ Ajoutés progressivement

### Conséquences

**Positives** :
- Couvre 80% des cas d'usage
- POC validable rapidement

**Négatives** :
- Besoin d'ajouter d'autres modes plus tard
- Documentation pour chaque mode

### Validation
- ✅ Test OAuth avec `org27b396f7.crm4.dynamics.com`
- ✅ Test Windows Auth avec `crmdev1.pt.co.il`

---

## ADR-005 : xUnit + Moq + FluentAssertions pour les tests

**Date** : 19 juin 2026  
**Statut** : ✅ Accepté  
**Décideurs** : Équipe XrmMCP  

### Contexte
Choisir un framework de test et des librairies de support.

### Décision
- **xUnit** comme test runner
- **Moq** pour les mocks
- **FluentAssertions** pour les assertions

### Justification

**xUnit** :
- ✅ Standard de facto pour .NET Core
- ✅ Parallélisation native
- ✅ Pas de setup global ([Fact] simple)
- ✅ Support Visual Studio & Rider

**Moq** :
- ✅ Syntaxe fluide et intuitive
- ✅ Vérifie les appels (Verify)
- ✅ Large adoption communautaire

**FluentAssertions** :
- ✅ Assertions lisibles (should be)
- ✅ Messages d'erreur détaillés
- ✅ Chaînable (fluent API)

### Exemple

```csharp
[Fact]
public async Task GetEntity_WhenCached_ShouldReturnFromCache()
{
	// Arrange
	var mockService = new Mock<IOrganizationService>();
	var cache = new MetadataCache(mockService.Object);

	// Act
	var result = await cache.GetEntityAsync("org1", "account");

	// Assert
	result.Should().NotBeNull();
	result.LogicalName.Should().Be("account");
	mockService.Verify(s => s.Execute(It.IsAny<RetrieveEntityRequest>()), 
		Times.Once);
}
```

### Alternatives Considérées

1. **NUnit**
   - ⚠️ Moins populaire en .NET Core
   - ✅ Setup/TearDown global

2. **MSTest**
   - ❌ Moins moderne
   - ✅ Intégré VS

3. **NSubstitute** (au lieu de Moq)
   - ✅ Syntaxe encore plus simple
   - ⚠️ Moins de features avancées

### Conséquences

**Positives** :
- Stack de test standard et éprouvée
- Bonne intégration Visual Studio 2026
- Documentation abondante

**Négatives** :
- 3 packages à maintenir (mais stables)

### Validation
- ✅ Tests créés en Phase 0 passent
- ✅ Code coverage > 70% visé

---

## ADR-006 : Serilog pour le logging structuré

**Date** : 19 juin 2026  
**Statut** : ✅ Accepté  
**Décideurs** : Équipe XrmMCP  

### Contexte
Besoin de logs pour debug et monitoring. Quel framework utiliser ?

### Décision
Utiliser **Serilog** avec sinks Console et File.

### Justification

**Avantages** :
- ✅ **Logging structuré** : JSON, pas juste string
- ✅ **Sinks multiples** : Console, File, Seq, Application Insights
- ✅ **Enrichers** : Ajoute contexte automatiquement
- ✅ **Performance** : Async logging

**Configuration** :
```csharp
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.Enrich.FromLogContext()
	.Enrich.WithMachineName()
	.WriteTo.Console()
	.WriteTo.File("logs/xrmmcp-.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();
```

### Format de Log

```json
{
  "@t": "2026-06-19T10:30:00.123Z",
  "@l": "Information",
  "@m": "Entity metadata retrieved",
  "EntityName": "account",
  "OrganizationId": "org1",
  "DurationMs": 245,
  "MachineName": "DEV-MACHINE"
}
```

### Alternatives Considérées

1. **ILogger (built-in ASP.NET)**
   - ✅ Simple
   - ❌ Pas structuré par défaut
   - ⚠️ Serilog s'intègre avec ILogger

2. **NLog**
   - ✅ Mature
   - ⚠️ Configuration XML verbose

3. **Log4Net**
   - ❌ Ancien, moins maintenu

### Conséquences

**Positives** :
- Logs facilement queryables (JSON)
- Intégration future avec Seq/AppInsights simple

**Négatives** :
- Package supplémentaire (mais léger)

### Validation
- ✅ Logs visibles dès Phase 0
- ✅ Fichiers de log créés dans `/logs`

---

## Template pour Futures ADR

```markdown
## ADR-XXX : [Titre court]

**Date** : YYYY-MM-DD  
**Statut** : 🔄 Proposé / ✅ Accepté / ❌ Rejeté / ⚠️ Déprécié  
**Décideurs** : [Noms]  

### Contexte
[Problème à résoudre]

### Décision
[Choix fait]

### Justification
[Pourquoi ce choix]

### Alternatives Considérées
1. **Option A**
   - ✅ Avantages
   - ❌ Inconvénients

### Conséquences
**Positives** :
- [Liste]

**Négatives** :
- [Liste]

### Validation
- [ ] Critère 1
- [ ] Critère 2
```

---

**Dernière mise à jour** : 19 juin 2026  
**Nombre total d'ADR** : 6  
**Statut projet** : Phase 0 - Infrastructure
