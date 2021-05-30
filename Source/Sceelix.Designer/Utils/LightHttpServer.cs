// Modified from: https://gist.github.com/aksakalli/9191056

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace Sceelix.Designer.Utils
{
    public class LightHttpServer
    {
        private static readonly string[] indexFiles =
        {
            "index.html",
            "index.htm",
            "default.html",
            "default.htm"
        };

        private static readonly Dictionary<string, string> mimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {".asf", "video/x-ms-asf"},
            {".asx", "video/x-ms-asf"},
            {".avi", "video/x-msvideo"},
            {".bin", "application/octet-stream"},
            {".cco", "application/x-cocoa"},
            {".crt", "application/x-x509-ca-cert"},
            {".css", "text/css"},
            {".deb", "application/octet-stream"},
            {".der", "application/x-x509-ca-cert"},
            {".dll", "application/octet-stream"},
            {".dmg", "application/octet-stream"},
            {".ear", "application/java-archive"},
            {".eot", "application/octet-stream"},
            {".exe", "application/octet-stream"},
            {".flv", "video/x-flv"},
            {".gif", "image/gif"},
            {".hqx", "application/mac-binhex40"},
            {".htc", "text/x-component"},
            {".htm", "text/html"},
            {".html", "text/html"},
            {".ico", "image/x-icon"},
            {".img", "application/octet-stream"},
            {".iso", "application/octet-stream"},
            {".jar", "application/java-archive"},
            {".jardiff", "application/x-java-archive-diff"},
            {".jng", "image/x-jng"},
            {".jnlp", "application/x-java-jnlp-file"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".js", "application/x-javascript"},
            {".json", "application/json"},
            {".mml", "text/mathml"},
            {".mng", "video/x-mng"},
            {".mov", "video/quicktime"},
            {".mp3", "audio/mpeg"},
            {".mpeg", "video/mpeg"},
            {".mpg", "video/mpeg"},
            {".msi", "application/octet-stream"},
            {".msm", "application/octet-stream"},
            {".msp", "application/octet-stream"},
            {".pdb", "application/x-pilot"},
            {".pdf", "application/pdf"},
            {".pem", "application/x-x509-ca-cert"},
            {".pl", "application/x-perl"},
            {".pm", "application/x-perl"},
            {".png", "image/png"},
            {".prc", "application/x-pilot"},
            {".ra", "audio/x-realaudio"},
            {".rar", "application/x-rar-compressed"},
            {".rpm", "application/x-redhat-package-manager"},
            {".rss", "text/xml"},
            {".run", "application/x-makeself"},
            {".sea", "application/x-sea"},
            {".shtml", "text/html"},
            {".sit", "application/x-stuffit"},
            {".swf", "application/x-shockwave-flash"},
            {".tcl", "application/x-tcl"},
            {".tk", "application/x-tcl"},
            {".txt", "text/plain"},
            {".war", "application/java-archive"},
            {".wbmp", "image/vnd.wap.wbmp"},
            {".wmv", "video/x-ms-wmv"},
            {".xml", "text/xml"},
            {".xpi", "application/x-xpinstall"},
            {".zip", "application/zip"},
            {".map", "application/json"}
        };

        private Thread thread;
        private volatile bool threadActive;

        private HttpListener listener;
        private string path;
        private string ip;
        private int port;

        public LightHttpServer(string path, string ip, int port)
        {
            this.path = path;
            this.ip = ip;
            this.port = port;
        }

        public void Start()
        {
            if (thread != null) throw new Exception("WebServer already active. (Call stop first)");
            thread = new Thread(Listen) {IsBackground = true};
            thread.Start();
        }

        public void Stop()
        {
            // stop thread and listener
            threadActive = false;
            if (listener != null && listener.IsListening) listener.Stop();

            // wait for thread to finish
            if (thread != null)
            {
                thread.Join();
                thread = null;
            }

            // finish closing listener
            if (listener != null)
            {
                listener.Close();
                listener = null;
            }
        }

        private void Listen()
        {
            threadActive = true;

            // start listener
            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://{0}:{1}/", ip, port));
                listener.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                threadActive = false;
                return;
            }

            // wait for requests
            while (threadActive)
            {
                try
                {
                    var context = listener.GetContext();
                    if (!threadActive) break;
                    ProcessContext(context);
                }
                catch (HttpListenerException e)
                {
                    if (e.ErrorCode != 995) Console.WriteLine("ERROR: " + e.Message);
                    threadActive = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                    threadActive = false;
                }
            }
        }

        private void ProcessContext(HttpListenerContext context)
        {
            // get filename path
            string filename = context.Request.Url.AbsolutePath;
            if (filename != null) filename = filename.Substring(1);

            // get default index file if needed
            if (string.IsNullOrEmpty(filename))
            {
                foreach (string indexFile in indexFiles)
                {
                    if (File.Exists(Path.Combine(path, indexFile)))
                    {
                        filename = indexFile;
                        break;
                    }
                }
            }

            //Console.WriteLine("Loading file: " + filename);
            filename = Path.Combine(path, filename);

            if (!File.Exists(filename))
            {
                DesignerProgram.Log.Error("File not found: " + filename);

                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else
            {
                //this is strange, but in Mono there is the complaint that the headers are sent if the status code is indicated after the outputstream code.
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(filename).ToString("r"));

                try
                {
                    using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // get mime type
                        context.Response.ContentType = mimeTypes[Path.GetExtension(filename)];
                        context.Response.ContentLength64 = stream.Length;

                        // copy file stream to response
                        stream.CopyTo(context.Response.OutputStream);
                        stream.Flush();
                        context.Response.OutputStream.Flush();
                    }
                }
                catch (Exception e)
                {
                    DesignerProgram.Log.Error(e);
                    
                    //this is probably not working in Mono/Mac/Linux
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
                
            }

            context.Response.OutputStream.Close();
        }
    }
}