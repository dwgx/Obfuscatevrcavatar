# Preview ova-web without Unity (MIME + attest stubs + last-build 404).
$ErrorActionPreference = "Stop"
$port = 17849
$root = Join-Path $PSScriptRoot "wwwroot"
$dataDir = Join-Path (Split-Path $PSScriptRoot -Parent | Split-Path -Parent) "Temp\ova-web-preview"
New-Item -ItemType Directory -Force -Path $dataDir | Out-Null
$settingsPath = Join-Path $dataDir "settings.json"
$secretsPath = Join-Path $dataDir "secrets.json"

$defaultSettings = @'
{"version":1,"seed":5145514,"nameLength":12,"preserveMmd":true,"autoDetectPreserve":true,"preserveNameSubstrings":"Go/,FT,eye,VRCEmote,VrcDcc/","names":{"obfuscateHierarchy":true,"obfuscateBlendShapes":true,"obfuscateAnimatorLayers":true,"obfuscateStates":true},"parameters":{"obfuscate":true,"extraPreserve":""},"assets":{"obfuscateClonedNames":true},"watermark":{"enabled":true,"amplitude":0.00001},"crypto":{"textureMode":"compose","encryptSettingsAtRest":false},"attest":{"provider":"off","owner":"","repo":"","branch":"main","path":"ova-attest.json"}}
'@
if (-not (Test-Path -LiteralPath $settingsPath)) {
    [IO.File]::WriteAllText($settingsPath, $defaultSettings.Trim(), [Text.UTF8Encoding]::new($false))
}
if (-not (Test-Path -LiteralPath $secretsPath)) {
    [IO.File]::WriteAllText($secretsPath, '{"githubToken":"","giteeToken":""}', [Text.UTF8Encoding]::new($false))
}

function Get-Mime([string]$ext) {
    switch ($ext.ToLowerInvariant()) {
        ".html" { "text/html; charset=utf-8" }
        ".css"  { "text/css; charset=utf-8" }
        ".js"   { "text/javascript; charset=utf-8" }
        ".mjs"  { "text/javascript; charset=utf-8" }
        ".svg"  { "image/svg+xml" }
        ".json" { "application/json; charset=utf-8" }
        default { "application/octet-stream" }
    }
}

function Get-Fingerprint([string]$json) {
    $s = $json | ConvertFrom-Json
    $en = if ($s.watermark.enabled) { "1" } else { "0" }
    $amp = [float]$s.watermark.amplitude
    $ampText = $amp.ToString("R", [Globalization.CultureInfo]::InvariantCulture)
    $payload = "ova-fp-v1`nseed=$($s.seed)`nnameLength=$($s.nameLength)`nwatermark.enabled=$en`nwatermark.amplitude=$ampText`n"
    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $bytes = $sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($payload))
        return -join ($bytes | ForEach-Object { $_.ToString("x2") })
    } finally { $sha.Dispose() }
}

function Write-Response($res, [byte[]]$bytes, [string]$mime, [int]$status = 200) {
    $res.StatusCode = $status
    $res.ContentType = $mime
    $res.Headers.Add("Cache-Control", "no-store")
    $res.Headers.Add("X-Content-Type-Options", "nosniff")
    $res.ContentLength64 = $bytes.Length
    $res.OutputStream.Write($bytes, 0, $bytes.Length)
}

