param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('BuildDatabase', 'ApplyPatches', 'RunDummyData')]
    [string]$Action
)

$ErrorActionPreference = 'Stop'

# Local development defaults. Edit these values when switching this runner to
# another server or database; SC_PG* variables can still override them.
$script:DefaultSettings = @{
    HostName           = 'localhost'
    Port               = '5432'
    UserName           = 'postgres'
    TargetDatabase     = 'smartcart_pro'
    MaintenanceDatabase = 'postgres'
    PsqlPath           = 'C:\Program Files\PostgreSQL\18\bin\psql.exe'
}

$script:HostName = $null
$script:Port = $null
$script:UserName = $null
$script:TargetDatabase = $null
$script:MaintenanceDatabase = $null
$script:PsqlPath = $null
$script:SecurePassword = $null

function New-RunnerException {
    param(
        [Parameter(Mandatory = $true)][string]$Message,
        [int]$ExitCode = 1
    )

    $exception = [System.Exception]::new($Message)
    $exception.Data['ExitCode'] = $ExitCode
    return $exception
}

function Get-ConfiguredValue {
    param(
        [Parameter(Mandatory = $true)][string]$EnvironmentName,
        [Parameter(Mandatory = $true)][string]$Default
    )

    $value = [Environment]::GetEnvironmentVariable($EnvironmentName, 'Process')
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $Default
    }
    return $value.Trim()
}

function Initialize-ConnectionSettings {
    $script:HostName = Get-ConfiguredValue 'SC_PGHOST' $script:DefaultSettings.HostName
    $portText = Get-ConfiguredValue 'SC_PGPORT' $script:DefaultSettings.Port
    $portNumber = 0
    if (-not [int]::TryParse($portText, [ref]$portNumber) -or $portNumber -lt 1 -or $portNumber -gt 65535) {
        throw (New-RunnerException "Invalid PostgreSQL port '$portText'. Enter a number from 1 to 65535.")
    }
    $script:Port = $portNumber.ToString([Globalization.CultureInfo]::InvariantCulture)
    $script:UserName = Get-ConfiguredValue 'SC_PGUSER' $script:DefaultSettings.UserName

    $databaseValue = Get-ConfiguredValue 'SC_PGDATABASE' $script:DefaultSettings.TargetDatabase
    if ($databaseValue -cnotmatch '^[A-Za-z_][A-Za-z0-9_-]{0,62}$') {
        throw (New-RunnerException "Invalid target database name '$databaseValue'. Use 1 to 63 ASCII letters, digits, underscores, or hyphens; start with a letter or underscore.")
    }
    $script:TargetDatabase = $databaseValue

    $script:MaintenanceDatabase = Get-ConfiguredValue 'SC_PGMAINTENANCEDATABASE' $script:DefaultSettings.MaintenanceDatabase
}

function Resolve-PsqlPath {
    $configuredPath = [Environment]::GetEnvironmentVariable('SC_PSQL', 'Process')
    if (-not [string]::IsNullOrWhiteSpace($configuredPath)) {
        $configuredPath = $configuredPath.Trim().Trim('"')
        if (Test-Path -LiteralPath $configuredPath -PathType Leaf) {
            $script:PsqlPath = (Resolve-Path -LiteralPath $configuredPath).Path
            return
        }
        $configuredCommand = Get-Command -Name $configuredPath -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $configuredCommand) {
            $script:PsqlPath = $configuredCommand.Source
            return
        }
    }

    if (Test-Path -LiteralPath $script:DefaultSettings.PsqlPath -PathType Leaf) {
        $script:PsqlPath = $script:DefaultSettings.PsqlPath
        return
    }

    foreach ($commandName in @('psql.exe', 'psql')) {
        $command = Get-Command -Name $commandName -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($null -ne $command) {
            $script:PsqlPath = $command.Source
            return
        }
    }

    $programRoots = @(${env:ProgramFiles}, ${env:ProgramFiles(x86)}) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
    foreach ($programRoot in $programRoots) {
        $postgresRoot = Join-Path $programRoot 'PostgreSQL'
        if (-not (Test-Path -LiteralPath $postgresRoot -PathType Container)) {
            continue
        }

        $installations = Get-ChildItem -LiteralPath $postgresRoot -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match '^\d+(\.\d+)*$' } |
            Sort-Object -Property @{ Expression = { [int]($_.Name.Split('.')[0]) }; Descending = $true }
        foreach ($installation in $installations) {
            $candidate = Join-Path $installation.FullName 'bin\psql.exe'
            if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                $script:PsqlPath = $candidate
                return
            }
        }
    }

    throw (New-RunnerException "psql.exe was not found through SC_PSQL, PATH, '$($script:DefaultSettings.PsqlPath)', or the standard PostgreSQL install folders. Set SC_PSQL or edit DefaultSettings.PsqlPath in DatabaseRunner.ps1.")
}

