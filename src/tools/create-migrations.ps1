Param(
  [Parameter(Mandatory = $true)]
  [string] $MigrationName
)

Push-Location

Get-ChildItem Thinktecture.Relay.Server.EntityFramework.MigrationCreation.* `
  | ForEach-Object `
  {
      Set-Location $_
      ./create-migration.ps1 $MigrationName
  }

Pop-Location
