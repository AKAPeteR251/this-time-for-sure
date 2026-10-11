$ErrorActionPreference = 'Stop'
$taskProject = Join-Path $PSScriptRoot '../../../这次一定出'
$taskProject = (Resolve-Path -LiteralPath $taskProject).Path
$taskEditorData = 'E:/Unity/2022.3.53f1c1/Editor/Data'
$taskDotnet = Join-Path $taskEditorData 'NetCoreRuntime/dotnet.exe'
$taskCsc = Join-Path $taskEditorData 'DotNetSdkRoslyn/csc.dll'
$taskExitCodes = @{}
Push-Location -LiteralPath $taskProject
try {
    foreach ($taskAssembly in @('Assembly-CSharp', 'Assembly-CSharp-Editor')) {
        $taskOriginal = Join-Path $taskProject "Library/Bee/artifacts/1900b0aE.dag/$taskAssembly.rsp"
        $taskLines = Get-Content -LiteralPath $taskOriginal
        $taskLines = $taskLines | Where-Object { $_ -notmatch '^(-out:|-refout:|-analyzer:|/additionalfile:)' }
        if ($taskAssembly -eq 'Assembly-CSharp-Editor') {
            $taskLines = $taskLines | Where-Object { $_ -notmatch '^-r:.*Assembly-CSharp\.ref\.dll' }
            $taskLines += '-r:"' + (Join-Path $PSScriptRoot 'Assembly-CSharp.ref.dll') + '"'
        }
        $taskLines += '-out:"' + (Join-Path $PSScriptRoot "$taskAssembly.dll") + '"'
        $taskLines += '-refout:"' + (Join-Path $PSScriptRoot "$taskAssembly.ref.dll") + '"'
        $taskRsp = Join-Path $PSScriptRoot "$taskAssembly.rsp"
        Set-Content -LiteralPath $taskRsp -Value $taskLines -Encoding utf8
        & $taskDotnet $taskCsc "@$taskRsp" 2>&1 | Out-File -LiteralPath (Join-Path $PSScriptRoot "$taskAssembly.log") -Encoding utf8
        $taskExitCodes[$taskAssembly] = $LASTEXITCODE
        if ($LASTEXITCODE -ne 0) { break }
    }
    $taskExitCodes | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $PSScriptRoot 'compile_status.json')
    $taskExitCodes | ConvertTo-Json
} finally { Pop-Location }