function Read-DatabasePassword {
    $configuredPassword = [Environment]::GetEnvironmentVariable('PGPASSWORD', 'Process')
    if (-not [string]::IsNullOrEmpty($configuredPassword)) {
        return
    }

    Write-Host 'Password input is hidden. Press Enter to use pgpass, trust, or psql authentication prompting.'
    $enteredPassword = Read-Host 'PostgreSQL password' -AsSecureString
    if ($enteredPassword.Length -eq 0) {
        $enteredPassword.Dispose()
        return
    }
    $script:SecurePassword = $enteredPassword
}

function ConvertTo-WindowsArgument {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value)

    $builder = New-Object System.Text.StringBuilder
    [void]$builder.Append([char]34)
    $backslashCount = 0

    foreach ($character in $Value.ToCharArray()) {
        if ($character -eq [char]92) {
            $backslashCount++
            continue
        }

        if ($character -eq [char]34) {
            for ($index = 0; $index -lt (2 * $backslashCount + 1); $index++) {
                [void]$builder.Append([char]92)
            }
            [void]$builder.Append([char]34)
        }
        else {
            for ($index = 0; $index -lt $backslashCount; $index++) {
                [void]$builder.Append([char]92)
            }
            [void]$builder.Append($character)
        }
        $backslashCount = 0
    }

    for ($index = 0; $index -lt (2 * $backslashCount); $index++) {
        [void]$builder.Append([char]92)
    }
    [void]$builder.Append([char]34)
    return $builder.ToString()
}

function Start-Psql {
    param(
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string[]]$TailArguments,
        [switch]$CaptureOutput
    )

    $arguments = @(
        '-X', '-v', 'ON_ERROR_STOP=1',
        '-h', $script:HostName,
        '-p', $script:Port,
        '-U', $script:UserName,
        '-d', $Database
    ) + $TailArguments

    $quotedArguments = foreach ($argument in $arguments) {
        ConvertTo-WindowsArgument -Value ([string]$argument)
    }

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $script:PsqlPath
    $startInfo.Arguments = [string]::Join(' ', [string[]]$quotedArguments)
    $startInfo.UseShellExecute = $false
    $startInfo.CreateNoWindow = $false
    $startInfo.RedirectStandardOutput = [bool]$CaptureOutput

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    $passwordBuffer = $null

    try {
        if ($null -ne $script:SecurePassword) {
            $passwordPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($script:SecurePassword)
            try {
                $passwordBuffer = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($passwordPointer)
                $startInfo.EnvironmentVariables['PGPASSWORD'] = $passwordBuffer
            }
            finally {
                [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($passwordPointer)
            }
        }

        if (-not $process.Start()) {
            throw (New-RunnerException 'Windows could not start psql.exe.')
        }

        if ($null -ne $script:SecurePassword) {
            $null = $startInfo.EnvironmentVariables.Remove('PGPASSWORD')
            $passwordBuffer = $null
        }

        $output = ''
        if ($CaptureOutput) {
            $output = $process.StandardOutput.ReadToEnd()
        }
        $process.WaitForExit()
        return [pscustomobject]@{ ExitCode = [int]$process.ExitCode; Output = [string]$output }
    }
    catch {
        if ($_.Exception.Data.Contains('ExitCode')) {
            throw
        }
        throw (New-RunnerException "Could not run psql.exe: $($_.Exception.Message)")
    }
    finally {
        if ($null -ne $script:SecurePassword) {
            $null = $startInfo.EnvironmentVariables.Remove('PGPASSWORD')
        }
        $passwordBuffer = $null
        $process.Dispose()
    }
}

function Invoke-PsqlChecked {
    param(
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string[]]$TailArguments,
        [Parameter(Mandatory = $true)][string]$Operation,
        [switch]$CaptureOutput
    )

    $result = Start-Psql -Database $Database -TailArguments $TailArguments -CaptureOutput:$CaptureOutput
    if ($result.ExitCode -ne 0) {
        throw (New-RunnerException "psql failed while $Operation (exit code $($result.ExitCode))." $result.ExitCode)
    }
    return $result.Output.Trim()
}

