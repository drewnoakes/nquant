$psake.use_exit_on_error = $true
properties {
    $currentDir = resolve-path .
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    $baseDir = Split-Path -parent $Invocation.MyCommand.Definition | split-path -parent | split-path -parent | split-path -parent
    $configuration = "debug"
	$filesDir = "$baseDir\BuildFiles"
	$version = "0.9." + (hg log --template '{rev}:{node}\n' | measure-object).Count
}

task Debug -depends Default
task Default -depends Clean-Solution, Build-Solution
task Download -depends Clean-Solution, Update-AssemblyInfoFiles, Build-Solution, Build-Output

task Clean-Solution -depends Clean-BuildFiles {
    exec { msbuild nQuant.sln /t:Clean /v:quiet }
}

task Update-AssemblyInfoFiles {
	$commit = hg log --template '{rev}:{node}\n' -l 1
	Update-AssemblyInfoFiles $version $commit
}

task Build-Solution {
    exec { msbuild nQuant.sln /maxcpucount /t:Build /v:Minimal /p:Configuration=$configuration }
}

task Clean-BuildFiles {
    clean $filesDir
}

task Push-Nuget {
	exec { .\Tools\nuget.exe push $filesDir\nQuant.$version.nupkg }
}

task Build-Output {
	clean $baseDir\nquant.core\Nuget\Lib
	create $baseDir\nquant.core\Nuget\Lib
	Copy-Item $baseDir\nquant.core\bin\$configuration\*.* $baseDir\nquant.core\Nuget\Lib
	clean $baseDir\nquant.core\Nuget\Tools
	create $baseDir\nquant.core\Nuget\Tools
	Copy-Item $baseDir\nquantShell\bin\$configuration\*.* $baseDir\nquant.core\Nuget\Tools
	clean $filesDir
	create $filesDir
    $Spec = [xml](get-content "nQuant.core\Nuget\nQuant.nuspec")
    $Spec.package.metadata.version = $version
    $Spec.Save("nQuant.core\Nuget\nQuant.nuspec")
	exec { .\Tools\zip.exe -j -9 $filesDir\nQuant$version.zip $baseDir\nQuant.core\Nuget\Lib\nQuant.Core.dll $baseDir\nQuant.core\Nuget\Lib\nQuant.Core.pdb $baseDir\License.txt $baseDir\nQuant.core\Nuget\Tools\nQuant.exe $baseDir\nQuant.core\Nuget\Tools\nQuant.pdb }
    exec { .\Tools\nuget.exe pack "nQuant.core\Nuget\nQuant.nuspec" -o $filesDir }
}

function roboexec([scriptblock]$cmd) {
    & $cmd | out-null
    if ($lastexitcode -eq 0) { throw "No files were copied for command: " + $cmd }
}

function clean($path) {
    remove-item -force -recurse $path -ErrorAction SilentlyContinue
}

function create([string[]]$paths) {
    foreach ($path in $paths) {
        if ((test-path $path) -eq $FALSE) {
            new-item -path $path -type directory | out-null
        }
    }
}

# Borrowed from Luis Rocha's Blog (http://www.luisrocha.net/2009/11/setting-assembly-version-with-windows.html)
function Update-AssemblyInfoFiles ([string] $version, [string] $commit) {
    $assemblyVersionPattern = 'AssemblyVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileVersionPattern = 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)'
    $fileCommitPattern = 'AssemblyTrademarkAttribute\("[0-9]+:[a-f0-9]{40}"\)'
    $assemblyVersion = 'AssemblyVersion("' + $version + '")';
    $fileVersion = 'AssemblyFileVersion("' + $version + '")';
    $commitVersion = 'AssemblyTrademarkAttribute("' + $commit + '")';

    Get-ChildItem -path $baseDir -r -filter AssemblyInfo.cs | ForEach-Object {
        $filename = $_.Directory.ToString() + '\' + $_.Name
        $filename + ' -> ' + $version
        
        # If you are using a source control that requires to check-out files before 
        # modifying them, make sure to check-out the file here.
        # For example, TFS will require the following command:
        # tf checkout $filename
    
        (Get-Content $filename) | ForEach-Object {
            % {$_ -replace $assemblyVersionPattern, $assemblyVersion } |
            % {$_ -replace $fileVersionPattern, $fileVersion } |
			% {$_ -replace $fileCommitPattern, $commitVersion }
        } | Set-Content $filename
    }
}