using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Threading.Tasks;
using System.Threading;
using NLog;


namespace TrolleyTracker.Controllers
{

    /// <summary>
    /// Get trolley locations and other info from Greenlink Syncromatics API and make available to our clients
    /// </summary>
    public class PollTrolleysTask : IRegisteredObject
    {

        private volatile bool _shuttingDown;
        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;

        private PollTrolleysHandler pollTrolleyProcess;
        private DateTime lastExceptionLogged = DateTime.Now.AddMinutes(-60);  // So first excception will be logged
        private const int MinExceptionInterval = 5; // In minutes

        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PollTrolleysTask()
        {
            // Register this job with the hosting environment.
            // Allows for a more graceful stop of the job, in the case of IIS shutting down.
            HostingEnvironment.RegisterObject(this);

            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            

            // Start task running; discard task result
            var pollTask = Execute();

        }
        public async Task Execute()
        {
            try
            {
                pollTrolleyProcess = new PollTrolleysHandler(cancellationToken);

                while (true)
                {
                    await Task.Delay(6000, cancellationToken);
                    if (_shuttingDown)
                        return;
                    cancellationToken.ThrowIfCancellationRequested();


                    try
                    {
                        await pollTrolleyProcess.UpdateTrolleys();
                    }
                    catch (TaskCanceledException)
                    {
                        throw;  // Normal IIS shutdown request
                    }
                    catch (GreenlinkTracker.Syncromatics.SyncromaticsException ex)
                    {
                        // Rate limit logging to avoid filling exception log
                        if ((DateTime.Now - lastExceptionLogged).TotalMinutes > MinExceptionInterval)
                        {
                            logger.Info(ex.Message);
                            lastExceptionLogged = DateTime.Now;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Rate limit logging to avoid filling exception log
                        if ((DateTime.Now - lastExceptionLogged).TotalMinutes > MinExceptionInterval)
                        {
                            logger.Error(ex, "Problem polling trolley locations");
                            lastExceptionLogged = DateTime.Now;
                        }
                    }

                    if (_shuttingDown)
                        return;
                    cancellationToken.ThrowIfCancellationRequested();
                }
            } catch (Exception ex)
            {
                // It is normal to come here with a TaskCanceledException when IIS shuts down
                if (!_shuttingDown)
                {
                    // Something unexpected
                    logger.Error(ex, "Fatal Exception in PollTrolleys Task");
                }

            }
            finally
            {
                // Always unregister the job when done.
                HostingEnvironment.UnregisterObject(this);
            }
            //Debug.WriteLine("PollTrolleys Task Exited");
        }


        public void Stop(bool immediate)
        {

            if (!immediate)
            {
                // First pass
                _shuttingDown = true;
                cancellationTokenSource.Cancel();

            } else {
                // Second pass
                // HostingEnvironment.UnregisterObject(this);  // Should be de-registered already, otherwise it's force stopping anyway
            }
        }
    }

}