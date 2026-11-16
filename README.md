Jibu
====

Jibu - Software library for parallel programming - CSP for .NET( C# ) - Communicating Sequential Processes (CSP)

**Archived, unmaintained. Kept for reference, not for production use.**

## What this is

Jibu 1.0 (RC3), a CSP library for .NET written in C# 2.0 (~5.5k lines of library code,
~3.2k lines of samples) by Axon7. It predates the TPL (2010), async/await (2012) and
`System.Threading.Channels` (2019): the scheduler, the channels and the choice operator are
all hand-written.

It provides:

* `Async` / `Future` tasks on a non-preemptive, work-stealing scheduler
* CSP channels — unbuffered rendezvous, FIFO / LIFO / infinite buffers — exposed as split
  `ChannelReader` / `ChannelWriter` ends, plus `Poison()` for shutting down a network of tasks
* `Choice`, i.e. guarded selection over channel readers, **channel writers** and timers:
  `PriSelect`, `FairSelect`, `TryPriSelect`, `TryFairSelect`
* Typed mailboxes with sender addresses (`Receive<T>`, `ReceiveFrom<T>`)
* `Parallel.For` / `Foreach` / `Reduce` / `Gather` / `Run`, `Barrier`, `Timer`, `Tuple<...>`
* 8 sample programs (Taxi dispatch, Sleeping barber, N-body, Mandelbrot, Convex hull,
  Game of life, Image pipeline, Monte-Carlo Pi) and a 255-page generated reference
  (including 76 runnable examples).

There is no test project.

## Provenance

* © 2008 Axon7, a venture spun out of the University of Copenhagen in 2007; per the
  cspjapan.org page, the library was first developed at the university. The product line
  also included Jibu for Java, C++ and Delphi. White paper, March 2008:
  `http://www.axon7.com/files/1/jibu_white_paper.pdf` (dead link — see web.archive.org).
* Sold commercially (EULA, trial builds) and released as open source under the GPLv2 in
  May 2008.
* Distributed as `jibu_net_1.0.0.zip` (JibuSetup.msi + Documentation + Samples).
* Vendor traces: `axon7.com/files/1/` (captures 2009-2012) and `cspjapan.org/JIBUNET/`
  (captures 2008-2016). Both sites are gone.
* Source snapshot declared as a copy of `jibu_net_1.0.0.zip` from
  `http://www.cspjapan.org/JIBUNET/JIBU1d0/`, assembled into a single solution on 2014-07-06
  (see git history).

## Versions

The only release line is **1.0.0**: the vendor's installers are all named `*_1.0.0.zip`,
`releasenotes.txt` stops at RC3, and `AssemblyInfo.cs` declares 1.0.0.0. Nothing newer shows
up in any of the traces below.

How to re-check, if the question comes up again:

1. Wayback CDX for the vendor download directory — every artifact is `*_1.0.0.zip`:
   `https://web.archive.org/cdx/search/cdx?url=axon7.com/files/1/*&output=text&fl=timestamp,original,statuscode&collapse=urlkey`
2. The Japanese redistribution page (2014) still points at `jibu_net_1.0.0.zip`:
   `https://web.archive.org/web/20140715204116/http://www.cspjapan.org/JIBUNET/main.htm`
3. No NuGet package (`azuresearch-usnc.nuget.org/query?q=jibu` returns an unrelated
   `Lib.Log_jibux`), no Maven artifact, and no other GitHub mirror surfaced
   (`geekcheng/jibu` and `gaixie/jibu` are unrelated Java projects).
4. Internal evidence: `releasenotes.txt` stops at RC3, `AssemblyVersion` is 1.0.0.0.

Do not be misled by the "Ver1.3" banner on the archived cspjapan.org page (2011-10-21): it
announces a version of CSPJAPAN's own "network channel library", not of Jibu. The Jibu
download on the same page is still 1.0.0.

Note that the original zip is **not** in the Wayback Machine: the captures of
`axon7.com/files/1/jibu_net_1.0.0.zip` are parked-domain HTML pages, not the archive. The
vendor bundle and the binaries are therefore gone from the public web as far as I could
check; what I did find is this source tree, which Software Heritage has snapshotted (see
Durability). If you know of another copy — an old `JibuSetup.msi`, a mirror, the Java/C++/
Delphi ports — please correct this paragraph.

