# Service monitor: logs column and a web row

The admin console's Services table shows the worker and the Discord bot. This adds a row for the web app itself, a
Logs column of Grafana links for every row, and then some stats for the web row.

Each step is built, tested, committed on its own, then reviewed before the next starts.

## Step 1: web row and Logs column

- `JobLogLinks` becomes `GrafanaLogLinks`, which builds both the per-job links it already does and per-service links:
  the last hour of one host's logs, newest first, filtered on its Loki `app` label and, for a worker flow, on the
  `Flow` label. The hosts' `app` labels and the worker's flow names are repeated in the web app, which references
  neither host; a unit test compares them with each host's shipped appsettings and `WorkerLogging.Flows`.
- `ServiceHealthViewModel` gains `LogLinks` (label and URL). Empty wherever job links are off (Development, or no URL).
  - Web: Logs
  - Worker: All, Job Runner, Scheduler, Canceller
  - Discord Bot: Logs
- `ServiceMonitorViewModel` gains `Web`, reported by the controller itself: always Healthy, since an unhealthy web app
  could not have answered. No details yet; steps 2 to 5 add them.
- `serviceMonitor.vue`: the web row first, and a Logs column of small buttons, like the jobs table's.
- Regenerate the API clients.

## Steps 2 to 5: web stats

One commit each, independent enough that any of them can be reverted on its own, so each can be judged on what it
costs against what it shows.

2. **Database round trip**: how long the monitor's read of the system-wide settings took, one small row.
3. **Live draft connections**: browsers connected to `UpdateHub` now, counted by a singleton the hub updates on
   connect and disconnect. The league page only connects while its draft is active.
4. **Memory**: the process's working set against the memory available to it (the container's limit where it has
   one), and the GC heap.
5. **Errors since start**: how many Error-or-worse events the process has logged, and when the last one was, counted
   by a small Serilog sink that both the bootstrap and the real logger write to.

## Status

All five steps are committed. Steps 2 to 5 are each waiting on a keep-or-revert decision.
