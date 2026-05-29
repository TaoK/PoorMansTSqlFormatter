# Launch.ps1 - Poor Man's T-SQL Formatter para SSMS 21/22

$script:extensionId   = "PoorMansTSqlFormatterSSMS21.TaoKlerks"
$script:extensionName = "T-SQL Formatter"
$script:ssmsVersions  = @(21, 22)
$script:vsixFile      = Join-Path $PSScriptRoot "PoorMansTSqlFormatterSSMS21.vsix"

$script:W    = 44
$script:LEFT = 4
$script:TOP  = 2

$script:TL = [char]0x250C; $script:TR = [char]0x2510
$script:BL = [char]0x2514; $script:BR = [char]0x2518
$script:HZ = [char]0x2500; $script:VT = [char]0x2502
$script:ML = [char]0x251C; $script:MR = [char]0x2524
$script:BULLET_FULL  = [char]0x25CF
$script:BULLET_EMPTY = [char]0x25CB

# ── Escritura posicionada ─────────────────────────────────────────────────────
function Write-At {
    param($x, $y, $text, $fg = "Gray", $bg = "Black")
    [Console]::SetCursorPosition($x, $y)
    Write-Host $text -ForegroundColor $fg -BackgroundColor $bg -NoNewline
}

function Write-HLine($y, $left, $right) {
    Write-At $script:LEFT $y "$left$([string]$script:HZ * ($script:W - 2))$right" "DarkGray"
}

function Write-Sides($y) {
    Write-At $script:LEFT               $y $script:VT "DarkGray"
    Write-At ($script:LEFT+$script:W-1) $y $script:VT "DarkGray"
}

function Write-Row($y, $text, $fg = "Gray", $align = "left") {
    $inner = $script:W - 4
    if ($align -eq "center") {
        $pad  = [Math]::Max(0, [int](($inner - $text.Length) / 2))
        $text = (" " * $pad) + $text
    }
    if ($text.Length -gt $inner) { $text = $text.Substring(0, $inner) }
    $text = $text.PadRight($inner)
    Write-Sides $y
    Write-At ($script:LEFT + 2) $y $text $fg
}

function Write-Option($y, $text, $selected, $disabled = $false) {
    $inner = $script:W - 4
    Write-Sides $y
    if ($disabled) {
        Write-At ($script:LEFT + 2) $y ("  $text").PadRight($inner) "DarkGray"
    } elseif ($selected) {
        Write-At ($script:LEFT + 2) $y ">" "Cyan"
        Write-At ($script:LEFT + 3) $y (" $text").PadRight($inner - 1) "White" "DarkBlue"
    } else {
        Write-At ($script:LEFT + 2) $y ("  $text").PadRight($inner) "Gray" "Black"
    }
}

# ── Detectar si la extension esta instalada para una instancia ────────────────
function Test-ExtensionInstalled($instanceId) {
    # Busca *PoorMans* bajo %LOCALAPPDATA%\Microsoft\SSMS filtrando por instanceId
    $ssmsRoot = "$env:LOCALAPPDATA\Microsoft\SSMS"
    if (-not (Test-Path $ssmsRoot)) { return $false }

    $hit = Get-ChildItem $ssmsRoot -Recurse -Filter "*PoorMans*" -ErrorAction SilentlyContinue |
           Where-Object { $_.FullName -like "*$instanceId*" } |
           Select-Object -First 1

    return ($null -ne $hit)
}

# ── Detectar instancias SSMS ──────────────────────────────────────────────────
function Find-Targets {
    $root = "$env:ProgramData\Microsoft\VisualStudio\Packages\_Instances"
    $out  = [System.Collections.Generic.List[object]]::new()

    foreach ($ver in $script:ssmsVersions) {
        $folder = "Microsoft SQL Server Management Studio $ver"
        $instId = $null

        foreach ($inst in Get-ChildItem $root -ErrorAction SilentlyContinue) {
            $sf = Join-Path $inst.FullName "state.json"
            if (Test-Path $sf) {
                $s = Get-Content $sf -Raw -ErrorAction SilentlyContinue | ConvertFrom-Json
                if ($s -and $s.installationPath -like "*$folder*") {
                    $instId = $inst.Name; break
                }
            }
        }
        if (-not $instId) { continue }

        $exe = $null
        foreach ($base in @($env:ProgramFiles, ${env:ProgramFiles(x86)})) {
            $c = Join-Path $base "$folder\Release\Common7\IDE\VSIXInstaller.exe"
            if (Test-Path $c) { $exe = $c; break }
        }
        if (-not $exe) { continue }

        $out.Add([PSCustomObject]@{
            Version    = $ver
            InstanceId = $instId
            Installer  = $exe
            Installed  = (Test-ExtensionInstalled $instId)
        })
    }
    return $out
}