function Get-PsqlScalar {
    param(
        [Parameter(Mandatory = $true)][string]$Database,
        [Parameter(Mandatory = $true)][string]$Sql,
        [Parameter(Mandatory = $true)][string]$Operation
    )

    return Invoke-PsqlChecked -Database $Database -TailArguments @('-tA', '-c', $Sql) -Operation $Operation -CaptureOutput
}

function Escape-SqlLiteral {
    param([Parameter(Mandatory = $true)][AllowEmptyString()][string]$Value)
    return $Value.Replace("'", "''")
}

function Assert-ReviewedBaseline {
    param([Parameter(Mandatory = $true)][string]$Database)

    $historyExists = Get-PsqlScalar -Database $Database -Sql "SELECT to_regclass('public.patch_history') IS NOT NULL;" -Operation 'checking for patch_history'
    if ($historyExists -ne 't') {
        throw (New-RunnerException "Database '$Database' has no public.patch_history table. Create a new database with BuildDatabase.bat first.")
    }

    $baselineState = Get-PsqlScalar -Database $Database -Sql "SELECT CASE WHEN EXISTS (SELECT 1 FROM public.patch_history WHERE version = 'V001' AND success = TRUE AND description = 'SmartCartPro baseline V001') THEN 'OK' WHEN EXISTS (SELECT 1 FROM public.patch_history WHERE version = 'V001') THEN 'LEGACY' ELSE 'MISSING' END;" -Operation 'checking the V001 baseline marker'
    if ($baselineState -ne 'OK') {
        throw (New-RunnerException "Database '$Database' has V001 state '$baselineState', not the reviewed baseline. Follow the documented legacy-database transition before applying patches or dummy data.")
    }
}

function Invoke-BuildDatabase {
    Write-Host "Checking PostgreSQL maintenance database '$script:MaintenanceDatabase'..."
    $null = Get-PsqlScalar -Database $script:MaintenanceDatabase -Sql 'SELECT 1;' -Operation "connecting to maintenance database '$script:MaintenanceDatabase'"
    Write-Host 'Connected to the maintenance database.'

    $databaseLiteral = Escape-SqlLiteral $script:TargetDatabase
    $exists = Get-PsqlScalar -Database $script:MaintenanceDatabase -Sql "SELECT CASE WHEN EXISTS (SELECT 1 FROM pg_database WHERE datname = '$databaseLiteral') THEN '1' ELSE '0' END;" -Operation 'checking whether the target database already exists'
    if ($exists -eq '1') {
        Write-Host "Database '$script:TargetDatabase' already exists. It was not changed." -ForegroundColor Yellow
        Write-Host 'To update an existing SmartCartPro database, double-click ApplyPatches.bat.'
        return
    }

    Write-Host "Creating new database '$script:TargetDatabase'..."
    $quotedIdentifier = '"' + $script:TargetDatabase.Replace('"', '""') + '"'
    $null = Invoke-PsqlChecked -Database $script:MaintenanceDatabase -TailArguments @('-c', "CREATE DATABASE $quotedIdentifier;") -Operation "creating database '$script:TargetDatabase'"

    $baselinePath = Join-Path $PSScriptRoot '03_Patches\V001__Baseline.sql'
    if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
        throw (New-RunnerException "Baseline script not found: '$baselinePath'. The new database was created but no schema was applied.")
    }

    Write-Host 'Applying the V001 baseline: tables, required system seeds, and patch history...'
    $null = Invoke-PsqlChecked -Database $script:TargetDatabase -TailArguments @('-f', $baselinePath) -Operation 'applying 03_Patches\V001__Baseline.sql'
    Write-Host "SUCCESS: V001 baseline installed in '$script:TargetDatabase'. Optional dummy data was not run." -ForegroundColor Green
}