## State as verified on 2026-11-16

* The library compiles unchanged on .NET 10 (0 errors; only obsolete-API warnings for
  `Thread.VolatileRead/Write` and formatter-based serialization).
* `Parallel.For`, `Reduce`, unbuffered and buffered channels, `Choice`
  (reader / writer / timer, fair), mailboxes, poison and barriers all work.
* Stress test: 4 producers × 25 000 messages over 4 channels, one `FairSelect` consumer →
  100 000 messages received, none lost, no hang.
* Performance: with a coarse grain size it matches the TPL (61 ms vs 57 ms for the same
  workload); with `grainSize = 1` it is ~6× slower (405 ms vs 60 ms), due to ~1.5 µs of
  overhead per task and a scheduler that wakes up every 10 ms.

The original solution is Visual Studio 2013 / .NET Framework 2.0; 5 of the 8 samples are
WinForms and will not build outside Windows.

## What is still worth looking at

1. **`Choice`** — the piece with no equivalent in the modern BCL. There is still no
   first-class `select`; hand-rolling it with `WaitToReadAsync` + `WhenAny` is racy, because
   another consumer can take the message between the wake-up and the read. The
   enable / signal / reserve protocol in `Choice.cs`, `UnbufferedChannel.cs` and
   `BufferedChannel.cs` is the interesting part, along with the fairness algorithm.
2. **The "writer" alternative** — selecting on "a reader is waiting / the buffer is not
   full". It is what makes `Samples/Taxi/Dispatch.cs` and
   `Samples/Sleeping Barber/Program.cs` so short.
3. **`Poison`** — structured shutdown that propagates through channels, choices and blocked
   tasks.
4. **The mailbox** — typed receive with sender filtering (`MailBox.cs`), Erlang-style.
5. **`DequeFixed.cs`** — a compact Chase-Lev work-stealing deque with a tagged `top`
   (ABA protection). Good teaching material.
6. **The documentation** — 76 runnable examples embedded in the HTML. The `.cs` files they
   were generated from are not in this repository, so within this archive the HTML pages are
   the only copy of them.

## What not to use, and why

* **The scheduler** (`Scheduling.cs`, `Task.cs`): one OS thread per task, a manager thread
  polling every 10 ms, blocked-thread detection via `ThreadState.WaitSleepJoin`, thread count
  growing with the number of detected blocked tasks. Superseded by the ThreadPool.
* **Blocking channels**: every `Read`/`Write` parks an OS thread. No async, no
  `CancellationToken`, no `IAsyncEnumerable`.
* **Thread-id-based reservation** (`Choice.cs`, `UnbufferedChannel.cs`): the design assumes
  a task stays on one OS thread; `Task.Wait()` can run a task inline on the caller's thread,
  which can confuse reservations. Also, if a selected alternative is never consumed, the
  channel stays reserved and writers block forever.
* **No tests**: 5.5k lines of concurrent code with zero automated coverage.

If you need a `select` today, re-implement `Choice` on top of `Channel<T>`,
`WaitToReadAsync` / `WaitToWriteAsync` and async primitives (~400 lines). Take the design,
not the code.

## Durability

The repository is already archived by Software Heritage (full git visit, 2024-06-10):

* browse:
  <https://archive.softwareheritage.org/browse/origin/?origin_url=https://github.com/manuc66/Jibu>

To archive the current state again after changes (the parameters must go in the query
string; a form-encoded body is rejected):

```sh
curl -X POST "https://archive.softwareheritage.org/api/1/origin/save/?origin_url=https%3A%2F%2Fgithub.com%2Fmanuc66%2FJibu&visit_type=git"
```

It returns a save request id; `curl https://archive.softwareheritage.org/api/1/origin/save/<id>/`
reports its progress.

## License

GPLv2 — see `LICENSE` and the per-file headers. Note the licensing history: the product was
first sold under a commercial EULA before being open-sourced, and the GPLv2 alone makes this
code unusable in closed-source software. The ideas are reusable; the code is not.
