using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient.Browser;

namespace WinformDesktopApp
{
    public class SystemBrowser : IBrowser
    {
        private readonly int _port;

        public SystemBrowser(int port) => _port = port;

        public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
        {
            var prefix = $"http://127.0.0.1:{_port}/";
            using var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            try
            {
                Process.Start(new ProcessStartInfo { FileName = options.StartUrl, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                listener.Stop();
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
            }

            Task<HttpListenerContext> contextTask;
            try
            {
                contextTask = listener.GetContextAsync();
            }
            catch (Exception ex)
            {
                listener.Stop();
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = ex.Message };
            }

            using (cancellationToken.Register(() =>
            {
                try { listener.Stop(); } catch { }
            }))
            {
                HttpListenerContext context;
                try
                {
                    context = await contextTask.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = ex.Message };
                }

                var response = context.Response;
                var responseString = "<html><body>Authentification OK. Vous pouvez fermer cette fenêtre.</body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "text/html";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                response.OutputStream.Close();

                var raw = context.Request.Url.ToString();
                listener.Stop();
                return new BrowserResult { Response = raw, ResultType = BrowserResultType.Success };
            }
        }
    }
}