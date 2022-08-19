New-Item CompressArchiveBenchmarks -ItemType Directory

$timesToRunEach = 1

New-Item CompressArchiveBenchmarks/file.txt -ItemType File
"Hello, World!" | Out-File CompressArchiveBenchmarks/file.txt

# Expand a small archive benchmarks
7z a CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/file.txt
tar -c CompressArchiveBenchmarks/file.txt -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench5
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x archive.zip -oCompressArchiveBenchmarks/bench5/} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "v2 Tar"
$env:bench5 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar




#pwsh-preview -NoExit -Command {Import-Module C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1; Invoke-Expression "C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1"}

Remove-Item .\CompressArchiveBenchmarks -Recurse

#| Select-Object Title, Average, Total, Minimum, Maximum | Format-Table