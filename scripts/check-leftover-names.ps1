<#
Leftover-names guard for the portal (it has no test project). Scans `git ls-files` for the old product vocabulary.
Run from the repo root:  pwsh scripts/check-leftover-names.ps1   (or powershell -File ...). Exit code 1 on any failure.

STRICT words must have no hits (outside the allowlist). PENDING words are tracked per scope and each closing session
removes its word from $Pending: P2 closes clinic/lead/procedure/appointment/staff/patient/surgeon/surgery/consultation,
P3 closes ai_agent and anything left. A pending word with NO hits fails too, so the list cannot go stale.
#>
$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $PSScriptRoot)

$Strict  = [ordered]@{ sculptflow = 'sculptflow'; plasticsurgery = 'plasticsurgery'; surgeon = 'surgeon'; max_agents = 'max_agents' }
# word -> regex. 'lead' matches Lead/leads/leadId/lead_ but not leading/mislead/leader.
$Pending = [ordered]@{
    clinic       = '(?i)clinic'
    lead         = '(?:(?<![A-Za-z])lead|Lead)s?(?![a-z])|LEAD(?![A-Z])'
    procedure    = '(?i)procedure'
    appointment  = '(?i)appointment'
    staff        = '(?i)staff'
    patient      = '(?i)patient'
    surgery      = '(?i)surgery'
    consultation = '(?i)consultation'
    ai_agent     = '(?i)ai_agent|AiAgent'
}

$paths = @(); $tokens = @()
foreach ($l in Get-Content scripts/leftover-names-allowlist.txt) {
    if ($l -match '^path:\s*(.+)$') { $paths += $Matches[1].Trim() }
    elseif ($l -match '^token:\s*(.+)$') { $tokens += $Matches[1].Trim() }
}
$binary = '\.(png|jpe?g|gif|ico|woff2?|ttf|svg)$'
$files = git ls-files | Where-Object { $_ -notmatch $binary } | Where-Object { $f = $_; -not ($paths | Where-Object { $f -like $_ }) }

$hits = @{}; foreach ($k in @($Strict.Keys) + @($Pending.Keys)) { $hits[$k] = New-Object System.Collections.Generic.List[string] }
foreach ($f in $files) {
    $n = 0
    foreach ($line in [System.IO.File]::ReadAllLines((Resolve-Path $f))) {
        $n++
        if ($line -match 'leftover-ok:') { continue }
        $t = $line
        foreach ($tok in $tokens) { $t = $t -ireplace [regex]::Escape($tok), '' }
        foreach ($k in $Strict.Keys)  { if ($t -match "(?i)$($Strict[$k])") { $hits[$k].Add("${f}:$n") } }
        foreach ($k in $Pending.Keys) { if ($t -cmatch $Pending[$k] -or ($Pending[$k].StartsWith('(?i)') -and $t -match $Pending[$k])) { $hits[$k].Add("${f}:$n") } }
    }
}

$fail = $false
foreach ($k in $Strict.Keys) {
    if ($hits[$k].Count) { $fail = $true; Write-Host "STRICT '$k': $($hits[$k].Count) hit(s)"; $hits[$k] | Select-Object -First 20 | ForEach-Object { "  $_" } }
}
foreach ($k in $Pending.Keys) {
    if (-not $hits[$k].Count) { $fail = $true; Write-Host "PENDING '$k' has no hits left: remove it from `$Pending and make it strict." }
}
Write-Host ("Pending hits: " + (($Pending.Keys | ForEach-Object { "$_=$($hits[$_].Count)" }) -join ', '))
if ($fail) { Write-Host 'Leftover-names guard FAILED'; exit 1 }
Write-Host 'Leftover-names guard OK'
