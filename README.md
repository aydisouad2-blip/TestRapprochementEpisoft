# Mini moteur de rapprochement (test technique ) — C#

Console app + tests unitaires (MSTest) qui rapprochent un flux bancaire et un flux comptable depuis deux CSV.

## Prérequis

- Visual Studio 2022
- .NET Framework **4.7.2** (SDK-style project).  


## Contenu du dépôt

- `TestRapprochementEpisoft` : console app
- `TestRapprochementEpisoft.Tests` : tests MSTest
- `TestRapprochementEpisoft\output/` : fichiers générés sur le dataset fourni (`matches.csv`, `report.txt`)

## Exécution

Depuis Visual Studio : définir `TestRapprochementEpisoft` comme projet de démarrage.

En ligne de commande (Developer Command Prompt) :

```bash
TestRapprochementEpisof.exe <bank.csv> <accounting.csv> <outputDir> [configPath]
```

Exemple :

```bash
TestRapprochementEpisof.exe .\bank.csv .\accounting.csv .\output
```

Codes de sortie :
- `0` : succès sans erreur de parsing
- `1` : succès avec erreurs de parsing (des lignes ont été ignorées)
- `2` : erreur d'usage/fichiers introuvables
- `3` : erreur fatale inattendue

## Hypothèses & choix

- **Robustesse parsing** : les lignes invalides sont **ignorées** mais reportées (auditabilité).
- **Déterminisme** : itération par `Id` croissant et tie-break final par `AccountingId` croissant.
- **Règles** : appliquées dans l’ordre demandé via `TryEvaluateRules(...)`.  
  Si plusieurs règles pourraient correspondre, la première rencontrée dans l’ordre gagne (ex. exact => 100).
- **Ambiguïté** : signalée si au moins 2 candidats restent à égalité après :
  `Score`, `diff date`, `diff montant` (puis on choisit le plus petit `AccountingId`).

## Bonus implémenté: paramétrage des seuils via config

Fichier texte `config.txt` (key=value), optionnel.

Clés supportées :
- `Rule2DateToleranceDays` (défaut `1`)
- `Rule3AmountTolerance` (défaut `5.00`)
- `Rule4DateToleranceDays` (défaut `2`)
- `Rule4AmountTolerance` (défaut `5.00`)

Si `configPath` est omis, l’app tente de lire `config.txt` dans le répertoire courant.

## Limites

- Pas de scoring sur la description


## Améliorations possibles (priorisées)

1. Export d’un rapport plus riche (statistiques par règle, distributions des scores)
2. Ajout d’un scoring “description”  pour réduire les faux négatifs.

## Tests

### Lancer les tests

Dans Visual Studio : **Test Explorer** → Run all.

Ou CLI (si vous avez `dotnet` + SDK) :

```bash
dotnet test
```

### Ce que je testerais en plus (si plus de temps)

- Conflits de réutilisation (une compta ne doit pas être utilisée 2 fois).
- Fichiers vides / header manquant (attendus par l’énoncé).
- Cas où une même paire satisfait plusieurs règles (ex: exact vs tolérance), pour vérifier la priorité.
