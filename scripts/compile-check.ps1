# Compile-check OVA without installing into Kaguya.
# Refs = Unity-generated NDMF editor csproj HintPaths (unity-4.8-api +
# UnityEngine\UnityEditor.dll, not Data\Managed\UnityEditor.dll).

$ErrorActionPreference = "Stop"
$unityData = "D:\Software\Unity Hub\Unity\Hub\Editor\2022.3.22f1\Editor\Data"
$dotnet = Join-Path $unityData "NetCoreRuntime\dotnet.exe"
$csc = Join-Path $unityData "DotNetSdkRoslyn\csc.dll"
$kaguya = "D:\Project\Unity\kaguya"
$kaguyaAsm = Join-Path $kaguya "Library\ScriptAssemblies"
$root = Split-Path $PSScriptRoot -Parent
$pkg = Join-Path $root "Packages\dev.ova.protection"
$outDir = Join-Path $root "Temp\compile"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function Get-CsprojHintPaths {
    param([string]$Csproj)
    if (-not (Test-Path $Csproj)) { throw "missing csproj $Csproj" }
    $paths = New-Object System.Collections.Generic.List[string]
    $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
    # Unity csproj uses a default MSBuild xmlns; regex is more reliable than SelectNodes.
    $text = Get-Content -Raw -LiteralPath $Csproj
    $matches = [regex]::Matches($text, '<HintPath>([^<]+)</HintPath>')
    foreach ($m in $matches) {
        $raw = $m.Groups[1].Value.Trim()
        if ([string]::IsNullOrWhiteSpace($raw)) { continue }
        $full = $raw
        if (-not [System.IO.Path]::IsPathRooted($raw)) {
            $full = Join-Path $kaguya $raw
        }
        if (-not (Test-Path -LiteralPath $full)) { continue }
        $resolved = (Resolve-Path -LiteralPath $full).Path
        if ($seen.Add($resolved)) { $paths.Add($resolved) }
    }
    return $paths
}

function Add-IfExists {
    param(
        [System.Collections.Generic.List[string]]$List,
        [System.Collections.Generic.HashSet[string]]$Seen,
        [string]$Path
    )
    if (-not (Test-Path -LiteralPath $Path)) { return }
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    if ($Seen.Add($resolved)) { $List.Add($resolved) }
}

function Invoke-Csc {
    param(
        [string[]]$Sources,
        [string[]]$ReferenceDlls,
        [string]$Output,
        [string[]]$Define = @()
    )
    $rsp = Join-Path $outDir (([IO.Path]::GetFileNameWithoutExtension($Output)) + ".rsp")
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("/nologo")
    $lines.Add("/nostdlib")
    $lines.Add("/t:library")
    $lines.Add("/langversion:9")
    $lines.Add("/nowarn:CS8019")
    $lines.Add("/out:`"$Output`"")
    foreach ($d in $Define) { $lines.Add("/define:$d") }
    foreach ($r in $ReferenceDlls) { $lines.Add("/r:`"$r`"") }
    foreach ($s in $Sources) {
        if (-not (Test-Path $s)) { throw "missing source $s" }
        $lines.Add("`"$s`"")
    }
    $utf8 = New-Object System.Text.UTF8Encoding $false
    [IO.File]::WriteAllLines($rsp, $lines, $utf8)
    & $dotnet exec $csc "@$rsp"
    if ($LASTEXITCODE -ne 0) { throw "csc failed ($LASTEXITCODE) out=$Output" }
}

$runtimeCsproj = Join-Path $kaguya "nadena.dev.ndmf.runtime.csproj"
$editorCsproj = Join-Path $kaguya "jp.suzuryg.face-emo.ndmf.Editor.csproj"

$runtimeRefs = New-Object System.Collections.Generic.List[string]
$runtimeSeen = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
foreach ($p in (Get-CsprojHintPaths $runtimeCsproj)) {
    if ($runtimeSeen.Add($p)) { $runtimeRefs.Add($p) }
}
Add-IfExists $runtimeRefs $runtimeSeen (Join-Path $kaguya "Packages\com.vrchat.base\Runtime\VRCSDK\Plugins\VRCSDKBase.dll")
Add-IfExists $runtimeRefs $runtimeSeen (Join-Path $kaguyaAsm "VRC.SDKBase.dll")
Add-IfExists $runtimeRefs $runtimeSeen (Join-Path $kaguyaAsm "VRC.SDK3A.dll")

$runtimeOut = Join-Path $outDir "dev.ova.protection.dll"
$runtimeCs = @(Get-ChildItem -LiteralPath (Join-Path $pkg "Runtime") -Filter *.cs | ForEach-Object { $_.FullName })
Write-Output "compile runtime... refs=$($runtimeRefs.Count) src=$($runtimeCs.Count)"
Invoke-Csc -Sources $runtimeCs -ReferenceDlls $runtimeRefs.ToArray() -Output $runtimeOut -Define @(
    "UNITY_EDITOR", "NET_4_6", "NET_UNITY_4_8", "UNITY_2022_3_OR_NEWER"
)

$editorRefs = New-Object System.Collections.Generic.List[string]
$editorSeen = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
foreach ($p in (Get-CsprojHintPaths $editorCsproj)) {
    $name = [IO.Path]::GetFileName($p)
    # Face-emo's own editor UI / tests are unused and can duplicate Unity types.
    if ($name -match '^(UnityEditor\.UI|UnityEditor\.TestRunner|UnityEngine\.TestRunner)\.dll$') { continue }
    if ($editorSeen.Add($p)) { $editorRefs.Add($p) }
}
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguyaAsm "nadena.dev.ndmf.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguyaAsm "nadena.dev.ndmf.runtime.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguyaAsm "VRC.SDKBase.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguyaAsm "VRC.SDK3A.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguya "Packages\com.vrchat.base\Runtime\VRCSDK\Plugins\VRCSDKBase.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguya "Packages\nadena.dev.ndmf\Dependencies~\System.Collections.Immutable.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguya "Packages\nadena.dev.ndmf\Dependencies~\System.Memory.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguya "Packages\nadena.dev.ndmf\Dependencies~\System.Buffers.dll")
Add-IfExists $editorRefs $editorSeen (Join-Path $kaguya "Packages\nadena.dev.ndmf\Dependencies~\System.Runtime.CompilerServices.Unsafe.dll")
Add-IfExists $editorRefs $editorSeen $runtimeOut

$editorOut = Join-Path $outDir "dev.ova.protection.editor.dll"
$editorCs = @(Get-ChildItem -LiteralPath (Join-Path $pkg "Editor") -Recurse -Filter *.cs | ForEach-Object { $_.FullName })
Write-Output "compile editor... refs=$($editorRefs.Count)"
Invoke-Csc -Sources $editorCs -ReferenceDlls $editorRefs.ToArray() -Output $editorOut -Define @(
    "UNITY_EDITOR", "UNITY_EDITOR_ONLY_COMPILATION", "NET_4_6", "NET_UNITY_4_8",
    "UNITY_2022_3_OR_NEWER", "OVA_VRCSDK3", "USE_NDMF"
)

Write-Output "OK runtime=$runtimeOut"
Write-Output "OK editor=$editorOut"
