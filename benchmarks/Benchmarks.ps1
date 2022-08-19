New-Item CompressArchiveBenchmarks -ItemType Directory

$timesToRunEach = 1000

# Compress a single small file benchmarks
New-Item CompressArchiveBenchmarks/file.txt -ItemType File
"Hello, World!" | Out-File CompressArchiveBenchmarks/file.txt
$outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive CompressArchiveBenchmarks/file.txt CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/file.txt} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c CompressArchiveBenchmarks/file.txt -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive CompressArchiveBenchmarks/file.txt CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
Remove-Item CompressArchiveBenchmarks/file.txt
@($outputv2, $output7z, $outputTar) | Select-Object Title, Average, Total, Minimum, Maximum | Format-Table

# Compress an image benchmarks

# Compress a directory structure containing multiple items

# Compress a 2GB file benchmarks

# Compress a 7-9GB file benchmarks

# Expand a small archive benchmarks

# Expand an archive containing an image benchmarks

# Expand an archive containing a directory structure benchmarks

# Expand a 2GB file benchmarks

# Expand a 7-9GB file benchmarks

#pwsh-preview -NoExit -Command {Import-Module C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1; Invoke-Expression "C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1"}

Remove-Item .\CompressArchiveBenchmarks -Recurse