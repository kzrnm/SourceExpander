BeforeAll {
    $script:SourceDirectory = Get-Item (Join-Path $PSScriptRoot ".." "Source")
    $script:PackageDirectory = New-Item -ItemType Directory ("$PSScriptRoot/obj/SourceExpander.Embedder.PackJson.Test") -Force

    Write-Host "Build projects in '$PackageDirectory'"

    "<Project></Project>" > "$PackageDirectory/Directory.Build.props"
    "<Project></Project>" > "$PackageDirectory/Directory.Packages.props"

    function Get-AssemblyMetadata {
        param ($dllPath)
        $alc = [System.Runtime.Loader.AssemblyLoadContext]::new("MetadataLoader", $true)

        try {
            $result = $alc.LoadFromAssemblyPath((Get-Item $dllPath)).GetCustomAttributes([System.Reflection.AssemblyMetadataAttribute], $false) | ForEach-Object {
                [PSCustomObject]@{
                    Key   = [string]::Copy($_.Key);
                    Value = [string]::Copy($_.Value);
                }
            }
        }
        finally {
            $alc.Unload()
            $alc = $null
    
            [System.GC]::Collect(); [System.GC]::WaitForPendingFinalizers()
            [System.GC]::Collect(); [System.GC]::WaitForPendingFinalizers()
        }
        return $result
    }

}

AfterAll {
    if ($env:SOURCE_EXPANDER_PACKJSON_TEST_CLEAN -iin @("true", "1")) {
        $PackageDirectory.Delete($true)
    }
}
function buildTarget {
    param (
        [Parameter(Position = 0, Mandatory = $true)]
        [string] $Framework,
        [Parameter(Position = 1, Mandatory = $true)]
        [string] $Num,
        [Parameter(Position = 2, Mandatory = $true)]
        [string] $CSharpVersion,
        [switch] $NoJson,
        [switch] $NoProps,
        [switch] $NoTargets
    )

    if ($NoJson) {
        $NoProps = $true
        $NoTargets = $true
    }

    return @{
        Framework     = $Framework;
        Num           = $Num;
        CSharpVersion = $CSharpVersion;
        NoJson        = [bool] $NoJson;
        NoProps       = [bool] $NoProps;
        NoTargets     = [bool] $NoTargets;
    }
}

