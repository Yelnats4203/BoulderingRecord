$json = [Console]::In.ReadToEnd() | ConvertFrom-Json
$path = $json.tool_response.filePath
if (-not $path) { $path = $json.tool_input.file_path }
if ($path -and $path -match '\.cs$') {
    $projectDir = "D:\BoulderingRecord\BoulderingRecordAPI"
    $projectFile = "$projectDir\BoulderingRecordAPI.csproj"
    Push-Location $projectDir
    $relativePath = Resolve-Path -Relative $path
    dotnet format $projectFile --include $relativePath *> $null
    Pop-Location
}
