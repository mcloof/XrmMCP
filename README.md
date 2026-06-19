# 🚀 XrmMCP - Model Context Protocol Server for Dynamics 365 / Dataverse

[![Build Status](https://github.com/mcloof/XrmMCP/actions/workflows/build.yml/badge.svg)](https://github.com/mcloof/XrmMCP/actions)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**XrmMCP** est un serveur MCP (Model Context Protocol) qui connecte **GitHub Copilot** à **Microsoft Dynamics 365** et **Dataverse**. Il permet aux développeurs d'interroger les métadonnées, de déployer du code, et de gérer des solutions directement depuis leur IDE avec l'assistance de l'IA.

---

## 🎯 Fonctionnalités

### ✅ Phase 0 - Infrastructure (Actuelle)
- [x] Solution .NET 8 multi-projets
- [x] Architecture Clean (Core, Infrastructure, API)
- [x] CI/CD GitHub Actions
- [x] Documentation complète

### 🚧 Phases Suivantes (Roadmap)
- [ ] **Phase 1** : Connexion simple à XRM (OAuth + Windows Auth)
- [ ] **Phase 2** : Gestionnaire de connexions multi-organisations
- [ ] **Phase 3** : Service de métadonnées avec cache
- [ ] **Phase 4** : Serveur MCP et intégration Copilot
- [ ] **Phase 5+** : Déploiement plugins/WebResources, gestion solutions, CI/CD

Voir [ROADMAP.md](ROADMAP.md) pour le plan détaillé.

---

## 📋 Prérequis

- **Visual Studio 2026** (ou VS Code + C# DevKit)
- **.NET 8 SDK** ([Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Accès à une instance Dynamics 365** (Online ou OnPrem)
- **Git** pour cloner le repository

### Environnements Supportés
- ✅ **Dynamics 365 Online** (OAuth)
- ✅ **Dynamics 365 OnPrem** (Windows Authentication)
- 🔜 **Dataverse** (même SDK que D365)

---

## 🚀 Quick Start

### 1. Cloner le repository

```bash
git clone https://github.com/mcloof/XrmMCP.git
cd XrmMCP
```

### 2. Restaurer les dépendances

```bash
dotnet restore
```

### 3. Compiler la solution

```bash
dotnet build
```

### 4. Exécuter les tests

```bash
dotnet test
```

### 5. Lancer l'API (à venir en Phase 1)

```bash
cd src/XrmMcp.Api
dotnet run
```

L'API sera disponible sur `https://localhost:5001` (Swagger UI : `/swagger`).

---

## 📁 Structure du Projet

```
XrmMCP/
├── src/
│   ├── XrmMcp.Api/              # 🌐 API WebAPI + MCP Server
│   ├── XrmMcp.Core/             # 🎯 Domain models + interfaces
│   └── XrmMcp.Infrastructure/   # 🔧 Implémentations XRM SDK
├── tests/
│   ├── XrmMcp.Tests.Unit/       # ✅ Tests unitaires
│   └── XrmMcp.Tests.Integration/# ✅ Tests d'intégration
├── docs/
│   ├── ARCHITECTURE.md          # 📐 Architecture détaillée
│   └── DECISIONS.md             # 📝 ADR (décisions techniques)
├── .github/
│   └── workflows/
│       └── build.yml            # 🔄 CI/CD pipeline
├── .gitignore
├── README.md                    # 📖 Ce fichier
├── ROADMAP.md                   # 🗺️ Plan de développement
└── XrmMCP.sln                   # 📦 Solution Visual Studio
```

---

## 🏗️ Architecture

```
┌─────────────────────────────────┐
│      GitHub Copilot             │
│   (Visual Studio / VS Code)     │
└───────────┬─────────────────────┘
			│ MCP Protocol
┌───────────▼─────────────────────┐
│      XrmMcp.Api                 │
│   (ASP.NET Core WebAPI)         │
└───────────┬─────────────────────┘
			│
┌───────────▼─────────────────────┐
│      XrmMcp.Core                │
│   (Interfaces + Models)         │
└───────────┬─────────────────────┘
			│
┌───────────▼─────────────────────┐
│  XrmMcp.Infrastructure          │
│  (XRM SDK Implementation)       │
└───────────┬─────────────────────┘
			│
	 ┌──────┴──────┐
	 ▼             ▼
┌─────────┐  ┌──────────┐
│ Online  │  │  OnPrem  │
│  OAuth  │  │ WinAuth  │
└─────────┘  └──────────┘
```

Voir [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) pour plus de détails.

---

## 🔧 Technologies Utilisées

| Composant | Technologie | Version |
|-----------|-------------|---------|
| **Framework** | .NET | 8.0 (LTS) |
| **API** | ASP.NET Core WebAPI | 8.0 |
| **XRM SDK** | Microsoft.PowerPlatform.Dataverse.Client | 1.1.* |
| **Cache** | Microsoft.Extensions.Caching.Memory | 8.0.* |
| **Logging** | Serilog | 8.0.* |
| **Tests** | xUnit + Moq + FluentAssertions | Latest |
| **CI/CD** | GitHub Actions | - |

---

## 🧪 Tests

### Exécuter tous les tests

```bash
dotnet test
```

### Tests unitaires uniquement

```bash
dotnet test tests/XrmMcp.Tests.Unit
```

### Tests d'intégration (nécessite connexion XRM)

```bash
dotnet test tests/XrmMcp.Tests.Integration
```

### Code Coverage (avec coverlet)

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

**Objectif** : > 70% de couverture

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [ROADMAP.md](ROADMAP.md) | Plan de développement complet (9 phases) |
| [ARCHITECTURE.md](docs/ARCHITECTURE.md) | Architecture technique détaillée |
| [DECISIONS.md](docs/DECISIONS.md) | ADR - Décisions architecturales |

---

## 🤝 Contribution

Les contributions sont les bienvenues ! Voici comment participer :

1. **Fork** le projet
2. Créer une branche pour votre feature (`git checkout -b feature/ma-super-feature`)
3. Commit vos changements (`git commit -m 'Ajout de ma super feature'`)
4. Push vers la branche (`git push origin feature/ma-super-feature`)
5. Ouvrir une **Pull Request**

### Guidelines
- ✅ Tests unitaires pour tout nouveau code
- ✅ Respecter la structure multi-projets
- ✅ Documenter les décisions importantes (ADR)
- ✅ Suivre les conventions C# (.NET)

---

## 🐛 Signaler un Bug

Ouvrir une [Issue](https://github.com/mcloof/XrmMCP/issues) avec :
- Description du problème
- Steps pour reproduire
- Environnement (OS, .NET version, D365 version)
- Logs si disponibles

---

## 📅 Roadmap

### MVP (Phases 0-4) - 3-4 semaines
- ✅ Phase 0 : Infrastructure
- 🚧 Phase 1 : Connexion XRM (POC)
- 📅 Phase 2 : Gestionnaire de connexions
- 📅 Phase 3 : Service de métadonnées
- 📅 Phase 4 : Serveur MCP

### Core Features (Phases 5-8) - 3-5 semaines
- 📅 Phase 5 : Déploiement WebResources
- 📅 Phase 6 : Déploiement Plugins
- 📅 Phase 7 : Gestion Solutions
- 📅 Phase 8 : Transfer CI/CD

Voir [ROADMAP.md](ROADMAP.md) pour le détail complet.

---

## 📜 License

Ce projet est sous licence **MIT**. Voir [LICENSE](LICENSE) pour plus d'informations.

---

## 🙏 Remerciements

- **Microsoft** pour le SDK Dataverse/PowerPlatform
- **Anthropic** pour le protocole MCP
- **Communauté Dynamics 365** pour l'inspiration
- **XrmToolbox** pour les patterns de connexion

---

## 📞 Contact

- **Auteur** : XrmMCP Team
- **Repository** : [github.com/mcloof/XrmMCP](https://github.com/mcloof/XrmMCP)
- **Issues** : [github.com/mcloof/XrmMCP/issues](https://github.com/mcloof/XrmMCP/issues)

---

## 🎯 Statut du Projet

**Phase actuelle** : Phase 0 - Infrastructure ✅  
**Prochaine étape** : Phase 1 - POC Connexion XRM  
**Démarrage prévu** : Juin 2026  

---

**🌟 Si ce projet vous intéresse, n'hésitez pas à mettre une étoile ⭐ !**
