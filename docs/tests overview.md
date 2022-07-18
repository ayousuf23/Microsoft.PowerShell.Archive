# PSArchive Tests Overview

## Compress-Archive Tests

### Parameter set validation tests

- Validate errors from empty or null `-Path` and `-DestinationPath` parameters

- Validate errors from invalid `-Path`

- **[NEEDED]** Validate errors from invalid `-LiteralPath`

-  Validate error when archive file already exists and `-Update` or `-Force` is not specified

- Validate error when `-DestinationPath` resolves to multiple locations

- Validate source path can be at SystenDrive location

#### Duplicate paths

- Validate error when `-Path` contains duplicate paths

- Validate error when `-LiteralPath` contains duplicate paths

#### Relative paths

- Validate `-Path` can be a relative path

- Validate `-LiteralPath` can be a relative path

- Validate `-DestinatonPath` can be a relative path

#### Special and Wildcard Characters

- Validate compression works when `-DestinationPath` has wildcard patterns and resolves to a single path

- Validate `-LiteralPath` with special characters

- Validate `-DestinationPath` with special characters

- Validate `-LiteralPath` for a directory with special characters in the directory's name

### Basic functional tests

- Validate a single file can be compressed

- Validate multiple files can be compressed

- Validate multiple files and non-empty directories can be compressed

- Validate a single non-empty directory can be compressed

- Validate a single directory containing multiple files and subdirectories can be compressed

- Validate a single non-empty directory and multiple files can be compressed

- **[NEEDED]** Validate a single empty directory can be compressed

- **[NEEDED]** Validate a zero-byte file can be compressed

- Validate compression works when the last write time of a file is <1980

- Validate a warning is given when compressing files with last write time <1980


### `-PassThru` paramemter

- Validate nothing is returned when `-PassThru` is not specified
- Validate nothing is returned when `-PassThru` is `false`
- Validate archive is returned when `-PassThru` is specified

### `-Verbose` parameter

- Validate verbose messages are generated when `-Verbose` is specified

### `-Update` parameter

- Validate `-Update` does not throw an error if the archive already exists

- Validate `-Update` by adding a new file to an existing archive

- Validate `-Update` modifies a pre-existing file in an archive if a file with the same path is specified as a source file

- **[NEEDED]** Validate `-Update` nothing changes when an empty directory is supplied as a source item and a directory with the same name already exists

- **[NEEDED]** Validate `-Update` when a non-empty directory is supplied as a source item and a directory with the same base path already exists in the archive, only new items are added to the directory and pre-existing items are updated if possible


Notes: 
- Ensure pre-existing files and directories are not replaced when updating an archive

### `-CompressionLevel` parameter

- Validate all `-CompressionLevel` values can be used
    - Note: Need to check invalid values as well

- **[NEEDED]** Validate error when invalid value is supplied to `-CompressionLevel`

### `-Force` parameter

- **[NEEDED]** Validate `-Force` recreates archive when it already exists
    - Ensure all previous contents of the archive are not retained unless passed

### `-WhatIf` parameter

- **[NEEDED]** Validate destination file is not deleted when `-Force` is specified in addition to `-WhatIf`

### `-Confirm` parameter

### File extension

- Validate if `.zip` extension is not supplied in `DestinationPath`, a warning is reported
    - Note: This does not necessarily occur if the file has another extension

- Validate a zip archive can be created with an extension other than `.zip`

### File permissions, locked files, hidden files, symbolic links, etc.

- Validate ReadOnly files can be compressed

- Validate an archive can be created when the source file is in use

- **[NEEDED]** Validate file permissions are maintained in an archive

- **[NEEDED]** Validate Compress-Archive works on hidden files

- **[NEEDED]** Validate Compress-Archive works on hidden directories

- **[NEEDED]** Validate Compress-Archive works on non-hidden directory containing hidden files and subdirectories

- **[NEEDED]** Validate Compress-Archive works on files that are symbolic links

- **[NEEDED]** Validate Compress-Archive works on directories that are symbolic links

- **[NEEDED]** Validate Compress-Archive works on directories containing symlink files and symlink subdirectories

- **[NEEDED]** Validate source items can be opened for reading by other processes when in use by cmdlet

- **[NEEDED]** Validate source items cannot be opened for writing by other processes when in use by cmdlet

- **[NEEDED]** Validate compression on case-sensative filesystems with paths that are the same except for casing

- **[NEEDED]** Validate expansion on with source path and destination path containing non-latin characters

- **[NEEDED]** Validate compression on with filenames containing non-latin characters

### Pipelining

- Validate pipelining to specify source paths

- **[NEEDED]** Validate pipelining by name to `-LiteralPath` works

- **[NEEDED]** Validate error when pipelining by name to `-DestinationPath`

- **[NEEDED]** Validate error when pipelining by name to `-CompressionLevel`

- **[NEEDED]** Validate error when pipelining by name to `-Update`

- **[NEEDED]** Validate error when pipelining by name to `-Force`

- **[NEEDED]** Validate what happens when pipelining by name to `-PassThru`

