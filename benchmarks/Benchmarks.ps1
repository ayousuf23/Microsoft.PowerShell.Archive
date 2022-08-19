New-Item CompressArchiveBenchmarks -ItemType Directory

$timesToRunEach = 1

# Compress a single small file benchmarks
New-Item CompressArchiveBenchmarks/file.txt -ItemType File
"Hello, World!" | Out-File CompressArchiveBenchmarks/file.txt
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive CompressArchiveBenchmarks/file.txt CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/file.txt} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c CompressArchiveBenchmarks/file.txt -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive CompressArchiveBenchmarks/file.txt CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench1 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# Compress an image benchmarks
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive cat.jpg CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip cat.jpg} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c cat.jpg -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive cat.jpg CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench2 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# Compress a directory structure containing multiple items
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive PowerShell CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip PowerShell} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c PowerShell -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive PowerShell CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench3 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# Compress a windows iso ~5GB file benchmarks
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive windows.iso CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip windows.iso} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c windows.iso -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive windows.iso CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench3 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# Expand a small archive benchmarks
7z a CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/file.txt
tar -c CompressArchiveBenchmarks/file.txt -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench4
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench4} -AfterEach {rm CompressArchiveBenchmarks/bench4/*} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x archive.zip -oCompressArchiveBenchmarks/bench4/} -AfterEach {rm CompressArchiveBenchmarks/bench4/*} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench4} -AfterEach {rm CompressArchiveBenchmarks/bench4/*} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench4} -AfterEach {rm CompressArchiveBenchmarks/bench4/*} -Titles "v2 Tar"
$env:bench4 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar


# Expand an archive containing an image benchmarks
7z a CompressArchiveBenchmarks/archive.zip cat.jpg
tar -c cat.jpg -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench5
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/*} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x archive.zip -oCompressArchiveBenchmarks/bench5/} -AfterEach {rm CompressArchiveBenchmarks/bench5/*} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/*} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/*} -Titles "v2 Tar"
$env:bench5 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# Expand an archive containing a directory structure benchmarks
7z a CompressArchiveBenchmarks/archive.zip PowerShell
tar -c PowerShell -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench6
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/*} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x archive.zip -oCompressArchiveBenchmarks/bench6/} -AfterEach {rm CompressArchiveBenchmarks/bench6/*} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/*} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/*} -Titles "v2 Tar"
$env:bench6 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar)
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# Expand windows iso archive benchmakrs
7z a CompressArchiveBenchmarks/archive.zip windows.iso
tar -c windows.iso -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench7
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/*} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x archive.zip -oCompressArchiveBenchmarks/bench7/} -AfterEach {rm CompressArchiveBenchmarks/bench7/*} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/*} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/*} -Titles "v2 Tar"
$env:bench7 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

#pwsh-preview -NoExit -Command {Import-Module C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1; Invoke-Expression "C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1"}

Remove-Item .\CompressArchiveBenchmarks -Recurse

#| Select-Object Title, Average, Total, Minimum, Maximum | Format-Table