function Read-Body($req) {
    $reader = New-Object IO.StreamReader($req.InputStream, [Text.Encoding]::UTF8)
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add("http://127.0.0.1:$port/")
try { $listener.Start() } catch { Write-Output "BIND_FAIL $($_.Exception.Message)"; exit 1 }
Write-Output "LISTENING http://127.0.0.1:$port/ root=$root"

while ($listener.IsListening) {
    $ctx = $listener.GetContext()
    $req = $ctx.Request
    $res = $ctx.Response
    $path = $req.Url.AbsolutePath
    try {
        if ($path -eq "/api/health") {
            $bytes = [Text.Encoding]::UTF8.GetBytes('{"ok":true,"name":"ova-web-preview","unity":false}')
            Write-Response $res $bytes "application/json; charset=utf-8"
        } elseif ($path -eq "/api/scene/parameters" -or $path -eq "/api/scene") {
            $dto = @'
{"ok":true,"unity":false,"preview":true,"hasAvatar":true,"avatar":"preview","note":"ova-web-preview","count":8,"rename":1,"transforms":48,"transformsRename":12,"blendshapes":24,"blendshapesRename":18,"animatorLayers":5,"physbonePrefixes":["Hair"],"items":[{"name":"Go/Jump","source":"both","keep":"reserved","reason":"prefix"},{"name":"FT/v_aa","source":"expression","keep":"auto","reason":"FT"},{"name":"IsLocal","source":"animator","keep":"reserved","reason":"vrc"},{"name":"VRCEmote","source":"expression","keep":"reserved","reason":"vrc"},{"name":"MyToggle","source":"both","keep":"none","reason":""},{"name":"Hair","source":"physbone","keep":"physbone","reason":"physbone"},{"name":"Hair_Spring","source":"animator","keep":"physbone","reason":"physbone"},{"name":"Hair_IsGrabbed","source":"physbone","keep":"physbone","reason":"physbone"}]}
'@
            Write-Response $res ([Text.Encoding]::UTF8.GetBytes($dto.Trim())) "application/json; charset=utf-8"
        } elseif ($path -eq "/api/last-build") {
            Write-Response $res ([Text.Encoding]::UTF8.GetBytes('{"error":"not found"}')) "application/json; charset=utf-8" 404
        } elseif ($path -eq "/api/settings" -and $req.HttpMethod -eq "GET") {
            $bytes = [IO.File]::ReadAllBytes($settingsPath)
            Write-Response $res $bytes "application/json; charset=utf-8"
        } elseif ($path -eq "/api/settings" -and $req.HttpMethod -eq "PUT") {
            $body = Read-Body $req
            [IO.File]::WriteAllText($settingsPath, $body, [Text.UTF8Encoding]::new($false))
            $bytes = [Text.Encoding]::UTF8.GetBytes('{"ok":true}')
            Write-Response $res $bytes "application/json; charset=utf-8"
        } elseif ($path -eq "/api/attest/status") {
            $s = (Get-Content -Raw -LiteralPath $settingsPath) | ConvertFrom-Json
            $sec = (Get-Content -Raw -LiteralPath $secretsPath) | ConvertFrom-Json
            $a = $s.attest
            $dto = @{
                ok = $true
                provider = [string]$a.provider
                owner = [string]$a.owner
                repo = [string]$a.repo
                branch = [string]$a.branch
                path = [string]$a.path
                hasGithubSecret = -not [string]::IsNullOrEmpty($sec.githubToken)
                hasGiteeSecret = -not [string]::IsNullOrEmpty($sec.giteeToken)
                wired = $false
                algo = "ova-fp-v1"
                note = "publish-not-wired"
            } | ConvertTo-Json -Compress
            Write-Response $res ([Text.Encoding]::UTF8.GetBytes($dto)) "application/json; charset=utf-8"
        } elseif ($path -eq "/api/attest/fingerprint") {
            $json = Get-Content -Raw -LiteralPath $settingsPath
            $fp = Get-Fingerprint $json
            $dto = @{ ok = $true; algo = "ova-fp-v1"; fingerprint = $fp; payloadKind = "seed+nameLength+watermark" } | ConvertTo-Json -Compress
            Write-Response $res ([Text.Encoding]::UTF8.GetBytes($dto)) "application/json; charset=utf-8"
        } elseif ($path -eq "/api/attest/secret" -and $req.HttpMethod -eq "POST") {
            $body = Read-Body $req | ConvertFrom-Json
            $sec = (Get-Content -Raw -LiteralPath $secretsPath) | ConvertFrom-Json
            if ($body.provider -eq "github") { $sec.githubToken = [string]$body.token }
            elseif ($body.provider -eq "gitee") { $sec.giteeToken = [string]$body.token }
            else {
                Write-Response $res ([Text.Encoding]::UTF8.GetBytes('{"error":"provider must be github or gitee"}')) "application/json; charset=utf-8" 400
                continue
            }
            ($sec | ConvertTo-Json -Compress) | ForEach-Object {
                [IO.File]::WriteAllText($secretsPath, $_, [Text.UTF8Encoding]::new($false))
            }
            $stored = -not [string]::IsNullOrEmpty([string]$body.token)
            $dto = @{ ok = $true; stored = $stored } | ConvertTo-Json -Compress
            Write-Response $res ([Text.Encoding]::UTF8.GetBytes($dto)) "application/json; charset=utf-8"
        } elseif ($path -eq "/api/attest/publish" -and $req.HttpMethod -eq "POST") {
            $json = Get-Content -Raw -LiteralPath $settingsPath
            $s = $json | ConvertFrom-Json
            $fp = Get-Fingerprint $json
            $dto = @{
                ok = $false
                error = "publish-not-wired"
                message = "Architecture only. Next wire PUTs a public fingerprint JSON. Never the PAT, never a decrypt key."
                wouldPost = @{
                    provider = [string]$s.attest.provider
                    owner = [string]$s.attest.owner
                    repo = [string]$s.attest.repo
                    branch = [string]$s.attest.branch
                    path = [string]$s.attest.path
                    algo = "ova-fp-v1"
                    fingerprint = $fp
                }
            } | ConvertTo-Json -Depth 5
            Write-Response $res ([Text.Encoding]::UTF8.GetBytes($dto)) "application/json; charset=utf-8" 501
        } else {
            if ($path -eq "/") { $path = "/index.html" }
            $file = Join-Path $root ($path.TrimStart("/").Replace("/", [IO.Path]::DirectorySeparatorChar))
            $fullRoot = [IO.Path]::GetFullPath($root)
            $fullFile = [IO.Path]::GetFullPath($file)
            if ($fullFile.StartsWith($fullRoot, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $fullFile)) {
                $bytes = [IO.File]::ReadAllBytes($fullFile)
                Write-Response $res $bytes (Get-Mime ([IO.Path]::GetExtension($fullFile)))
            } else {
                Write-Response $res ([Text.Encoding]::UTF8.GetBytes("not found")) "text/plain; charset=utf-8" 404
            }
        }
    } catch {
        try { Write-Response $res ([Text.Encoding]::UTF8.GetBytes('{"error":"internal"}')) "application/json; charset=utf-8" 500 } catch {}
    } finally {
        $res.OutputStream.Close()
    }
}
