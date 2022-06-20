using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.PowerShell.Archive
{
internal static class ErrorMessages
{
        public static string PathNotFoundError = "The path '{0}' either does not exist or is not a valid file system path.";

        static string ExpandArchiveInValidDestinationPath = "The path '{0}' is not a valid file system directory path.";

        static string InvalidZipFileExtensionError = "{0} is not a supported archive file format. {1} is the only supported archive file format.";
        
        static string ArchiveFileIsReadOnly = "The attributes of the archive file { 0} is set to 'ReadOnly' hence it cannot be updated.If you intend to update the existing archive file, remove the 'ReadOnly' attribute on the archive file else use -Force parameter to override and create a new archive file.";

        public static string ZipFileExistError = "The archive file {0} already exists.Use the -Update parameter to update the existing archive file or use the -Force parameter to overwrite the existing archive file.";

        public static string DuplicatePathFoundError = "The input to {0} parameter contains duplicate path(s) '{1}'. Provide a unique set of paths as input to the parameter.";

        static string ArchiveFileIsEmpty = "The archive file {0} is empty.";

        static string BadArchiveEntry = "Can not process invalid archive entry '{0}'.";

        static string InvalidArchiveFilePathError = @"The archive file path '{0}' specified as input to the { 1}\r\n
                                                      parameter is resolving to multiple file system paths.Provide a unique path to the { 2}\r\n
                                                      parameter where the archive file has to be created.";


        static string InvalidExpandedDirPathError = @" The directory path '{0}' specified as input to the DestinationPath parameter 
                                                        is resolving to multiple file system paths.Provide a unique path to the Destination parameter 
                                                        where the archive file contents have to be expanded.";


        static string FileExistsError = @"Failed to create file '{0}' while expanding the archive file '{1}' contents as the file '{2}' already exists. 
                                            Use the -Force parameter if you want to overwrite the existing directory '{3}' contents when expanding the archive file.";


        static string DeleteArchiveFile = "The partially created archive file '{0}' is deleted as it is not usable.";

        static string InvalidDestinationPath = "The destination path '{0}' does not contain a valid archive file name.";

        static string PreparingToCompressVerboseMessage = "Preparing to compress...";

        static string PreparingToExpandVerboseMessage = "Preparing to expand...";

        static string ItemDoesNotAppearToBeAValidZipArchive = "File '{0}' does not appear to be a valid zip archive.";
   

    
    }
}
