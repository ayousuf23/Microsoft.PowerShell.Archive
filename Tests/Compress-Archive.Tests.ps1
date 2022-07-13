<############################################################################################
 # File: Pester.Commands.Cmdlets.ArchiveTests.ps1
 # Commands.Cmdlets.ArchiveTests suite contains Tests that are
 # used for validating Microsoft.PowerShell.Archive module.
 ############################################################################################>
 $script:TestSourceRoot = $PSScriptRoot
 Write-Output $script:TestSourceRoot
 $DS = [System.IO.Path]::DirectorySeparatorChar
 if ($IsWindows -eq $null) {
     $IsWindows = $PSVersionTable.PSEdition -eq "Desktop"
 } 
 
 Describe("Microsoft.PowerShell.Archive tests") {
    BeforeAll {

        $DS = [System.IO.Path]::DirectorySeparatorChar

        $originalProgressPref = $ProgressPreference
        $ProgressPreference = "SilentlyContinue"
        $originalPSModulePath = $env:PSModulePath
        # make sure we use the one in this repo
        $env:PSModulePath = "$($script:TestSourceRoot)\..;$($env:PSModulePath)"

        # Add compression assemblies
        function Add-CompressionAssemblies {
            Add-Type -AssemblyName System.IO.Compression
            if ($psedition -eq "Core")
            {
                Add-Type -AssemblyName System.IO.Compression.ZipFile
            }
            else
            {
                Add-Type -AssemblyName System.IO.Compression.FileSystem
            }
        }

        Add-CompressionAssemblies

        # Used for validating an archive's contents
        function Test-Archive {
            param
            (
                [string] $archivePath,
                [string[]] $expectedEntries
            )
    
            try
            {
                $archiveFileStreamArgs = @($archivePath, [System.IO.FileMode]::Open)
                $archiveFileStream = New-Object -TypeName System.IO.FileStream -ArgumentList $archiveFileStreamArgs
    
                $zipArchiveArgs = @($archiveFileStream, [System.IO.Compression.ZipArchiveMode]::Read, $false)
                $zipArchive = New-Object -TypeName System.IO.Compression.ZipArchive -ArgumentList $zipArchiveArgs
    
                $actualEntryCount = $zipArchive.Entries.Count
                $actualEntryCount | Should -Be $expectedEntries.Length

                # Get a list of archive entries

                ForEach ($expectedArchiveEntry in $expectedEntries) {
                    $expectedArchiveEntry | Should -BeIn $zipArchive.Entries
                }
            }
            finally
            {
                if ($null -ne $zipArchive) { $zipArchive.Dispose()}
                if ($null -ne $archiveFileStream) { $archiveFileStream.Dispose() }
            }
        }
    }
    
    AfterAll {
        $global:ProgressPreference = $originalProgressPref
        $env:PSModulePath = $originalPSModulePath
    }

    Context "Parameter set validation tests" {
        BeforeAll {
            function CompressArchivePathParameterSetValidator {
                param
                (
                    [string[]] $path,
                    [string] $destinationPath
                )
        
                try
                {
                    Compress-Archive -Path $path -DestinationPath $destinationPath
                    throw "ValidateNotNullOrEmpty attribute is missing on one of parameters belonging to Path parameterset."
                }
                catch
                {
                    $_.FullyQualifiedErrorId | Should -Be "ParameterArgumentValidationError,Microsoft.PowerShell.Archive.CompressArchiveCommand"
                }
            }
        
            function CompressArchiveLiteralPathParameterSetValidator {
                param
                (
                    [string[]] $literalPath,
                    [string] $destinationPath,
                    [string] $compressionLevel = "Optimal"
                )
        
                try
                {
                    Compress-Archive -LiteralPath $literalPath -DestinationPath $destinationPath
                    throw "ValidateNotNullOrEmpty attribute is missing on one of parameters belonging to LiteralPath parameterset."
                }
                catch
                {
                    $_.FullyQualifiedErrorId | Should -Be "ParameterArgumentValidationError,Microsoft.PowerShell.Archive.CompressArchiveCommand"
                }
            }
        
        
            function CompressArchiveInvalidPathValidator {
                param
                (
                    [string[]] $path,
                    [string] $destinationPath,
                    [string] $invalidPath,
                    [string] $expectedFullyQualifiedErrorId
                )
        
                try
                {
                    Compress-Archive -Path $path -DestinationPath $destinationPath
                    throw "Failed to validate that an invalid Path $invalidPath was supplied as input to Compress-Archive cmdlet."
                }
                catch
                {
                    $_.FullyQualifiedErrorId | Should -Be $expectedFullyQualifiedErrorId
                }

                try
                {
                    Compress-Archive -LiteralPath $path -DestinationPath $destinationPath
                    throw "Failed to validate that an invalid Path $invalidPath was supplied as input to Compress-Archive cmdlet."
                }
                catch
                {
                    $_.FullyQualifiedErrorId | Should -Be $expectedFullyQualifiedErrorId
                }
            }
            
            # Set up files for tests
            New-Item $TestDrive$($DS)SourceDir -Type Directory | Out-Null
            $content = "Some Data"
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)Sample-1.txt
        }


        It "Validate errors from Compress-Archive with NULL & EMPTY values for Path, LiteralPath, DestinationPath, CompressionLevel parameters" {
            $sourcePath = "$TestDrive$($DS)SourceDir"
            $destinationPath = "$TestDrive$($DS)SampleSingleFile.zip"

            CompressArchivePathParameterSetValidator $null $destinationPath
            CompressArchivePathParameterSetValidator $sourcePath $null
            CompressArchivePathParameterSetValidator $null $null

            CompressArchivePathParameterSetValidator "" $destinationPath
            CompressArchivePathParameterSetValidator $sourcePath ""
            CompressArchivePathParameterSetValidator "" ""

            CompressArchiveLiteralPathParameterSetValidator $null $destinationPath
            CompressArchiveLiteralPathParameterSetValidator $sourcePath $null
            CompressArchiveLiteralPathParameterSetValidator $null $null

            CompressArchiveLiteralPathParameterSetValidator "" $destinationPath
            CompressArchiveLiteralPathParameterSetValidator $sourcePath ""
            CompressArchiveLiteralPathParameterSetValidator "" ""
        }

        It "Validate errors from Compress-Archive when invalid path (non-existing path / non-filesystem path) is supplied for Path or LiteralPath parameters" {
            CompressArchiveInvalidPathValidator "$TestDrive$($DS)InvalidPath" $TestDrive "$TestDrive$($DS)InvalidPath" "PathNotFound,Microsoft.PowerShell.Archive.CompressArchiveCommand"

            $path = @("$TestDrive", "$TestDrive$($DS)InvalidPath")
            CompressArchiveInvalidPathValidator $path $TestDrive "$TestDrive$($DS)InvalidPath" "PathNotFound,Microsoft.PowerShell.Archive.CompressArchiveCommand"
        }

        It "Validate error from Compress-Archive when duplicate paths are supplied as input to Path parameter" {
            $sourcePath = @(
                "$TestDrive$($DS)SourceDir$($DS)Sample-1.txt",
                "$TestDrive$($DS)SourceDir$($DS)Sample-1.txt")
            $destinationPath = "$TestDrive$($DS)DuplicatePaths.zip"

            try
            {
                Compress-Archive -Path $sourcePath -DestinationPath $destinationPath
                throw "Failed to detect that duplicate Path $sourcePath is supplied as input to Path parameter."
            }
            catch
            {
                $_.FullyQualifiedErrorId | Should -Be "DuplicatePathFound,Microsoft.PowerShell.Archive.CompressArchiveCommand"
            }
        }

        It "Validate error from Compress-Archive when duplicate paths are supplied as input to LiteralPath parameter" -tag th {
            $sourcePath = @(
                "$TestDrive$($DS)SourceDir$($DS)Sample-1.txt",
                "$TestDrive$($DS)SourceDir$($DS)Sample-1.txt")
            $destinationPath = "$TestDrive$($DS)DuplicatePaths.zip"

            try
            {
                Compress-Archive -LiteralPath $sourcePath -DestinationPath $destinationPath
                throw "Failed to detect that duplicate Path $sourcePath is supplied as input to LiteralPath parameter."
            }
            catch
            {
                $_.FullyQualifiedErrorId | Should -Be "DuplicatePathFound,Microsoft.PowerShell.Archive.CompressArchiveCommand"
            }
        }

        ## From 504
        It "Validate that Source Path can be at SystemDrive location" -skip:(!$IsWindows) {
            $sourcePath = "$env:SystemDrive$($DS)SourceDir"
            $destinationPath = "$TestDrive$($DS)SampleFromSystemDrive.zip"
            New-Item $sourcePath -Type Directory | Out-Null # not enough permissions to write to drive root on Linux
            "Some Data" | Out-File -FilePath $sourcePath$($DS)SampleSourceFileForArchive.txt
            try
            {
                Compress-Archive -Path $sourcePath -DestinationPath $destinationPath
                Test-Path $destinationPath | Should -Be $true
            }
            finally
            {
                del "$sourcePath" -Force -Recurse -ErrorAction SilentlyContinue
            }
        }
    }

    Context "Basic functional tests" {
        BeforeAll {
            New-Item $TestDrive$($DS)SourceDir -Type Directory | Out-Null
            New-Item $TestDrive$($DS)SourceDir$($DS)ChildDir-1 -Type Directory | Out-Null
            New-Item $TestDrive$($DS)SourceDir$($DS)ChildDir-2 -Type Directory | Out-Null
            New-Item $TestDrive$($DS)SourceDir$($DS)ChildEmptyDir -Type Directory | Out-Null

            # create an empty directory
            New-Item $TestDrive$($DS)EmptyDir -Type Directory | Out-Null
    
            $content = "Some Data"
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)Sample-1.txt
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)Sample-2.txt
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)ChildDir-1$($DS)Sample-3.txt
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)ChildDir-1$($DS)Sample-4.txt
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)ChildDir-2$($DS)Sample-5.txt
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)ChildDir-2$($DS)Sample-6.txt
    
            "Some Text" > $TestDrive$($DS)Sample.unzip
            "Some Text" > $TestDrive$($DS)Sample.cab
    
            #$preCreatedArchivePath = Join-Path $PSScriptRoot "SamplePreCreatedArchive.archive"
            #Copy-Item $preCreatedArchivePath $TestDrive$($DS)SamplePreCreatedArchive.zip -Force
    
            #$preCreatedArchivePath = Join-Path $PSScriptRoot "TrailingSpacer.archive"
            #Copy-Item $preCreatedArchivePath $TestDrive$($DS)TrailingSpacer.zip -Force
        }


        It "Validate that a single file can be compressed using Compress-Archive cmdlet" {
            $sourcePath = "$TestDrive$($DS)SourceDir$($DS)ChildDir-1$($DS)Sample-3.txt"
            $destinationPath = "$TestDrive$($DS)SampleSingleFile.zip"
            Compress-Archive -Path $sourcePath -DestinationPath $destinationPath
            $destinationPath | Should -Exist
        }

        It "Validate that an empty folder can be compressed" -Tag "this" {
            $sourcePath = "$TestDrive$($DS)EmptyDir"
            $destinationPath = "$TestDrive$($DS)EmptyDir.zip"
            Compress-Archive -Path $sourcePath -DestinationPath $destinationPath
            Test-Archive $destinationPath "EmptyDir/"
        }
    }

    Context "Update tests" {
        BeforeAll {
            New-Item $TestDrive$($DS)SourceDir -Type Directory | Out-Null
            New-Item $TestDrive$($DS)SourceDir$($DS)ChildDir-1 -Type Directory | Out-Null
    
            $content = "Some Data"
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)Sample-1.txt
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)ChildDir-1$($DS)Sample-2.txt 
        }

        It "Validate error from Compress-Archive when archive file already exists and -Update and -Force parameters is not specified" {
            $sourcePath = "$TestDrive$($DS)SourceDir"
            $destinationPath = "$TestDrive$($DS)ValidateErrorWhenUpdateNotSpecified.zip"

            try
            {
                "Some Data" > $destinationPath
                Compress-Archive -Path $sourcePath -DestinationPath $destinationPath
                throw "Failed to validate that an archive file format $destinationPath already exists and -Update switch parameter is not specified while running Compress-Archive command."
            }
            catch
            {
                $_.FullyQualifiedErrorId | Should -Be "ArchiveFileExists,Microsoft.PowerShell.Archive.CompressArchiveCommand"
            }
        }
    }

    Context "Relative Path tests" {
        BeforeAll {
            New-Item $TestDrive$($DS)SourceDir -Type Directory | Out-Null
            New-Item $TestDrive$($DS)SourceDir$($DS)ChildDir-1 -Type Directory | Out-Null
    
            $content = "Some Data"
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)Sample-1.txt
            $content | Out-File -FilePath $TestDrive$($DS)SourceDir$($DS)ChildDir-1$($DS)Sample-2.txt
        }

        # From 568
        It "Validate that relative path can be specified as Path parameter of Compress-Archive cmdlet" {
            $sourcePath = ".$($DS)SourceDir"
            $destinationPath = "RelativePathForPathParameter.zip"
            try
            {
                Push-Location $TestDrive
                Compress-Archive -Path $sourcePath -DestinationPath $destinationPath
        	    Test-Path $destinationPath | Should Be $true
            }
            finally
            {
                Pop-Location
            }
        }

        # From 582
        It "Validate that relative path can be specified as LiteralPath parameter of Compress-Archive cmdlet" {
            $sourcePath = ".$($DS)SourceDir"
            $destinationPath = "RelativePathForLiteralPathParameter.zip"
            try
            {
                Push-Location $TestDrive
                Compress-Archive -LiteralPath $sourcePath -DestinationPath $destinationPath
        	    Test-Path $destinationPath | Should Be $true
            }
            finally
            {
                Pop-Location
            }
        }

        # From 596
        It "Validate that relative path can be specified as DestinationPath parameter of Compress-Archive cmdlet" {
            $sourcePath = "$TestDrive$($DS)SourceDir"
            $destinationPath = ".$($DS)RelativePathForDestinationPathParameter.zip"
            try
            {
                Push-Location $TestDrive
                Compress-Archive -Path $sourcePath -DestinationPath $destinationPath
        	    Test-Path $destinationPath | Should Be $true
            }
            finally
            {
                Pop-Location
            }
        }
    }
}