New-Item CompressArchiveBenchmarks -ItemType Directory

# Compress a single small file benchmarks
New-Item CompressArchiveBenchmarks/file.txt -ItemType File
"Hello, World!" | Out-File CompressArchiveBenchmarks/file.txt
$output = Measure-These -Count 2 -ToMeasure {Compress-Archive CompressArchiveBenchmarks/file.txt CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
Write-Output $output.Count
Remove-Item CompressArchiveBenchmarks/file.txt
$output | Select-Object Title, Average, Total, Minimum, Maximum | Format-Table

# Compress an image benchmarks

# Compress a directory structure containing multiple items

# Compress a 2GB file benchmarks

# Compress a 7-9GB file benchmarks

# Update a small archive w/directory structure containing multiple items benchmarks

# Update an archive containing a 2GB file

# Expand a small archive benchmarks

# Expand an archive containing an image benchmarks

# Expand an archive containing a directory structure benchmarks

# Expand a 2GB file benchmarks

# Expand a 7-9GB file benchmarks

#pwsh-preview -NoExit -Command {Import-Module C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1; Invoke-Expression "C:\Users\t-ayousuf\Code\Microsoft.PowerShell.Archive-vnext\benchmarks\Benchmarks.ps1"}

Remove-Item .\CompressArchiveBenchmarks -Recurse