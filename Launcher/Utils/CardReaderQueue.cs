using Launcher.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Launcher.Utils;
class JobWrapper
{
    public int ID;
    public Action<String>? WriteMessageCallback;
    public Action<CardReaderResponse, CancellationTokenSource>? FinishCallback;
    public Action? InputLockCallback;
    public CancellationTokenSource Cancel;
}

public class CardQueue
{
    private BlockingCollection<JobWrapper> _Queue = new BlockingCollection<JobWrapper>(new ConcurrentQueue<JobWrapper>());

    private Thread _thread;
    private CancellationTokenSource _cancelWork = new CancellationTokenSource();

    private CardReader _reader;

    ~CardQueue()
    {
        this.Kill();
    }

    public CardQueue(ulong timeout)
    {
        _reader = new CardReader();
        if (!_reader.Setup() || !_reader.ConnectReader(""))
        {
            throw new InvalidOperationException("Card reader could not be established.");
        }
        _reader.SetTimeout(timeout);
        _thread = new Thread(this.work);
        _thread.Start();
    }

    public void Kill()
    {
        if (_thread != null)
        {
            _cancelWork.Cancel();
            _thread.Interrupt();
            _thread.Join();
            _thread = null;
        }
    }

    private void work()
    {
        try
        {
            foreach (JobWrapper job in _Queue.GetConsumingEnumerable(_cancelWork.Token))
            {
                // Check if job is cancelled before starting any work.
                if (job.Cancel.IsCancellationRequested)
                {
                    if (job.FinishCallback != null)
                    {
                        job.FinishCallback(new CardReaderResponse(), job.Cancel);
                    }
                    continue;
                }

                // Process job.
                try
                {
                    // Signal to user that they can scan a card, and start scanning for cards.
                    if (job.WriteMessageCallback != null)
                    {
                        job.WriteMessageCallback("SCAN YOUR CARD");
                    }
                    CardReaderResponse resp = _reader.GetUUIDWithRepeatAndCancel(job.ID, job.Cancel.Token);

                    // Signal to the requesting thread that it's no longer possible to cancel.
                    if (job.InputLockCallback != null)
                    {
                        job.InputLockCallback();
                    }

                    // One final check for cancellation. If there is no cancel, then proceed to process response.
                    if (!job.Cancel.IsCancellationRequested)
                    {
                        string id = resp.ID;
                    }

                    // Pass results back to the finishing callback.
                    if (job.FinishCallback != null)
                    {
                        job.FinishCallback(resp, job.Cancel);
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    Console.WriteLine("interrupt detected, re-raising exception...");
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                    throw;
                }
                catch (Exception e)
                {
                    Console.WriteLine("error, failed to read card: " + e.ToString());
                }
                // Give users 1 second after their scan before the next one begins.
                Thread.Sleep(1000);
            }
            foreach (JobWrapper job in _Queue)
            {
                Console.WriteLine("Cardreader process terminating, skipping remaining request from user: " + job.ID);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("cardreader thread interrupted: " + ex.ToString());
        }
    }

    public void AddJob(int ID, Action<String> message, Action<CardReaderResponse, CancellationTokenSource> finish, Action inputLock, CancellationTokenSource token)
    {
        // wrap job
        var wrap = new JobWrapper()
        {
            ID = ID,
            WriteMessageCallback = message,
            FinishCallback = finish,
            InputLockCallback = inputLock,
            Cancel = token,
        };
        // Attempt to add to queue; block for up to 10ms.
        _Queue.TryAdd(wrap, 10);
    }
}