# ── Dibujar pantalla ──────────────────────────────────────────────────────────
function Draw-Screen($targets, $options, $selected, $disabled) {
    Clear-Host
    $y = $script:TOP

    Write-HLine $y $script:TL $script:TR;                                         $y++
    Write-Row   $y $script:extensionName "White" "center";                        $y++
    Write-Row   $y "para SQL Server Management Studio" "DarkGray" "center";       $y++
    Write-HLine $y $script:ML $script:MR;                                         $y++
    Write-Row   $y "";                                                             $y++

    foreach ($t in $targets) {
        $dot   = if ($t.Installed) { $script:BULLET_FULL  } else { $script:BULLET_EMPTY }
        $label = if ($t.Installed) { "instalada"          } else { "no instalada" }
        $color = if ($t.Installed) { "DarkGreen"          } else { "DarkGray" }
        Write-Row $y "  $dot  SSMS $($t.Version)   $label" $color
        $y++
    }

    Write-Row   $y "";                                                             $y++
    Write-HLine $y $script:ML $script:MR;                                         $y++
    Write-Row   $y "";                                                             $y++

    $optY = $y
    for ($i = 0; $i -lt $options.Count; $i++) {
        Write-Option $y $options[$i] ($i -eq $selected) $disabled[$i]
        $y++
    }

    Write-Row   $y "";                                                             $y++
    Write-HLine $y $script:BL $script:BR;                                         $y++
    Write-At $script:LEFT ($y + 1) "  flechas  /  enter  /  esc" "DarkGray"

    return $optY
}

# ── Menu interactivo ──────────────────────────────────────────────────────────
function Show-Menu($targets) {
    $options = @("instalar", "desinstalar", "salir")

    $anyInstalled   = ($targets | Where-Object {  $_.Installed } | Measure-Object).Count
    $anyUninstalled = ($targets | Where-Object { !$_.Installed } | Measure-Object).Count

    $disabled = @(
        ($anyUninstalled -eq 0),   # instalar:    disabled si todas ya instaladas
        ($anyInstalled   -eq 0),   # desinstalar: disabled si ninguna instalada
        $false                     # salir:        siempre habilitado
    )

    # Preseleccionar la opcion mas logica
    $selected = if ($anyInstalled -eq 0) { 0 } elseif ($anyUninstalled -eq 0) { 1 } else { 0 }
    while ($disabled[$selected]) { $selected++ }

    $optY = Draw-Screen $targets $options $selected $disabled

    [Console]::CursorVisible = $false
    try {
        while ($true) {
            $key  = [Console]::ReadKey($true)
            $prev = $selected

            switch ($key.Key) {
                "UpArrow"   {
                    do { $selected = ($selected - 1 + $options.Count) % $options.Count }
                    while ($disabled[$selected])
                }
                "DownArrow" {
                    do { $selected = ($selected + 1) % $options.Count }
                    while ($disabled[$selected])
                }
                "Enter"  { return $selected }
                "Escape" { return -1 }
            }

            if ($selected -ne $prev) {
                Write-Option ($optY + $prev)     $options[$prev]     $false $disabled[$prev]
                Write-Option ($optY + $selected) $options[$selected] $true  $disabled[$selected]
            }
        }
    } finally {
        [Console]::CursorVisible = $true
    }
}

# ── Ejecutar operacion ────────────────────────────────────────────────────────
function Run-Operation($targets, $action) {
    Clear-Host
    $y    = $script:TOP
    $verb = if ($action -eq 0) { "instalando" } else { "desinstalando" }

    Write-HLine $y $script:TL $script:TR;                             $y++
    Write-Row   $y $script:extensionName "White" "center";            $y++
    Write-HLine $y $script:ML $script:MR;                             $y++
    Write-Row   $y "";                                                 $y++
    Write-Row   $y $verb "DarkGray" "center";                         $y++
    Write-Row   $y "";                                                 $y++

    foreach ($t in $targets) {
        if ($action -eq 0 -and $t.Installed) {
            Write-Row $y "  SSMS $($t.Version)   ya instalada, omitiendo" "DarkGray"
            $y++; continue
        }
        if ($action -eq 1 -and -not $t.Installed) {
            Write-Row $y "  SSMS $($t.Version)   no instalada, omitiendo" "DarkGray"
            $y++; continue
        }

        Write-Row $y "  SSMS $($t.Version)..." "Gray"
        $lineY = $y; $y++

        $vsixArgs = if ($action -eq 0) {
            "/instanceIds:$($t.InstanceId) `"$script:vsixFile`""
        } else {
            "/instanceIds:$($t.InstanceId) /uninstall:$script:extensionId"
        }
        Start-Process $t.Installer -ArgumentList $vsixArgs -Wait

        Write-At ($script:LEFT + 2) $lineY `
            ("  SSMS $($t.Version)   listo").PadRight($script:W - 4) "DarkGreen"
    }

    Write-Row   $y "";                                                 $y++
    Write-HLine $y $script:BL $script:BR;                             $y++
    Write-At $script:LEFT ($y + 1) "  enter para salir" "DarkGray"

    [Console]::CursorVisible = $false
    $null = [Console]::ReadKey($true)
    [Console]::CursorVisible = $true
    Clear-Host
}

# ── Main ──────────────────────────────────────────────────────────────────────
if (-not (Test-Path $script:vsixFile)) {
    Write-Host "`n  error: .vsix no encontrado en $script:vsixFile`n" -ForegroundColor "DarkRed"
    pause; exit 1
}

$targets = Find-Targets

if ($targets.Count -eq 0) {
    Write-Host "`n  error: no se encontro SSMS 21 ni 22`n" -ForegroundColor "DarkRed"
    pause; exit 1
}

$choice = Show-Menu $targets

switch ($choice) {
    0       { Run-Operation $targets 0 }
    1       { Run-Operation $targets 1 }
    default { Clear-Host }
}