function Invoke-ApplyPatches {
    $null = Get-PsqlScalar -Database $script:TargetDatabase -Sql 'SELECT 1;' -Operation "connecting to target database '$script:TargetDatabase'"
    Assert-ReviewedBaseline -Database $script:TargetDatabase

    $patchDirectory = Join-Path $PSScriptRoot '03_Patches'
    $patchFiles = @(Get-ChildItem -LiteralPath $patchDirectory -Filter 'V*.sql' -File | Sort-Object -Property Name)
    $appliedCount = 0
    $skippedCount = 0

    foreach ($patchFile in $patchFiles) {
        if ($patchFile.Name -notmatch '^(V[0-9]{3})__([A-Za-z0-9][A-Za-z0-9_-]*)\.sql$') {
            throw (New-RunnerException "Unexpected patch filename '$($patchFile.Name)'. Expected V###__Description.sql.")
        }
        $version = $Matches[1]
        if ($version -eq 'V001') {
            Write-Host 'SKIP V001: the baseline is already installed.'
            continue
        }

        $versionLiteral = Escape-SqlLiteral $version
        $alreadyApplied = Get-PsqlScalar -Database $script:TargetDatabase -Sql "SELECT CASE WHEN EXISTS (SELECT 1 FROM public.patch_history WHERE version = '$versionLiteral' AND success = TRUE) THEN '1' ELSE '0' END;" -Operation "checking patch $version history"
        if ($alreadyApplied -eq '1') {
            Write-Host "SKIP ${version}: already recorded as successful."
            $skippedCount++
            continue
        }

        Write-Host "Applying $($patchFile.Name)..."
        $null = Invoke-PsqlChecked -Database $script:TargetDatabase -TailArguments @('-f', $patchFile.FullName) -Operation "applying $($patchFile.Name)"

        $fileLiteral = Escape-SqlLiteral $patchFile.Name
        $historySql = "INSERT INTO public.patch_history (version, description, appliedby, success) VALUES ('$versionLiteral', '$fileLiteral', current_user, TRUE) ON CONFLICT (version) DO UPDATE SET description = EXCLUDED.description, appliedat = NOW(), appliedby = current_user, success = TRUE;"
        $null = Invoke-PsqlChecked -Database $script:TargetDatabase -TailArguments @('-c', $historySql) -Operation "recording successful patch $version"
        Write-Host "SUCCESS: $version applied and recorded." -ForegroundColor Green
        $appliedCount++
    }

    if ($appliedCount -eq 0 -and $skippedCount -eq 0) {
        Write-Host 'No V002+ patch files are present. The baseline was verified.'
    }
    elseif ($appliedCount -eq 0) {
        Write-Host 'No pending patches. All available V002+ patches are already recorded as successful.'
    }
    else {
        Write-Host "SUCCESS: Applied $appliedCount patch(es); skipped $skippedCount already-applied patch(es)." -ForegroundColor Green
    }
}

function Invoke-RunDummyData {
    $null = Get-PsqlScalar -Database $script:TargetDatabase -Sql 'SELECT 1;' -Operation "connecting to target database '$script:TargetDatabase'"
    Assert-ReviewedBaseline -Database $script:TargetDatabase

    $dummyFiles = @(
        '001_DevelopmentPlatformSuperAdmin.sql',
        '002_DevelopmentTrialLicense.sql',
        '003_GreenBasketSupermarket.sql'
    )
    $dummyDirectory = Join-Path $PSScriptRoot '06_DummyData'
    foreach ($fileName in $dummyFiles) {
        $filePath = Join-Path $dummyDirectory $fileName
        if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
            throw (New-RunnerException "Required manual dummy-data script not found: '$filePath'.")
        }

        Write-Host "MANUAL DUMMY DATA: $fileName"
        $null = Invoke-PsqlChecked -Database $script:TargetDatabase -TailArguments @('-f', $filePath) -Operation "running dummy-data script $fileName"
    }

    Write-Host 'SUCCESS: The explicitly selected dummy-data scripts completed.' -ForegroundColor Green
}

$runnerExitCode = 0
try {
    Initialize-ConnectionSettings
    Resolve-PsqlPath
    Read-DatabasePassword

    Write-Host "Using PostgreSQL client: $script:PsqlPath"
    Write-Host "Connection: $script:HostName`:$script:Port as $script:UserName"

    switch ($Action) {
        'BuildDatabase' { Invoke-BuildDatabase }
        'ApplyPatches' { Invoke-ApplyPatches }
        'RunDummyData' { Invoke-RunDummyData }
    }
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Data.Contains('ExitCode')) {
        $runnerExitCode = [int]$_.Exception.Data['ExitCode']
    }
    else {
        $runnerExitCode = 1
    }
}
finally {
    if ($null -ne $script:SecurePassword) {
        $script:SecurePassword.Dispose()
        $script:SecurePassword = $null
    }
}

exit $runnerExitCode
