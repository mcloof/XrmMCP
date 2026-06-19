# Architecture XrmMCP

## 🎯 Vue d'Ensemble

**XrmMCP** est un serveur MCP (Model Context Protocol) qui connecte GitHub Copilot à Dynamics 365 / Dataverse (XRM). Il permet aux développeurs d'interroger les métadonnées, déployer du code, et gérer des solutions directement depuis leur IDE avec l'assistance de l'IA.

---

## 🏗️ Architecture Globale

```
┌─────────────────────────────────────────────────────────┐
│                  GitHub Copilot                         │
│            (Visual Studio / VS Code)                    │
└────────────────────┬────────────────────────────────────┘
					 │ 
					 │ MCP Protocol (stdio/SSE)
					 │
┌────────────────────▼────────────────────────────────────┐
│              XrmMcp.Api (ASP.NET Core 8)                │
│  ┌────────────┐ ┌─────────────┐ ┌──────────────┐       │
│  │ MCP Server │ │ REST API    │ │ Health Check │       │
│  └──────┬─────┘ └──────┬──────┘ └──────────────┘       │
└─────────┼──────────────┼─────────────────────────────────┘
		  │              │
┌─────────▼──────────────▼─────────────────────────────────┐
│                  XrmMcp.Core (Domain)                    │
│                                                           │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Interfaces (Contracts)                          │   │
│  │  - IMetadataService                              │   │
│  │  - IConnectionManager                            │   │
│  │  - IDeploymentService                            │   │
│  │  - ISolutionService                              │   │
│  └──────────────────────────────────────────────────┘   │
│                                                           │
│  ┌──────────────────────────────────────────────────┐   │
│  │  Models & DTOs                                   │   │
│  │  - EntityMetadataDto                             │   │
│  │  - ConnectionProfile                             │   │
│  │  - DeploymentRequest                             │   │
│  └──────────────────────────────────────────────────┘   │
└───────────────────────────┬───────────────────────────────┘
							│
┌───────────────────────────▼───────────────────────────────┐
│            XrmMcp.Infrastructure (Implementation)         │
│                                                            │
│  ┌──────────────┐ ┌────────────┐ ┌─────────────┐         │
│  │ Connection   │ │ Metadata   │ │ Deployment  │         │
│  │ Pool         │ │ Cache      │ │ Engine      │         │
│  │              │ │            │ │             │         │
│  │ ServiceClient│ │ MemoryCache│ │ Assembly    │         │
│  │ Management   │ │ Redis (v2) │ │ Analyzer    │         │
│  └──────┬───────┘ └──────┬─────┘ └──────┬──────┘         │
└─────────┼────────────────┼───────────────┼────────────────┘
		  │                │               │
		  ▼                ▼               ▼
  ┌────────────────────────────────────────────────┐
  │     Microsoft.PowerPlatform.Dataverse.Client   │
  │              (ServiceClient)                    │
  └────────────────────┬───────────────────────────┘
					   │
		  ┌────────────┴────────────┐
		  ▼                         ▼
  ┌───────────────┐          ┌────────────┐
  │ Org 1 Online  │          │ Org 2 OnPrem│
  │ (OAuth)       │          │ (Win Auth) │
  └───────────────┘          └────────────┘
```

---

## 📦 Projets et Responsabilités

### 1. **XrmMcp.Api** (ASP.NET Core WebAPI)
**Rôle** : Point d'entrée HTTP et MCP

**Responsabilités** :
- Exposer les endpoints REST pour administration
- Implémenter le serveur MCP (stdio ou SSE)
- Gestion de l'authentification/autorisation
- Configuration de Serilog et logging
- Health checks et monitoring

**Technologies** :
- ASP.NET Core 8 (WebAPI)
- Swagger/OpenAPI pour documentation
- Serilog pour logs structurés

**Endpoints principaux** :
```
GET  /health                    # Health check
GET  /api/connections           # Liste des connexions
POST /api/connections           # Ajouter une connexion
GET  /api/metadata/{orgId}      # Métadonnées
POST /api/deploy/plugin         # Déployer un plugin
POST /api/deploy/webresource    # Déployer une WebResource
```

---

### 2. **XrmMcp.Core** (Class Library)
**Rôle** : Contrats et modèles du domaine

