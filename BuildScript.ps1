$psake.use_exit_on_error = $true
properties {
    $currentDir = resolve-path .
    $baseDir = $psake.build_script_dir
    $configuration = "debug"
	$filesDir = "$baseDir\BuildFiles"
	$version = "0.9." + (hg log --template '{rev}:{node}\n' | measure-object).Count
	$nugetDir = "$baseDir\.NuGet"
}

task Debug -depends Default
task Default -depends Clean-Solution, Build-Solution, Test-Solution
task Download -depends Clean-Solution, Update-AssemblyInfoFiles, Build-Output

task Test-Solution -depends Build-Solution {
	$runnerDir = ([array](dir $baseDir\packages\xunit.runners.*))[-1];
    exec { .$runnerDir\tools\xunit.console.clr4.exe "nquant.Facts\bin\$configuration\nquant.Facts.dll" }
}

task Build-35-Solution {
  $conf = $configuration+35
  exec { msbuild 'nquant.core\nquant.core.csproj' /maxcpucount /t:Build /v:Minimal /p:Configuration=$conf }
  exec { msbuild 'nquantshell\nquant.csproj' /maxcpucount /t:Build /v:Minimal /p:Configuration=$conf }
}

task Clean-Solution -depends Clean-BuildFiles {
	$conf = $configuration+35
    clean $baseDir\nquant.core\Nuget\Lib
	create $baseDir\nquant.core\Nuget\Lib
    exec { msbuild nQuant.sln /t:Clean /v:quiet }
	exec { msbuild 'nQuant.core\nquant.core.csproj' /t:Clean /v:quiet /p:Configuration=$conf }
	exec { msbuild 'nQuantShell\nQuant.csproj' /t:Clean /v:quiet /p:Configuration=$conf }
}

task Update-AssemblyInfoFiles {
	$commit = hg log --template '{rev}:{node}\n' -l 1
	Update-AssemblyInfoFiles $version $commit
}

task Build-Solution -depends Build-35-Solution {
    exec { msbuild nQuant.sln /maxcpucount /t:Build /v:Minimal /p:Configuration=$configuration }
}

task Clean-BuildFiles {
    clean $filesDir
}

task Push-Nuget {
	$pkg = Get-Item -path $filesDir/nquant.0.*.*.nupkg
	exec { .$nugetDir\nuget.exe push $filesDir\$($pkg.Name) }
}

task Build-Output -depends Build-Solution {
	clean $baseDir\nquant.core\Nuget\Lib
	create $baseDir\nquant.core\Nuget\Lib\net20
	create $baseDir\nquant.core\Nuget\Lib\net40
	Copy-Item $baseDir\nquant.core\bin\v3.5\$configuration\*.* $baseDir\nquant.core\Nuget\Lib\net20
	Copy-Item $baseDir\nquant.core\bin\v4.0\$configuration\*.* $baseDir\nquant.core\Nuget\Lib\net40
	clean $baseDir\nquant.core\Nuget\Tools
	create $baseDir\nquant.core\Nuget\Tools\net20
	create $baseDir\nquant.core\Nuget\Tools\net40
	Copy-Item $baseDir\nquantShell\bin\v3.5\$configuration\*.* $baseDir\nquant.core\Nuget\Tools\net20
	Copy-Item $baseDir\nquantShell\bin\v4.0\$configuration\*.* $baseDir\nquant.core\Nuget\Tools\net40
	clean $filesDir
	create $filesDir
	create $filesDir\net35
	create $filesDir\net40
	Copy-Item $baseDir\nquantShell\bin\v3.5\$configuration\*.* $filesDir\net35
	Copy-Item $baseDir\nquantShell\bin\v4.0\$configuration\*.* $filesDir\net40
	Copy-Item $baseDir\License.txt $filesDir
	cd $filesDir
	exec { ..\Tools\zip.exe -9 -r nQuant-$version.zip . }
	cd $currentDir
    exec { .$nugetDir\nuget.exe pack "nQuant.core\Nuget\nQuant.nuspec" -o $filesDir -version $version }
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