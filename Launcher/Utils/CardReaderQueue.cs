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
    public Action<String>? WriteMessage;
    public Action<CardReaderResponse>? Finish;
    public CancellationToken Cancel;
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
        _reader.ConnectReader();
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
                try
                {
                    if (job.WriteMessage != null)
                    {
                        job.WriteMessage("SCAN YOUR CARD");
                    }
                    CardReaderResponse resp = _reader.GetUUIDWithRepeatAndCancel(job.ID, job.Cancel);

                    string id = resp.ID;
                    if (job.Finish != null)
                    {
                        job.Finish(resp);
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

    public void AddJob(int ID, Action<String> message, Action<CardReaderResponse> finish, CancellationToken token)
    {
        // wrap job
        var wrap = new JobWrapper()
        {
            ID = ID,
            WriteMessage = message,
            Finish = finish,
            Cancel = token,
        };
        // Attempt to add to queue; block for up to 10ms.
        _Queue.TryAdd(wrap, 10);
    }
}
