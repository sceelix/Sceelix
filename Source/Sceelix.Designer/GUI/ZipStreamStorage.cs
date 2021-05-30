using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DigitalRune.Storages;
using Ionic.Zip;

namespace Sceelix.Designer.GUI
{
    /// <summary>
    /// Provides access to the files stored in a ZIP archive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ZipStorage"/> does not directly read the ZIP archive from the OS file system.
    /// Instead, it opens the ZIP archive from another storage.
    /// </para>
    /// <para>
    /// The <see cref="PasswordCallback"/> needs to be set to read encrypted ZIP archives. The
    /// <see cref="ZipStorage"/> supports ZipCrypto (all platforms) and AES-256 encryption (Windows
    /// only).
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> The <see cref="ZipStorage"/> is thread-safe. ZIP entries can
    /// be read simultaneously from one or multiple threads.
    /// </para>
    /// </remarks>
    public class ZipStreamStorage : Storage
    {
        //--------------------------------------------------------------

        #region Fields

        //--------------------------------------------------------------

        private readonly object _lock = new object();
        //private readonly Stream _zipStream;
        private readonly ZipFile _zipFile;

#if ANDROID
// A list of all temp files created in this session.
    private static List<string> _tempFiles = new List<string>();
#endif

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override char DirectorySeparator
        {
            get { return '/'; }
        }



        /// <summary>
        /// Gets the file name (incl. path) of the ZIP archive.
        /// </summary>
        /// <value>The file name (incl. path) of the ZIP archive.</value>
        public string FileName
        {
            get;
            private set;
        }



        /// <summary>
        /// Gets the storage that provides the ZIP archive.
        /// </summary>
        /// <value>The storage that provides the ZIP archive.
        /// </value>
        public Storage Storage
        {
            get;
            private set;
        }



        /// <summary>
        /// Gets or sets the callback method that provides the password for encrypted ZIP file entries.
        /// </summary>
        /// <value>
        /// The callback method that provides the password for encrypted ZIP file entries.
        /// </value>
        /// <remarks>
        /// The callback is a function which takes one string argument and returns a string.
        /// The function argument is the path of the entry that should be retrieved from the ZIP
        /// archive. The function returns the password that was used to protect the entry in the ZIP
        /// archive. The method may return any value (including <see langword="null"/> or ""), if the
        /// ZIP entry is not encrypted.
        /// </remarks>
        public Func<string, string> PasswordCallback
        {
            get;
            set;
        }

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ZipStorage"/> class.
        /// </summary>
        /// <param name="storage">The storage that contains the ZIP archive.</param>
        /// <param name="fileName">The file name (incl. path) of the ZIP archive.</param>
        /// <remarks>
        /// An exception is raised if the ZIP archive could not be opened.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="storage"/> is null.
        /// </exception>
        public ZipStreamStorage(Storage storage, String baseName, Stream zipStream)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            Storage = storage;
            FileName = baseName;

            try
            {
                _zipFile = ZipFile.Read(zipStream);
                return;
            }
            catch
            {
                //_zipStream.Dispose();
                throw;
            }
        }



        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="ZipStorage"/> class
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    _zipFile.Dispose();
                    //_zipStream.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override string GetRealPath(string path)
        {
            path = NormalizePath(path);
            var zipEntry = _zipFile[path];
            if (zipEntry != null && !zipEntry.IsDirectory)
                return Storage.GetRealPath(FileName);

            return null;
        }



        /// <summary>
        /// Validates and normalizes the path of a file in a storage.
        /// </summary>
        /// <param name="path">The path the file.</param>
        /// <returns>The normalized path.</returns>
        internal static string NormalizePath(string path)
        {
            const string message = "Invalid path. The path needs to be specified relative to the root directory in canonical form.";

            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("Invalid path. Path must not be empty.", "path");

            // Paths with "pathA/pathB/../pathC" are not supported.
            if (path.Contains(".."))
                throw new ArgumentException(message, "path");

            // Switch to forward slashes '/'.
            path = SwitchDirectorySeparator(path, '/');

            // Reduce "./path" to "path".
            while (path.StartsWith("./", StringComparison.Ordinal))
                path = path.Substring(2);

            // Reduce "pathA/./pathB" to "pathA/pathB".
            path = path.Replace("/./", "/");

            if (path.Length == 0)
                throw new ArgumentException(message, "path");

            // Trim leading '/'. All mount points are relative to root directory!
            if (path[0] == '/')
                path = path.Substring(1);

            if (path.Length == 0)
                throw new ArgumentException(message, "path");

            // Absolute paths are not supported.
            if (System.IO.Path.IsPathRooted(path))
                throw new ArgumentException(message, "path");

            // Trim trailing '/'.
            while (path.Length > 0 && path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            if (path.Length == 0)
                throw new ArgumentException(message, "path");

            return path;
        }



        /// <summary>
        /// Switches the directory separator character in the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="directorySeparator">The desired directory separator character.</param>
        /// <returns>The path using only the specified directory separator.</returns>
        internal static string SwitchDirectorySeparator(string path, char directorySeparator)
        {
            switch (directorySeparator)
            {
                case '/':
                    path = path.Replace('\\', '/');
                    break;
                case '\\':
                    path = path.Replace('/', '\\');
                    break;
                default:
                    path = path.Replace('\\', directorySeparator);
                    path = path.Replace('/', directorySeparator);
                    break;
            }

            return path;
        }



        /// <inheritdoc/>
        //[DebuggerNonUserCode]
        public override Stream OpenFile(string path)
        {
            var stream = TryOpenFile(path);
            if (stream != null)
                return stream;

            throw new FileNotFoundException("The file was not found in the ZIP archive.", path);
        }



        /// <inheritdoc/>
        /*Stream IStorageInternal.TryOpenFile(string path)
        {
            return TryOpenFile(path);
        }*/
        /// <inheritdoc/>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing MemoryStream.")]
        private Stream TryOpenFile(string path)
        {
            // The ZIP file is loaded as a single stream (_zipStream). Streams are not
            // thread-safe and require locking.
            lock (_lock)
            {
                // The current thread may read multiple ZIP entries simultaneously.
                // Example: The ContentManager reads "Model.xnb". While the "Model.xnb"
                // is still open, the ContentManager starts to read "Texture.xnb".
                // --> ZIP entries need to be copied into a temporary memory stream.
                var zipEntry = _zipFile[path];
                if (zipEntry != null && !zipEntry.IsDirectory)
                {
                    string password = (PasswordCallback != null) ? PasswordCallback(path) : null;

                    // Extract ZIP entry to memory.
                    var uncompressedStream = new MemoryStream((int) zipEntry.UncompressedSize);
                    zipEntry.ExtractWithPassword(uncompressedStream, password);

                    // Reset memory stream for reading.
                    uncompressedStream.Position = 0;

                    return uncompressedStream;
                }

                return null;
            }
        }

        #endregion
    }
}