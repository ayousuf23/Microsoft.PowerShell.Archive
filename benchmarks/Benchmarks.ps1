New-Item CompressArchiveBenchmarks -ItemType Directory

$timesToRunEach = 1

New-Item CompressArchiveBenchmarks/file.txt -ItemType File
"Hello, World!" | Out-File CompressArchiveBenchmarks/file.txt

# Expand a small archive benchmarks
7z a CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/file.txt
tar -c CompressArchiveBenchmarks/file.txt -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench5
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench5/} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "v2 Tar"
$env:bench5 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# Expand an archive containing an image benchmarks
7z a CompressArchiveBenchmarks/archive.zip cat.jpg
tar -c cat.jpg -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench6
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench6/} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "v2 Tar"
$env:bench6 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# Expand an archive containing a directory structure benchmarks
7z a CompressArchiveBenchmarks/archive.zip PowerShell
tar -c PowerShell -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench7
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench7/} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "v2 Tar"
$env:bench7 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar)
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# Expand windows iso archive benchmakrs
7z a CompressArchiveBenchmarks/archive.zip windows.iso
tar -c windows.iso -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench8
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench8} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench8/} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench8} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench8} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "v2 Tar"
$env:bench8 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar




#pwsh-preview -NoExit -Command {Import-Module C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1; Invoke-Expression "C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1"}

Remove-Item .\CompressArchiveBenchmarks -Recurse

#| Select-Object Title, Average, Total, Minimum, Maximum | Format-Table