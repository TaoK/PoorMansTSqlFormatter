#
# Version Bump script for Poor Man's T-Sql Formatter (main project)
#
# Expects one of 3 kinds of argument:
#  1) a 3-part version number (MAJOR.MINOR.PATCH)
#  2) one of "major", "minor" or "patch", to bump with respect to the version in appveyor.yml
#  3) no argument at all - this will bump "patch" wrt appveyor.yml
#
# Expects all files to be UTF-8, with Windows linebreaks, and with BOM
#
# Expects to be called from the project root, for "relative" version bumps. (so it finds the appveyor yaml file)
#
# Can also be called from any folder hierarchy with an "absolute" version, but beware:
#  This will update ANYTHING that looks like a 3-part version string (with or without 4th-part wildcard) in file with a target pattern/extension
#

$VersionSourceFile = 'appveyor.yml';
$VersionSourcePattern = '(\d+)\.(\d+)\.(\d+)(?=\.\{build\})'; #the 3 capture groups must be major, minor, patch
$ReplacePathPattern = '(AssemblyInfo\.cs)|(appveyor\.yml)|(FormatterPackage\.cs)|(source\.extension\.vsixmanifest)$'
$ReplacePathExcludePattern = 'packages'
$ReplaceVersionPatterns = @(
  '(?<=AssemblyVersion\(")\d+\.\d+\.\d+(?=(\.\*)?")',
  '(?<=\<Identity.*")\d+\.\d+\.\d+(?=(\.\*)?")', 
  '(?<=InstalledProductRegistration.*")\d+\.\d+\.\d+(?=(\.\*)?")', 
  $VersionSourcePattern #happens to be constructed for a safe replace also (lookbehind for non-replaceable bit)
  )

function Get-BasisVersion {
  Param (
    [string]$SourceFile,
    [string]$SourcePattern
  )

  $LinesFound = Select-String -Path $SourceFile -Pattern $SourcePattern
  if ($LinesFound.Count -eq 1) {
    $RegexMatchGroups = $LinesFound[0].Matches[0].Groups
    $VersionObject = New-Object PSObject -Property @{
            Major = $RegexMatchGroups[1].Value;
            Minor = $RegexMatchGroups[2].Value;
            Patch = $RegexMatchGroups[3].Value;
        }
    Write-Output $VersionObject
  }
  else {
    Write-Error "Version Source File contains zero or multiple matches!"
  }
}

function Set-VersionInFile {
  Param (
    [string]$NewVersion
  )

  foreach ($MatchingFile in $input) {
    Write-output "Processing file $($MatchingFile.FullName)"

    $TmpFile = $MatchingFile.FullName + ".tmp"

    $FileContents = Get-Content $MatchingFile.FullName -Encoding UTF8

    for ($i=0; $i -lt $FileContents.Count; $i++) {
      foreach ($ReplaceVersionPattern in $ReplaceVersionPatterns) {
        if ($FileContents[$i] -match $ReplaceVersionPattern) {
          $FileContents[$i] = $FileContents[$i] -replace $ReplaceVersionPattern, $NewVersion
        }
      }
    }

    Set-Content -Path $TmpFile -Value $FileContents -Encoding UTF8

    Move-Item $TmpFile $MatchingFile.FullName -force
  }
}

function Set-VersionAcrossFiles {
  Param (
    [string]$NewVersion
  )

  Get-Childitem -recurse | ? { $_.FullName -notmatch $ReplacePathExcludePattern -and $_.FullName -match $ReplacePathPattern } | Set-VersionInFile $NewVersion
}


function Bump-VersionAcrossFiles {
  Param (
    [ValidateSet('major','minor','patch')][string]$BumpType
  )

  $BasisVersion = Get-BasisVersion $VersionSourceFile $VersionSourcePattern
  $NewVersionString = ""

  switch ($BumpType) { 
    'major' {$NewVersionString = "$(1 + $BasisVersion.Major).0.0" }
    'minor' {$NewVersionString = "$($BasisVersion.Major).$(1 + $BasisVersion.Minor).0" }
    'patch' {$NewVersionString = "$($BasisVersion.Major).$($BasisVersion.Minor).$(1 + $BasisVersion.Patch)" }
  }

  Set-VersionAcrossFiles $NewVersionString
}

# Actual script processing
if ($args.Count -gt 1) {
  Write-Error "Too many Arguments (see source for instructions)"
}
elseif ($args.Count -eq 1) {
  if ($args[0] -match "major|minor|patch") {
    Bump-VersionAcrossFiles $args[0]
  }
  elseif ($args[0] -match "^\d+\.\d+\.\d+$") {
    Set-VersionAcrossFiles $args[0]
  }
  else {
    Write-Error "Incorrect argument (see source for instructions)"
  }
}
else {
  Bump-VersionAcrossFiles "patch"
}
