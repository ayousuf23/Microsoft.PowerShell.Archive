New-Item CompressArchiveBenchmarks -ItemType Directory

# How many times to run each benchmark
$timesToRunEach = 1000

# I assigned benchmark results to an env variable because I was debuggging this script.

# This benchmarks creates a text file and measures time taken to compress it
New-Item CompressArchiveBenchmarks/file.txt -ItemType File
"Hello, World!" | Out-File CompressArchiveBenchmarks/file.txt
# Each benchmark was written on its own line seperately due to bugs I was ironing out
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive CompressArchiveBenchmarks/file.txt CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/file.txt} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c CompressArchiveBenchmarks/file.txt -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive CompressArchiveBenchmarks/file.txt CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench1 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# This benchmarks measures time taken to compress an image (cat.jpg)
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive cat.jpg CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip cat.jpg} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c cat.jpg -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive cat.jpg CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench2 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# This benchmarks measures time taken to compress PowerShell repo
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive PowerShell CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip PowerShell} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c PowerShell -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive PowerShell CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench3 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# This benchmarks measures time taken to compress an windows iso (https://www.microsoft.com/en-us/software-download/windows11?ranMID=24542&ranEAID=kXQk6*ivFEQ&ranSiteID=kXQk6.ivFEQ-NH3PI00AodaCCh7KlBHGUg&epi=kXQk6.ivFEQ-NH3PI00AodaCCh7KlBHGUg&irgwc=1&irclickid=_z0keysh3wskf6nkp133xftfux32xrcvu0svqhe0l00)
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive windows.iso CompressArchiveBenchmarks/archive.zip} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z a CompressArchiveBenchmarks/archive.zip windows.iso} -AfterEach {rm CompressArchiveBenchmarks/archive.zip} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -c windows.iso -f CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Compress-Archive windows.iso CompressArchiveBenchmarks/archive.tar} -AfterEach {rm CompressArchiveBenchmarks/archive.tar} -Titles "v2 Tar"
$env:bench4 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 

# This benchmarks measures time taken to expand an archive containing a text file
# Create the zip archive to expand
7z a CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/file.txt
# Create the tar archive to expand
tar -c CompressArchiveBenchmarks/file.txt -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench5
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench5/} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench5} -AfterEach {rm CompressArchiveBenchmarks/bench5/* -r -f} -Titles "v2 Tar"
$env:bench5 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# This benchmarks measures time taken to expand an archive containing an image cat.jpg
# Create the zip archive to expand
7z a CompressArchiveBenchmarks/archive.zip cat.jpg
# Create the tar archive to expand
tar -c cat.jpg -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench6
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench6/} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench6} -AfterEach {rm CompressArchiveBenchmarks/bench6/* -r -f} -Titles "v2 Tar"
$env:bench6 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# This benchmarks measures time taken to expand an archive containing the PowerShell repo
# Create the zip archive to expand
7z a CompressArchiveBenchmarks/archive.zip PowerShell
# Create the tar archive to expand
tar -c PowerShell -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench7
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench7/} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench7} -AfterEach {rm CompressArchiveBenchmarks/bench7/* -r -f} -Titles "v2 Tar"
$env:bench7 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar)
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar

# This benchmarks measures time taken to expand an archive containing a windows iso (https://www.microsoft.com/en-us/software-download/windows11?ranMID=24542&ranEAID=kXQk6*ivFEQ&ranSiteID=kXQk6.ivFEQ-NH3PI00AodaCCh7KlBHGUg&epi=kXQk6.ivFEQ-NH3PI00AodaCCh7KlBHGUg&irgwc=1&irclickid=_z0keysh3wskf6nkp133xftfux32xrcvu0svqhe0l00)
# Create the zip archive to expand
7z a CompressArchiveBenchmarks/archive.zip windows.iso
# Create the tar archive to expand
tar -c windows.iso -f CompressArchiveBenchmarks/archive.tar
mkdir CompressArchiveBenchmarks/bench8
$env:outputv2 = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.zip CompressArchiveBenchmarks/bench8} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "v2"
$env:output7z = Measure-These -Count $timesToRunEach -ToMeasure {7z x CompressArchiveBenchmarks/archive.zip -oCompressArchiveBenchmarks/bench8/} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "7z"
$env:outputTar = Measure-These -Count $timesToRunEach -ToMeasure {tar -xf CompressArchiveBenchmarks/archive.tar -C CompressArchiveBenchmarks/bench8} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "Tar"
$env:outputv2Tar = Measure-These -Count $timesToRunEach -ToMeasure {Expand-Archive CompressArchiveBenchmarks/archive.tar CompressArchiveBenchmarks/bench8} -AfterEach {rm CompressArchiveBenchmarks/bench8/* -r -f} -Titles "v2 Tar"
$env:bench8 = @($env:outputv2, $env:output7z, $env:outputTar, $env:outputv2Tar) 
rm CompressArchiveBenchmarks/archive.zip
rm CompressArchiveBenchmarks/archive.tar