**Responsabilités** :
- Définir les interfaces (contracts)
- Modèles du domaine (entities, value objects)
- DTOs pour les transferts de données
- Pas de dépendances externes (pure domain)

**Structure** :
```
XrmMcp.Core/
├── Interfaces/
│   ├── IConnectionManager.cs
│   ├── IMetadataService.cs
│   ├── IDeploymentService.cs
│   └── ISolutionService.cs
├── Models/
│   ├── ConnectionProfile.cs
│   ├── EntityMetadata.cs
│   └── DeploymentResult.cs
└── DTOs/
	├── EntityMetadataDto.cs
	├── AttributeMetadataDto.cs
	└── RelationshipDto.cs
```

**Principes** :
- Inversion de dépendances (Dependency Inversion Principle)
- Pas de logique métier complexe (juste contrats)
- Immuabilité (records C# 10+)

---

### 3. **XrmMcp.Infrastructure** (Class Library)
**Rôle** : Implémentations concrètes avec XRM SDK

**Responsabilités** :
- Implémenter les interfaces de Core
- Gérer les connexions au SDK XRM
- Cache des métadonnées
- Interactions avec Dataverse/Dynamics 365
- Gestion des erreurs XRM

**Composants clés** :

#### **ConnectionPool**
```csharp
public class ConnectionPool : IConnectionPool
{
	private readonly ConcurrentDictionary<string, ServiceClient> _clients;

	public async Task<ServiceClient> GetOrCreateAsync(string connectionId)
	{
		// Récupère ou crée une connexion ServiceClient
		// Thread-safe avec lazy initialization
	}
}
```

#### **MetadataCache**
```csharp
public class MetadataCache : IMetadataService
{
	private readonly IMemoryCache _cache;
	private readonly TimeSpan _ttl = TimeSpan.FromHours(1);

	public async Task<EntityMetadataDto> GetEntityAsync(string orgId, string name)
	{
		// Cache-aside pattern
		// Clé : "{orgId}:entity:{name}"
	}
}
```

#### **DeploymentEngine**
```csharp
public class PluginDeploymentService : IDeploymentService
{
	public async Task<Guid> DeployPluginAsync(DeploymentRequest request)
	{
		// 1. Charger assembly sans lock (AssemblyLoadContext)
		// 2. Analyser (trouver IPlugin, vérifier strong name)
		// 3. Upload vers XRM (PluginAssembly entity)
		// 4. Enregistrer les steps
	}
}
```

**Dépendances** :
- Microsoft.PowerPlatform.Dataverse.Client (1.1.*)
- Microsoft.Extensions.Caching.Memory (8.0.*)

---

### 4. **XrmMcp.Tests.Unit** (xUnit)
**Rôle** : Tests unitaires isolés

**Responsabilités** :
- Tester la logique métier sans dépendances externes
- Mocker les interfaces avec Moq
- Assertions avec FluentAssertions
- Couverture > 70%

**Exemple** :
```csharp
public class MetadataCacheTests
{
	[Fact]
	public async Task GetEntity_WhenCached_ShouldNotCallXrm()
	{
		// Arrange
		var mockService = new Mock<IOrganizationService>();
		var cache = new MetadataCache(mockService.Object);

		// Act
		await cache.GetEntityAsync("org1", "account");
		await cache.GetEntityAsync("org1", "account"); // 2ème appel

		// Assert
		mockService.Verify(s => s.Execute(It.IsAny<RetrieveEntityRequest>()), 
			Times.Once); // Appelé qu'une fois
	}
}
```

---

### 5. **XrmMcp.Tests.Integration** (xUnit)
**Rôle** : Tests avec vraies instances XRM

**Responsabilités** :
- Tester les interactions réelles avec Dynamics
- Valider l'authentification
- Tester les scénarios de bout en bout
- Utiliser des organisations de test

**Configuration** :
```json
// appsettings.Test.json
{
  "TestConnections": {
	"Online": "AuthType=OAuth;Url=https://org27b396f7.crm4.dynamics.com;...",
	"OnPrem": "AuthType=AD;Url=http://crmdev1.pt.co.il/PetachTikva;..."
  }
}
```

---

## 🔐 Sécurité

### Stockage des Credentials
**Problème** : Comment stocker les connexion strings de façon sécurisée ?

**Solution** : Windows DPAPI (Data Protection API)
```csharp
public class SecureConnectionStorage
{
	public string Encrypt(string plainText)
	{
		var bytes = Encoding.UTF8.GetBytes(plainText);
		var encrypted = ProtectedData.Protect(
			bytes,
			entropy: null,
			DataProtectionScope.CurrentUser
		);
		return Convert.ToBase64String(encrypted);
	}

	public string Decrypt(string encryptedText)
	{
		var bytes = Convert.FromBase64String(encryptedText);
		var decrypted = ProtectedData.Unprotect(
			bytes,
			entropy: null,
			DataProtectionScope.CurrentUser
		);
		return Encoding.UTF8.GetString(decrypted);
	}
}
```

**Fichier** : `connections.json` (chiffré)
```json
{
  "connections": [
	{
	  "id": "dev-online",
	  "name": "Dev Environment",
	  "connectionString": "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAA..." // Chiffré
	}
  ]
}
```

---

## 🚀 Performance

### Stratégie de Cache

**Metadata Cache** :
- **TTL** : 1 heure
- **Clé** : `{orgId}:entity:{logicalName}`
- **Invalidation** : Manuelle après déploiement
- **Provider** : MemoryCache (Phase 1-3) → Redis (Phase 4+)

**Connection Pool** :
- **Singleton** : Une instance ServiceClient par organisation
- **Lazy** : Créé à la première requête
- **Cleanup** : Timeout après 30 min d'inactivité

### Mesures de Performance
```csharp
using var _ = _logger.BeginTimedOperation("GetEntityMetadata");
var result = await _metadataService.GetEntityAsync(orgId, entityName);
// Log automatique : "GetEntityMetadata completed in 245ms"
```

---

## 🔄 Patterns Utilisés

### 1. **Dependency Injection**
Tous les services sont injectés via DI (built-in ASP.NET Core)

### 2. **Repository Pattern** (implicite)
Infrastructure abstrait l'accès aux données XRM

### 3. **Cache-Aside Pattern**
```
1. Check cache
2. Si hit → retourner
3. Si miss → fetch from XRM → cache → retourner
```

### 4. **Singleton Pattern**
Pour ConnectionPool et ServiceClient

### 5. **Strategy Pattern** (futur)
Pour supporter différents types d'authentification

---

## 📊 Décisions Techniques Clés

### Pourquoi .NET 8 et pas .NET Framework ?
✅ **ServiceClient** moderne supporte .NET Core  
✅ Performance supérieure  
✅ Multi-plateforme (Linux containers)  
✅ Support LTS (jusqu'en 2026)  
✅ Async/await natif partout  

### Pourquoi multi-projets ?
✅ Séparation des responsabilités (Clean Architecture)  
✅ Testabilité (mocker les interfaces)  
✅ Réutilisabilité (Core peut être partagé)  
✅ Évolutivité (ajouter des layers facilement)  

### Pourquoi MemoryCache d'abord ?
✅ Simplicité pour le MVP  
✅ Pas de dépendance externe (Redis)  
✅ Suffisant pour 1 serveur  
⚠️ Limite : Pas de cache distribué (pour plus tard)

---

## 🛣️ Évolution Future

### Phase 1-4 (MVP)
- ✅ Multi-projets établis
- ✅ Connection Manager
- ✅ Metadata Service avec cache
- ✅ MCP Protocol

### Phase 5-6
- Ajout : Deployment services (Plugin, WebResource)
- Ajout : File mapping (SQLite local)

### Phase 7-8
- Ajout : Solution Management
- Ajout : CI/CD Transfer

### Phase 9+
- Migration vers Redis pour cache distribué
- Support de plusieurs instances API (load balancing)
- Kubernetes deployment
- Telemetry & monitoring (Application Insights)

---

## 📚 Références

- [Microsoft.PowerPlatform.Dataverse.Client](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/org-service/overview)
- [Model Context Protocol (MCP)](https://modelcontextprotocol.io/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)

---

**Dernière mise à jour** : 19 juin 2026  
**Version** : 0.1.0 (Phase 0)  
**Auteur** : XrmMCP Team
