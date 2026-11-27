# UI layout

Fluent-style responsive Windows layout, light/dark/system modes, keyboard navigation, accessible contrast, compact/comfortable density. Sidebar groups and searchable commands; right-side inspector for evidence and actions.

```text
┌───────────────────────────────────────────────────────────────────┐
│ VELTRIVON     Search tools / commands          Jobs   Settings    │
├───────────────────┬───────────────────────────────────────────────┤
│ Dashboard         │ Workstation overview                          │
│ Smart Scan        │ [Run read-only scan] [View last report]       │
│                   │                                               │
│ SYSTEM            │ CPU       Memory      Disk       Security     │
│ Clean / Repair    │ live      live        free       status       │
│ Apps / Updates    │                                               │
│ Startup / Storage │ Findings                  Operation history   │
│ Hardware / Network│ [Review items]            [View logs]         │
│                   │                                               │
│ PROTECT           │ Recommended actions (preview before apply)    │
│ Security / Privacy│                                               │
│ Backup / Recovery │                                               │
│                   │                                               │
│ DEVELOP           │                                               │
│ Toolbox / SDKs    │                                               │
│ Git / Unity / XR  │                                               │
│ GPU / Media       │                                               │
│ Automation        │                                               │
└───────────────────┴───────────────────────────────────────────────┘
```

No fabricated health score: show actual measured values, timestamps, unavailable sensors and severity of verified findings. Dashboard actions: scan, app inventory, storage analysis, report export. Module screens: filters, table/grid, detail inspector, action preview and cancelable progress. Security page shows Defender state, firewall profiles and update status rather than claiming independent protection. Package page shows exact package ID, source, installed version, available version, publisher and consent. Development page has workspace profiles with protected paths; XR page shows connected authorized ADB devices and deployment logs. Destructive operations require dedicated confirmation with affected targets and rollback limits.