- **[NEEDED]** Validate what happens when pipelining by name to `-Verbose`

- **[NEEDED]** Validate what happens when pipelining by name to `-Confirm`

- **[NEEDED]** Validate what happens when pipelining by name to `-WhatIf`

### Archive entries

- Validate only `/` are used as archive directory seperators

### Job system tests

### Advanced functions tests

- **[NEEDED]** Validate Compress-Archive works inside a function

- **[NEEDED]** Validate ShouldProcess parameters are passed to cmdlet from an advanced function

### Large file tests

- **[NEEDED]** Validate a zip archive >4GB can be created

- **[NEEDED]** Validate a zip archive containing files >4GB can be created

### Compatability with other archive software tests

### Archive formats and `-Format` parameter

- **[NEEDED]** Validate error when an invalid archive format is supplied

### Custom PSDrive

- Validate that you can compress an archive to a custom PSDrive (842)

### Abrupt Command Stop tests

- **[NEEDED]** Validate that clean up is performed when the command is stopped abruptly

## Expand-Archive Tests

### Parameter set validation tests

- Validate error when `-Path` is a non-existing archive

- Validate error when `-Path`, `-LiteralPath`, or `-DestinationPath` are null or empty

- Validate error when an invalid path is supplied to `-Path` or `-LiteralPath` (821)

- Validate error when an invalid path is supplied to `-DestinationPath`

- Validate that Expand-Archive work with backslashes and forward slashes in paths (1198)

#### Duplicate paths

- Validate error when `-DestinationPath` resolves to multiple locations (880)

#### Relative paths

- Invoke Expand-Archive with relative path in Path parameter and -Force parameter (938)

- Invoke Expand-Archive with relative path in LiteralPath parameter and -Force parameter (955)

- Invoke Expand-Archive with non-existing relative directory in DestinationPath parameter and -Force parameter (972)

#### Special and Wildcard Characters

- Validate that Expand-Archive cmdlet works when `-DestinationPath` resolves has wild card pattern and resolves to a single valid path (900)

### Basic functional tests

- Validate expanding an archive (858)

- Validate expanding an archive containing multiple files, directories with subdirectories and empty directories (1007)

- Validate that Expand-Archive work with dates earlier than 1980 (1218)

### `-DestinationPath` parameter

- Validate that without `-DestinationPath` parameter Expand-Archive cmdlet succeeds in expanding the archive (1082)

- Validate that without `-DestinationPath` parameter Expand-Archive cmdlet succeeds in expanding the archive when destination directory exists (1101)

### `-PassThru` paramemter

- Validate that Expand-Archive returns nothing when `-PassThru` is not used (1121)

- Validate that Expand-Archive returns nothing when `-PassThru` is used with a value of $false (1136)

- Validate that Expand-Archive returns the contents of the archive when `-PassThru` is used (1150)

### `-Verbose` parameter

- Validate that Expand-Archive generates Verbose messages (1039)

### `-Force` parameter

- Validate that without -Force parameter Expand-Archive generates non-terminating errors without overwriting existing files (1061)

### `-WhatIf` parameter

### `-Confirm` parameter

### File extension

- Validate Expand-Archive works with zip files that have non-zip file extensions (1167)

### File permissions, locked files, hidden files, symbolic links, etc.

- **[NEEDED]** Validate file permissions in the archive are maintained after expanding the archive
    - Try this with different file permissions, e.g. executable

- **[NEEDED]** Validate Expand-Archive works on a read only archive

- **[NEEDED]** Validate Expand-Archive works on an archive in use

- **[NEEDED]** Validate archive file can be opened for reading by other processes when in use by cmdlet

- **[NEEDED]** Validate archive file cannot be opened for writing by other processes when in use by cmdlet

- **[NEEDED]** Validate error on compression on case-insensative filesystems with archive entries that are the same except for casing

- **[NEEDED]** Validate expansion on case-sensative filesystems with archive entries that are the same except for casing

- **[NEEDED]** Validate expansion on with source path and destination path containing non-latin characters

- **[NEEDED]** Validate expansion on with archive entries containing non-latin characters

### Password-protected archives

- **[NEEDED]** Validate error when a password-protected archive is passed to Expand-Archive

### Pipelining

- Validate pipeline scenario (1022)

### Archive entries

- Validate Expand-Archive works with zip files where the contents contain trailing whitespace (1183)

### Job system tests

### Advanced functions tests

- **[NEEDED]** Validate Compress-Archive works inside a function

- **[NEEDED]** Validate ShouldProcess parameters are passed to cmdlet from an advanced function

### Large file tests

### Compatability with other archive software tests

### Archive formats and `-Format` parameter 

- **[NEEDED]** Validate error when an archive in an unsupported format is supplied to Expand-Archive

- **[NEEDED]** Validate error when a non-archive file is supplied to Expand-Archive

### Abrupt Command Stop tests

- **[NEEDED]** Validate that clean up is performed when the command is stopped abruptly

## Module

- Validate module can be imported when current language is not en-US (1247)
