# Comparison of PS.Archive v1.2.0 to tar

## Create Archive

### Basics

Create an archive for single file `tar --create Test/test.txt -f archive.tar`

Create an archive for directory `tar --create Test -f archive`

Crete an archive with globbing `tar --create Test/* -f archive`

### File structure consistency

`tar --create Test/test.txt -f archive.tar` will create an archive while maintaining the relative file structure. The archive's structure will look like:

Test
|---test.txt

. in path:
`tar --create ./Test/test.txt -f archive.tar` will create an archive while maintaining the relative file structure. The archive's structure will look like:

Test
|---test.txt

Actual file entry:
./Test/test.txt


.. in  path:
`tar --create ../build/Test/test.txt -f archive.tar` will create an archive while maintaining the relative file structure. The archive's structure starts from the top-most directory in the relative path (after processing ..):

build
|---Test
    |---test.txt

~ in path:
`tar --create ../build/Test/test.txt -f archive.tar` will preserve the name of the home directory:

home/t-ayousuf/Test/test.txt

* in path:
`tar --create Test/* -f archive.tar` will create an archive like:

Test/hi.text
Test/test.txt

absolute path:
`tar --create /home/t-ayousuf/Test/test.txt -f archive.tar` will create an archive like:

home/t-ayousuf/Test/test.txt

## Reveal archive contents

Reveal archive contents `tar --list -f archive`

This functionationality is not included in PowerShell.Archive

## Extract Archive

Extract an archive `tar --extract -f Playground/archive -C ./Playground`

Notes:
- I thought the syntax was a little unintuitive because the `-f` parameter refers to destination path when creating an archive while referring to source path when extracting an archive.
- Extraction behavior is unintuitive. When extracting the archive, the Test folder was created even though I only compressed the entries inside that folder. 

By comparison, `Expand-Archive Playground/archive.zip` will create a folder called archive with the archives contents inside it. `Expand-Archive Playground/archive.zip .` will not create a folder and will merely put the contents of the archive inside the current folder. The different is `tar` included the root folder while `Compress-Archive` did not.

## File Override Behavior

Suppose you want to extract an archive to a destination where the files exist.

`Expand-Archive` will let you know the files already exist and throw an error. 

`Tar` will NOT let you know the files already exist. `Tar` overrides pre-existing files (even if they are newer or have different contents)

## Symlink Behavior