Describe 'TestProject:<Name>' -ForEach @(
    @{
        Name    = "Target.Single";
        Targets = @(
            (buildTarget "net9.0" 9 '13.0' -NoProps)
        );
    },
    @{
        Name    = "Targets.Multi";
        Targets = @(
            (buildTarget "net10.0" 10 '14.0'),
            (buildTarget "netstandard2.1" 21  '8.0' -NoTargets),
            (buildTarget "net9.0" 9 '13.0' -NoProps),
            (buildTarget "net8.0" 8 '12.0' -NoJson)
        );
    }
) {
    BeforeAll {
        $projectDir = New-Item -ItemType Directory (Join-Path $PackageDirectory $Name) -Force
        $projectFile = (Join-Path $projectDir ($Name + ".csproj"))
        "
class P
{
public static int Num =>
#if NET10_0_OR_GREATER
    10
#elif NET9_0_OR_GREATER
    9
#elif NET8_0_OR_GREATER
    8
#elif NET7_0_OR_GREATER
    7
#elif NET6_0_OR_GREATER
    6
#elif NET5_0_OR_GREATER
    5
#elif NETSTANDARD2_1_OR_GREATER
    21
#elif NETSTANDARD2_0_OR_GREATER
    20
#endif
    ;
}
" > "$projectDir/Prog.cs"

        @"
<Project Sdk="Microsoft.NET.Sdk">
    <Import Project="$SourceDirectory/SourceExpander.Embedder.PackJson/build/SourceExpander.Embedder.PackJson.props" />
    <PropertyGroup>
        $(($Targets.Length -eq 1) ? "<TargetFramework>$($Targets.Framework)</TargetFramework>" :"<TargetFrameworks>$($Targets.Framework -join ';')</TargetFrameworks>")
        <AllowUnsafeBlocks>false</AllowUnsafeBlocks>
        <Nullable>disable</Nullable>
        <ImplicitUsings>false</ImplicitUsings>

        <SourceExpander_PackJson_Enabled Condition="'`$(TargetFramework)' == 'net8.0'">false</SourceExpander_PackJson_Enabled>
        <SourceExpander_PackJsonProps_Enabled Condition="'`$(TargetFramework)' == 'net9.0'">false</SourceExpander_PackJsonProps_Enabled>
        <SourceExpander_PackJsonTargets_Enabled Condition="'`$(TargetFramework)' == 'netstandard2.1'">false</SourceExpander_PackJsonTargets_Enabled>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="SourceExpander.Embedder" Version="9.0.2-beta4">
          <PrivateAssets>all</PrivateAssets>
          <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
        </PackageReference>
    </ItemGroup>

    <Import Project="$SourceDirectory/SourceExpander.Embedder.PackJson/build/SourceExpander.Embedder.PackJson.targets" />
</Project>
"@ > $projectFile
    }

    Describe 'Pack:<_>' -ForEach @(
        "PackBuild",
        "PackNoBuild"
    ) {
        BeforeAll {
            $NupkgDirectory = Join-Path $PackageDirectory $_ $Name
            if ($env:SOURCE_EXPANDER_PACKJSON_TEST_NO_BUILD -inotin @("true", "1")) {
                dotnet clean $projectFile
                if ($_ -eq 'PackBuild') {
                    dotnet pack $projectFile -c Release -o (Join-Path $PackageDirectory $_)
                }
                elseif ($_ -eq 'PackNoBuild') {
                    dotnet build $projectFile -c Release
                    dotnet pack $projectFile -c Release -o (Join-Path $PackageDirectory $_) --no-build
                }
                else {
                    throw 'Not supported'
                }

                if (Test-Path $NupkgDirectory) {
                    ([System.IO.DirectoryInfo]$NupkgDirectory).Delete($true)
                }

                Expand-Archive (Get-Item (Join-Path $PackageDirectory $_ ($Name + "*.nupkg"))) -DestinationPath $NupkgDirectory
            }
        }

        Describe 'TargetFramework:<Framework>' -ForEach $Targets {
            BeforeAll {
                $script:buildTransitive = Join-Path $NupkgDirectory 'buildTransitive' $Framework
                $script:propsPath = Join-Path $buildTransitive "${Name}.props"
                $script:targetsPath = Join-Path $buildTransitive "${Name}.targets"
                $script:embeddedJsonPath = Join-Path $buildTransitive "${Name}_SourceExpander.Embedded.json"

                $script:dllPath = Join-Path $NupkgDirectory 'lib' $Framework "$Name.dll"
                $script:Metadata = Get-AssemblyMetadata $dllPath | Where-Object Key -Like "SourceExpander.*"

                $script:embeddedJson = ('{"AssemblyName":"' +
                    $Name + '","Sources":[{"CodeBody":"class P { public static int Num => ' +
                    "$Num" +
                    '; }","Dependencies":[],"FileName":"' +
                    $Name + '>Prog.cs","TypeNames":["P"],"Usings":[]}],"EmbedderVersion":"9.0.2.24","CSharpVersion":"' +
                    $CSharpVersion + '","AllowUnsafe":false,"EmbeddedNamespaces":[]}'
                )

                $UnderscoredNameTag = "$Name".Replace('.', '_') + '_Source'
                $script:exptectedProps = [xml](@'
<Project>
  <PropertyGroup>
    <
'@ + $UnderscoredNameTag + '>$(MSBuildThisFileDirectory)' + $Name + '_SourceExpander.Embedded.json</' + $UnderscoredNameTag + '><' +
                    $UnderscoredNameTag + '_Visible>false</' + $UnderscoredNameTag + @'
_Visible>
  </PropertyGroup>
</Project>
'@)
                $exptectedProps.PreserveWhitespace = $false

                $script:exptectedTargets = [xml](@'
<Project>
  <ItemGroup Condition="'$(SourceExpander_Generator)'=='true' And Exists('$(
'@ + $UnderscoredNameTag + @'
)')">
    <AdditionalFiles LinkBase="Properties/SourceExpander.Embedded"
'@ + ' Include="$(' + $UnderscoredNameTag + ')" Visible="$(' + $UnderscoredNameTag + '_Visible)" />' + @'
  </ItemGroup>
</Project>
'@)
                $exptectedTargets.PreserveWhitespace = $false
            }

            It 'Dll must be exist and embedded' {
                $dllPath | Should -Exist
                $Metadata | Where-Object Key -EQ 'SourceExpander.EmbedderVersion' |
                Select-Object -ExpandProperty Value | Should -Be '9.0.2.24'
            }
            if ($NoJson) {
                It 'Dll with metadata and without json' {
                    $Metadata | Should -HaveCount 2
                    $Metadata | Where-Object Key -EQ 'SourceExpander.EmbeddedDataJson' |
                    Select-Object -ExpandProperty Value |
                    Should -Be $embeddedJson
                }

                It "buildTransitive directory doesn't exist" {
                    $buildTransitive | Should -Not -Exist
                }
            }
            else {
                It 'Dll without metadata and with json' {
                    $Metadata | Should -HaveCount 1
                }

                It "EmbeddedJson content" {
                    $embeddedJsonPath | Should -Exist
                    (Get-Content -Raw $embeddedJsonPath) | Should -Be $embeddedJson
                }
            }

            if ($NoProps) {
                It ".props doesn't exist" {
                    $propsPath | Should -Not -Exist
                }
            }
            else {
                It '.props content' {
                    $xml = [xml](Get-Content $propsPath)
                    $xml.PreserveWhitespace = $false

                    $xml.OuterXml | Should -Be $exptectedProps.OuterXml
                }
            }

            if ($NoTargets) {
                It ".targets doesn't exist" {
                    $targetsPath | Should -Not -Exist
                }
            }
            else {
                It '.targets content' {
                    $xml = [xml](Get-Content $targetsPath)
                    $xml.PreserveWhitespace = $false

                    $xml.OuterXml | Should -Be $exptectedTargets.OuterXml
                }
            }
        }
    }
}

Describe 'GeneratePackageOnBuild' {
    BeforeAll {
        $Name = 'GeneratePackageOnBuild'
        $projectDir = New-Item -ItemType Directory (Join-Path $PackageDirectory $Name) -Force
        $projectFile = (Join-Path $projectDir ($Name + ".csproj"))
        "
class P
{
public static int Num =>0;
}
" > "$projectDir/Prog.cs"

        @"
<Project Sdk="Microsoft.NET.Sdk">
    <Import Project="$SourceDirectory/SourceExpander.Embedder.PackJson/build/SourceExpander.Embedder.PackJson.props" />
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
        <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="SourceExpander.Embedder" Version="9.0.2-beta4">
          <PrivateAssets>all</PrivateAssets>
          <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
        </PackageReference>
    </ItemGroup>

    <Import Project="$SourceDirectory/SourceExpander.Embedder.PackJson/build/SourceExpander.Embedder.PackJson.targets" />
</Project>
"@ > $projectFile
    }

    It 'Build failure' {
        $psi = [System.Diagnostics.ProcessStartInfo]@{
            RedirectStandardOutput = $true;
            StandardOutputEncoding = [System.Text.UTF8Encoding]::new($false);
            FileName               = "dotnet";
            Arguments              = "build $projectFile";
        }
        $Psi.EnvironmentVariables['DOTNET_CLI_UI_LANGUAGE'] = 'en'
        $proc = [System.Diagnostics.Process]::Start($psi)
        $proc.WaitForExit(10000)
        $output = $proc.StandardOutput.ReadToEnd()
        $proc.ExitCode | Should -Not -Be 0
        $output | Should -Match 'SourceEmbeddingPackJson0001'
        $output | Should -Match "'GeneratePackageOnBuild' must be false"
    }
}