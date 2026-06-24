# ============================================
# Script de validation Phase 1 - XrmMCP
# ============================================
# Ce script compile la solution et exécute tous les tests
# Pour reprendre le plan Phase 1 après une pause

Write-Host "=== XrmMCP Phase 1 Validation ===" -ForegroundColor Cyan
Write-Host ""

# Étape 1 : Compilation
Write-Host "1️⃣  Compilation de la solution..." -ForegroundColor Yellow
dotnet build --configuration Release -warnaserror
if ($LASTEXITCODE -ne 0) {
	Write-Host "❌ Échec de compilation" -ForegroundColor Red
	exit 1
}
Write-Host "✅ Compilation réussie`n" -ForegroundColor Green

# Étape 2 : Tests unitaires
Write-Host "2️⃣  Exécution des tests unitaires..." -ForegroundColor Yellow
dotnet test tests/XrmMcp.Tests.Unit --no-build --verbosity normal
if ($LASTEXITCODE -ne 0) {
	Write-Host "❌ Échec des tests unitaires" -ForegroundColor Red
	exit 1
}
Write-Host "✅ Tests unitaires OK`n" -ForegroundColor Green

# Étape 3 : Tests d'intégration (optionnels)
Write-Host "3️⃣  Exécution des tests d'intégration..." -ForegroundColor Yellow
$onlineCs = $env:XRMMCP_ONLINE_CONNECTION_STRING
$onpremCs = $env:XRMMCP_ONPREM_CONNECTION_STRING

if ([string]::IsNullOrWhiteSpace($onlineCs) -and [string]::IsNullOrWhiteSpace($onpremCs)) {
	Write-Host "⏭️  Tests d'intégration skippés (pas de variables d'environnement)" -ForegroundColor Gray
} else {
	Write-Host "🔗 Connection strings détectées:" -ForegroundColor Cyan
	if (![string]::IsNullOrWhiteSpace($onlineCs)) {
		Write-Host "   - XRMMCP_ONLINE_CONNECTION_STRING: présente" -ForegroundColor Cyan
	}
	if (![string]::IsNullOrWhiteSpace($onpremCs)) {
		Write-Host "   - XRMMCP_ONPREM_CONNECTION_STRING: présente" -ForegroundColor Cyan
	}

	dotnet test tests/XrmMcp.Tests.Integration --no-build --verbosity normal
	if ($LASTEXITCODE -ne 0) {
		Write-Host "❌ Échec des tests d'intégration" -ForegroundColor Red
		exit 1
	}
	Write-Host "✅ Tests d'intégration OK" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== ✅ Phase 1 Validation Complète ===" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Prochaines étapes:" -ForegroundColor Cyan
Write-Host "   1. Commit & Push vers GitHub" -ForegroundColor White
Write-Host "      git add ." -ForegroundColor Gray
Write-Host "      git commit -m 'Phase 1: Integration tests with runtime skip'" -ForegroundColor Gray
Write-Host "      git push" -ForegroundColor Gray
Write-Host ""
Write-Host "   2. Configurer les secrets GitHub (optionnel):" -ForegroundColor White
Write-Host "      - XRMMCP_ONLINE_CONNECTION_STRING" -ForegroundColor Gray
Write-Host "      - XRMMCP_ONPREM_CONNECTION_STRING" -ForegroundColor Gray
Write-Host ""
Write-Host "   3. Pour reprendre avec Copilot, dites:" -ForegroundColor White
Write-Host "      'Reprends le plan Phase 1, les tests sont OK'" -ForegroundColor Gray
